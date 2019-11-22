using Controller.Core;
using Domain;
using Domain.Domains.Tests;
using Domain.EfCoreContent;
using Microsoft.Extensions.Caching.Memory;
using System;
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

        public test2 TestAddTest2(test2 test22,int testId)
        {
            var test = EfContent.Where<Test1>(p => p.Id == testId).FirstOrDefault();
            var tset23 = test.AddTest2(test22);



            EfContent.Add(tset23);
            EfContent.SaveChanges();
            return tset23;
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
            var a =Convert.ToInt32("dsads");
            Domain.Domains.Tests.Test1 test = new Domain.Domains.Tests.Test1();
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
            return EntityToDto<Dto.TestDtos.TestDto>(EfContent.Where<Domain.Domains.Tests.Test1>(p => p.HelloWorld == "你好世界").FirstOrDefault()); 
        }
    }
}
