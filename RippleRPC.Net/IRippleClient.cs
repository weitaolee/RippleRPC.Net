using System;
using System.Collections.Generic;
using System.Net;
using RippleRPC.Net.Model;

namespace RippleRPC.Net
{
    public interface IRippleClient
    {
        Uri Uri { get; set; }
        NetworkCredential Credentials { get; set; }
        AccountInformation GetAccountInformation(string account, uint? index, string ledgerHash, bool strict, string ledgerIndex);
        List<AccountLine> GetAccountLines(string account, string peer, string ledgerIndex);
        List<AccountOffer> GetAccountOffers(string account, int accountIndex, string ledgerHash, string ledgerIndex);

        List<Transaction> GetTransactions(string account, int minimumLedgerIndex, int maximumLedgerIndex, bool binary,
            bool forward, int limit);

        List<BookOffer> GetBookOffers(CurrencyItem takerPays, CurrencyItem takerGets, string ledger, string taker, int limit,
            bool proof, bool autoBridge);

        LedgerSummary GetLedgerInformation(string ledgerIndex, bool full);

        /// <summary>
        /// May not be working
        /// </summary>
        /// <returns></returns>
        string GetClosedLedgerHash();

        /// <summary>
        /// May not be working
        /// </summary>
        /// <returns></returns>
        int GetCurrentLedgerIndex();

        /// <summary>
        /// May not be working
        /// </summary>
        /// <param name="fromAccount"></param>
        /// <param name="toAccount"></param>
        /// <param name="amount"></param>
        /// <param name="currencies"></param>
        /// <param name="ledgerHash"></param>
        /// <param name="ledgerIndex"></param>
        /// <returns></returns>
        RipplePath FindRipplePath(string fromAccount, string toAccount, int amount, List<CurrencyItem> currencies,
            string ledgerHash, string ledgerIndex);

    }
}
