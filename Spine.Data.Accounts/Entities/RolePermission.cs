using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Accounts.Entities
{
    [Index(nameof(CompanyId), nameof(RoleId))]
    public class RolePermission : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid RoleId { get; set; }
        public Permissions Permission { get; set; }

    }
}
