using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RippleRPC.Net.Model
{
    public class Transaction
    {
        public TransactionType TransactionType { get; set; }

        public string Account { get; set; }

        public string Destination { get; set; }

        public List<object> Paths { get; set; }

        public RippleCurrencyValue Amount { get; set; }

        public RippleCurrencyValue SendMax { get; set; }
    }
}
