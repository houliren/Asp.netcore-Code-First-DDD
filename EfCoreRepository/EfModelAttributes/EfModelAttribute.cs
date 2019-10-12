using System;

namespace EfCoreRepository.EfModelAttributes
{
    /// <summary>
    /// EFmodel的实体特性
    /// 作者：侯立仁 2019-09-19
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EfModelAttribute: Attribute
    {
    }
}
