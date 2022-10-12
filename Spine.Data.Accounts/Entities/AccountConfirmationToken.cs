﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    public class AccountConfirmationToken : IEntity
    {
        public Guid Id { get; set; }

        public string Token { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }
    }
}
