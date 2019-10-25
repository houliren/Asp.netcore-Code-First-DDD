using Controller.Core;
using Domain;
using Domain.EfCoreContent;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace Controller.Controllers.Test
{
    /// <summary>
    /// 测试类
    /// </summary>
    public class TestAppService : AppService, IAppService
    {
        public EfContent EfContent { get; set; }
        public IMemoryCache memoryCache { get; set; }
        public TestAppService(EfContent efContent, IMemoryCache _memoryCache)
        {
            EfContent = efContent;
            memoryCache = _memoryCache;
        }


        public void updatedatabase()
        {
            EfContent.RunUpdateDataBaseEntity();
        }


        public string GetHelloWorld()
        {
            return "Hello World !!";
        }

        /// <summary>
        /// 添加一条测试数据
        /// </summary>
        /// <returns></returns>
        public string AddTest()
        {
            Domain.Domains.Tests.Test test = new Domain.Domains.Tests.Test();
            test.HelloWorld="你好世界";
            EfContent.Add(test);
            EfContent.SaveChanges();
            return "写入成功";
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <returns></returns>
        public Dto.TestDtos.TestDto GetTest()
        {
            return EntityToDto<Dto.TestDtos.TestDto>(EfContent.Where<Domain.Domains.Tests.Test>(p => p.HelloWorld == "你好世界").FirstOrDefault()); 
        }
    }
}
