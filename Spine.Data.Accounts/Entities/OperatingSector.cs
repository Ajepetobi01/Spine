using System;
using System.ComponentModel.DataAnnotations;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    public class OperatingSector
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string Sector { get; set; }

    }
}
