using EfCoreRepository.EfModelAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Domains.Tests
{
    /// <summary>
    /// 测试类
    /// </summary>
    [EfModel]
    public class Test1
    {
        [Key]
        public int Id { get; set; }
        public string HelloWorld { get; set; }

        public int aaa { get; set; }

        public test2 AddTest2(test2 test2)
        {
            test2.TestId = Id;
            return test2;
        }

    }
}
