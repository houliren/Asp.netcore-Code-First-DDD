using EfCoreRepository;
using EfCoreRepository.EfModelAttributes;
using Microsoft.EntityFrameworkCore;
using System;
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
}
