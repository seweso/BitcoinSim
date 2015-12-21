using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinSim
{
    public class Transaction
    {
        private static long _autoId = 0;

        private readonly long _id = ++_autoId;

        public long Id { get { return _id; }}

        public int Fees { get; set; }
        public UseCase UseCase { get; set; }

        // When added to mempool
        public int AddedInMempoolTick { get; set; }
        public int AddedInMemPoolHeight { get; set; }

        // When in block
        public Block ConfirmedInBlock { get; set; }

        // When removed from mempool
        public int RemoveFromMempool { get; set; }


        public int ConfirmationTime {
            get { return ConfirmedInBlock.Height - AddedInMemPoolHeight; }
        }

        public override string ToString()
        {
            return String.Format("Transaction({0})", Id);
        }

        private bool Equals(Transaction other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Transaction)obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static readonly IComparer<Transaction> TransactionComparerInstance = new TransactionComparer();

        private sealed class TransactionComparer : IComparer<Transaction>
        {
            public int Compare(Transaction x, Transaction y)
            {
                int result = y.Fees.CompareTo(x.Fees);
                if (result == 0)
                    result = x.Id.CompareTo(y.Id);
                return result;
            }
        }
    }
}
