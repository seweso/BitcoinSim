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

        private readonly List<Block> _blocks = new List<Block>();

        private IDictionary<int, int> _currentFeeEstimates = null;

        private List<Miner> _miners = new List<Miner>();

        private readonly List<UseCase> _useCases = new List<UseCase>();

        private readonly MemPool _memPool = new MemPool();

        public int CurrentTick { get; set; }

        public Bitcoin()
        {
            StoreFeeEstimation();
        }

        /// <summary>
        ///     Number between 1 and infinity  (at 10 it should take about 10 ticks to create a block)
        /// </summary>
        public double Dificulty { get; set; }

        public MemPool MemPool
        {
            get { return _memPool; }
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
        
        private IEnumerable<Block> GetLastBlocks(int nrOfBlocks)
        {
            int count = Math.Min(nrOfBlocks, _blocks.Count);
            return count == 0 ? new List<Block>() : _blocks.GetRange(_blocks.Count - count, count);
        }


        /// <summary>
        /// Calculate fees neccesary 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="maxConfirmations"></param>
        /// <param name="percentageDeviationAllowed"></param>
        /// <returns></returns>
        public static IDictionary<int, int> CalculateFeesPerConfirmationTime(IEnumerable<Block> list, int maxConfirmations, double percentageDeviationAllowed)
        {
            // Count all confirmation / fee combinations 
            var grouped = list.SelectMany(b => b.ConfirmationsPerFee).ToList();

            // Grab all distinct fees
            var distinctFees = grouped.Select(g => g.Fees).Distinct().OrderByDescending(k => k).ToList();

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
                    double theBad = grouped.Where(g => g.ConfirmationTime > conf && g.Fees >= fee).Sum(g => g.Count);
                    double theGood = grouped.Where(g => g.ConfirmationTime <= conf && g.Fees <= fee).Sum(g => g.Count);

                    double percentage = theGood/(theBad + theGood);
                    maxPercentage = Math.Max(maxPercentage, percentage - percentageDeviationAllowed);

                    if (percentage >= maxPercentage)
                        result[conf] = fee;

                    percentages[conf][fee] = percentage;
                }
            }

            return result;
        }


        public void AddTransaction(Transaction t)
        {
            t.AddedInMempoolTick = CurrentTick;
            t.AddedInMemPoolHeight = _blocks.Count;

            _memPool.AddTransaction(t);
        }


        public void AddBlock(Block block)
        {
            block.Height = _blocks.Count;
            block.MinedAtTick = CurrentTick;

            //Remove transactions from mempool
            _memPool.RemoveTransactions(block.Transactions);

            // Set confirmed in block (for easy navigation)
            foreach (Transaction t in block.Transactions)
                t.ConfirmedInBlock = block;

            _blocks.Add(block);

            // Removed uses cases
            string sRemovedUseCases = _memPool.GetAndClearRemoved().GroupBy(t => t.UseCase).Aggregate("", (current, @group) => current + String.Format("{0}={1},", @group.Key.MaxFees, @group.Count()));

            // Fee statistics from block
            string blockFees = "emtpy block";

            if (block.Transactions.Count > 0)
                blockFees = String.Format("Max fee: {0}, Min fee: {1}, Average fees: {2}, Total fees: {3}", block.Transactions.First().Fees, block.Transactions.Last().Fees, block.Transactions.Average(t => t.Fees), block.Transactions.Sum(t => t.Fees));
            
            // Print block staticics
            Debug.Print("Miner: {0}, Height: {1}, Est 1: {6}, Est 2: {7}, Est 3: {8}, Est 4: {9}, Est 5: {10}, Est 6: {11}, Est 20: {12}, Difficulty: {13}, Transactions: {2}, Mempool: {3}, Block fees: {5}, Removed use cases: ({4})", block.Origin.Name, block.Height, block.Transactions.Count, _memPool.Size, sRemovedUseCases, blockFees, _currentFeeEstimates[1], _currentFeeEstimates[2], _currentFeeEstimates[3], _currentFeeEstimates[4], _currentFeeEstimates[5], _currentFeeEstimates[6], _currentFeeEstimates[20], Dificulty);
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
            _currentFeeEstimates = CalculateFeesPerConfirmationTime(GetLastBlocks(100), 20, 0.0001);
        }

        public void CleanMempool()
        {
            _memPool.CleanMempool(CurrentTick);
        }
    }
}