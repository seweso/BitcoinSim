using System.Collections.Generic;
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

            Assert.AreEqual(0, mempool.Transactions.Count);

            mempool.AddTransaction(t1);
            mempool.CleanMempool(0);

            Assert.AreEqual(1, mempool.Transactions.Count);
            Assert.AreEqual(t1, mempool.Transactions[0]);

            mempool.AddTransaction(t2);
            mempool.CleanMempool(0);

            Assert.AreEqual(2, mempool.Transactions.Count);
            Assert.AreEqual(t1, mempool.Transactions[0]);
            Assert.AreEqual(t2, mempool.Transactions[1]);

            mempool.AddTransaction(t3);
            mempool.CleanMempool(0);

            Assert.AreEqual(3, mempool.Transactions.Count);
            Assert.AreEqual(t3, mempool.Transactions[0]);
            Assert.AreEqual(t1, mempool.Transactions[1]);
            Assert.AreEqual(t2, mempool.Transactions[2]);

            mempool.AddTransaction(t4);
            mempool.CleanMempool(0);

            Assert.AreEqual(4, mempool.Transactions.Count);
            Assert.AreEqual(t3, mempool.Transactions[0]);
            Assert.AreEqual(t4, mempool.Transactions[1]);
            Assert.AreEqual(t1, mempool.Transactions[2]);
            Assert.AreEqual(t2, mempool.Transactions[3]);

            mempool.AddTransaction(t5);
            mempool.CleanMempool(0);

            Assert.AreEqual(5, mempool.Transactions.Count);
            Assert.AreEqual(t5, mempool.Transactions[0]);
            Assert.AreEqual(t3, mempool.Transactions[1]);
            Assert.AreEqual(t4, mempool.Transactions[2]);
            Assert.AreEqual(t1, mempool.Transactions[3]);
            Assert.AreEqual(t2, mempool.Transactions[4]);


            mempool.AddTransaction(t6);
            mempool.CleanMempool(0);

            Assert.AreEqual(5, mempool.Transactions.Count);
            Assert.AreEqual(t6, mempool.Transactions[0]);
            Assert.AreEqual(t5, mempool.Transactions[1]);
            Assert.AreEqual(t3, mempool.Transactions[2]);
            Assert.AreEqual(t4, mempool.Transactions[3]);
            Assert.AreEqual(t1, mempool.Transactions[4]);

            mempool.RemoveTransactions(new List<Transaction> {t1, t3, t5});

            Assert.AreEqual(2, mempool.Transactions.Count);
            Assert.AreEqual(t6, mempool.Transactions[0]);
            Assert.AreEqual(t4, mempool.Transactions[1]);
        }
    }
}