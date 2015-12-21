using System.Collections.Generic;

namespace BitcoinSim
{
    public class MemPool
    {
        private SortedSet<Transaction> _memPool = new SortedSet<Transaction>(Transaction.TransactionComparerInstance);
        private List<Transaction> _removed = new List<Transaction>();


        public MemPool()
        {
            MaxMempoolSize = 100000;
        }

        public int MaxMempoolSize { get; set; }

        public IEnumerable<Transaction> Transactions
        {
            get { return _memPool; }
        }

        public int Size
        {
            get { return _memPool.Count; }
        }

        public IEnumerable<Transaction> GetAndClearRemoved()
        {
            List<Transaction> result = _removed;
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
                foreach (Transaction t in _memPool)
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
            foreach (Transaction t in transactions)
                _memPool.Remove(t);
        }
    }
}