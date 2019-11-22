using Panda.DynamicWebApi;
using Panda.DynamicWebApi.Attributes;
using System.Collections.Generic;

namespace Controller.Core
{
    [DynamicWebApi]
    public interface IAppService : IDynamicWebApi
    {

        /// <summary>
        /// 实体转实体
        /// </summary>
        /// <typeparam name="T">要转成的实体</typeparam>
        /// <param name="needToEntity">需要转的实体</param>
        /// <returns></returns>
        T EntityToEntity<T>(object needToEntity) where T : class, new();


        /// <summary>
        /// 把dto的值全部赋值到entity上
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        T DtoAssignmentEntity<T>(T entity, object dto) where T : class, new();
    }
}
