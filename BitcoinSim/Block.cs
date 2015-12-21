using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinSim
{
    public class Block
    {
        public Miner Origin { get; set; }

        public IReadOnlyList<Transaction> Transactions { get; set;  } 

        // When block added
        public int Height { get; set; }
        public int MinedAtTick { get; set; }

    }
}
