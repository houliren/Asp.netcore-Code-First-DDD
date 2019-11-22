using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Controller.Core
{
    public class AppService : IAppService
    {

        public AppService()
        {
        }


        /// <summary>
        /// 实体转实体
        /// </summary>
        /// <typeparam name="T">要转成的实体</typeparam>
        /// <param name="needToEntity">需要转的实体</param>
        /// <returns></returns>
        public T EntityToEntity<T>(object needToEntity) where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(needToEntity));
        }

        /// <summary>
        /// 把dto的值全部赋值到entity上
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public T DtoAssignmentEntity<T>(T entity, object dto) where T : class, new()
        {
            return DtoAssignmentEntityRecursion(entity, dto) as T;
        }

        /// <summary>
        /// 把dto的值全部赋值到entity上的递归函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private object DtoAssignmentEntityRecursion<T>(T entity, object dto) where T : class, new()
        {
            if (dto == null || entity == null)
            {
                return entity;
            }
            System.Reflection.PropertyInfo[] properties = entity.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            System.Reflection.PropertyInfo[] dtoproperties = dto.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return entity;
            }
            if (dtoproperties.Length <= 0)
            {
                return entity;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                foreach (var dtoItem in dtoproperties)
                {
                    if (item.Name == dtoItem.Name)
                    {
                        if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                        {

                            object value = dtoItem.GetValue(dto, null);
                            if (value != null)
                                item.SetValue(entity, value);
                            break;
                        }
                        else
                        {
                            object value = item.GetValue(entity, null);
                            object dtovalue = dtoItem.GetValue(dto, null);
                            value = DtoAssignmentEntityRecursion(value, dtovalue);
                            if (value != null)
                                item.SetValue(entity, value);
                            break;
                        }
                    }
                }
            }
            return entity;
        }
    }
}
