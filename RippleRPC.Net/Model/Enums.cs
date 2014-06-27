namespace RippleRPC.Net.Model
{
    public enum TransactionType
    {
        Payment,
        OfferCreate,
        OfferCancel,
        TrustSet,
        AccountSet
    }

    public enum RippleType
    {
        Int16 = 1,
        Int32 = 2,
        Int64 = 3,
        Hash128 = 4,
        Hash256 = 5,
        Amount = 6,
        VL = 7,
        Account = 8,
        Object = 14,
        Array = 15,
        Int8 = 16,
        Hash160 = 17,
        PathSet = 18,
        Vector256 = 19
    }
}
