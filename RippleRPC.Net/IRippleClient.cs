using System;
using System.Collections.Generic;
using System.Net;
using RippleRPC.Net.Model;
using RippleRPC.Net.Model.Paths;

namespace RippleRPC.Net
{
    public interface IRippleClient
    {
        Uri Uri { get; set; }
        NetworkCredential Credentials { get; set; }
        AccountInformation GetAccountInformation(string account, uint? index, string ledgerHash, bool strict, string ledgerIndex);
        List<AccountLine> GetAccountLines(string account, string peer, string ledgerIndex);
        List<AccountOffer> GetAccountOffers(string account, int accountIndex, string ledgerHash, string ledgerIndex);

        List<TransactionRecord> GetTransactions(string account, int minimumLedgerIndex, int maximumLedgerIndex, bool binary,
            bool forward, int limit);

        List<BookOffer> GetBookOffers(RippleCurrency takerPays, RippleCurrency takerGets, string ledger, string taker, int limit,
            bool proof, bool autoBridge);

        LedgerSummary GetLedgerInformation(string ledgerIndex, bool full);

        string GetClosedLedgerHash();

        int GetCurrentLedgerIndex();

        PathSummary FindRipplePath(string fromAccount, string toAccount, double amount, List<RippleCurrency> currencies,
            string ledgerHash, string ledgerIndex);

        string SendXRP(string fromAccount, string toAccount, double amount);

        string Submit(string transaction);

        string Sign(string transaction);

    }
}
