using Domain.MigrationLogs;
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
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Domain.EfCoreContent
{
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
                // 读取迁移记录，把快照还原会model
                var lastMigration = _dbContext.Set<MigrationLog>()
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefault();
                lastModel = lastMigration == null ? null : (CreateModelSnapshot(lastMigration.SnapshotDefine, "ApiHost.Migrations", "EfContentModelSnapshot")?.Model);
            }
            catch (DbException) { }
            var modelDiffer = _dbContext.Database.GetService<IMigrationsModelDiffer>();
            //判断是否有更改
            if (modelDiffer.HasDifferences(lastModel, _dbContext.Model))
            {
                // 用 IMigrationsModelDiffer 的 GetDifferences 方法获取迁移的操作对象
                var upOperations = modelDiffer.GetDifferences(lastModel, _dbContext.Model);

                //执行迁移
                Migrationing(upOperations, _dbContext);

                // 生成新的快照，存起来
                var snapshotCode = ModelSnapshotToString(_dbContext, "ApiHost.Migrations", "EfContentModelSnapshot");

                _dbContext.Set<MigrationLog>().Add(new MigrationLog()
                {
                    SnapshotDefine = snapshotCode,
                    MigrationTime = DateTime.Now
                });
                _dbContext.SaveChanges();
            }
        }


        /// <summary>
        /// 迁移数据库结构
        /// </summary>
        /// <param name="upOperations"></param>
        /// <param name="_dbContext"></param>
        private static void Migrationing(IReadOnlyList<MigrationOperation> upOperations, EfContent _dbContext)
        {
            List<string> sqlChangeColumNameList = new List<string>();
            List<MigrationOperation> list = new List<MigrationOperation>();
            //执行迁移列名修改
            foreach (var upOperation in upOperations)
            {
                if (upOperation is RenameColumnOperation)
                {

                    sqlChangeColumNameList.Add(RenameColumnOperationToSql(upOperation as RenameColumnOperation, _dbContext));
                }
                else
                {
                    list.Add(upOperation);
                }
            }
            int columChangeCount = _dbContext.ExecuteListSqlCommand(sqlChangeColumNameList);
            //处理剩余迁移
            if (list.Count > 0)
            {
                //通过 IMigrationsSqlGenerator 将操作迁移操作对象生成迁移的sql脚本，并执行
                var sqlList = _dbContext.Database.GetService<IMigrationsSqlGenerator>()
                    .Generate(list, _dbContext.Model)
                    .Select(p => p.CommandText).ToList();
                int changeCount = _dbContext.ExecuteListSqlCommand(sqlList);
            }
        }

        /// <summary>
        /// 把RenameColumnOperation该字段转换成sql
        /// </summary>
        /// <param name="renameColumnOperation"></param>
        /// <returns></returns>
        private static string RenameColumnOperationToSql(RenameColumnOperation renameColumnOperation, EfContent _dbContext)
        {
            string column_type = string.Empty;
            string sql = "select column_type from information_schema.columns where table_name='" + (renameColumnOperation as RenameColumnOperation).Table + "'  and column_name='" + (renameColumnOperation as RenameColumnOperation).Name + "'";
            var dataTable = _dbContext.SqlQuery(sql);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                column_type = dataTable.Rows[0].ItemArray[0].ToString();
            }
            return "alter table " + (renameColumnOperation as RenameColumnOperation).Table + " change  column " + (renameColumnOperation as RenameColumnOperation).Name + " " + (renameColumnOperation as RenameColumnOperation).NewName + " " + column_type + " ;";
        }


        /// <summary>
        /// 把model生成快照
        /// </summary>
        /// <param name="_dbContext">efcontent</param>
        /// <param name="nameSpace">快照类的空间名称</param>
        /// <param name="className">快照类的名称</param>
        /// <returns></returns>
        private static string ModelSnapshotToString(EfContent _dbContext, string nameSpace, string className)
        {
            var snapshotCode = new DesignTimeServicesBuilder(typeof(EfContent).Assembly, Assembly.GetEntryAssembly(), new OperationReporter(new OperationReportHandler()), new string[0])
                    .Build((DbContext)_dbContext)
                    .GetService<IMigrationsCodeGenerator>()
                    .GenerateSnapshot(nameSpace, typeof(EfContent), className, _dbContext.Model);//modelSnapshotNamespace：给动态生成类添加nameplace（必须和当前代码所在的命名控件下或者一样）modelSnapshotName：动态生成类的名称
            return snapshotCode;
        }



        /// <summary>
        /// 把实体的string转成该对象
        /// </summary>
        /// <param name="codedefine"></param>
        /// <param name="nameSpace">快照类的空间名称</param>
        /// <param name="className">快照类的名称</param>
        /// <returns></returns>
        private static ModelSnapshot CreateModelSnapshot(string codedefine, string nameSpace, string className)
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
            var compilation = CSharpCompilation.Create(nameSpace)//assemblyName：给动态生成类添加nameplace（必须和当前代码所在的命名控件下或者一样）和生成快照时要保持一直
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(codedefine));

            using (var stream = new MemoryStream())
            {
                var compileResult = compilation.Emit(stream);
                return compileResult.Success
                    ? Assembly.Load(stream.GetBuffer()).CreateInstance(nameSpace + "." + className) as ModelSnapshot //typeName即生成的快照时设置的modelSnapshotNamespace+modelSnapshotName（nameplace+动态生成类的名称）
                    : null;
            }
        }
    }
}
