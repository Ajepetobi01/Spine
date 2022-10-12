using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Spine.Common.Data.Interfaces;
using Spine.Common.Helper;
using Spine.Data.Accounts.Entities;
using Spine.Data.Accounts.Helpers;

namespace Spine.Data.Accounts
{
    public class AccountDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
        {
        }

        //dbsets
        public DbSet<Address> Addresses { get; set; }
        public DbSet<BusinessType> BusinessTypes { get; set; }
        public DbSet<OperatingSector> OperatingSectors { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyDocument> CompanyDocuments { get; set; }
        public DbSet<CompanyFinancial> CompanyFinancials { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<AccountConfirmationToken> AccountConfirmationTokens { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("ApplicationUsers");
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("ApplicationRoles");
            });

            modelBuilder.Entity<BusinessType>().HasData(StaticData.BusinessTypes());
            modelBuilder.Entity<OperatingSector>().HasData(StaticData.OperatingSectors());
        }

        public override int SaveChanges()
        {
            PreSaveChanges();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            PreSaveChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private interface IAuditableEntity : IEntity, IAuditable
        {
        }

        private void PreSaveChanges()
        {
            //foreach (var entry in GetOfType<IAuditableEntity>())
            //{
            //    if (entry.State == EntityState.Added)
            //    {
            //        if (entry.Entity.Id == default)
            //            entry.Entity.Id = SequentialGuid.Create();

            //        entry.Entity.CreatedOn = DateTime.UtcNow;
            //    }

            //    entry.Entity.ModifiedOn = DateTime.UtcNow;
            //}

            foreach (var entry in GetOfType<IAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedOn = DateTime.UtcNow;
                }

                entry.Entity.ModifiedOn = DateTime.UtcNow;
            }
        }

        private class TypeToEntry<TType>
        {
            public TType Entity { get; set; }
            public EntityState State { get; set; }
            public EntityEntry Entry { get; set; }
        }

        private IEnumerable<TypeToEntry<TType>> GetOfType<TType>()
        {
            return ChangeTracker.Entries()
                                .Where(e => e.Entity is IAuditable && (e.State == EntityState.Added
                                                        || e.State == EntityState.Modified))
                                .Select(x => new TypeToEntry<TType>
                                {
                                    Entity = (TType)x.Entity,
                                    State = x.State,
                                    Entry = x
                                });
        }
    }
}
