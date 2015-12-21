using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace BitcoinSim
{
    public class UseCase
    {
        private static Random rand = new Random();

        /// <summary>
        /// Number of transactions per tick, should be between 0 and 1
        /// </summary>
        public double TransactionsPerTick { get; set; }

        /// <summary>
        /// Max fees willing to pay for this use case
        /// </summary>
        public int MaxFees { get; set; }

        /// <summary>
        /// Transaction needs to be in certain block (0 = first block, 1 = second etc)
        /// </summary>
        public int ConfirmationSpeedNeeded { get; set; }


        public void Tick(Bitcoin system)
        {
            if (rand.NextDouble() > TransactionsPerTick)
                return;

            int fees = (int) Math.Min(system.GetFees(ConfirmationSpeedNeeded) + Math.Max(1, MaxFees * 0.01), MaxFees);

            system.AddTransaction(new Transaction
            {
                Fees = fees,
                UseCase = this
            });
        }

    }
}
