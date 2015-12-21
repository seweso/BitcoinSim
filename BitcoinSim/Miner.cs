using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace BitcoinSim
{
    public class Miner
    {
        private static readonly Random rand = new Random();


        public string Name { get; set; }

        /// <summary>
        ///     Hashing power (0 = 0%, 1 = 100%)
        /// </summary>
        public double HashingPower { get; set; }

        /// <summary>
        ///     Number of max transactions in block
        /// </summary>
        public int SoftLimit { get; set; }

        /// <summary>
        ///     Allow subsidizing other transactions (other transaction fees can cover other transactions)
        /// </summary>
        public bool Subsidize { get; set; }

        /// <summary>
        ///     Cost per transaction (orphan cost / bandwidth / percieved cost for network)
        /// </summary>
        public int TransactionCost { get; set; }


        public bool Tick(Bitcoin system)
        {
            if (rand.NextDouble() > (HashingPower/system.Dificulty))
                return false;

            system.CleanMempool();

            // Create a block
            var block = new List<Transaction>();
            int totalFees = 0; 
            int i = 0;

            while (true)
            {
                if (i >= SoftLimit)
                    break;

                if (i >= system.MemPool.Count)
                    break; 

                Transaction t = system.MemPool[i++];
                totalFees += t.Fees;

                // Enough transactions?
                if (Subsidize ? totalFees < (i*TransactionCost) : t.Fees < TransactionCost)
                    break;

                // Add to block
                block.Add(t);
            }

            system.AddBlock(new Block {Origin = this, Transactions = block});
            return true;
        }
    }
}