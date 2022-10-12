using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Spine.Common.Converters;

namespace Spine.Common.Models
{

    public class PreviewTransactionImport
    {
        [Description("Transaction Date")]
        [JsonConverter(typeof(DateTimeConverterFactory))]
        public DateTime TransactionDate { get; set; }

        [Description("Description")]
        public string Description { get; set; }

        [Description("Reference Number")]
        public string ReferenceNumber { get; set; }

        [Description("Amount Spent")]
        public string AmountSpent { get; set; }

        [Description("Amount Received")]
        public string AmountReceived { get; set; }

        [Description("Payee")]
        public string Payee { get; set; }

        [Description("Cheque Number")]
        public string ChequeNumber { get; set; }

    }

}
