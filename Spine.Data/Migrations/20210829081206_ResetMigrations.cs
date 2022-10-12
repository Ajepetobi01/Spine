using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class ResetMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "AccountConfirmationTokens",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AccountConfirmationTokens", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Addresses",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        AddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        City = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        State = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Country = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Addresses", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ApplicationRoles",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
            //        IsOwnerRole = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ApplicationRoles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ApplicationUsers",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        FirstName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsBusinessOwner = table.Column<bool>(type: "bit", nullable: false),
            //        Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AuditLogs",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        EntityType = table.Column<int>(type: "int", nullable: false),
            //        Action = table.Column<int>(type: "int", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AuditLogs", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "BankAccounts",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        BankName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BankCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        AccountType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
            //        AccountName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        AccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        SubLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsCash = table.Column<bool>(type: "bit", nullable: false),
            //        Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        DateDeactivated = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        IntegrationProvider = table.Column<int>(type: "int", nullable: false),
            //        AccountCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        AccountId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BankAccounts", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "BankTransactions",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        ReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        Payee = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ChequeNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BankTransactions", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "BusinessTypes",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BusinessTypes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CableTvs",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Bouquet = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Merchant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        SmartCardNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CableTvs", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Companies",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
            //        Website = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        EmployeeCount = table.Column<int>(type: "int", nullable: true),
            //        DateEstablished = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        AddresssId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        BusinessType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        OperatingSector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        LogoId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        SocialMediaProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Motto = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        IsVerified = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        BaseCurrencyId = table.Column<int>(type: "int", nullable: false),
            //        ID_Subscription = table.Column<int>(type: "int", nullable: false),
            //        TIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Ref_ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Companies", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CompanyCurrencies",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        OldCurrencyId = table.Column<int>(type: "int", nullable: false),
            //        CurrencyId = table.Column<int>(type: "int", nullable: false),
            //        Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        ActivatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ActivatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        DeactivatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        DeactivatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CompanyCurrencies", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CompanyDocuments",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        DocumentId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CompanyDocuments", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CompanyFinancials",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Year = table.Column<int>(type: "int", nullable: false),
            //        EstimatedTurnOver = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        LastTurnOver = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        LastProfitBeforeTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        EstimatedProfitBeforeTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        LastProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        EstimatedProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        LastEarningBeforeInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        HasCreditDebitTerminal = table.Column<bool>(type: "bit", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CompanyFinancials", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CompanySubscription",
            //    columns: table => new
            //    {
            //        ID_Subscription = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ID_Company = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ID_Plan = table.Column<int>(type: "int", nullable: false),
            //        PlanType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TransactionRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        PaymentStatus = table.Column<bool>(type: "bit", nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        ExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CompanySubscription", x => x.ID_Subscription);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Currencies",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
            //        Symbol = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Currencies", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CustomerAddresses",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        IsPrimary = table.Column<bool>(type: "bit", nullable: false),
            //        IsBilling = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CustomerAddresses", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CustomerNotes",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CustomerNotes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CustomerReminders",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ReminderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CustomerReminders", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Customers",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
            //        BusinessName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BusinessType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        OperatingSector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        AmountReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        AmountOwed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Gender = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TIN = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TotalPurchases = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        LastTransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Customers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Documents",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ParentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Documents", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "GeneralLedgers",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        SubLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        PostedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Narration = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        ReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        TransactionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Type = table.Column<int>(type: "int", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_GeneralLedgers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InternetServices",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Product = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Biller = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InternetServices", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Inventories",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ParentInventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        InventoryType = table.Column<int>(type: "int", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        QuantityInStock = table.Column<int>(type: "int", nullable: false),
            //        InventoryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        LastRestockDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        SerialNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        SKU = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ReorderLevel = table.Column<int>(type: "int", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        MeasurementUnitId = table.Column<int>(type: "int", nullable: true),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        UnitCostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        UnitSalesPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        NotifyOutOfStock = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Inventories", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InventoryLocations",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        State = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InventoryLocations", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InventoryNotes",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InventoryNotes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InvoiceCustomizations",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        LogoEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        SignatureEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        Banner = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        BannerId = table.Column<int>(type: "int", nullable: false),
            //        CompanyLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Theme = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ThemeId = table.Column<int>(type: "int", nullable: false),
            //        SignatureName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InvoiceCustomizations", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InvoiceNoSettings",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Prefix = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
            //        Separator = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
            //        LastGenerated = table.Column<int>(type: "int", nullable: false),
            //        LastGeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InvoiceNoSettings", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InvoicePayments",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        PaymentSource = table.Column<int>(type: "int", nullable: false),
            //        InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        IsPartPayment = table.Column<bool>(type: "bit", nullable: false),
            //        AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        PaymentIntegrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        BankCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        ReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InvoicePayments", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InvoicePreferences",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Discount = table.Column<int>(type: "int", nullable: false),
            //        Tax = table.Column<int>(type: "int", nullable: false),
            //        ApplyTax = table.Column<int>(type: "int", nullable: false),
            //        DueDate = table.Column<int>(type: "int", nullable: false),
            //        CurrencyId = table.Column<int>(type: "int", nullable: false),
            //        RoundAmountToNearestWhole = table.Column<bool>(type: "bit", nullable: false),
            //        EnableDueDate = table.Column<bool>(type: "bit", nullable: false),
            //        CustomizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ShareMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PaymentLinkEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        PaymentIntegrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InvoicePreferences", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Invoices",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        InvoiceTypeId = table.Column<int>(type: "int", nullable: false),
            //        InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        InvoiceStatus = table.Column<int>(type: "int", nullable: false),
            //        PhoneNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        InvoiceNoString = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        CustomerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CustomerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CustomerNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        DiscountType = table.Column<int>(type: "int", nullable: false),
            //        DiscountRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        InvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        InvoiceTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        InvoiceBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        IsRetainer = table.Column<bool>(type: "bit", nullable: false),
            //        IsRecurring = table.Column<bool>(type: "bit", nullable: false),
            //        RecurringFrequency = table.Column<int>(type: "int", nullable: false),
            //        CustomerReminder = table.Column<int>(type: "int", nullable: false),
            //        ReminderTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        TaxLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        BillingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        ShippingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        BillingAddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BillingAddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BillingState = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BillingCountry = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BillingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        ShippingAddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ShippingAddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ShippingState = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ShippingCountry = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ShippingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Invoices", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InvoiceTypes",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InvoiceTypes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "LedgerAccounts",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsInflow = table.Column<bool>(type: "bit", nullable: false),
            //        IsDefault = table.Column<bool>(type: "bit", nullable: false),
            //        AccountType = table.Column<int>(type: "int", nullable: false),
            //        KindOfAccount = table.Column<int>(type: "int", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_LedgerAccounts", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "LineItems",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ParentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Item = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DiscountType = table.Column<int>(type: "int", nullable: false),
            //        DiscountRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TaxLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_LineItems", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MeasurementUnits",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MeasurementUnits", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MoneyTransfers",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        RecipientAccountNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        RecipientAccountName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RecipientBank = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BankCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MoneyTransfers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Notifications",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Category = table.Column<int>(type: "int", nullable: false),
            //        EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Message = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
            //        IsRead = table.Column<bool>(type: "bit", nullable: false),
            //        IsCleared = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Notifications", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OperatingSectors",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Sector = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_OperatingSectors", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PasswordResetTokens",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PaymentIntegrations",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        IntegrationProvider = table.Column<int>(type: "int", nullable: false),
            //        IntegrationType = table.Column<int>(type: "int", nullable: false),
            //        SubaccountCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RecipientCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BusinessName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        PrimaryContactName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PrimaryContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PrimaryContactPhone = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        SettlementBankCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        SettlementBankName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        SettlementBankCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        SettlementAccountNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PaymentIntegrations", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Plan",
            //    columns: table => new
            //    {
            //        PlanId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        PlanName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        PlanDuration = table.Column<int>(type: "int", nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
            //        IsFreePlan = table.Column<bool>(type: "bit", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Plan", x => x.PlanId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProductCategories",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProductCategories", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProductLocations",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        QuantityInStock = table.Column<int>(type: "int", nullable: false),
            //        DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProductLocations", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProductStocks",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        RestockDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ReorderQuantity = table.Column<int>(type: "int", nullable: false),
            //        UnitCostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        UnitSalesPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        PurchaseOrderReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProductStocks", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PurchaseOrderReceipts",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        LineItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DateReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ReceivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PurchaseOrderReceipts", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PurchaseOrders",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ExpectedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        VendorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        VendorEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        OrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        AdditionalNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RolePermissions",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Permission = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RolePermissions", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SavedBeneficiaries",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        AccountName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BankName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        BankCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SavedBeneficiaries", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SentInvoices",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CustomizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        PaymentLink = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        PaymentLinkReference = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        PaymentLinkAccessCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        DateSent = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SentInvoices", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SubLedgerAccounts",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SubLedgerAccounts", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SubscriberBilling",
            //    columns: table => new
            //    {
            //        ID_Billing = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ID_Subscriber = table.Column<int>(type: "int", nullable: false),
            //        Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ID_Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ID_State = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PostalCode = table.Column<int>(type: "int", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SubscriberBilling", x => x.ID_Billing);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SubscriberNote",
            //    columns: table => new
            //    {
            //        ID_Note = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SubscriberNote", x => x.ID_Note);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SubscriberNotification",
            //    columns: table => new
            //    {
            //        ID_Notification = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsRead = table.Column<bool>(type: "bit", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DateRead = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        TimeRead = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Comments = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SubscriberNotification", x => x.ID_Notification);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SubscriberShipping",
            //    columns: table => new
            //    {
            //        ID_Shipping = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ID_Subscriber = table.Column<int>(type: "int", nullable: false),
            //        Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ID_Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ID_State = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PostalCode = table.Column<int>(type: "int", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SubscriberShipping", x => x.ID_Shipping);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TaxTypes",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Tax = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        TaxRate = table.Column<double>(type: "float", nullable: false),
            //        IsVAT = table.Column<bool>(type: "bit", nullable: false),
            //        IsCompound = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TaxTypes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TransactionCategories",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsInflow = table.Column<bool>(type: "bit", nullable: false),
            //        IsDefault = table.Column<bool>(type: "bit", nullable: false),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TransactionCategories", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Transactions",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TransactionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            //        ReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Source = table.Column<int>(type: "int", nullable: false),
            //        Type = table.Column<int>(type: "int", nullable: false),
            //        Payee = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ChequeNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Transactions", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UtilityServices",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        AccountFrom = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TransactionTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Product = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Biller = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        MeterNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        RefNo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UtilityServices", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoleClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetRoleClaims_ApplicationRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "ApplicationRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserClaims_ApplicationUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "ApplicationUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserLogins",
            //    columns: table => new
            //    {
            //        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserLogins_ApplicationUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "ApplicationUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserRoles",
            //    columns: table => new
            //    {
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_ApplicationRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "ApplicationRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_ApplicationUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "ApplicationUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserTokens",
            //    columns: table => new
            //    {
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserTokens_ApplicationUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "ApplicationUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.InsertData(
            //    table: "BusinessTypes",
            //    columns: new[] { "Id", "Type" },
            //    values: new object[,]
            //    {
            //        { 1, "Sole Proprietorship" },
            //        { 2, "Partnership" },
            //        { 3, "Limited Liability Company (LLC)" },
            //        { 4, "Corporation" },
            //        { 5, "Nonprofit Organization" }
            //    });

            //migrationBuilder.InsertData(
            //    table: "Currencies",
            //    columns: new[] { "Id", "Code", "Name", "Symbol" },
            //    values: new object[,]
            //    {
            //        { 1, "NGN", "NIGERIAN NAIRA", "₦" },
            //        { 2, "USD", "US DOLLARS", "$" },
            //        { 3, "GBP", "BRITISH POUNDS", "£" },
            //        { 4, "EUR", "EURO", "€" }
            //    });

            //migrationBuilder.InsertData(
            //    table: "InvoiceTypes",
            //    columns: new[] { "Id", "Type" },
            //    values: new object[,]
            //    {
            //        { 9, "Pro Forma Invoice" },
            //        { 8, "Expense Report" },
            //        { 7, "Timesheet Invoice" },
            //        { 6, "Commercial Invoice" },
            //        { 5, "Mixed Invoice" },
            //        { 4, "Debit Invoice" },
            //        { 3, "Credit Invoice" },
            //        { 1, "Standard Invoice" },
            //        { 2, "Retainer Invoice" }
            //    });

            //migrationBuilder.InsertData(
            //    table: "MeasurementUnits",
            //    columns: new[] { "Id", "Name", "Unit" },
            //    values: new object[,]
            //    {
            //        { 5, "Millilitres", "ml" },
            //        { 4, "Litres", "l" },
            //        { 3, "Kilogram", "Kg" },
            //        { 2, "Gram", "g" },
            //        { 1, "Metres", "m" }
            //    });

            //migrationBuilder.InsertData(
            //    table: "OperatingSectors",
            //    columns: new[] { "Id", "Sector" },
            //    values: new object[,]
            //    {
            //        { 14, "Retail" },
            //        { 15, "Media" },
            //        { 16, "Wholesale" },
            //        { 17, "Print/Publishing" },
            //        { 18, "Energy" },
            //        { 22, "Import/Export" },
            //        { 20, "E-commerce" },
            //        { 21, "Recruitment Services" },
            //        { 23, "Transportation Services" },
            //        { 19, "Fashion and accessories" },
            //        { 13, "Manufacturing" },
            //        { 1, "Aerospace" },
            //        { 11, "Education" },
            //        { 10, "Insurance" },
            //        { 9, "Data Analytics/Data Science" },
            //        { 8, "Healthcare" },
            //        { 7, "Purchase Stock" },
            //        { 6, "Government/Public Sector Services" },
            //        { 5, "Courier" }
            //    });

            //migrationBuilder.InsertData(
            //    table: "OperatingSectors",
            //    columns: new[] { "Id", "Sector" },
            //    values: new object[,]
            //    {
            //        { 4, "Financial Services" },
            //        { 3, "Agriculture" },
            //        { 2, "Entertainment" },
            //        { 24, "Logistics Stock" },
            //        { 12, "IT" },
            //        { 25, "Construction" }
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Addresses_CompanyId",
            //    table: "Addresses",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ApplicationRoles_CompanyId",
            //    table: "ApplicationRoles",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "RoleNameIndex",
            //    table: "ApplicationRoles",
            //    column: "NormalizedName",
            //    unique: true,
            //    filter: "[NormalizedName] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "EmailIndex",
            //    table: "ApplicationUsers",
            //    column: "NormalizedEmail");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ApplicationUsers_CompanyId",
            //    table: "ApplicationUsers",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "UserNameIndex",
            //    table: "ApplicationUsers",
            //    column: "NormalizedUserName",
            //    unique: true,
            //    filter: "[NormalizedUserName] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetRoleClaims_RoleId",
            //    table: "AspNetRoleClaims",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserClaims_UserId",
            //    table: "AspNetUserClaims",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserLogins_UserId",
            //    table: "AspNetUserLogins",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserRoles_RoleId",
            //    table: "AspNetUserRoles",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AuditLogs_CompanyId",
            //    table: "AuditLogs",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CompanyDocuments_CompanyId",
            //    table: "CompanyDocuments",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CustomerAddresses_CompanyId_CustomerId",
            //    table: "CustomerAddresses",
            //    columns: new[] { "CompanyId", "CustomerId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_CustomerNotes_CompanyId_CustomerId",
            //    table: "CustomerNotes",
            //    columns: new[] { "CompanyId", "CustomerId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_CustomerReminders_CompanyId_CustomerId",
            //    table: "CustomerReminders",
            //    columns: new[] { "CompanyId", "CustomerId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Customers_CompanyId",
            //    table: "Customers",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Documents_CompanyId_ParentItemId",
            //    table: "Documents",
            //    columns: new[] { "CompanyId", "ParentItemId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Inventories_CompanyId",
            //    table: "Inventories",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_InventoryLocations_CompanyId",
            //    table: "InventoryLocations",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_InventoryNotes_CompanyId_InventoryId",
            //    table: "InventoryNotes",
            //    columns: new[] { "CompanyId", "InventoryId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_InvoiceCustomizations_CompanyId",
            //    table: "InvoiceCustomizations",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_InvoiceNoSettings_CompanyId",
            //    table: "InvoiceNoSettings",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_InvoicePayments_CompanyId_InvoiceId",
            //    table: "InvoicePayments",
            //    columns: new[] { "CompanyId", "InvoiceId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_InvoicePreferences_CompanyId",
            //    table: "InvoicePreferences",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Invoices_CompanyId_CustomerId",
            //    table: "Invoices",
            //    columns: new[] { "CompanyId", "CustomerId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_LineItems_CompanyId_ParentItemId",
            //    table: "LineItems",
            //    columns: new[] { "CompanyId", "ParentItemId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Notifications_CompanyId_UserId",
            //    table: "Notifications",
            //    columns: new[] { "CompanyId", "UserId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_PaymentIntegrations_CompanyId",
            //    table: "PaymentIntegrations",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ProductCategories_CompanyId",
            //    table: "ProductCategories",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ProductLocations_CompanyId_InventoryId_LocationId",
            //    table: "ProductLocations",
            //    columns: new[] { "CompanyId", "InventoryId", "LocationId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_ProductStocks_CompanyId_InventoryId",
            //    table: "ProductStocks",
            //    columns: new[] { "CompanyId", "InventoryId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_PurchaseOrderReceipts_CompanyId_PurchaseOrderId_LineItemId",
            //    table: "PurchaseOrderReceipts",
            //    columns: new[] { "CompanyId", "PurchaseOrderId", "LineItemId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_PurchaseOrders_CompanyId",
            //    table: "PurchaseOrders",
            //    column: "CompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_RolePermissions_CompanyId_RoleId",
            //    table: "RolePermissions",
            //    columns: new[] { "CompanyId", "RoleId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_SentInvoices_CompanyId_InvoiceId",
            //    table: "SentInvoices",
            //    columns: new[] { "CompanyId", "InvoiceId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountConfirmationTokens");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "BankTransactions");

            migrationBuilder.DropTable(
                name: "BusinessTypes");

            migrationBuilder.DropTable(
                name: "CableTvs");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "CompanyCurrencies");

            migrationBuilder.DropTable(
                name: "CompanyDocuments");

            migrationBuilder.DropTable(
                name: "CompanyFinancials");

            migrationBuilder.DropTable(
                name: "CompanySubscription");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "CustomerAddresses");

            migrationBuilder.DropTable(
                name: "CustomerNotes");

            migrationBuilder.DropTable(
                name: "CustomerReminders");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "GeneralLedgers");

            migrationBuilder.DropTable(
                name: "InternetServices");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "InventoryLocations");

            migrationBuilder.DropTable(
                name: "InventoryNotes");

            migrationBuilder.DropTable(
                name: "InvoiceCustomizations");

            migrationBuilder.DropTable(
                name: "InvoiceNoSettings");

            migrationBuilder.DropTable(
                name: "InvoicePayments");

            migrationBuilder.DropTable(
                name: "InvoicePreferences");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "InvoiceTypes");

            migrationBuilder.DropTable(
                name: "LedgerAccounts");

            migrationBuilder.DropTable(
                name: "LineItems");

            migrationBuilder.DropTable(
                name: "MeasurementUnits");

            migrationBuilder.DropTable(
                name: "MoneyTransfers");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OperatingSectors");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PaymentIntegrations");

            migrationBuilder.DropTable(
                name: "Plan");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductLocations");

            migrationBuilder.DropTable(
                name: "ProductStocks");

            migrationBuilder.DropTable(
                name: "PurchaseOrderReceipts");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SavedBeneficiaries");

            migrationBuilder.DropTable(
                name: "SentInvoices");

            migrationBuilder.DropTable(
                name: "SubLedgerAccounts");

            migrationBuilder.DropTable(
                name: "SubscriberBilling");

            migrationBuilder.DropTable(
                name: "SubscriberNote");

            migrationBuilder.DropTable(
                name: "SubscriberNotification");

            migrationBuilder.DropTable(
                name: "SubscriberShipping");

            migrationBuilder.DropTable(
                name: "TaxTypes");

            migrationBuilder.DropTable(
                name: "TransactionCategories");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UtilityServices");

            migrationBuilder.DropTable(
                name: "ApplicationRoles");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");
        }
    }
}
