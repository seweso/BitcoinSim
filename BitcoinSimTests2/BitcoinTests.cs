using System.Collections.Generic;
using NUnit.Framework;

namespace BitcoinSim.Tests
{
    [TestFixture]
    public class BitcoinTests
    {
        [Test]
        public void CalculateFeesPerConfirmationTimeTest()
        {
            var block = new Block {Height = 10};
            block.Transactions = new List<Transaction>
            {
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 9, Fees = 2},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 9, Fees = 2},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 9, Fees = 2},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 7, Fees = 2},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 9, Fees = 1},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 9, Fees = 1},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 9, Fees = 1},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 8, Fees = 0},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 8, Fees = 0},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 7, Fees = 0},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 7, Fees = 0},
                new Transaction {ConfirmedInBlock = block, AddedInMemPoolHeight = 7, Fees = 0},
            };
            IDictionary<int, int> result = Bitcoin.CalculateFeesPerConfirmationTime(
                new List<Block> {block},
                3, 0.1);

            Assert.AreEqual(new Dictionary<int, int> {{1, 2}, {2, 1}, {3, 0}}, result);
        }

        [Test]
        public void CalculateFeesPerConfirmationTimeTesteEmpty()
        {
            var block = new Block {Transactions = new List<Transaction>()};

            IDictionary<int, int> result = Bitcoin.CalculateFeesPerConfirmationTime(
                new List<Block> {block}, 3, 1);

            Assert.AreEqual(new Dictionary<int, int> {{1, 0}, {2, 0}, {3, 0}}, result);
        }
    }
}