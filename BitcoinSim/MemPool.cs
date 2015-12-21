using System.Collections.Generic;

namespace BitcoinSim
{
    public class MemPool
    {
        private const int MaxMempoolSize = 100000;

        private List<Transaction> _removed = new List<Transaction>();
        private readonly List<Transaction> _memPool = new List<Transaction>();

        
        public IReadOnlyList<Transaction> Transactions
        {
            get { return _memPool; }
        }

        public IEnumerable<Transaction> GetAndClearRemoved()
        {
            var result = _removed;
            _removed = new List<Transaction>();
            return result;
        }

        public void AddTransaction(Transaction t)
        {
            _memPool.Add(t);
        }

        public void CleanMempool(int currentTick)
        {
            _memPool.Sort((x, y) => y.Fees.CompareTo(x.Fees));

            // Drop transactions above MaxMempoolSize
            if (_memPool.Count > MaxMempoolSize)
            {
                List<Transaction> removed = _memPool.GetRange(MaxMempoolSize, _memPool.Count - MaxMempoolSize);
                _removed.AddRange(removed);
                
                foreach (Transaction removedTransaction in removed)
                {
                    removedTransaction.RemoveFromMempool = currentTick;
                    _memPool.Remove(removedTransaction);
                }
            }
        }

        public void RemoveTransactions(IEnumerable<Transaction> transactions)
        {
            foreach(var t in transactions)
                _memPool.Remove(t);
        }
    }
}