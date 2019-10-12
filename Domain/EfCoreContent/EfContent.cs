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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            var modelDiffer = _dbContext.GetInfrastructure().GetService<IMigrationsModelDiffer>();
            if (modelDiffer.HasDifferences(lastModel, _dbContext.Model))
            {
                // 用 IMigrationsModelDiffer 的 GetDifferences 方法获取迁移的操作对象
                var upOperations = modelDiffer.GetDifferences(lastModel, _dbContext.Model);
                using (var trans = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // 通过 IMigrationsSqlGenerator 将操作迁移操作对象生成迁移的sql脚本，并执行
                        _dbContext.GetInfrastructure()
                            .GetRequiredService<IMigrationsSqlGenerator>()
                            .Generate(upOperations, _dbContext.Model)
                            .ToList()
                            .ForEach(cmd => _dbContext.Database.ExecuteSqlCommand(cmd.CommandText));
                        _dbContext.Database.CommitTransaction();
                    }
                    catch (DbException ex)
                    {
                        _dbContext.Database.RollbackTransaction();
                        throw ex;
                    }
                    // 生成新的快照，存起来
                    var snapshotCode = new DesignTimeServicesBuilder(typeof(EfContent).Assembly, Assembly.GetEntryAssembly(), new OperationReporter(new OperationReportHandler()), new string[0])
                        .Build((DbContext)_dbContext)
                        .GetService<IMigrationsCodeGenerator>()
                        .GenerateSnapshot("Domain.Migrations", typeof(EfContent), "EfContentModelSnapshot", _dbContext.Model);//modelSnapshotNamespace：给动态生成类添加nameplace（必须和当前代码所在的命名控件下或者一样）modelSnapshotName：动态生成类的名称
                    _dbContext.Set<MigrationLog>().Add(new MigrationLog()
                    {
                        SnapshotDefine = snapshotCode,
                        MigrationTime = DateTime.Now
                    });
                    _dbContext.SaveChanges();
                }
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
            var compilation = CSharpCompilation.Create("Domain.Migrations")//assemblyName：给动态生成类添加nameplace（必须和当前代码所在的命名控件下或者一样）和生成快照时要保持一直
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(codedefine));
            using (var stream = new MemoryStream())
            {
                var compileResult = compilation.Emit(stream);
                return compileResult.Success
                    ? Assembly.Load(stream.GetBuffer()).CreateInstance("Domain.Migrations.EfContentModelSnapshot") as ModelSnapshot //typeName即生成的快照时设置的modelSnapshotNamespace+modelSnapshotName（nameplace+动态生成类的名称）
                    : null;
            }
        }
    }
}
