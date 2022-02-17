using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OQAS.XeroIntegration.Objects;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OQAS.XeroIntegration.Logic
{
    public static class Utils
    {
        private static bool RemoteCertificateValidate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert,
    System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            // trust any certificate!!!
#if TRACE_SSL_ERRORS
            if (cert is object)
            {
                logger.Trace(() => string.Format("SSL Certificate Subject: {0}. Issuer: {1}. Algo: {2}. Expiration: {3}", cert.Subject, cert.Issuer,
                    cert.GetKeyAlgorithmParametersString(),
                    cert.GetExpirationDateString()));
            }
            logger.Trace(() => "SSL Certificate validation disabled. Trusting any certificate.");
#endif
            return true;
        }

        public static XeroAuthorization ReadConfigurationFile(string configurationFilePath)
        {
            return JsonConvert.DeserializeObject<XeroAuthorization>(File.ReadAllText(configurationFilePath));
        }

        public static void SaveConfigurationFile(string configurationFilePath, XeroAuthorization authorization)
        {
            File.WriteAllText(configurationFilePath, JsonConvert.SerializeObject(authorization, new JsonSerializerSettings { ContractResolver = new  CamelCasePropertyNamesContractResolver(), Formatting=Formatting.Indented }));
        }

        public static void GenerateAuthorizationBaseUri(XeroAuthorization authorization)
        {
            string[] queryParamsCollection = { $"?response_type={authorization.ResponseType}",
                                 $"code_challenge_method={authorization.CodeChallengeMethod}",
                                 $"client_id={authorization.ClientId}",
                                 $"scope={authorization.Scope}",
                                 $"redirect_uri={authorization.RedirectUri}",
                                 $"state={authorization.State}",
                                 $"code_challenge={authorization.CodeChallenge}"};

            string queryParams = string.Join("&", queryParamsCollection);

            authorization.GeneratedUri = new Uri(authorization.GetAuthorizationBaseUri, relativeUri: queryParams);
        }

        public static async Task GenerateToken(XeroAuthorization authorization)
        {
            var restClient = new RestClient(authorization.PostTokenBaseUri)
            {
                RemoteCertificateValidationCallback = RemoteCertificateValidate
            };
            var restRequest = new RestRequest(Method.POST);
            restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            restRequest.AddParameter("application/x-www-form-urlencoded", $"grant_type={authorization.GrantType}&client_id={authorization.ClientId}&code={authorization.Code}&redirect_uri={authorization.RedirectUri}&code_verifier={authorization.CodeVerifier}", ParameterType.RequestBody);
            IRestResponse response = await restClient.ExecuteAsync(restRequest, restRequest.Method).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Content))
            {
                XeroToken token = JsonConvert.DeserializeObject<XeroToken>(response.Content);
                authorization.AccessToken = token.Access_token;
                authorization.IdToken = token.Id_token;
                authorization.RefreshToken = token.Refresh_token;
                authorization.TokenType = token.Token_type;
                authorization.ExpiresIn = token.Expires_in;
                authorization.ExpiresDateTime = DateTime.Now.AddSeconds(token.Expires_in).ToString("MM-dd-yyyy hh:mm:ss");
            }
            else
            {
                var xeroError = JsonConvert.DeserializeObject<XeroErrorMessage>(response.Content);
                throw new XeroRequestException($"Request failed to {authorization.PostTokenBaseUri}. Response {response.StatusCode}. Message {xeroError.Status} - {xeroError.Title}. Message Details: {xeroError.Detail}");
            }
        }

        public static async Task<List<XeroTenant>> GetXeroTenants(XeroAuthorization authorization)
        {
            var restClient = new RestClient(authorization.TenantUri)
            {
                RemoteCertificateValidationCallback = RemoteCertificateValidate
            };
            var restRequest = new RestRequest(Method.GET);
            restRequest.AddHeader("Authorization", authorization.TokenType + " " + authorization.AccessToken);
            IRestResponse response = await restClient.ExecuteAsync(restRequest, restRequest.Method).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Content))
            {
                return JsonConvert.DeserializeObject<List<XeroTenant>>(response.Content);
            }
            else
            {
                var xeroError = JsonConvert.DeserializeObject<XeroErrorMessage>(response.Content);
                throw new XeroRequestException($"Request failed to {authorization.TenantUri}. Response {response.StatusCode}. Message {xeroError.Status} - {xeroError.Title}. Message Details: {xeroError.Detail}");
            }
        }

        public static async Task<List<XeroAccount>> GetXeroAccounts(XeroAuthorization authorization)
        {
            var restClient = new RestClient(authorization.XeroAccountsApiUri)
            {
                RemoteCertificateValidationCallback = RemoteCertificateValidate
            };
            var restRequest = new RestRequest(Method.GET);
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddHeader("Accept", "application/json");
            restRequest.AddHeader("xero-tenant-id", authorization.XeroTenantId);
            restRequest.AddHeader("Authorization", authorization.TokenType + " " + authorization.AccessToken);
            IRestResponse response = await restClient.ExecuteAsync(restRequest, restRequest.Method).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Content))
            {
                return JsonConvert.DeserializeObject<XeroAccountCollection>(response.Content).Accounts;
            }
            else
            {
                var xeroError = JsonConvert.DeserializeObject<XeroErrorMessage>(response.Content);
                throw new XeroRequestException($"Request failed to {authorization.XeroAccountsApiUri}. Response {response.StatusCode}. Message {xeroError.Status} - {xeroError.Title}. Message Details: {xeroError.Detail}");
            }
        }

        public static async Task<List<XeroBill>> GetXeroBills(XeroAuthorization authorization)
        {
            List<XeroAccount> xeroAccounts = await GetXeroAccounts(authorization).ConfigureAwait(false);
            Uri newUri;
            if (!string.IsNullOrWhiteSpace(authorization.XeroBillsUriQueryParameters))
            {
                string escapedParameter = "?where=" + Uri.EscapeDataString(authorization.XeroBillsUriQueryParameters);
                newUri = new Uri(authorization.XeroBillsApiUri, escapedParameter);
            }
            else
            {
                newUri = authorization.XeroBillsApiUri;
            }

            var restClient = new RestClient(newUri)
            {
                RemoteCertificateValidationCallback = RemoteCertificateValidate
            };
            var restRequest = new RestRequest(Method.GET);
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddHeader("Accept", "application/json");
            restRequest.AddHeader("xero-tenant-id", authorization.XeroTenantId);
            restRequest.AddHeader("Authorization", authorization.TokenType + " " + authorization.AccessToken);
            IRestResponse response = await restClient.ExecuteAsync(restRequest, restRequest.Method).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Content))
            {
                List<XeroBill> invoices = JsonConvert.DeserializeObject<XeroBillCollection>(response.Content).Invoices;
                foreach (XeroBill invoice in invoices)
                {
                    invoice.TotalInWords = NumericUtils.ConvertToWords(invoice.Total.ToString());
                    invoice.LineItems.AddRange(await GetLineItemsByXeroBill(invoice.InvoiceNumber, authorization).ConfigureAwait(false));
                    foreach (XeroLineItem lineItem in invoice.LineItems)
                    {
                        if (!string.IsNullOrWhiteSpace(lineItem.AccountCode))
                        {
                            lineItem.AccountCode = lineItem.AccountCode + " - " + xeroAccounts.Where(t => t.Code == lineItem.AccountCode).FirstOrDefault().Name;
                        }
                    }
                }
                return invoices;
            }
            else
            {
                var xeroError = JsonConvert.DeserializeObject<XeroErrorMessage>(response.Content);
                throw new XeroRequestException($"Request failed to {newUri}. Response {response.StatusCode}. Message {xeroError.Status} - {xeroError.Title}. Message Details: {xeroError.Detail}");
            }
        }

        public static async Task<List<XeroLineItem>> GetLineItemsByXeroBill(string invoiceNumber, XeroAuthorization authorization)
        {
            Uri newUri = new Uri(authorization.XeroBillsApiUri.ToString() + "/" + invoiceNumber);
            var restClient = new RestClient(newUri)
            {
                RemoteCertificateValidationCallback = RemoteCertificateValidate
            };
            var restRequest = new RestRequest(Method.GET);
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddHeader("Accept", "application/json");
            restRequest.AddHeader("xero-tenant-id", authorization.XeroTenantId);
            restRequest.AddHeader("Authorization", authorization.TokenType + " " + authorization.AccessToken);
            IRestResponse response = await restClient.ExecuteAsync(restRequest, restRequest.Method).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Content))
            {
                List<XeroBill> invoices = JsonConvert.DeserializeObject<XeroBillCollection>(response.Content).Invoices;
                return invoices.Where(t => t.InvoiceNumber == invoiceNumber).SingleOrDefault().LineItems;
            }
            else
            {
                var xeroError = JsonConvert.DeserializeObject<XeroErrorMessage>(response.Content);
                throw new XeroRequestException($"Request failed to {newUri}. Response {response.StatusCode}. Message {xeroError.Status} - {xeroError.Title}. Message Details: {xeroError.Detail}");
            }
        }

        public static async Task<IList<CheckManagerVoucher>> SaveVouchers(string connectionString, IList<CheckManagerVoucher> vouchers)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (CheckManagerVoucher voucher in vouchers)
                        {
                            voucher.Posted_by = "";
                            voucher.Approved_by = "";
                            voucher.Prepared_by = "";
                            voucher.Updated_at = DateTime.Now;
                            voucher.Check_number = "";
                            voucher.Remarks = "";
                            string voucherCommandString = @"INSERT INTO vouchers(amount_in_words,
                                                              approved_by,
                                                              check_number,
                                                              created_at,
                                                              particulars,
                                                              particulars_amount,
                                                              payee,
                                                              posted_by,
                                                              prepared_by,
                                                              remarks,
                                                              updated_at) 
                                                       values(@amount_in_words,
                                                              @approved_by,
                                                              @check_number,
                                                              @created_at,
                                                              @particulars,
                                                              @particulars_amount,
                                                              @payee,
                                                              @posted_by,
                                                              @prepared_by,
                                                              @remarks,
                                                              @updated_at);
                                                       SELECT LAST_INSERT_ID();";
                            voucher.Id = await SaveVoucher(connection, transaction, voucherCommandString, voucher).ConfigureAwait(false);

                            foreach (CheckManagerItem item in voucher.Items)
                            {
                                item.Voucher_id = voucher.Id;
                                item.Account = "";
                                item.Updated_at = DateTime.Now;
                                string itemCommandString = @"INSERT INTO items(account,
                                                                   account_title,
                                                                   created_at,
                                                                   credit,
                                                                   debit,
                                                                   updated_at,
                                                                   voucher_id)
                                                             VALUES(@account,
                                                                    @account_title,
                                                                    @created_at,
                                                                    @credit,
                                                                    @debit,
                                                                    @updated_at,
                                                                    @voucher_id);
                                                          SELECT LAST_INSERT_ID();";
                                item.Id = await SaveItem(connection, transaction, itemCommandString, item).ConfigureAwait(false);
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return vouchers;
        }

        private static async Task<int> SaveVoucher(MySqlConnection connection, MySqlTransaction transaction, string commandString, CheckManagerVoucher voucher) 
        {
            MySqlCommand command = new MySqlCommand(commandString, connection);
            command.Transaction = transaction;
            command.CommandType = System.Data.CommandType.Text;
            command.Parameters.AddWithValue("@amount_in_words", voucher.Amount_in_words);
            command.Parameters.AddWithValue("@approved_by", voucher.Approved_by);
            command.Parameters.AddWithValue("@check_number", voucher.Check_number);
            command.Parameters.AddWithValue("@created_at", voucher.Created_at);
            command.Parameters.AddWithValue("@particulars", voucher.Particulars);
            command.Parameters.AddWithValue("@particulars_amount", voucher.Particulars_amount);
            command.Parameters.AddWithValue("@payee", voucher.Payee);
            command.Parameters.AddWithValue("@posted_by", voucher.Posted_by);
            command.Parameters.AddWithValue("@prepared_by", voucher.Prepared_by);
            command.Parameters.AddWithValue("@remarks", voucher.Remarks);
            command.Parameters.AddWithValue("@updated_at", voucher.Updated_at);
            return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
        }

        private static async Task<int> SaveItem(MySqlConnection connection, MySqlTransaction transaction, string commandString, CheckManagerItem item)
        {
            MySqlCommand command = new MySqlCommand(commandString, connection);
            command.CommandType = System.Data.CommandType.Text;
            command.Transaction = transaction;
            command.Parameters.AddWithValue("@account", item.Account);
            command.Parameters.AddWithValue("@account_title", item.Account_title);
            command.Parameters.AddWithValue("@created_at", item.Created_at);
            command.Parameters.AddWithValue("@credit", item.Credit);
            command.Parameters.AddWithValue("@debit", item.Debit);
            command.Parameters.AddWithValue("@updated_at", item.Updated_at);
            command.Parameters.AddWithValue("@voucher_id", item.Voucher_id);
            return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
        }
    }
}
