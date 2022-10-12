using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum TransactionType
    {
        [ExcludeEnumValue]
        None = 0,
        
        ReceivePayment,
        PayForService,
        
        GenerateInvoice,
        ReceiveInvoicePayment,

        ConfirmGoodsReceived,
        PaySupplier,
        
        AddInventory,
        ReduceInventory,
        
        AddInventoryCost,
        ReduceInventoryCost,
        
        AddTransaction,
        JournalPosting,
        CloseAccounting
    }

}
