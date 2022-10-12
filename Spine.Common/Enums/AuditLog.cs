namespace Spine.Common.Enums
{

    // for every EntityType, there should be an enum for the Actions
    public enum AuditLogEntityType
    {
        Company = 1,
        User,
        Customer,
        Role,
        Invoice,
        TaxType,
        Transactions,
        Inventory,
        PurchaseOrder,
        BillsPayment,
        Vendor,
        Admin,
        Subscription
    }

    public enum AuditLogCompanyAction
    {
        UpdateInfo = 1,
        CreateAccountingPeriod,
        CloseAccountingPeriod,
        ReopenAccountingPeriod
    }

    public enum AuditLogUserAction
    {
        Create = 1,
        Update,
        UpdateRole,
        Delete,
        RestoreDeleted
    }

    public enum AuditLogTaxTypeAction
    {
        Create = 1,
        Update,
        Delete
    }

    public enum AuditLogCustomerAction
    {
        Create = 1,
        Update,
        Delete,
        AddNote,
        UpdateNote,
        DeleteNote,
        SetReminder
    }

    public enum AuditLogRoleAction
    {
        Create = 1,
        Update,
        UpdatePermission
    }

    public enum AuditLogInvoiceAction
    {
        Create = 1,
        Cancel,
        Send,
        Download,
        AddInvoiceSettings,
        UpdateInvoiceSettings,
        AddInvoiceTemplate,
        UpdateInvoiceTemplate,
        AddPayment
    }

    public enum AuditLogTransactionAction
    {
        Create = 1,
        UpdateTransaction,
        SetReminder,
        ImportBankTransaction,
        MatchBankTransaction, //match bank transaction to second leg of account
        AddManualTransaction,
        CreateCategory,
        UpdateCategory,
        DeleteCategory,
        CreateBankAccount,
        ActivateDeactivateBankAccount,
        UpdateBankAccount,
        DeleteBankAccount,
        PostJournal
    }

    public enum AuditLogInventoryAction
    {
        CreateInventoryCategory = 1,
        UpdateInventoryCategory,
        DeleteInventoryCategory,
        CreateInventoryLocation,
        UpdateInventoryLocation,
        DeleteInventoryLocation,
        Create,
        Update,
        Delete,
        Allocate,
        Restock,
        UpdateStatus,
        AddNote,
        UpdateNote,
        DeleteNote,
        AdjustInventory
    }

    public enum AuditLogPurchaseOrderAction
    {
        Create = 1,
        Update,
        ConfirmReceipt,
        Delete,
        PaySupplier,
        ReceiveGoodsWithoutPO,
        ReturnGoodsReceived,
        LinkGRToPO
    }

    public enum AuditLogBillsPaymentAction
    {
        PayUtilityService = 1,

    }
    
    public enum AuditLogVendorAction
    {
        Create = 1,
        Update,
        Delete,
        UpdateStatus,
        AddAddress,
        UpdateAddress,
        DeleteAddress,
    }

}
