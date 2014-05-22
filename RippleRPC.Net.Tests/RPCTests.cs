using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RippleRPC.Net.Exceptions;
using RippleRPC.Net.Model;

namespace RippleRPC.Net.Tests
{
    [TestClass]
    public class RpcTests
    {

        private const string RippleAccount = "rho3u4kXc5q3chQFKfn9S1ZqUCya1xT3t4";
        //rPGKpTsgSaQiwLpEekVj1t5sgYJiqf2HDC

        [TestMethod]
        public void CanGetAccount()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            AccountInformation accountInformation = client.GetAccountInformation(RippleAccount);
            Assert.IsNotNull(accountInformation);
        }

        [TestMethod]
        public void CanHandleError()
        {

            try
            {
                RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
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
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            List<AccountLine> lines = client.GetAccountLines(RippleAccount);
            Assert.IsTrue(lines.Count > 0, "Returned zero lines");
        }

        [TestMethod]
        public void CanGetAccountOffers()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            List<AccountOffer> offers = client.GetAccountOffers(RippleAccount);
            Assert.IsTrue(offers.Count > 0, "Returned zero lines");
        }

        [TestMethod]
        public void CanListTransactions()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            List<TransactionRecord> transactions = client.GetTransactions(RippleAccount);
            //List<Transaction> transactions = client.GetTransactions(RippleAccount, -1, -1, false, false, 1);

            //List<Transaction> transactions = client.GetTransactions(RippleAccount, -1, -1, false, false, 1);

            Assert.IsTrue(transactions.Count > 0, "Returned no transactions");
        }

        [TestMethod]
        public void CanGetBookOffers()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            //CurrencyItem takerPays = new CurrencyItem {Currency = "USD", Issuer = "rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh"};
            RippleCurrency takerGets = new RippleCurrency {Currency = "XRP"};

            RippleCurrency takerPays = new RippleCurrency { Currency = "PHP", Issuer = "rho3u4kXc5q3chQFKfn9S1ZqUCya1xT3t4" };
            
            List<BookOffer> offers = client.GetBookOffers(takerPays, takerGets);

            Assert.IsTrue(offers.Count > 0, "Returned no transactions");
        }

        [TestMethod]
        public void CanGetLedgerInformation()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            var ledgerInfo = client.GetLedgerInformation();
            Assert.IsNotNull(ledgerInfo);
        }

        [TestMethod]
        public void CanGetCurrentLedgerIndex()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            var index = client.GetCurrentLedgerIndex();
            Assert.IsTrue(index > 0);
        }

        [TestMethod]
        public void CanGetClosedLedgerHash()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            var hash = client.GetClosedLedgerHash();
            Assert.IsTrue(!string.IsNullOrEmpty(hash));
        }

        [TestMethod]
        public void CanFindPath()
        {
            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));                 

            var path = client.FindRipplePath(RippleAccount, "rPGKpTsgSaQiwLpEekVj1t5sgYJiqf2HDC", 200);
        }

    }
}
