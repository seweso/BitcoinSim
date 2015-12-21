using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;

namespace BitcoinSim
{
    public class Bitcoin
    {
        public const int HardLimit = 1000;
        public const int MaxMempoolSize = 100000;

        private readonly List<Block> _blocks = new List<Block>();
        private readonly List<Transaction> _memPool = new List<Transaction>();
        private readonly List<Transaction> _removed = new List<Transaction>();

        private IDictionary<int, int> _currentFeeEstimates = null;

        private List<Miner> _miners = new List<Miner>();

        private List<UseCase> _useCases = new List<UseCase>();

        private int CurrentTick { get; set; }

        public IReadOnlyList<Transaction> MemPool
        {
            get { return _memPool; }
        }

        public Bitcoin()
        {
            StoreFeeEstimation();
        }

        /// <summary>
        ///     Number between 1 and infinity  (at 10 it should take about 10 ticks to create a block)
        /// </summary>
        public double Dificulty { get; set; }


        public void AddTransaction(Transaction t)
        {
            t.AddedInMempoolTick = CurrentTick;
            t.AddedInMemPoolHeight = _blocks.Count;

            _memPool.Add(t);
        }

        public void CleanMempool()
        {
            _memPool.Sort((x, y) => y.Fees.CompareTo(x.Fees));

            // Drop transactions above MaxMempoolSize
            if (_memPool.Count > MaxMempoolSize)
            {
                List<Transaction> removed = _memPool.GetRange(MaxMempoolSize, _memPool.Count - MaxMempoolSize);
                _removed.AddRange(removed);
                
                foreach (Transaction removedTransaction in removed)
                {
                    removedTransaction.RemoveFromMempool = CurrentTick;
                    _memPool.Remove(removedTransaction);
                }
            }
        }


        /// <summary>
        /// Calculate fees needed 
        /// </summary>
        /// <param name="confirmationSpeedNeeded"></param>
        /// <returns></returns>
        public int GetFees(int confirmationSpeedNeeded)
        {
            if (_currentFeeEstimates == null)
                return 100;

            if (!_currentFeeEstimates.ContainsKey(confirmationSpeedNeeded))
                return _currentFeeEstimates.Values.Last();

            return _currentFeeEstimates[confirmationSpeedNeeded];
        }


        public int GetFeesViaMempool(int confirmationSpeedNeeded)
        {
            // Calculate average block size of last 100 blocks
            var average = AverageBlockSize(100);

            int lookAt = (int)Math.Min(_memPool.Count - 1, average * (confirmationSpeedNeeded + 0.5));

            if (lookAt < 0)
                return 0;

            return _memPool[lookAt].Fees;
        }

        private double AverageBlockSize(int nrOfBlocks)
        {
            int count = Math.Min(nrOfBlocks, _blocks.Count);

            if (count == 0)
                return 0;

            double average = _blocks.GetRange(_blocks.Count - count, count).Average(b => b.Transactions.Count);
            return average;
        }

        private IEnumerable<Transaction> GetTransactions(int nrOfBlocks)
        {
            int count = Math.Min(nrOfBlocks, _blocks.Count);

            var result = new List<Transaction>();
            if (count == 0)
                return result;

            foreach (var block in _blocks.GetRange(_blocks.Count - count, count))
            {
                result.AddRange(block.Transactions);
            }
            return result;
        }


        /// <summary>
        /// Calculate fees neccesary 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="maxConfirmations"></param>
        /// <param name="percentageDeviationAllowed"></param>
        /// <returns></returns>
        public static IDictionary<int, int> CalculateFeesPerConfirmationTime(IEnumerable<Transaction> list, int maxConfirmations, double percentageDeviationAllowed)
        {
            // Count all confirmation / fee combinations 
            var grouped = list.GroupBy(t => new { t.ConfirmationTime, t.Fees }).ToList();

            // Grab all distinct fees
            var distinctFees = grouped.Select(g => g.Key.Fees).Distinct().OrderByDescending(k => k).ToList();

            // No transactions / fees
            if (distinctFees.Count == 0)
                distinctFees.Add(0);

            // Create matrix with all fees
            var percentages = new SortedDictionary<int, IDictionary<int, double>>();

            // Result structure
            var result = new SortedDictionary<int, int>();

            // Loop through all confimations
            for (int conf = 1; conf <= maxConfirmations; conf++)
            {
                result[conf] = distinctFees[0];
                percentages[conf] = new SortedDictionary<int, double>();
                double maxPercentage = 0;

                // Calculate percentage (per confirmation/fee combination
                foreach (int fee in distinctFees)
                {
                    double theBad = grouped.Where(g => g.Key.ConfirmationTime > conf && g.Key.Fees >= fee).Sum(g => g.Count());
                    double theGood = grouped.Where(g => g.Key.ConfirmationTime <= conf && g.Key.Fees <= fee).Sum(g => g.Count());

                    double percentage = theGood/(theBad + theGood);
                    maxPercentage = Math.Max(maxPercentage, percentage - percentageDeviationAllowed);

                    if (percentage >= maxPercentage)
                        result[conf] = fee;

                    percentages[conf][fee] = percentage;
                }
            }

            return result;
        }


        public void AddBlock(Block block)
        {
            block.Height = _blocks.Count;
            block.MinedAtTick = CurrentTick;

            //Remove transactions from mempool
            foreach (Transaction t in block.Transactions)
            {
                t.ConfirmedInBlock = block;
                _memPool.Remove(t);
            }

            _blocks.Add(block);

            // Removed uses cases
            string sRemovedUseCases = _removed.GroupBy(t => t.UseCase).Aggregate("", (current, @group) => current + String.Format("{0}={1},", @group.Key.MaxFees, @group.Count()));
            _removed.Clear();

            // Fee statistics from block
            string blockFees = "emtpy block";

            if (block.Transactions.Count > 0)
                blockFees = String.Format("Max fee: {0}, Min fee: {1}, Average fees: {2}, Total fees: {3}", block.Transactions.First().Fees, block.Transactions.Last().Fees, block.Transactions.Average(t => t.Fees), block.Transactions.Sum(t => t.Fees));
            
            // Print block staticics
            Debug.Print("Miner: {0}, Height: {1}, Est 1: {6}, Est 2: {7}, Est 3: {8}, Est 4: {9}, Est 5: {10}, Est 6: {11}, Difficulty: {12}, Transactions: {2}, Mempool: {3}, Block fees: {5}, Removed use cases: ({4})", block.Origin.Name, block.Height, block.Transactions.Count, MemPool.Count, sRemovedUseCases, blockFees, _currentFeeEstimates[1], _currentFeeEstimates[2], _currentFeeEstimates[3], _currentFeeEstimates[4], _currentFeeEstimates[5], _currentFeeEstimates[6], Dificulty);
        }

        public void ReplaceMiners(List<Miner> miners)
        {
            if (Math.Abs(miners.Sum(m => m.HashingPower) - 1) > 0.00001)
                throw new Exception("Total hashing power not 100%");

            _miners = miners;
        }

        public void AddUseCases(IEnumerable<UseCase> useCases)
        {
            _useCases.AddRange(useCases);
        }

        public void Tick()
        {
            CurrentTick++;

            _miners.Shuffle();

            foreach (UseCase usecase in _useCases)
                usecase.Tick(this);

            foreach (Miner miner in _miners) {
                if (miner.Tick(this)) {
                    // Found a block

                    StoreFeeEstimation();

                    Dificulty *= 1.001;

                    break;
                }
            }
        }

        private void StoreFeeEstimation()
        {
            _currentFeeEstimates = CalculateFeesPerConfirmationTime(GetTransactions(100), 20, 0.0001);
        }
    }
}