﻿using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace RippleRPC.Net.Crypto.Encodings
{
    public static class B16
    {
        public static string ToString(byte[] bytes)
        {
            return Hex.ToHexString(bytes).ToUpper();
        }

        public static byte[] Decode(String hex)
        {
            return Hex.Decode(hex);
        }
    }
}
