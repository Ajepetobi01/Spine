using Spine.Payment.Helpers;
using Spine.Payment.Paystack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spine.Payment.Paystack.Transactions
{
    public class PaystackPayment
    {
        private readonly string _secretKey;
        private readonly string _callBackUrl;
        public PaystackPayment(string secretKey, string callBackUrl)
        {
            this._secretKey = secretKey;
            _callBackUrl = callBackUrl;
        }

        public PaystackPayment(string secretKey)
        {
            this._secretKey = secretKey;
        }


        /// <summary>
        /// Sends body parameters to transaction initialization url
        /// </summary>
        /// <param name="email"></param>
        /// <param name="amount"></param>
        /// <param name="firstName"></param>        
        /// <param name="lastName"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="reference"></param>
        /// <param name="makeReferenceUnique"></param>
        /// <returns>PaymentInitalizationResponseModel</returns>

        public async Task<PaymentInitalizationResponseModel> InitializeTransaction(string email, int amount, string currency = null, string firstName = null,
                                                                                string lastName = null,
                                                                                string reference = null, bool makeReferenceUnique = false)
        {

            var client = HttpFactory.InitHttpClient(PaystackConstant.BASE_URL)
                      .AddAuthorizationHeader(PaystackConstant.AUTHORIZATION_TYPE, _secretKey)
                      .AddMediaType(PaystackConstant.REQUEST_MEDIA_TYPE)
                      .AddHeader("cache-control", "no-cache");

            var bodyKeyValues = new List<KeyValuePair<string, string>>();

            bodyKeyValues.Add(new KeyValuePair<string, string>("email", email));
            bodyKeyValues.Add(new KeyValuePair<string, string>("amount", amount.ToString()));
            bodyKeyValues.Add(new KeyValuePair<string, string>("callback_url", _callBackUrl));

            //Optional Params

            if (!string.IsNullOrWhiteSpace(currency))
            {
                bodyKeyValues.Add(new KeyValuePair<string, string>("currency", currency));
            }

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                bodyKeyValues.Add(new KeyValuePair<string, string>("first_name", firstName));
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                bodyKeyValues.Add(new KeyValuePair<string, string>("last_name", lastName));
            }




            var formContent = new FormUrlEncodedContent(bodyKeyValues);

            var response = await client.PostAsync("transaction/initialize", formContent);

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PaymentInitalizationResponseModel>(json);

        }

        /// <summary>
        /// Verifies transaction by reference number
        /// </summary>
        /// <param name="reference"></param>
        /// <returns>TransactionVerificationResponseModel</returns>

        public async Task<TransactionResponseModel> VerifyTransaction(string reference)
        {
            var client = HttpFactory.InitHttpClient(PaystackConstant.BASE_URL)
                      .AddAuthorizationHeader(PaystackConstant.AUTHORIZATION_TYPE, _secretKey)
                      .AddMediaType(PaystackConstant.REQUEST_MEDIA_TYPE)
                      .AddHeader("cache-control", "no-cache");

            var response = await client.GetAsync($"transaction/verify/{reference}");

            var json = await response.Content.ReadAsStringAsync();


            return JsonSerializer.Deserialize<TransactionResponseModel>(json);
        }

        /// <summary>
        /// This endpoint can be used to confirm that an account number and bank code match
        /// </summary>
        /// <param name="account_number"></param>
        /// <param name="bank_code"></param>
        /// <returns></returns>
        public async Task<AccountNumberValidationResponseModel> ResolveAccountNumber(string account_number,
            string bank_code)
        {
            var client = HttpFactory.InitHttpClient(PaystackConstant.BASE_URL)
                      .AddAuthorizationHeader(PaystackConstant.AUTHORIZATION_TYPE, _secretKey)
                      .AddMediaType(PaystackConstant.REQUEST_MEDIA_TYPE)
                      .AddHeader("cache-control", "no-cache");

            var response = await client.GetAsync($"bank/resolve?account_number={account_number}&bank_code={bank_code}");

            var json = await response.Content.ReadAsStringAsync();


            return JsonSerializer.Deserialize<AccountNumberValidationResponseModel>(json);

        }


    }
}
