using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RippleRPC.Net.Exceptions;
using RippleRPC.Net.Infrastructure;
using RippleRPC.Net.Model;
using RippleRPC.Net.Model.Paths;

namespace RippleRPC.Net
{
    public class RippleClient : IRippleClient
    {
        public Uri Uri { get; set; }
        public NetworkCredential Credentials { get; set; }

        private const string UserAgent = "Ripple.NET 0.1";

        public RippleClient()
        {
            //If you are using a self-signed certificate on the rippled server, retain the line below, otherwise you'll generate an exception as the certificate cannot be truested.
            //If you are using a purchased or trusted certificate, you can comment out this line.
            //ServicePointManager.ServerCertificateValidationCallback +=
            //    (s, cert, chain, sslPolicyErrors) => true;

            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
        }

        public RippleClient(Uri uri) : this()
        {
            Uri = uri;
        }

        public RippleClient(Uri uri, NetworkCredential credentials): this()
        {
            Uri = uri;
            Credentials = credentials;
        }

        private string MakeRequest(string jsonRequest)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Uri);
            webRequest.Credentials = Credentials;

            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.UserAgent = UserAgent;
            if (Credentials != null)
                webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Credentials.UserName, Credentials.Password))));

            byte[] byteArray = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.ContentLength = byteArray.Length;

            using (Stream dataStream = webRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {                
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    using (Stream str = webResponse.GetResponseStream())
                    {
                        if (str != null)
                        {
                            using (StreamReader sr = new StreamReader(str))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                        return null;
                    }
                }
            }
            catch (WebException ex)
            {
                using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                {
                    if (response == null)
                        throw new Exception("null response");
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        if (response.StatusCode != HttpStatusCode.InternalServerError)
                        {
                            throw;
                        }
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        async private Task<string> MakeRequestAsync(string jsonRequest)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Uri);
            webRequest.Credentials = Credentials;

            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.UserAgent = UserAgent;
            if (Credentials != null)
                webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Credentials.UserName, Credentials.Password))));

            byte[] byteArray = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.ContentLength = byteArray.Length;

            using (Stream dataStream = await webRequest.GetRequestStreamAsync())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {
                using (WebResponse webResponse = await webRequest.GetResponseAsync())
                {
                    using (Stream str = webResponse.GetResponseStream())
                    {
                        if (str != null)
                        {
                            using (StreamReader sr = new StreamReader(str))
                            {
                                var data = await sr.ReadToEndAsync();
                                return data;
                            }
                        }
                        return null;
                    }
                }
            }
            catch (WebException ex)
            {
                using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        if (response.StatusCode != HttpStatusCode.InternalServerError)
                        {
                            throw;
                        }
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }         
        }

        private T RpcRequest<T>(RippleRequest rippleRequest, string objectElement)
        {
            string jsonRequest = JsonConvert.SerializeObject(rippleRequest);
            string response = MakeRequest(jsonRequest);

            JObject jObject = JObject.Parse(response);

            var resultToken = jObject.SelectToken("result", false);

            JValue status = resultToken["status"] as JValue;
            if (status != null && string.Compare(status.Value.ToString(), "error", StringComparison.InvariantCulture) == 0)
            {
                RippleError rippleError = resultToken.ToObject<RippleError>();
                throw new RippleRpcException(rippleError); ;
            }

            if (!string.IsNullOrEmpty(objectElement))
                return resultToken.SelectToken(objectElement, false).ToObject<T>();
            return resultToken.ToObject<T>();
        }

        private T PagedRpcRequest<T>(RippleRequest rippleRequest, string objectElement, out Marker marker)
        {
            string jsonRequest = JsonConvert.SerializeObject(rippleRequest);
            string response = MakeRequest(jsonRequest);

            JObject jObject = JObject.Parse(response);

            var resultToken = jObject.SelectToken("result", false);
            
            marker = null;
            
            JValue status = resultToken["status"] as JValue;
            if (status != null && string.Compare(status.Value.ToString(), "error", StringComparison.InvariantCulture) == 0)
            {
                RippleError rippleError = resultToken.ToObject<RippleError>();
                throw new RippleRpcException(rippleError); ;
            }

            JToken markerToken = resultToken["marker"];
            if (markerToken != null)
            {
                marker = markerToken.ToObject<Marker>();
            }

            return resultToken.SelectToken(objectElement, false).ToObject<T>();
        }
        
        public AccountInformation GetAccountInformation(string account, uint? index = null, string ledgerHash = null, bool strict = false, string ledgerIndex = "current" )
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentException("Account must be provided", "account");

            dynamic param = new ExpandoObject();
            param.account = account;
            
            if (index.HasValue)
                param.index = index.Value;

            if (!string.IsNullOrEmpty(ledgerHash))
                param.ledger_hash = ledgerHash;

            param.strict = strict;
            
            param.ledger_index = ledgerIndex;

            RippleRequest request = new RippleRequest("account_info", new List<ExpandoObject> { param });

            return RpcRequest<AccountInformation>(request, "account_data");
        }

        public List<AccountLine> GetAccountLines(string account, string peer = null, string ledgerIndex = "current")
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentException("Account must be provided", "account");

            dynamic param = new ExpandoObject();
            param.account = account;

            if (!string.IsNullOrEmpty(peer))
                param.peer = peer;

            param.ledger_index = ledgerIndex;

            RippleRequest request = new RippleRequest("account_lines", new List<ExpandoObject> { param });

            return RpcRequest<List<AccountLine>>(request, "lines");
        }

        public List<AccountOffer> GetAccountOffers(string account, int accountIndex = 0, string ledgerHash = null, string ledgerIndex = "current")
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentException("Account must be provided", "account");

            dynamic param = new ExpandoObject();
            param.account = account;
            param.account_index = accountIndex;

            if (!string.IsNullOrEmpty(ledgerHash))
                param.ledger_hash = ledgerHash;

            param.ledger_index = ledgerIndex;

            RippleRequest request = new RippleRequest("account_offers", new List<ExpandoObject> { param });

            return RpcRequest<List<AccountOffer>>(request, "offers");

            //return BuildAccountOfferList(request);            
        }

        public List<TransactionRecord> GetTransactions(string account, int minimumLedgerIndex = -1, int maximumLedgerIndex = -1, bool binary = false,
            bool forward = false, int limit = 200)
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentException("Account must be provided", "account");

            dynamic param = new ExpandoObject();
            param.account = account;
            param.ledger_index_min = minimumLedgerIndex;
            param.ledger_index_max = maximumLedgerIndex;
            param.binary = binary;
            param.forward = forward;
            param.limit = limit;

            List<TransactionRecord> transactions = new List<TransactionRecord>();

            Marker marker = null;

            do
            {
                param.marker = marker;
                RippleRequest request = new RippleRequest("account_tx", new List<ExpandoObject> { param });

                transactions.AddRange(PagedRpcRequest<List<TransactionRecord>>(request, "transactions", out marker));

            } while (marker != null);

            return transactions;
        }

        public List<BookOffer> GetBookOffers(RippleCurrency takerPays, RippleCurrency takerGets, string ledger = "current", string taker = null, int limit = 200,
            bool proof = false, bool autoBridge = false)
        {
            if (takerPays == null)
                throw new ArgumentException("Taker pays must be provided", "takerPays");

            if (takerGets == null)
                throw new ArgumentException("Taker gets must be provided", "takerGets");

            dynamic param = new ExpandoObject();
            param.taker_pays = takerPays;
            param.taker_gets = takerGets;
            if (!string.IsNullOrEmpty(taker))
                param.taker_gets = taker;
            param.limit = limit;
            param.proof = proof;
            param.autobridge = autoBridge;

            List<BookOffer> transactions = new List<BookOffer>();

            Marker marker = null;

            do
            {
                if (marker != null)
                    param.marker = marker;
                RippleRequest request = new RippleRequest("book_offers", new List<ExpandoObject> { param });

                transactions.AddRange(PagedRpcRequest<List<BookOffer>>(request, "offers", out marker));

            } while (marker != null);

            return transactions;
        }

        public LedgerSummary GetLedgerInformation(string ledgerIndex = "current", bool full = true)
        {
            dynamic param = new ExpandoObject();
            param.ledger_selector = ledgerIndex;
            param.full = full;

            RippleRequest request = new RippleRequest("ledger", new List<ExpandoObject> { param });
            
            string jsonRequest = JsonConvert.SerializeObject(request);
            string response = MakeRequest(jsonRequest);

            JObject jObject = JObject.Parse(response);

            var resultToken = jObject.SelectToken("result", false);

            JValue status = resultToken["status"] as JValue;
            if (status != null && string.Compare(status.Value.ToString(), "error", StringComparison.InvariantCulture) == 0)
            {
                RippleError rippleError = resultToken.ToObject<RippleError>();
                throw new RippleRpcException(rippleError); ;
            }

            LedgerSummary summary = new LedgerSummary();
            summary.Open = resultToken.SelectToken("open", false).ToObject<OpenLedger>();
            summary.Closed = resultToken.SelectToken("closed", false).ToObject<ClosedLedger>();

            return summary;

        }

        public string GetClosedLedgerHash()
        {
            RippleRequest request = new RippleRequest("ledger_closed", new List<ExpandoObject>());

            return RpcRequest<string>(request, "offers");
        }

        public int GetCurrentLedgerIndex()
        {
            RippleRequest request = new RippleRequest("ledger_current", new List<ExpandoObject>());

            return RpcRequest<int>(request, "ledger_current_index");
        }

        public PathSummary FindRipplePath(string fromAccount, string toAccount, double amount, List<RippleCurrency> currencies = null,
            string ledgerHash = null, string ledgerIndex = "current")
        {
            if (string.IsNullOrEmpty(fromAccount))
                throw new ArgumentException("From account must be provided", "fromAccount");

            if (string.IsNullOrEmpty(toAccount))
                throw new ArgumentException("To account must be provided", "toAccount");

            dynamic param = new ExpandoObject();
            param.source_account = fromAccount;
            param.destination_account = toAccount;
            param.destination_amount = (amount * 1000000).ToString(CultureInfo.InvariantCulture);
            if (currencies != null)
                param.source_currencies = currencies;
            if (!string.IsNullOrEmpty(ledgerHash))
                param.ledger_hash = ledgerHash;
            param.ledger_index = ledgerIndex;

            RippleRequest request = new RippleRequest("ripple_path_find", new List<ExpandoObject> { param });

            return RpcRequest<PathSummary>(request, null);
        }

        public string SendXRP(string fromAccount, string toAccount, double amount)
        {
            throw new NotImplementedException();
        }

        public string Submit(string transactionHash)
        {
            if (string.IsNullOrEmpty(transactionHash))
                throw new ArgumentException("Transaction hash cannot be null", "transactionHash");
            dynamic param = new ExpandoObject();
            param.tx_blob = transactionHash;

            RippleRequest request = new RippleRequest("submit", new List<ExpandoObject> { param });

            return RpcRequest<string>(request, "engine_result_message");
        }

        public string Sign(string transaction, string secret, bool offline)
        {
            throw new NotImplementedException();
        }

        private static bool CertificateValidationCallBack( object sender,
           System.Security.Cryptography.X509Certificates.X509Certificate certificate,
           System.Security.Cryptography.X509Certificates.X509Chain chain,
           System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) && (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                            if (certificate.Subject == "O=Internet Widgits Pty Ltd, S=Some-State, C=AU")
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }
            // In all other cases, return false.
            return false;
        }
    }
}
