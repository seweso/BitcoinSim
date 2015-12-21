using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace BitcoinSim.Tests
{
    [TestFixture]
    public class MemPoolTests
    {
        [Test]
        public void GetAndClearRemovedTest()
        {
            var mempool = new MemPool {MaxMempoolSize = 5};

            var t1 = new Transaction {Fees = 1};
            var t2 = new Transaction {Fees = 1};
            var t3 = new Transaction {Fees = 2};
            var t4 = new Transaction {Fees = 2};
            var t5 = new Transaction {Fees = 3};
            var t6 = new Transaction {Fees = 4};

            Assert.AreEqual(0, mempool.Transactions.ToList().Count);

            mempool.AddTransaction(t1);
            mempool.CleanMempool(0);

            Assert.AreEqual(1, mempool.Size);
            Assert.AreEqual(new List<Transaction> {t1}, mempool.Transactions.ToList());
            
            mempool.AddTransaction(t2);
            mempool.CleanMempool(0);

            Assert.AreEqual(2, mempool.Size);
            Assert.AreEqual(new List<Transaction> { t1, t2 }, mempool.Transactions.ToList());

            mempool.AddTransaction(t3);
            mempool.CleanMempool(0);

            Assert.AreEqual(3, mempool.Size);
            Assert.AreEqual(new List<Transaction> { t3, t1, t2 }, mempool.Transactions.ToList());

            mempool.AddTransaction(t4);
            mempool.CleanMempool(0);

            Assert.AreEqual(4, mempool.Size);
            Assert.AreEqual(new List<Transaction> { t3, t4, t1, t2 }, mempool.Transactions.ToList());

            mempool.AddTransaction(t5);
            mempool.CleanMempool(0);

            Assert.AreEqual(5, mempool.Size);
            Assert.AreEqual(new List<Transaction> { t5, t3, t4, t1, t2 }, mempool.Transactions.ToList());

            mempool.AddTransaction(t6);
            mempool.CleanMempool(0);

            Assert.AreEqual(5, mempool.Size);
            Assert.AreEqual(t6, mempool.Transactions.ToList()[0]);
            Assert.AreEqual(t5, mempool.Transactions.ToList()[1]);
            Assert.AreEqual(t3, mempool.Transactions.ToList()[2]);
            Assert.AreEqual(t4, mempool.Transactions.ToList()[3]);
            Assert.AreEqual(t1, mempool.Transactions.ToList()[4]);

            mempool.RemoveTransactions(new List<Transaction> {t1, t3, t5});

            Assert.AreEqual(2, mempool.Size);
            Assert.AreEqual(t6, mempool.Transactions.ToList()[0]);
            Assert.AreEqual(t4, mempool.Transactions.ToList()[1]);
        }
    }
}