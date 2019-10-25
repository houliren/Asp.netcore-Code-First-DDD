using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace Domain.EfCoreContent.EfCoreFun
{
    /// <summary>
    /// efcore扩展sql查询
    /// </summary>
    public static class EntityFrameworkCoreExtensions
    {
        /// <summary>
        /// 执行sql返回datatable
        /// </summary>
        /// <param name="efcontent"></param>
        /// <param name="sql"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 执行多条sql
        /// </summary>
        /// <param name="efcontent"></param>
        /// <param name="sqlList"></param>
        public static int ExecuteListSqlCommand(this EfContent efcontent, List<string> sqlList)
        {
            int retunInt = 0;
            try
            {
                using (var trans = efcontent.Database.BeginTransaction())
                {

                    sqlList.ForEach(cmd => retunInt += efcontent.Database.ExecuteSqlCommand(cmd));
                    efcontent.Database.CommitTransaction();
                }
            }
            catch (DbException ex)
            {
                try
                {
                    efcontent.Database.RollbackTransaction();
                }
                catch (DbException)
                {

                }
            }
            return retunInt;
        }

        /// <summary>
        /// 执行sql返回list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="efcontent"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static List<T> SqlQuery<T>(this EfContent efcontent, string sql, params object[] parameters) where T : class, new()
        {
            var dt = SqlQuery(efcontent, sql, parameters);
            return dt.ToList<T>();
        }

        /// <summary>
        /// datatable转list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
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
}
