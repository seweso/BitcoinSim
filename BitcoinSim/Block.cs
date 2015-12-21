using System.Collections.Generic;
using System.Linq;

namespace BitcoinSim
{
    public class Block
    {
        private List<ConfirmationFeeCount> _confirmationsPerFee;
        public Miner Origin { get; set; }

        public IReadOnlyList<Transaction> Transactions { get; set; }

        // When block added
        public int Height { get; set; }
        public int MinedAtTick { get; set; }

        /// <summary>
        ///     Calculate and store confirmation times per fee for this block
        /// </summary>
        public List<ConfirmationFeeCount> ConfirmationsPerFee
        {
            get
            {
                if (_confirmationsPerFee == null)
                {
                    _confirmationsPerFee = Transactions.GroupBy(t => new {t.ConfirmationTime, t.Fees})
                        .Select(g => new ConfirmationFeeCount(g.Key.Fees, g.Key.ConfirmationTime, g.Count())
                        ).ToList();
                }
                return _confirmationsPerFee;
            }
        }

        public class ConfirmationFeeCount
        {
            public ConfirmationFeeCount(int fees, int confirmationTime, int count)
            {
                Fees = fees;
                ConfirmationTime = confirmationTime;
                Count = count;
            }

            public int Fees { get; private set; }
            public int ConfirmationTime { get; private set; }
            public int Count { get; private set; }
        }
    }
}