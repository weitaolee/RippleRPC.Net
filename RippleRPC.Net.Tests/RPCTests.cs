using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RippleRPC.Net.Exceptions;
using RippleRPC.Net.Model;

namespace RippleRPC.Net.Tests
{
    [TestClass]
    public class RpcTests
    {

        private const string RippleAccount = "rho3u4kXc5q3chQFKfn9S1ZqUCya1xT3t4";
        private const string RippledServer = "http://s1.ripple.com:51234";
        //http://s1.ripple.com:51234
        //rPGKpTsgSaQiwLpEekVj1t5sgYJiqf2HDC
        //"https://s1.ripple.com:443";

        [TestMethod]
        public void CanGetAccount()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            AccountInformation accountInformation = client.GetAccountInformation(RippleAccount);
            Assert.IsNotNull(accountInformation);
        }

        [TestMethod]
        public void CanHandleError()
        {

            try
            {
                RippleClient client = new RippleClient(new Uri(RippledServer));
                client.GetAccountInformation("foo");

                Assert.Fail("We passed without failing");
            }
            catch (RippleRpcException ex)
            {
                Assert.IsNotNull(ex);                
            }            
        }

        [TestMethod]
        public void CanGetAccountLines()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            List<AccountLine> lines = client.GetAccountLines(RippleAccount);
            Assert.IsTrue(lines.Count > 0, "Returned zero lines");
        }

        [TestMethod]
        public void CanGetAccountOffers()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            List<AccountOffer> offers = client.GetAccountOffers(RippleAccount);
            Assert.IsTrue(offers.Count > 0, "Returned zero lines");
        }

        [TestMethod]
        public void CanListTransactions()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            List<TransactionRecord> transactions = client.GetTransactions(RippleAccount);
            //List<Transaction> transactions = client.GetTransactions(RippleAccount, -1, -1, false, false, 1);

            //List<Transaction> transactions = client.GetTransactions(RippleAccount, -1, -1, false, false, 1);

            Assert.IsTrue(transactions.Count > 0, "Returned no transactions");
        }

        [TestMethod]
        public void CanGetBookOffers()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            //CurrencyItem takerPays = new CurrencyItem {Currency = "USD", Issuer = "rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh"};
            RippleCurrency takerGets = new RippleCurrency {Currency = "XRP"};

            RippleCurrency takerPays = new RippleCurrency { Currency = "PHP", Issuer = "rho3u4kXc5q3chQFKfn9S1ZqUCya1xT3t4" };
            
            List<BookOffer> offers = client.GetBookOffers(takerPays, takerGets);

            Assert.IsTrue(offers.Count > 0, "Returned no transactions");
        }

        [TestMethod]
        public void CanGetLedgerInformation()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            var ledgerInfo = client.GetLedgerInformation();
            Assert.IsNotNull(ledgerInfo);
        }

        [TestMethod]
        public void CanGetCurrentLedgerIndex()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            var index = client.GetCurrentLedgerIndex();
            Assert.IsTrue(index > 0);
        }

        [TestMethod]
        public void CanGetClosedLedgerHash()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            var hash = client.GetClosedLedgerHash();
            Assert.IsTrue(!string.IsNullOrEmpty(hash));
        }

        [TestMethod]
        public void CanFindPath()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));                 

            var path = client.FindRipplePath(RippleAccount, "rPGKpTsgSaQiwLpEekVj1t5sgYJiqf2HDC", 200);
        }

        [TestMethod]
        public void CanSubmitTransaction()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));

            var result = client.Submit("1200002280000000240000003A6140000000000003E868400000000000000A73210254C6E84863EB37360F33430E9CE2C28CF609CE15E3FF6AEC909E62620659EC75744630440220429208E5B63A6FC5C28DA9587A2608A7694B67AB8739D882146F6BC1494DB67B02207CC2AC24C7C3EF94B6A2E0BBCC4E1DC5F4C4260ADC4E2B130569E7370EF3F86D811429A21BD3C8CEDECFBBA54F2D1A9B1967DA2868BC83148752CC353EA495AE6830E6B7D2469C88DB59403F");
        }

        [TestMethod]
        public void CanSignTransaction()
        {
            RippleClient client = new RippleClient(new Uri(RippledServer));
            Transaction transaction = new Transaction();
            transaction.Sign("sh5VoxNBQgFYt32icy6T2jkC7MapV");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(transaction.Hash));
          //  var result = client.Submit(transaction.Hash);
        }

        [TestMethod]
        public void CanSerializeExpandoObject()
        {
            dynamic transaction = new ExpandoObject();
            transaction.TransactionType = "Payment";
            transaction.Account = "rho3u4kXc5q3chQFKfn9S1ZqUCya1xT3t4";
            transaction.Destination = "rDLXQ8KEBn3Aw313bGzhEemx8cCPpGha3d";
            transaction.Amount = "1000";
            transaction.Fee = "10";
            transaction.Sequence = 58;
            transaction.SigningPubKey = "0254C6E84863EB37360F33430E9CE2C28CF609CE15E3FF6AEC909E62620659EC75";
            transaction.Flags = 2147483648;
            transaction.TxnSignature = "304402201B7CBF5ABF13041C46E586631F2A17BD1A564FF57784410E15ED4BC9CC2CE23A02205C211BB6E1254DA7062847C0445AA96B556402052AA2BDE47705294AFD60D670";

            var result =
                "1200002280000000240000003A6140000000000003E868400000000000000A73210254C6E84863EB37360F33430E9CE2C28CF609CE15E3FF6AEC909E62620659EC757446304402201B7CBF5ABF13041C46E586631F2A17BD1A564FF57784410E15ED4BC9CC2CE23A02205C211BB6E1254DA7062847C0445AA96B556402052AA2BDE47705294AFD60D670811429A21BD3C8CEDECFBBA54F2D1A9B1967DA2868BC83148752CC353EA495AE6830E6B7D2469C88DB59403F";

            var serializedValue = SerializeObject(transaction);

            Assert.AreEqual(result, serializedValue);
        }

        private string SerializeObject(ExpandoObject expandoObject)
        {
            var expandoDict = (IDictionary<string, object>)expandoObject;
            var keys = expandoDict.Keys.ToList();
            keys.Sort();

            foreach (var key in keys)
            {
                var keyPair = GetInverseList(key);
                if (!keyPair.HasValue) throw new Exception("Value not found: " + key);

                KeyValuePair<int, int> keyValuePair = keyPair.Value;
                var typeBits = keyValuePair.Key;
                var fieldBits = keyValuePair.Value;
                var tagByte = (typeBits < 16 ? typeBits << 4 : 0) | (fieldBits < 16 ? fieldBits : 0);
                Console.WriteLine(tagByte);
            }

            return "";
        }

        private static KeyValuePair<int, int>? GetInverseList(string value)
        {
            Dictionary<string, KeyValuePair<int, int>> values = new Dictionary<string, KeyValuePair<int, int>>();

            //Int16
            values.Add("LedgerEntryType", new KeyValuePair<int, int>(1,1));
            values.Add("TransactionType", new KeyValuePair<int, int>(1, 2));

            //Int32
            values.Add("Flags", new KeyValuePair<int, int>(2, 2));
            values.Add("SourceTag", new KeyValuePair<int, int>(2, 3));
            values.Add("Sequence", new KeyValuePair<int, int>(2, 4));
            values.Add("PreviousTxnLgrSeq", new KeyValuePair<int, int>(2, 5));
            values.Add("LedgerSequence", new KeyValuePair<int, int>(2, 6));
            values.Add("CloseTime", new KeyValuePair<int, int>(2, 7));
            values.Add("ParentCloseTime", new KeyValuePair<int, int>(2, 8));
            values.Add("SigningTime", new KeyValuePair<int, int>(2, 9));
            values.Add("Expiration", new KeyValuePair<int, int>(2, 10));
            values.Add("TransferRate", new KeyValuePair<int, int>(2, 11));
            values.Add("WalletSize", new KeyValuePair<int, int>(2, 12));
            values.Add("OwnerCount", new KeyValuePair<int, int>(2, 13));
            values.Add("DestinationTag", new KeyValuePair<int, int>(2, 14));
            //15 empty
            values.Add("HighQualityIn", new KeyValuePair<int, int>(2, 16));
            values.Add("HighQualityOut", new KeyValuePair<int, int>(2, 17));
            values.Add("LowQualityIn", new KeyValuePair<int, int>(2, 18));
            values.Add("LowQualityOut", new KeyValuePair<int, int>(2, 19));
            values.Add("QualityIn", new KeyValuePair<int, int>(2, 20));
            values.Add("QualityOut", new KeyValuePair<int, int>(2, 21));
            values.Add("StampEscrow", new KeyValuePair<int, int>(2, 22));
            values.Add("BondAmount", new KeyValuePair<int, int>(2, 23));
            values.Add("LoadFee", new KeyValuePair<int, int>(2, 24));
            values.Add("OfferSequence", new KeyValuePair<int, int>(2, 25));
            values.Add("FirstLedgerSequence", new KeyValuePair<int, int>(2, 26));
            values.Add("LastLedgerSequence", new KeyValuePair<int, int>(2, 27));
            values.Add("TransactionIndex", new KeyValuePair<int, int>(2, 28));
            values.Add("OperationLimit", new KeyValuePair<int, int>(2, 29));
            values.Add("ReferenceFeeUnits", new KeyValuePair<int, int>(2, 30));
            values.Add("ReserveBase", new KeyValuePair<int, int>(2, 31));
            values.Add("ReserveIncrement", new KeyValuePair<int, int>(2, 32));
            values.Add("SetFlag", new KeyValuePair<int, int>(2, 33));
            values.Add("ClearFlag", new KeyValuePair<int, int>(2, 34));

            //Int64
            values.Add("IndexNext", new KeyValuePair<int, int>(3, 1));
            values.Add("IndexPrevious", new KeyValuePair<int, int>(3, 2));
            values.Add("BookNode", new KeyValuePair<int, int>(3, 3));
            values.Add("OwnerNode", new KeyValuePair<int, int>(3, 4));
            values.Add("BaseFee", new KeyValuePair<int, int>(3, 5));
            values.Add("ExchangeRate", new KeyValuePair<int, int>(3, 6));
            values.Add("LowNode", new KeyValuePair<int, int>(3, 7));
            values.Add("HighNode", new KeyValuePair<int, int>(3, 8));

            // Hash128
            values.Add("EmailHash", new KeyValuePair<int, int>(4, 1));

            // Hash256
            values.Add("LedgerHash", new KeyValuePair<int, int>(5, 1));
            values.Add("ParentHash", new KeyValuePair<int, int>(5, 2));
            values.Add("TransactionHash", new KeyValuePair<int, int>(5, 3));
            values.Add("AccountHash", new KeyValuePair<int, int>(5, 4));
            values.Add("PreviousTxnID", new KeyValuePair<int, int>(5, 5));
            values.Add("LedgerIndex", new KeyValuePair<int, int>(5, 6));
            values.Add("WalletLocator", new KeyValuePair<int, int>(5, 7));
            values.Add("RootIndex", new KeyValuePair<int, int>(5, 8));
            values.Add("AccountTxnID", new KeyValuePair<int, int>(5, 9));
            //break in sequence
            values.Add("BookDirectory", new KeyValuePair<int, int>(5, 16));
            values.Add("InvoiceID", new KeyValuePair<int, int>(5, 17));
            values.Add("Nickname", new KeyValuePair<int, int>(5, 18));
            values.Add("Feature", new KeyValuePair<int, int>(5, 19));

            // Amount
            values.Add("Amount", new KeyValuePair<int, int>(6, 1));
            values.Add("Balance", new KeyValuePair<int, int>(6, 2));
            values.Add("LimitAmount", new KeyValuePair<int, int>(6, 3));
            values.Add("TakerPays", new KeyValuePair<int, int>(6, 4));
            values.Add("TakerGets", new KeyValuePair<int, int>(6, 5));
            values.Add("LowLimit", new KeyValuePair<int, int>(6, 6));
            values.Add("HighLimit", new KeyValuePair<int, int>(6, 7));
            values.Add("Fee", new KeyValuePair<int, int>(6, 8));
            values.Add("SendMax", new KeyValuePair<int, int>(6, 9));
            //
            values.Add("MinimumOffer", new KeyValuePair<int, int>(6, 16));
            values.Add("RippleEscrow", new KeyValuePair<int, int>(6, 17));
            values.Add("DeliveredAmount", new KeyValuePair<int, int>(6, 18));

            //VL
            values.Add("PublicKey", new KeyValuePair<int, int>(7, 1));
            values.Add("MessageKey", new KeyValuePair<int, int>(7, 2));
            values.Add("SigningPubKey", new KeyValuePair<int, int>(7, 3));
            values.Add("TxnSignature", new KeyValuePair<int, int>(7, 4));
            values.Add("Generator", new KeyValuePair<int, int>(7, 5));
            values.Add("Signature", new KeyValuePair<int, int>(7, 6));
            values.Add("Domain", new KeyValuePair<int, int>(7, 7));
            values.Add("FundCode", new KeyValuePair<int, int>(7, 8));
            values.Add("RemoveCode", new KeyValuePair<int, int>(7, 9));
            values.Add("ExpireCode", new KeyValuePair<int, int>(7, 10));
            values.Add("CreateCode", new KeyValuePair<int, int>(7, 11));
            values.Add("MemoType", new KeyValuePair<int, int>(7, 12));
            values.Add("MemoData", new KeyValuePair<int, int>(7, 13));

            //Account
            values.Add("Account", new KeyValuePair<int, int>(8, 1));
            values.Add("Owner", new KeyValuePair<int, int>(8, 2));
            values.Add("Destination", new KeyValuePair<int, int>(8, 3));
            values.Add("Issuer", new KeyValuePair<int, int>(8, 4));
            //
            values.Add("Target", new KeyValuePair<int, int>(8, 7));
            values.Add("RegularKey", new KeyValuePair<int, int>(8, 8));

            //Object
            //1: void(0),  //end of Object
            values.Add("TransactionMetaData", new KeyValuePair<int, int>(14, 2));
            values.Add("CreatedNode", new KeyValuePair<int, int>(14, 3));
            values.Add("DeletedNode", new KeyValuePair<int, int>(14, 4));
            values.Add("ModifiedNode", new KeyValuePair<int, int>(14, 5));
            values.Add("PreviousFields", new KeyValuePair<int, int>(14, 6));
            values.Add("FinalFields", new KeyValuePair<int, int>(14, 7));
            values.Add("NewFields", new KeyValuePair<int, int>(14, 8));
            values.Add("TemplateEntry", new KeyValuePair<int, int>(14, 9));
            values.Add("Memo", new KeyValuePair<int, int>(14, 10));

            //Array
            //1: void(0),  //end of Array
            values.Add("SigningAccounts", new KeyValuePair<int, int>(15, 2));
            values.Add("TxnSignatures", new KeyValuePair<int, int>(15, 3));
            values.Add("Signatures", new KeyValuePair<int, int>(15, 4));
            values.Add("Template", new KeyValuePair<int, int>(15, 5));
            values.Add("Necessary", new KeyValuePair<int, int>(15, 6));
            values.Add("Sufficient", new KeyValuePair<int, int>(15, 7));
            values.Add("AffectedNodes", new KeyValuePair<int, int>(15, 8));
            values.Add("Memos", new KeyValuePair<int, int>(15, 9));

            //Uncommon types
            //Int8
            values.Add("CloseResolution", new KeyValuePair<int, int>(16, 1));
            values.Add("TemplateEntryType", new KeyValuePair<int, int>(16, 2));
            values.Add("TransactionResult", new KeyValuePair<int, int>(16, 3));

            //Hash160
            values.Add("TakerPaysCurrency", new KeyValuePair<int, int>(17, 1));
            values.Add("TakerPaysIssuer", new KeyValuePair<int, int>(17, 2));
            values.Add("TakerGetsCurrency", new KeyValuePair<int, int>(17, 3));
            values.Add("TakerGetsIssuer", new KeyValuePair<int, int>(17, 4));

            // PathSet
            values.Add("Paths", new KeyValuePair<int, int>(18, 1));

            // Vector256
            values.Add("Indexes", new KeyValuePair<int, int>(19, 1));
            values.Add("Hashes", new KeyValuePair<int, int>(19, 2));
            values.Add("Features", new KeyValuePair<int, int>(19, 3));

            if (values.ContainsKey(value))
                return values[value];

            return null;
        }

    }
}
