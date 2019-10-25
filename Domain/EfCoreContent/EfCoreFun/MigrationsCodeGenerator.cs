using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EfCoreContent.EfCoreFun
{
    public class MigrationsCodeGenerator : IMigrationsCodeGenerator
    {
        public string FileExtension => throw new NotImplementedException();

        public string Language => throw new NotImplementedException();

        public string GenerateMetadata(string migrationNamespace, Type contextType, string migrationName, string migrationId, IModel targetModel)
        {
            throw new NotImplementedException();
        }

        public string GenerateMigration(string migrationNamespace, string migrationName, IReadOnlyList<MigrationOperation> upOperations, IReadOnlyList<MigrationOperation> downOperations)
        {
            throw new NotImplementedException();
        }

        public string GenerateSnapshot(string modelSnapshotNamespace, Type contextType, string modelSnapshotName, IModel model)
        {
            throw new NotImplementedException();
        }
    }
}
