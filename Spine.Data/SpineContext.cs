using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Spine.Common.Data.Interfaces;
using Spine.Common.Helpers;
using Spine.Data.Entities;
using Spine.Data.Entities.Admin;
using Spine.Data.Entities.BillsPayments;
using Spine.Data.Entities.Inventories;
using Spine.Data.Entities.Invoices;
using Spine.Data.Entities.Subscription;
using Spine.Data.Entities.Transactions;
using Spine.Data.Entities.Vendor;
using Spine.Data.Helpers;

namespace Spine.Data
{
    public class SpineContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public SpineContext(DbContextOptions<SpineContext> options) : base(options)
        {
        }

        //dbsets
        //common
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<BusinessType> BusinessTypes { get; set; }
        public DbSet<OperatingSector> OperatingSectors { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Document> Documents { get; set; } // documents for invoice, transactions, inventory

        public DbSet<AccountClass> AccountClasses { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<AccountSubClass> AccountSubClasses { get; set; }
        public DbSet<AccountingPeriod> AccountingPeriods { get; set; }
        
        //company
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyDocument> CompanyDocuments { get; set; }
        public DbSet<CompanySerial> CompanySerials { get; set; }
        public DbSet<CompanyFinancial> CompanyFinancials { get; set; }
        public DbSet<CompanyCurrency> CompanyCurrencies { get; set; }

        //customers
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerNote> CustomerNotes { get; set; }
        public DbSet<CustomerReminder> CustomerReminders { get; set; }

        //account
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<AccountConfirmationToken> AccountConfirmationTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        
        public DbSet<Entities.RolePermission> RolePermissions { get; set; }

        //invoices
        public DbSet<InvoiceColorTheme> InvoiceColorThemes { get; set; }
        public DbSet<InvoiceType> InvoiceTypes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceNoSetting> InvoiceNoSettings { get; set; }
        public DbSet<InvoicePreference> InvoicePreferences { get; set; }
        public DbSet<InvoiceCustomization> InvoiceCustomizations { get; set; }
        public DbSet<SentInvoice> SentInvoices { get; set; }
        public DbSet<PaymentIntegration> PaymentIntegrations { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        
        public DbSet<LineItem> LineItems { get; set; }
        public DbSet<TaxType> TaxTypes { get; set; }

        //transactions
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionReminder> TransactionReminders { get; set; }
        public DbSet<TransactionCategory> TransactionCategories { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<LedgerAccount> LedgerAccounts { get; set; }
        public DbSet<JournalPosting> JournalPostings { get; set; }
        
        [Obsolete("No Longer in use", true)]
        public DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        public DbSet<GeneralLedger> GeneralLedgers { get; set; }
        public DbSet<OpeningBalance> OpeningBalances { get; set; }
        
        /// <summary>
        /// to hold uploaded bank transactions till the second leg is selected and they're moved to Transactions and GeneralLedger
        /// </summary>
        public DbSet<BankTransaction> BankTransactions { get; set; }

        //billspayments
        public DbSet<BillCategory> BillCategories { get; set; }
        public DbSet<BillPayment> BillPayments { get; set; }
        public DbSet<MoneyTransfer> MoneyTransfers { get; set; }
        public DbSet<SavedBeneficiary> SavedBeneficiaries { get; set; }

        //inventories
        public DbSet<InventoryLocation> InventoryLocations { get; set; }
        public DbSet<MeasurementUnit> MeasurementUnits { get; set; }

        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryNote> InventoryNotes { get; set; }
        public DbSet<InventoryCategory> ProductCategories { get; set; }
        public DbSet<ProductLocation> ProductLocations { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<ReceivedGood> ReceivedGoods { get; set; }
        public DbSet<ReceivedGoodsLineItem> ReceivedGoodsLineItems { get; set; }
        public DbSet<InventoryPriceHistory> InventoryPriceHistories { get; set; }
        
        //Vendors
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorPayment> VendorPayments { get; set; }
        public DbSet<VendorAddress> VendorAddresses { get; set; }
        public DbSet<VendorBankAccount> VendorBankAccounts { get; set; }
        public DbSet<VendorContactPerson> VendorContactPersons { get; set; }

        //Subscription
        public DbSet<Plan> Plans { get; set; }
        public DbSet<CompanySubscription> CompanySubscriptions { get; set; }
        public DbSet<SubscriberBilling> SubscriberBillings { get; set; }
        public DbSet<SubscriberShipping> SubscriberShippings { get; set; }
        public DbSet<SubscriberNote> SubscriberNotes { get; set; }
        public DbSet<SubscriberNotification> SubscriberNotifications { get; set; }

        //Admin
        public DbSet<DocumentTemplate> Templates { get; set; }
        public DbSet<NotificationPath> NotificationPaths { get; set; }
        public DbSet<OfferPromotion> OfferPromotions { get; set; }
        public DbSet<ReferralCode> ReferralCodes { get; set; }
        public DbSet<PromotionalCode> PromotionalCodes { get; set; }
        public DbSet<UsedReferralCode> UsedReferralCodes { get; set; }


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

            //modelBuilder.HasSequence("InvoiceNoSequence").StartsAt(1000).IncrementsBy(1);
            //modelBuilder.Entity<Invoice>(builder =>
            //{
            //    // HiLo Id generation only for InvoiceNo
            //    builder
            //        .Property(x => x.InvoiceNo)
            //        .UseHiLo("InvoiceNoSequence");
            //});

            modelBuilder.Entity<ApplicationRole>().HasData(StaticData.SystemDefinedRoles());
            modelBuilder.Entity<BusinessType>().HasData(StaticData.BusinessTypes());
            modelBuilder.Entity<OperatingSector>().HasData(StaticData.OperatingSectors());
            modelBuilder.Entity<InvoiceType>().HasData(StaticData.InvoiceTypes());
            modelBuilder.Entity<Currency>().HasData(StaticData.Currencies());
            modelBuilder.Entity<MeasurementUnit>().HasData(StaticData.MeasurementUnits());
            modelBuilder.Entity<InvoiceColorTheme>().HasData(StaticData.InvoiceColorThemes());
            modelBuilder.Entity<BillCategory>().HasData(StaticData.BillCategories());
            modelBuilder.Entity<AccountClass>().HasData(StaticData.AccountClasses());
            modelBuilder.Entity<AccountSubClass>().HasData(StaticData.AccountSubClasses());
            modelBuilder.Entity<AccountType>().HasData(StaticData.AccountTypes());
            modelBuilder.Entity<Bank>().HasData(StaticData.Banks());
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

            //        entry.Entity.CreatedOn = Constants.GetCurrentDateTime();
            //    }

            //    entry.Entity.ModifiedOn = Constants.GetCurrentDateTime()w;
            //}

            foreach (var entry in GetOfType<IAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedOn = Constants.GetCurrentDateTime();
                }

                entry.Entity.ModifiedOn = Constants.GetCurrentDateTime();
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
