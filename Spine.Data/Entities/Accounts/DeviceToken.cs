using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Spine.Data.Entities
{
    [Index(nameof(UserId))]
    public class DeviceToken
    {
        [Key]
        public int Id { get; set; }
        
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime DateCreated { get; set; }
    }
}