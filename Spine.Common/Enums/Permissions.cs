using System.ComponentModel.DataAnnotations;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum Permissions
    {
        //Business
        // [Display(GroupName = "Business", Description = "Can view business performance metrics")]
        // ViewPerformanceMetrics = 1,
        [Display(GroupName = "Business", Description = "Can view company profile")]
        ViewCompanyProfile,
        [Display(GroupName = "Business", Description = "Can update company profile")]
        UpdateCompanyProfile,
        [Display(GroupName = "Business", Description = "Can manage accounting periods")]
        ManageAccountingPeriod,
        // [Display(GroupName = "Business", Description = "Can view business financial")]
        // ViewCompanyFinancial,

        //Users
        [Display(GroupName = "Users", Description = "Can view users")]
        ViewUsers = 11,
        [Display(GroupName = "Users", Description = "Can invite users")]
        [PermissionDependency(InviteUsers, ViewUsers)]
        InviteUsers,
        [Display(GroupName = "Users", Description = "Can update users")]
        [PermissionDependency(UpdateUser, ViewUsers)]
        UpdateUser,
        [Display(GroupName = "Users", Description = "Can update users role")]
        [PermissionDependency(UpdateUserRole, ViewUsers)]
        UpdateUserRole,
        [Display(GroupName = "Users", Description = "Can delete users")]
        [PermissionDependency(DeleteUsers, ViewUsers)]
        DeleteUsers,

        //Roles (commented out because subscribers no longer add roles and permissions)
        // [Display(GroupName = "Roles", Description = "Can view roles")]
        // ViewRole = 18,
        // [Display(GroupName = "Roles", Description = "Can create new roles")]
        // [PermissionDependency(AddRole, ViewRole)]
        // AddRole = 19,
        // [Display(GroupName = "Roles", Description = "Can update role permissions")]
        // [PermissionDependency(UpdateRolePermission, ViewRole)]
        // UpdateRolePermission = 20,

        //Vendors
        [Display(GroupName = "Vendor", Description = "Can view vendor")]
        ViewVendor = 21,
        [Display(GroupName = "Vendor", Description = "Can create new vendor")]
        [PermissionDependency(AddVendor, ViewVendor)]
        AddVendor,
        [Display(GroupName = "Vendor", Description = "Can update vendor")]
        [PermissionDependency(UpdateVendor, ViewVendor)]
        UpdateVendor,
        [Display(GroupName = "Vendor", Description = "Can delete vendor")]
        [PermissionDependency(DeleteVendor, ViewVendor)]
        DeleteVendor,
        [Display(GroupName = "Vendor", Description = "Can view vendor payments")]
        [PermissionDependency(ViewVendorPayment, ViewVendor)]
        ViewVendorPayment,
        [Display(GroupName = "Vendor", Description = "Can add new vendor payment")]
        [PermissionDependency(AddVendorPayment, ViewVendor, ViewVendorPayment)]
        AddVendorPayment,
        [Display(GroupName = "Vendor", Description = "Can export vendor")]
        [PermissionDependency(ExportVendor, ViewVendor)]
        ExportVendor,
        
        //Customers
        [Display(GroupName = "Customers", Description = "Can view customers")]
        ViewCustomers = 31,
        [Display(GroupName = "Customers", Description = "Can create new customers")]
        [PermissionDependency(AddCustomer, ViewCustomers)]
        AddCustomer,
        [Display(GroupName = "Customers", Description = "Can update customers")]
        [PermissionDependency(UpdateCustomer, ViewCustomers)]
        UpdateCustomer,
        [Display(GroupName = "Customers", Description = "Can delete customer")]
        [PermissionDependency(DeleteCustomer, ViewCustomers)]
        DeleteCustomer,
        [Display(GroupName = "Customers", Description = "Can export customer")]
        [PermissionDependency(ExportCustomer, ViewCustomers)]
        ExportCustomer,


        //Transactions
        [Display(GroupName = "Transactions", Description = "Can view transactions")]
        ViewTransactions = 41,
        [Display(GroupName = "Transactions", Description = "Can export transactions")]
        [PermissionDependency(ExportTransactions, ViewTransactions)]
        ExportTransactions,
        [Display(GroupName = "Transactions", Description = "Can create transactions")]
        [PermissionDependency(CreateTransactions, ViewTransactions)]
        CreateTransactions,
        [Display(GroupName = "Transactions", Description = "Can update transactions")]
        [PermissionDependency(UpdateTransactions, ViewTransactions, CreateTransactions)]
        UpdateTransactions,
        [Display(GroupName = "Transactions", Description = "Can import bank transactions")]
        [PermissionDependency(ImportBankTransactions, ViewTransactions, ViewBankAccount)]
        ImportBankTransactions,
        [Display(GroupName = "Transactions", Description = "View transaction report")]
        ViewTransactionReport,

        [Display(GroupName = "Transactions", Description = "Can create transaction category")]
        [PermissionDependency(CreateTransactionCategory)]
        CreateTransactionCategory,
        [Display(GroupName = "Transactions", Description = "Can update transaction category")]
        [PermissionDependency(UpdateTransactionCategory, CreateTransactions)]
        UpdateTransactionCategory,
        [Display(GroupName = "Transactions", Description = "Can delete transaction category")]
        DeleteTransactionCategory,


        [Display(GroupName = "Transactions", Description = "Can view bank accounts")]
        ViewBankAccount = 51,
        [Display(GroupName = "Transactions", Description = "Can create bank account")]
        [PermissionDependency(CreateBankAccount, ViewBankAccount)]
        CreateBankAccount,
        [Display(GroupName = "Transactions", Description = "Can update bank account")]
        [PermissionDependency(UpdateBankAccount, CreateBankAccount, ViewBankAccount)]
        UpdateBankAccount,
        [Display(GroupName = "Transactions", Description = "Can delete bank account")]
        [PermissionDependency(DeleteBankAccount, ViewBankAccount)]
        DeleteBankAccount,

        [Display(GroupName = "Transactions", Description = "Can make bill payments")]
        BillsPayment,

        //Invoice
        [Display(GroupName = "Invoice", Description = "Can view invoices")]
        ViewInvoice = 61,
        [Display(GroupName = "Invoice", Description = "Can create new invoice")]
        [PermissionDependency(AddInvoice, ViewInvoice, ViewInventory)]
        AddInvoice,
        //[Display(GroupName = "Invoice", Description = "Can update invoice ")]
        //[PermissionDependency(UpdateInvoice, ViewInvoice)]
        //UpdateInvoice,
        [Display(GroupName = "Invoice", Description = "Can cancel invoice")]
        [PermissionDependency(CancelInvoice, ViewInvoice)]
        CancelInvoice = 64,
        [Display(GroupName = "Invoice", Description = "Can export invoice")]
        [PermissionDependency(ExportInvoice, ViewInvoice)]
        ExportInvoice,
        [Display(GroupName = "Invoice", Description = "Can add/update invoice settings")]
        [PermissionDependency(UpdateInvoiceSettings, ViewBankAccount)]
        UpdateInvoiceSettings,
        [Display(GroupName = "Invoice", Description = "Can view invoice payment")]
        [PermissionDependency(ViewInvoicePayment, ViewInvoice)]
        ViewInvoicePayment,
        [Display(GroupName = "Invoice", Description = "Can add invoice payment")]
        [PermissionDependency(AddInvoicePayment, ViewInvoice, ViewInvoicePayment, ViewBankAccount)]
        AddInvoicePayment,


        //inventory
        [Display(GroupName = "Inventory", Description = "Can view inventory")]
        ViewInventory = 71,
        [Display(GroupName = "Inventory", Description = "Can create new inventory")]
        [PermissionDependency(AddInventory, ViewInventory, ViewInventoryCategory, ViewInventoryLocation)]
        AddInventory,
        [Display(GroupName = "Inventory", Description = "Can update inventory ")]
        [PermissionDependency(UpdateInventory, ViewInventory)]
        UpdateInventory,
        [Display(GroupName = "Inventory", Description = "Can delete inventory ")]
        [PermissionDependency(DeleteInventory, ViewInventory)]
        DeleteInventory,
        [Display(GroupName = "Inventory", Description = "Can export inventory")]
        [PermissionDependency(ExportInventory, ViewInventory)]
        ExportInventory,
        [Display(GroupName = "Inventory", Description = "Can restock inventory")]
        [PermissionDependency(RestockInventory, ViewInventory, AddInventory)]
        RestockInventory,
        [Display(GroupName = "Inventory", Description = "Can allocate inventory to location")]
        [PermissionDependency(AllocateInventory, ViewInventory, AddInventory, ViewInventoryLocation, ViewInventoryCategory)]
        AllocateInventory,
        [Display(GroupName = "Inventory", Description = "Can view purchase orders")]
        ViewPurchaseOrder,
        [Display(GroupName = "Inventory", Description = "Can create purchase order")]
        [PermissionDependency(CreatePurchaseOrder, ViewPurchaseOrder, ViewInventory, AddInventory, RestockInventory)]
        CreatePurchaseOrder,
        [Display(GroupName = "Inventory", Description = "Can update purchase order")]
        [PermissionDependency(UpdatePurchaseOrder, ViewPurchaseOrder, ViewInventory, AddInventory, RestockInventory)]
        UpdatePurchaseOrder,
        [Display(GroupName = "Inventory", Description = "Can delete purchase order")]
        [PermissionDependency(DeletePurchaseOrder, ViewPurchaseOrder)]
        DeletePurchaseOrder,
        [Display(GroupName = "Inventory", Description = "Can view goods received")]
        [PermissionDependency(ViewGoodsReceived, ViewInventory)]
        ViewGoodsReceived,
        [Display(GroupName = "Inventory", Description = "Can confirm goods received")]
        [PermissionDependency(ConfirmGoodsReceived, ViewInventory, AddInventory, RestockInventory)]
        ConfirmGoodsReceived,
        [Display(GroupName = "Inventory", Description = "Can return goods received")]
        [PermissionDependency(ReturnGoodsReceived, ConfirmGoodsReceived, ViewInventory, AddInventory, RestockInventory)]
        ReturnGoodsReceived,

        [Display(GroupName = "Inventory", Description = "Can view inventory locations")]
        ViewInventoryLocation,
        [Display(GroupName = "Inventory", Description = "Can create new inventory location")]
        [PermissionDependency(AddInventoryLocation, ViewInventoryLocation)]
        AddInventoryLocation,
        [Display(GroupName = "Inventory", Description = "Can update inventory location ")]
        [PermissionDependency(UpdateInventoryLocation, ViewInventoryLocation)]
        UpdateInventoryLocation,
        [Display(GroupName = "Inventory", Description = "Can delete inventory location")]
        [PermissionDependency(DeleteInventoryLocation, ViewInventoryLocation)]
        DeleteInventoryLocation,

        [Display(GroupName = "Inventory", Description = "Can view inventory categories")]
        ViewInventoryCategory,
        [Display(GroupName = "Inventory", Description = "Can create new inventory category")]
        [PermissionDependency(AddInventoryCategory, ViewInventoryCategory)]
        AddInventoryCategory,
        [Display(GroupName = "Inventory", Description = "Can update inventory category ")]
        [PermissionDependency(UpdateInventoryCategory, ViewInventoryCategory)]
        UpdateInventoryCategory,
        [Display(GroupName = "Inventory", Description = "Can delete inventory category")]
        [PermissionDependency(DeleteInventoryCategory, ViewInventoryCategory)]
        DeleteInventoryCategory,
        
        [Display(GroupName = "Inventory", Description = "Can view journal postings")]
        ViewJournals,
        [Display(GroupName = "Inventory", Description = "Can post journal")]
        [PermissionDependency(PostJournal, ViewJournals)]
        PostJournal,

        //Admin and Subscriber
        [Display(GroupName = "Subscriber", Description = "Can view subscriber")]
        ViewSubscriber = 102,
        [Display(GroupName = "Subscriber", Description = "Can create subscriber")]
        [PermissionDependency(AddSubscriber, ViewSubscriber)]
        AddSubscriber,
        [Display(GroupName = "Subscriber", Description = "Can update subscriber")]
        [PermissionDependency(UpdateSubscriber, ViewSubscriber)]
        UpdateSubscriber,
        [Display(GroupName = "Subscriber", Description = "Can delete subscriber")]
        [PermissionDependency(DeleteSubscriber, ViewSubscriber)]
        DeleteSubscriber,
        [Display(GroupName = "Subscriber", Description = "Can enable and disable subscriber")]
        EnableDisableSubscriber,
        [Display(GroupName = "Subscriber", Description = "Can upload subscriber")]
        UploadSubscriber,
        [Display(GroupName = "Subscriber", Description = "Can download subscriber template")]
        DownloadSubscriberTemplate,

        [Display(GroupName = "Role", Description = "Can view role")]
        ViewRole = 112,
        [Display(GroupName = "Role", Description = "Can create role")]
        [PermissionDependency(AddRole, ViewRole)]
        AddRole,
        [Display(GroupName = "Role", Description = "Can update role")]
        [PermissionDependency(UpdateRole, ViewRole)]
        UpdateRole,
        [Display(GroupName = "Role", Description = "Can delete role")]
        [PermissionDependency(DeleteRole, ViewRole)]
        DeleteRole,

        [Display(GroupName = "Setting", Description = "Can view plan")]
        ViewSetting = 122,
        [Display(GroupName = "Setting", Description = "Can create plan")]
        [PermissionDependency(AddPlan, ViewSetting)]
        AddPlan,
        [Display(GroupName = "Setting", Description = "Can update plan")]
        [PermissionDependency(UpdatePlan, ViewSetting)]
        UpdatePlan,
        [Display(GroupName = "Setting", Description = "Can delete plan")]
        [PermissionDependency(DeletePlan, ViewSetting)]
        DeletePlan,
        [Display(GroupName = "Setting", Description = "Can view promotion")]
        ViewPromotion,
        [Display(GroupName = "Setting", Description = "Can create promotion")]
        [PermissionDependency(AddPromotion, ViewPromotion, ViewSetting)]
        AddPromotion,
        [Display(GroupName = "Setting", Description = "Can update promotion")]
        [PermissionDependency(UpdatePromotion, ViewPromotion)]
        UpdatePromotion,
        [Display(GroupName = "Setting", Description = "Can delete promotion")]
        [PermissionDependency(DeletePromotion, ViewPromotion, ViewSetting)]
        DeletePromotion,
        [Display(GroupName = "Setting", Description = "Can view referralcode")]
        ViewReferralCode,
        [Display(GroupName = "Setting", Description = "Can create referralcode")]
        [PermissionDependency(AddReferralCode, ViewReferralCode, ViewSetting)]
        AddReferralCode,
        [Display(GroupName = "Setting", Description = "Can update referralcode")]
        [PermissionDependency(UpdateReferralCode, ViewReferralCode, ViewSetting)]
        UpdateReferralCode,
        [Display(GroupName = "Setting", Description = "Can delete referralcode")]
        [PermissionDependency(DeleteReferralCode, ViewReferralCode, ViewSetting)]
        DeleteReferralCode,
        [Display(GroupName = "Setting", Description = "Can view template")]
        ViewTemplate,
        [Display(GroupName = "Setting", Description = "Can create template")]
        [PermissionDependency(AddTemplate, ViewTemplate, ViewSetting)]
        AddTemplate,
        [Display(GroupName = "Setting", Description = "Can update template")]
        [PermissionDependency(UpdateTemplate, ViewTemplate, ViewSetting)]
        UpdateTemplate,
        [Display(GroupName = "Setting", Description = "Can delete template")]
        [PermissionDependency(DeleteTemplate, ViewTemplate, ViewSetting)]
        DeleteTemplate,
    }

}
