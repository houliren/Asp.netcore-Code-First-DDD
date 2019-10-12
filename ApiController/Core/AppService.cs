using AutoMapper;
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
        /// 传入实体内容，转化为对应的Dto
        /// </summary>
        /// <typeparam name="TDto">要转化的Dto类</typeparam>
        /// <param name="entity">传入的实体数据</param>
        /// <returns></returns>
        public TDto EntityToDto<TDto>(object entity) where TDto : class, new()
        {
            return Entitytodto<TDto>(entity);
        }


        /// <summary>
        /// 传入List实体内容，转化为对应的List Dto
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="entityList">传入的List实体数据</param>
        /// <returns></returns>
        public List<TDto> EntityToDto<TDto>(List<object> entityList) where TDto : class, new()
        {
            List<TDto> list = new List<TDto>();
            foreach (var childObject in entityList)
            {
                list.Add(Entitytodto<TDto>(childObject));
            }
            return list;
        }

        /// <summary>
        /// 传入实体内容，转化为对应的Dto
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private TDto Entitytodto<TDto>(object entity) where TDto : class, new()
        {
            TDto dto = new TDto();
            PropertyInfo[] propertyInfos = entity.GetType().GetProperties();
            PropertyInfo[] dtoproperty = dto.GetType().GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                for (int j = 0; j < dtoproperty.Length; j++)
                {
                    if (propertyInfos[i].Name == dtoproperty[j].Name)
                    {
                        dtoproperty[j].SetValue(dto, propertyInfos[i].GetValue(entity));
                        break;
                    }
                }
            }
            return dto;
        }
    }
}
