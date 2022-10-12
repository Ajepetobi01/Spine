using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum DiscountSettings
    {
        [Description("I don't give discount")]
        None = 0,
        [Description("On Total")]
        OnTotal,
        [Description("At Line Item Level")]
        OnLineItem
    }

    public enum ApplyTaxSettings
    {
        [ExcludeEnumValue]
        None = 0,
        [Description("On Total")]
        OnTotal,
        [Description("At Line Item Level")]
        OnLineItem,
        [Description("On Both")]
        OnBoth,
    }

    public enum TaxSettings
    {
        [Description("Tax Inclusive")]
        Inclusive = 1,
        [Description("Tax Exclusive")]
        Exclusive,
        [Description("Tax Inclusive and Exclusive")]
        Both
    }

    //public enum Currency
    //{
    //    [ExcludeEnumValue]
    //    None = 0,
    //    [Description("NGN")]
    //    Naira = 1,
    //    [Description("USD")]
    //    Dollar,
    //    [Description("EUR")]
    //    Euro,
    //    [Description("GBP")]
    //    Pounds
    //}

    public enum PaymentIntegrationProvider
    {
        [ExcludeEnumValue]
        None = 0,
        Paystack,
        [ExcludeEnumValue]
        Flutterwave
    }

    /// <summary>
    /// <summary>
    /// As Customer, the customer will be created as a sub account, so customer's account receives the payment and the charges..
    /// As Spine, customer will be created as a transfer recipient, so Spine receives the payment. Then once, payment is received. (should be automatic, but how???)
    /// The invoice is automatically marked as paid, and the amount (less the paystack charge) is transferred to the customer's account using the recipient code
    /// </summary>
    public enum PaymentIntegrationType
    {
        [ExcludeEnumValue]
        None = 0,

        [Description("Customer recieves payment. Invoice payment will be done manually")]
        Customer,

        [Description("Spine recieves payments, so invoice is automatically marked as paid")]
        [ExcludeEnumValue] // currently excluded
        Spine,
    }

}
