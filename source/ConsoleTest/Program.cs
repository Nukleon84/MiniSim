using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.ModelLibrary;
using MiniSim.Core.Numerics;
using MiniSim.Core.PropertyDatabase;
using MiniSim.Core.Reporting;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;

namespace ConsoleTest
{
    class Program
    {
        static void FlashVLE()
        {
            var db = new ChemSepAdapter();

            var subst1 = db.FindComponent("Water").RenameID("Wasser").SetL2Split(0.9);
            var subst2 = db.FindComponent("Phenol").RenameID("Phenol").SetL2Split(0.1);

            var sys = new ThermodynamicSystem("sys", "NRTL", "Default")
                        .AddComponent(subst1)
                        .AddComponent(subst2);

            db.FillBIPs(sys);

            sys.VariableFactory.SetOutputDimensions(UnitSet.CreateDefault());
            // sys.EquilibriumMethod.AllowedPhases = AllowedPhases.VLLE;

            var logger = new ColoredConsoleLogger();
            var reporter = new Generator(logger);
            // reporter.Report(sys);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("T", 25, METRIC.C);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[Wasser]", 0.5);
            s01.Specify("x[Phenol]", 0.5);
            s01.InitializeFromMolarFractions();
            s01.FlashPT();

            reporter.Report(s01);

            var eq = new AlgebraicSystem("test");
            s01.CreateEquations(eq);
            var solver = new DecompositionSolver(logger);
            //var solver = new BasicNewtonSolver(logger);

            solver.Solve(eq);

            reporter.Report(s01);
        }


        static void FlashLLE()
        {
            var db = new ChemSepAdapter();

            var subst1 = db.FindComponent("Water").RenameID("Wasser").SetL2Split(0.05);
            var subst2 = db.FindComponent("1-Hexanol").RenameID("Hexanol").SetL2Split(0.95);

            var sys = new ThermodynamicSystem("sys", "NRTL", "Default")
                        .AddComponent(subst1)
                        .AddComponent(subst2);

            db.SetLogCallback(Console.Write);
            db.FillBIPs(sys);

            // sys.VariableFactory.SetOutputDimensions(UnitSet.CreateDefault());
            sys.EquilibriumMethod.AllowedPhases = AllowedPhases.VLLE;

            var logger = new ColoredConsoleLogger();
            var reporter = new Generator(logger);
            // reporter.Report(sys);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("T", 40, METRIC.C);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[Wasser]", 0.5);
            s01.Init("x[Hexanol]", 0.6);
            s01.InitializeFromMolarFractions();
            s01.FlashPT();

            reporter.Report(s01);

            var eq = new AlgebraicSystem("test");
            s01.CreateEquations(eq);
            var solver = new DecompositionSolver(logger);
            //var solver = new BasicNewtonSolver(logger);

            solver.Solve(eq);

            reporter.Report(s01);
        }

        static void TestIKCAPE()
        {
            var Database = new IKCapeAdapter();

            var content = System.IO.File.ReadAllText("btx.dat");

            var sys = Database.LoadNeutralFile(content);

        }

        static void CanSolveFlashBubblePoint()
        {
            var db = new ChemSepAdapter();

            var subst1 = db.FindComponent("Water").RenameID("H2O");
            var subst2 = db.FindComponent("Ethanol").RenameID("EtOH");
            var subst3 = db.FindComponent("Methanol").RenameID("MeOH");

            var sys = new ThermodynamicSystem("sys", "Ideal")
                        .AddComponent(subst1)
                        .AddComponent(subst2)
                        .AddComponent(subst3);

            db.FillBIPs(sys);
            sys.VariableFactory.SetTemperatureLimits(273, 373);

            var logger = new ColoredConsoleLogger();

            var solver = new DecompositionSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("VF", 0.01);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 1);
            s01.Specify("x[EtOH]", 1);
            s01.Specify("x[MeOH]", 0);
            s01.InitializeFromMolarFractions();
            s01.FlashPZ();

            var s02 = new MaterialStream("S02", sys);
            var s03 = new MaterialStream("S03", sys);

            var splt = new Flash("Flash", sys);
            splt.Connect("In", s01);
            splt.Connect("Vap", s02);
            splt.Connect("Liq", s03);
            splt.Specify("P", 1, METRIC.bar);
            splt.Specify("VF", 0);
            splt.Initialize();

            var flowsheet = new Flowsheet("Test: Splitter");
            flowsheet.AddMaterialStreams(s01, s02, s03);
            flowsheet.AddUnits(splt);
            var status = solver.Solve(flowsheet);


           
        }

        static void Main(string[] args)
        {
            CanSolveFlashBubblePoint();
            //FlashVLE();
            // Console.WriteLine("\n\n\n");
            //FlashLLE();
            TestIKCAPE();
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();

        }
    }
}
