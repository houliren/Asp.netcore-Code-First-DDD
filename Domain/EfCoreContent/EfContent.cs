using Domain.MigrationLogs;
using EfCoreRepository;
using EfCoreRepository.EfModelAttributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;


namespace Domain.EfCoreContent
{
    public class EfContent : DbContext
    {
        public DbContextOptions<EfContent> dbContextOptions { get; set; }
        public EfContent(DbContextOptions<EfContent> options) : base(options)
        {
            dbContextOptions = options;
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEntityConfigurationsFromAssembly<EfModelAttribute>(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// 扩展查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return base.Set<T>().Where(predicate);
        }
    }


    public static class EntityFrameworkCoreExtensions
    {

        public static DataTable SqlQuery(this EfContent efcontent, string sql, params object[] commandParameters)
        {
            var dt = new DataTable();
            using (var connection = efcontent.Database.GetDbConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    efcontent.Database.OpenConnection();
                    cmd.CommandText = sql;

                    if (commandParameters != null && commandParameters.Length > 0)
                        cmd.Parameters.AddRange(commandParameters);
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public static List<T> SqlQuery<T>(this EfContent efcontent, string sql, params object[] parameters) where T : class, new()
        {
            var dt = SqlQuery(efcontent, sql, parameters);
            return dt.ToList<T>();
        }

        public static List<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            var propertyInfos = typeof(T).GetProperties();
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                var t = new T();
                foreach (PropertyInfo p in propertyInfos)
                {
                    if (dt.Columns.IndexOf(p.Name) != -1 && row[p.Name] != DBNull.Value)
                        p.SetValue(t, row[p.Name], null);
                }
                list.Add(t);
            }
            return list;
        }
    }

    /// <summary>
    /// 给efcontent扩展自动处理数据库版本的方法
    /// </summary>
    public static class EfCoreMigration
    {
        /// <summary>
        /// 给EfContent添加的更新数据库结构的方法
        /// </summary>
        public static void RunUpdateDataBaseEntity(this EfContent _dbContext)
        {
            // 创建一个DbContext，具体方法怎样都行
            IModel lastModel = null;
            try
            {
                // 这里用一个表来存迁移记录，迁移时将上一个版本取出来
                var lastMigration = _dbContext.Set<MigrationLog>()
                    .OrderByDescending(e => e.Id) // mysql下自动生成的时间日期字段时间精度为秒
                    .FirstOrDefault();
                // 上一个版本生成的类型文本是以base64编码存储的，取出来还原成DbContext模型对象
                lastModel = lastMigration == null ? null : (CreateModelSnapshot(lastMigration.SnapshotDefine)?.Model);
            }
            catch (DbException) { }
            // 需要从历史版本库中取出快照定义，反序列化成类型 GetDifferences(快照模型, context.Model);
            // 实际情况下要传入历史快照
            var modelDiffer = _dbContext.Database.GetService<IMigrationsModelDiffer>();
            if (modelDiffer.HasDifferences(lastModel, _dbContext.Model))
            {
                // 用 IMigrationsModelDiffer 的 GetDifferences 方法获取迁移的操作对象
                var upOperations = modelDiffer.GetDifferences(lastModel, _dbContext.Model);


                List<string> sqlChangeColumNameList = new List<string>();
                List<MigrationOperation> list = new List<MigrationOperation>();
                foreach (var upOperation in upOperations)
                {
                    if (upOperation is RenameColumnOperation)
                    {
                        string column_type = string.Empty;
                        string sql = "select column_type from information_schema.columns where table_name='" + (upOperation as RenameColumnOperation).Table + "'  and column_name='" + (upOperation as RenameColumnOperation).Name + "'";
                        var dataTable = _dbContext.SqlQuery(sql);
                        if (dataTable != null && dataTable.Rows.Count > 0)
                        {
                            column_type = dataTable.Rows[0].ItemArray[0].ToString();
                        }
                        sqlChangeColumNameList.Add("alter table " + (upOperation as RenameColumnOperation).Table + " change  column " + (upOperation as RenameColumnOperation).Name + " " + (upOperation as RenameColumnOperation).NewName + " " + column_type + " ;");
                    }
                    else
                    {
                        list.Add(upOperation);
                    }
                }
                try
                {
                    using (var trans = _dbContext.Database.BeginTransaction())
                    {

                        sqlChangeColumNameList.ForEach(cmd => _dbContext.Database.ExecuteSqlCommand(cmd));
                        _dbContext.Database.CommitTransaction();
                    }
                }
                catch (DbException ex)
                {
                    _dbContext.Database.RollbackTransaction();
                }
                if (list.Count > 0)
                {
                    //通过 IMigrationsSqlGenerator 将操作迁移操作对象生成迁移的sql脚本，并执行
                    var sqlList = _dbContext.Database.GetService<IMigrationsSqlGenerator>()
                        .Generate(list, _dbContext.Model)
                        .ToList();


                    try
                    {
                        using (var trans = _dbContext.Database.BeginTransaction())
                        {

                            sqlList.ForEach(cmd => _dbContext.Database.ExecuteSqlCommand(cmd.CommandText));
                            _dbContext.Database.CommitTransaction();
                        }
                    }
                    catch (DbException ex)
                    {
                        _dbContext.Database.RollbackTransaction();
                    } 
                }

                // 生成新的快照，存起来
                var snapshotCode = new DesignTimeServicesBuilder(typeof(EfContent).Assembly, Assembly.GetEntryAssembly(), new OperationReporter(new OperationReportHandler()), new string[0])
                    .Build((DbContext)_dbContext)
                    .GetService<IMigrationsCodeGenerator>()
                    .GenerateSnapshot("ApiHost.Migrations", typeof(EfContent), "EfContentModelSnapshot", _dbContext.Model);//modelSnapshotNamespace：给动态生成类添加nameplace（必须和当前代码所在的命名控件下或者一样）modelSnapshotName：动态生成类的名称
                _dbContext.Set<MigrationLog>().Add(new MigrationLog()
                {
                    SnapshotDefine = snapshotCode,
                    MigrationTime = DateTime.Now
                });
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 把实体的string转成该对象
        /// </summary>
        /// <param name="codedefine"></param>
        /// <returns></returns>
        private static ModelSnapshot CreateModelSnapshot(string codedefine)
        {
            // 生成快照，需要存到数据库中供更新版本用
            var references = typeof(EfContent).Assembly
                .GetReferencedAssemblies()
                .Select(e => MetadataReference.CreateFromFile(Assembly.Load(e).Location))
                .Union(new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                    MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(EfContent).Assembly.Location)
                });
            var compilation = CSharpCompilation.Create("ApiHost.Migrations")//assemblyName：给动态生成类添加nameplace（必须和当前代码所在的命名控件下或者一样）和生成快照时要保持一直
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(codedefine));
            using (var stream = new MemoryStream())
            {
                var compileResult = compilation.Emit(stream);
                return compileResult.Success
                    ? Assembly.Load(stream.GetBuffer()).CreateInstance("ApiHost.Migrations.EfContentModelSnapshot") as ModelSnapshot //typeName即生成的快照时设置的modelSnapshotNamespace+modelSnapshotName（nameplace+动态生成类的名称）
                    : null;
            }
        }
    }
}
