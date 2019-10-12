using Panda.DynamicWebApi;
using Panda.DynamicWebApi.Attributes;
using System.Collections.Generic;

namespace Controller.Core
{
    [DynamicWebApi]
    public interface IAppService : IDynamicWebApi
    {
        /// <summary>
        /// 传入实体内容，转化为对应的Dto
        /// </summary>
        /// <typeparam name="TDto">要转化的Dto类</typeparam>
        /// <param name="entity">传入的实体数据</param>
        /// <returns></returns>
        TDto EntityToDto<TDto>(object entity) where TDto : class, new();


        /// <summary>
        /// 传入List实体内容，转化为对应的List Dto
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="entityList">传入的List实体数据</param>
        /// <returns></returns>
        List<TDto> EntityToDto<TDto>(List<object> entityList) where TDto : class, new();
    }
}
