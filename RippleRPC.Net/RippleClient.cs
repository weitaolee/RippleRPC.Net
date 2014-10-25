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
using WebSocket4Net;
using System.Linq;

namespace RippleRPC.Net
{
    public class RippleClient : IDisposable, IRippleClient
    {
        #region RPC Call ID
        private static int rpcId = 0;
        private static object rpcIdLocker = new object();
        private static object rpcDicLocker = new object();
        private int RPCCallId
        {
            get
            {
                lock (rpcIdLocker)
                {
                    return rpcId += 1;
                }
            }
        }
        #endregion

        #region RippleClient Config
        public bool AutoReconnect { get; set; }
        public bool WebSocketConnected { get; set; }

        public Uri Uri { get; set; }
        public NetworkCredential Credentials { get; set; }
        private static WebSocket WebSocketRL { get; set; }
        private static List<string> WaitCallStack { get; set; }

        private const string UserAgent = "Ripple.NET 0.1";
        #endregion

        private Dictionary<int, Tuple<Type, string, Action<RippleError, object>>> RPCStack = new Dictionary<int, Tuple<Type, string, Action<RippleError, object>>>();

        #region Web Socket Callback
        private void WebScoketRL_MessageReceived(object sender, MessageReceivedEventArgs e)
        {

            var rippleError = default(RippleError);
            object resT = null;

            JObject jObject = JObject.Parse(e.Message);

            var resultToken = jObject.SelectToken("result", false);

            JValue status = resultToken["status"] as JValue;
            var id = jObject.SelectToken("id", false);
            if (status != null && string.Compare(status.Value.ToString(), "error", StringComparison.InvariantCulture) == 0)
            {
                rippleError = resultToken.ToObject<RippleError>();
            }

            if (this.RPCStack.Count == 0)
                return;
            else
            {
                var _id = id.Value<int>();

                if (_id > 0 && RPCStack.ContainsKey(_id))
                {
                    try
                    {
                        var rpcCall = this.RPCStack.SingleOrDefault(rpc => rpc.Key == _id);

                        if (!string.IsNullOrEmpty(rpcCall.Value.Item2))
                            resT = resultToken.SelectToken(rpcCall.Value.Item2, false).ToObject(rpcCall.Value.Item1);
                        else
                            resT = resultToken.ToObject(rpcCall.Value.Item1);

                        rpcCall.Value.Item3(rippleError, resT);
                    }
                    finally
                    {
                        lock (rpcDicLocker)
                        {
                            this.RPCStack.Remove(_id);
                        }
                    }
                }
            }
        }
        void WebScoketRL_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        void WebScoketRL_Closed(object sender, EventArgs e)
        {
            this.WebSocketConnected = false;
            Task.Factory.StartNew(() =>
            {
                if (this.AutoReconnect == true)
                {
                    WebSocketRL.Open();
                }
            });
        }

        #endregion

        private RippleClient()
        {
            //If you are using a self-signed certificate on the rippled server, retain the line below, otherwise you'll generate an exception as the certificate cannot be truested.
            //If you are using a purchased or trusted certificate, you can comment out this line.
            //ServicePointManager.ServerCertificateValidationCallback +=
            //    (s, cert, chain, sslPolicyErrors) => true;
            //ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack; 
            WaitCallStack = new List<string>();
        }

        public RippleClient(Uri uri)
            : this()
        {
            Uri = uri;
            if (WebSocketRL == null)
            {
                WebSocketRL = new WebSocket(this.Uri.ToString());
                WebSocketRL.Opened += (s, e) =>
                {
                    this.WebSocketConnected = true;
                    if (WaitCallStack != null && WaitCallStack.Count > 0)
                    {
                        WaitCallStack.ForEach(wct => WebSocketRL.Send(wct));
                    }
                };
                WebSocketRL.Error += WebScoketRL_Error;
                WebSocketRL.Closed += WebScoketRL_Closed;
                WebSocketRL.MessageReceived += WebScoketRL_MessageReceived;
                WebSocketRL.Open();
            }
        }

        public RippleClient(Uri uri, NetworkCredential credentials)
            : this(uri)
        {
            Uri = uri;
            Credentials = credentials;
        }

        #region private methods

        private void RpcRequest<T>(RippleRequest rippleRequest, string objectElement, Action<RippleError, object> callback)
        {
            string jsonRequest = rippleRequest.ToJsonString();
            if (callback != null)
                this.RPCStack.Add(rippleRequest.id, new Tuple<Type, string, Action<RippleError, object>>(typeof(T), objectElement, callback));
            if (this.WebSocketConnected)
                WebSocketRL.Send(jsonRequest);
            else
                WaitCallStack.Add(jsonRequest);
        }

        //private T PagedRpcRequest<T>(RippleRequest rippleRequest, string objectElement, out Marker marker,Action<RippleError, object> callback=null)
        //{
        //    marker = null;
        //    return default(T);
        //    string jsonRequest = JsonConvert.SerializeObject(rippleRequest);

        //    if (callback != null)
        //        this.RPCStack.Add(rippleRequest.id, new Tuple<Type, string, Action<RippleError, object>>(typeof(T), objectElement, callback));
        //    if (this.WebSocketConnected)
        //        WebSocketRL.Send(jsonRequest);
        //    else
        //        WaitCallStack.Add(jsonRequest);

        //    JObject jObject = JObject.Parse(response);

        //    var resultToken = jObject.SelectToken("result", false);

        //    marker = null;

        //    JValue status = resultToken["status"] as JValue;
        //    if (status != null && string.Compare(status.Value.ToString(), "error", StringComparison.InvariantCulture) == 0)
        //    {
        //        RippleError rippleError = resultToken.ToObject<RippleError>();
        //        throw new RippleRpcException(rippleError); ;
        //    }

        //    JToken markerToken = resultToken["marker"];
        //    if (markerToken != null)
        //    {
        //        marker = markerToken.ToObject<Marker>();
        //    }

        //    return resultToken.SelectToken(objectElement, false).ToObject<T>();
        //}
        #endregion

        public void GetAccountInformation(string account, Action<RippleError, AccountInformation> callback = null, uint? index = null, string ledgerHash = null, bool strict = false, string ledgerIndex = "current")
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

            RippleRequest request = new RippleRequest(this.RPCCallId, "account_info", param);

            RpcRequest<AccountInformation>(request, "account_data", (error, result) =>
            {
                if (callback != null)
                {
                    var res = result as AccountInformation;
                    callback(error, res);
                }
            });
        }

        public void GetAccountLines(string account, Action<RippleError, List<AccountLine>> callback = null, string peer = null, string ledgerIndex = "current")
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentException("Account must be provided", "account");

            dynamic param = new ExpandoObject();
            param.account = account;

            if (!string.IsNullOrEmpty(peer))
                param.peer = peer;

            param.ledger_index = ledgerIndex;

            RippleRequest request = new RippleRequest(this.RPCCallId, "account_lines", param);

            RpcRequest<List<AccountLine>>(request, "lines", (error, result) =>
            {
                if (callback != null)
                {
                    var res = result as List<AccountLine>;
                    callback(error, res);
                }
            });
        }

        public void GetAccountOffers(string account, Action<RippleError, List<AccountOffer>> callback = null, int accountIndex = 0, string ledgerHash = null, string ledgerIndex = "current")
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentException("Account must be provided", "account");

            dynamic param = new ExpandoObject();
            param.account = account;
            param.account_index = accountIndex;

            if (!string.IsNullOrEmpty(ledgerHash))
                param.ledger_hash = ledgerHash;

            param.ledger_index = ledgerIndex;

            RippleRequest request = new RippleRequest(this.RPCCallId, "account_offers", param);

            RpcRequest<List<AccountOffer>>(request, "offers", (error, result) =>
            {
                if (callback != null)
                {
                    var res = result as List<AccountOffer>;
                    callback(error, res);
                }
            });
            //return BuildAccountOfferList(request);            
        }

        public void GetTransactions(string account, Action<RippleError, List<TransactionRecord>> callback = null, int minimumLedgerIndex = -1, int maximumLedgerIndex = -1, bool binary = false,
            bool forward = false, int limit = 200, Marker marker = null)
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

            if (marker != null)
                param.marker = marker;

            RippleRequest request = new RippleRequest(this.RPCCallId, "account_tx", param);

            RpcRequest<List<TransactionRecord>>(request, "transactions", (error, result) =>
            {
                if (callback != null)
                {
                    var res = result as List<TransactionRecord>;
                    callback(error, res);
                }
            });
        }

        public void GetBookOffers(RippleCurrency takerPays, RippleCurrency takerGets, Action<RippleError, List<BookOffer>> callback = null, string ledger = "current", string taker = null, int limit = 200,
            bool proof = false, bool autoBridge = false, Marker marker = null)
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

            if (marker != null)
                param.marker = marker;
            RippleRequest request = new RippleRequest(this.RPCCallId, "book_offers", param);
            RpcRequest<List<BookOffer>>(request, "offers", (error, result) =>
            {
                if (callback != null)
                {
                    var res = result as List<BookOffer>;
                    callback(error, res);
                }
            });
        }

        //public LedgerSummary GetLedgerInformation(string ledgerIndex = "current", bool full = true)
        //{
        //    dynamic param = new ExpandoObject();
        //    param.ledger_selector = ledgerIndex;
        //    param.full = full;

        //    RippleRequest request = new RippleRequest(this.RPCCallId, "ledger", param);

        //    string jsonRequest = JsonConvert.SerializeObject(request);
        //    string response = ""; //MakeRequest(jsonRequest);

        //    JObject jObject = JObject.Parse(response);

        //    var resultToken = jObject.SelectToken("result", false);

        //    JValue status = resultToken["status"] as JValue;
        //    if (status != null && string.Compare(status.Value.ToString(), "error", StringComparison.InvariantCulture) == 0)
        //    {
        //        RippleError rippleError = resultToken.ToObject<RippleError>();
        //        throw new RippleRpcException(rippleError); ;
        //    }

        //    LedgerSummary summary = new LedgerSummary();
        //    summary.Open = resultToken.SelectToken("open", false).ToObject<OpenLedger>();
        //    summary.Closed = resultToken.SelectToken("closed", false).ToObject<ClosedLedger>();

        //    return summary;

        //}

        public void GetClosedLedgerHash(Action<RippleError, string> callback = null)
        {
            RippleRequest request = new RippleRequest(this.RPCCallId, "ledger_closed", new ExpandoObject());

            RpcRequest<string>(request, "ledger_hash", (error, result) =>
            {
                callback(error, result.ToString());
            });
        }

        public void GetCurrentLedgerIndex(Action<RippleError, int> callback = null)
        {
            RippleRequest request = new RippleRequest(this.RPCCallId, "ledger_current", new ExpandoObject());

            RpcRequest<int>(request, "ledger_current_index", (error, result) =>
            {
                int res = -1;
                int.TryParse(result.ToString(), out res);
                callback(error, res);
            });
        }

        public void FindRipplePath(string fromAccount, string toAccount, double amount, Action<RippleError, PathSummary> callback = null, List<RippleCurrency> currencies = null,
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

            RippleRequest request = new RippleRequest(this.RPCCallId, "ripple_path_find", param);

            RpcRequest<PathSummary>(request, null, (error, result) =>
            {
                var paths = result as PathSummary;
                callback(error, paths);
            });
        }

        public void SendXRP(string fromAccount, string secret, string toAccount, double amount, Action<RippleError, PaymentSubmitResult> callback = null)
        {
            var transaction = new Transaction { Flags = TransactionFlags.tfFullyCanonicalSig, Fee = 10000, TransactionType = TransactionType.Payment, Account = fromAccount, Destination = toAccount, Amount = new RippleCurrencyValue { Currency = "XRP", Issuer = string.Empty, Value = amount } };

            this.Sign(transaction, secret, (error, result) =>
            {
                if (error == null)
                {
                    this.Submit(result.TxBlob, callback);
                }
            });

        }

        public void Submit(string transactionHash, Action<RippleError, PaymentSubmitResult> callback = null)
        {
            if (string.IsNullOrEmpty(transactionHash))
                throw new ArgumentException("Transaction hash cannot be null", "transactionHash");
            dynamic param = new ExpandoObject();
            param.tx_blob = transactionHash;

            RippleRequest request = new RippleRequest(this.RPCCallId, "submit", param);

            RpcRequest<PaymentSubmitResult>(request, null, (error, result) =>
            {
                var res = result as PaymentSubmitResult;
                callback(error, res);
            });
        }

        public void Sign(Transaction transaction, string secret, Action<RippleError, TransactionSigned> callback = null, bool offline = false)
        {
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentException("secret must be provided", "secret");

            if (transaction == null)
                throw new ArgumentException("transaction must be provided", "transaction");

            if (offline)
            {
                //var accountInfo = GetAccountInformation(transaction.Account);
                //transaction.Sequence = accountInfo.Sequence + 1;
                //transaction.Sign(secret);
                //hash = transaction.Hash;


            }
            else
            {
                dynamic param = new ExpandoObject();
                param.offline = offline;
                param.secret = secret;
                param.tx_json = transaction;

                RippleRequest request = new RippleRequest(this.RPCCallId, "sign", param);

                RpcRequest<TransactionSigned>(request, null, (error, result) =>
                {
                    var res = result as TransactionSigned;
                    callback(error, res);
                });
            }
        }

        private static bool CertificateValidationCallBack(object sender,
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

        public void Dispose()
        {
            this.AutoReconnect = false;
            WebSocketRL.Close(200, "close by code");
        }
    }
}
