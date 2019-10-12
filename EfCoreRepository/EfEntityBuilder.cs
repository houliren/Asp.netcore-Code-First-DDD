using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace EfCoreRepository
{
    /// <summary>
    /// ef实体的创建
    /// </summary>
    public static class EfEntityBuilder
    {
        /// <summary>
        /// 从程序反射添加实体配置
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="modelBuilder"></param>
        /// <param name="assembly"></param>
        public static void AddEntityConfigurationsFromAssembly<TAttribute>(this ModelBuilder modelBuilder, Assembly assembly)
            where TAttribute : Attribute
        {
            var autoTypes = assembly.GetTypes().Where(x => x.GetCustomAttribute<TAttribute>() != null && x.IsPublic);
            foreach (var entity in autoTypes)
            {
                modelBuilder.Model.AddEntityType(entity);
            }
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var currentTableName = modelBuilder.Entity(entity.Name).Metadata.Relational().TableName;
                modelBuilder.Entity(entity.Name).ToTable(currentTableName.ToLower());
                var properties = entity.GetProperties();
                foreach (var property in properties)
                    modelBuilder.Entity(entity.Name).Property(property.Name).HasColumnName(property.Name.ToLower());
            }
        }
    }
}
