using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinSim
{
    public class Transaction
    {
        public int Fees { get; set; }
        public UseCase UseCase { get; set; }

        // When added to mempool
        public int AddedInMempoolTick { get; set; }
        public int AddedInMemPoolHeight { get; set; }

        // When in block
        public Block ConfirmedInBlock { get; set; }

        // When removed from mempool
        public int RemoveFromMempool { get; set; }


        public int ConfirmationTime {
            get { return ConfirmedInBlock.Height - AddedInMemPoolHeight; }
        }
    }
}
