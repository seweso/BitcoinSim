using System.Collections.Generic;
using System.Windows;

namespace BitcoinSim
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var bitcoin = new Bitcoin {Dificulty = 10};

            // Create miners (total hashpower should be 1)
            bitcoin.ReplaceMiners(new List<Miner>
            {
                new Miner {Name = "A", HashingPower = 0.1, SoftLimit = 10000, Subsidize = false, TransactionCost = 1},
                new Miner {Name = "B", HashingPower = 0.2, SoftLimit = 10000, Subsidize = true, TransactionCost = 5},
                new Miner {Name = "C", HashingPower = 0.4, SoftLimit = 7500, Subsidize = false, TransactionCost = 4},
                new Miner {Name = "D", HashingPower = 0.2, SoftLimit = 2500, Subsidize = false, TransactionCost = 6},
                new Miner {Name = "E", HashingPower = 0.1, SoftLimit = 10000, Subsidize = true, TransactionCost = 20},
            });


            // Create initial use cases
            bitcoin.AddUseCases(new List<UseCase>
            {
                new UseCase { TransactionsPerTick = 0.12, ConfirmationSpeedNeeded = 99, MaxFees = 0},
                new UseCase { TransactionsPerTick = 0.06, ConfirmationSpeedNeeded = 99, MaxFees = 1},
                new UseCase { TransactionsPerTick = 0.06, ConfirmationSpeedNeeded = 99, MaxFees = 2},
                new UseCase { TransactionsPerTick = 0.06, ConfirmationSpeedNeeded = 99, MaxFees = 4},
                new UseCase { TransactionsPerTick = 0.06, ConfirmationSpeedNeeded = 99, MaxFees = 5},
                new UseCase { TransactionsPerTick = 0.05, ConfirmationSpeedNeeded = 6, MaxFees = 10},
                new UseCase { TransactionsPerTick = 0.04, ConfirmationSpeedNeeded = 3, MaxFees = 20}, 
                new UseCase { TransactionsPerTick = 0.03, ConfirmationSpeedNeeded = 2, MaxFees = 500}, 
                new UseCase { TransactionsPerTick = 0.02, ConfirmationSpeedNeeded = 1, MaxFees = 1000}, 
                new UseCase { TransactionsPerTick = 0.01, ConfirmationSpeedNeeded = 1, MaxFees = 2000},
            });

            bitcoin.Tick();

            while (bitcoin.Dificulty < 120000)
                bitcoin.Tick();

            bitcoin.Tick();
        }
    }
}