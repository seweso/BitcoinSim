using System.Collections.Generic;
using System.Linq;

namespace BitcoinSim
{
    public class MemPool
    {
        public int MaxMempoolSize { get; set; }

        private List<Transaction> _removed = new List<Transaction>();
        private SortedSet<Transaction> _memPool = new SortedSet<Transaction>(Transaction.TransactionComparerInstance);


        public MemPool()
        {
            MaxMempoolSize = 100000;
        }

        public IReadOnlyList<Transaction> Transactions
        {
            get { return _memPool.ToList(); }
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
            // Drop transactions above MaxMempoolSize
            if (_memPool.Count > MaxMempoolSize)
            {
                var newMempool = new SortedSet<Transaction>(Transaction.TransactionComparerInstance);
                int i = 0;
                foreach (var t in _memPool)
                {
                    if (++i <= MaxMempoolSize)
                    {
                        newMempool.Add(t);
                    }
                    else
                    {
                        t.RemoveFromMempool = currentTick;
                        _removed.Add(t);
                    }
                }
                _memPool = newMempool;
            }
        }

        public void RemoveTransactions(IEnumerable<Transaction> transactions)
        {
            foreach(var t in transactions)
                _memPool.Remove(t);
        }
    }
}