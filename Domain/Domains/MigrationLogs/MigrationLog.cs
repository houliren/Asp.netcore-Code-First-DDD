using EfCoreRepository.EfModelAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.MigrationLogs
{
    [EfModel]
    public class MigrationLog
    {
        [Key]
        public int Id { get; set; }
        public string SnapshotDefine { get; set; }

        public DateTime MigrationTime { get; set; }
    }
}
