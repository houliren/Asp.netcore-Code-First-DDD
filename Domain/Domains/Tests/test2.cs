using EfCoreRepository.EfModelAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Domains.Tests
{
    [EfModel]
    public class test2
    {
        [Key]
        public int Id { get; set; }

        public string helloworld { get; set; }

        public int TestId { get; set; }

    }
}
