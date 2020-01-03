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
            s01.Specify("x[Wasser]", 1.0);
            s01.Specify("x[Phenol]", 0.0);
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


        static void TestNeuralNet()
        {
            var a = new Variable("a", 1);
            var b = new Variable("b", 1);
            var z = new Variable("z", 1);

            var net = new NeuralNet("NN1", 2, new int[] { 1 }, 1);

            net.BindInput(0, a)
                .BindInput(1, b)
                .BindOutput(0, z);

            var logger = new ColoredConsoleLogger();

            var solver = new DecompositionSolver(logger);
            var flowsheet = new Flowsheet("Test: Neural Net");
            var reporter = new Generator(logger);
            flowsheet.AddCustomVariable(z);
            flowsheet.AddUnits(net);
            var status = solver.Solve(flowsheet);

            foreach (var sys in solver.Subproblems)
            {
                Console.WriteLine($"Solve equation {sys.Equations.First()} for variable {sys.Variables.First()}");
            }

            Console.WriteLine($"a={a.Val()}");
            Console.WriteLine($"b={b.Val()}");
            Console.WriteLine($"z={z.Val()}");
            reporter.Report(net);
            Console.WriteLine();

            a.Fix(2);

            status = solver.Solve(flowsheet);
            foreach (var sys in solver.Subproblems)
            {
                Console.WriteLine($"Solve equation {sys.Equations.First()} for variable {sys.Variables.First()}");
            }

            Console.WriteLine($"a={a.Val()}");
            Console.WriteLine($"b={b.Val()}");
            Console.WriteLine($"z={z.Val()}");
            reporter.Report(net);

            // flowsheet.CustomVariables.Remove(z);

            Console.WriteLine();
            a.Fix(-100);
            b.Fix(-1000);
            //z.Fix(0.9);
            // a.Unfix();
            // flowsheet.AddCustomVariable(a);
            // flowsheet.CustomVariables.Remove(z);
            status = solver.Solve(flowsheet);
            foreach (var sys in solver.Subproblems)
            {
                Console.WriteLine($"Solve equation {sys.Equations.First()} for variable {sys.Variables.First()}");
            }
            Console.WriteLine($"a={a.Val()}");
            Console.WriteLine($"b={b.Val()}");
            Console.WriteLine($"z={z.Val()}");
            reporter.Report(net);
        }


        static void TestColumnInit()
        {
            var db = new ChemSepAdapter();

            var subst1 = db.FindComponent("Water").RenameID("H2O");
            var subst2 = db.FindComponent("Methanol").RenameID("MeOH");

            var sys = new ThermodynamicSystem("sys", "Ideal")
                        .AddComponent(subst1)
                        .AddComponent(subst2);

            db.FillBIPs(sys);
            sys.VariableFactory.SetTemperatureLimits(273, 373);

            var logger = new ColoredConsoleLogger();
            var solver = new DecompositionSolver(logger);
            var reporter = new Generator(logger);

            var FEED = new MaterialStream("FEED", sys);
            FEED.Specify("VF", 0);
            FEED.Specify("P", 1, METRIC.bar);
            FEED.Specify("n", 1, SI.kmol / SI.h);
            FEED.Specify("x[H2O]", 0.5);
            FEED.Specify("x[MeOH]", 0.5);
            FEED.InitializeFromMolarFractions();
            FEED.FlashPZ();

            var VOUT = new MaterialStream("VOUT", sys);
            var LOUT = new MaterialStream("LOUT", sys);

            var LIN = new MaterialStream("LIN", sys);

            LIN.Specify("VF", 0)
                    .Specify("P", 1, METRIC.bar)
                    .Specify("n", 1, SI.kmol / SI.h)
                    .Specify("x[H2O]", 0.01)
                    .Specify("x[MeOH]", 0.99);
            LIN.InitializeFromMolarFractions()
                     .FlashPZ();

            var VIN = new MaterialStream("VIN", sys);
            VIN.Specify("VF", 1.0)
                     .Specify("P", 1, METRIC.bar)
                     .Specify("n", 1, SI.kmol / SI.h)
                     .Specify("x[H2O]", 0.90)
                     .Specify("x[MeOH]", 0.1);
            VIN.InitializeFromMolarFractions()
                     .FlashPZ();

            var COL = new EquilibriumStageSection("COL", sys, 10);
            COL.Connect("VIn", VIN)
                        .Connect("LIn", LIN)
                        .Connect("VOut", VOUT)
                        .Connect("LOut", LOUT);
            COL.ConnectFeed(FEED, 5)
                        .MakeAdiabatic()
                        .MakeIsobaric()
                        .FixStageEfficiency(1.0)
                        .Initialize();

            var flowsheet = new Flowsheet("Test: Splitter");
            flowsheet.AddMaterialStreams(FEED, LIN, VIN, VOUT, LOUT);
            flowsheet.AddUnits(COL);
            reporter.Report(COL,false);

            var status = solver.Solve(flowsheet);
            reporter.Report(COL,false);
        }

        static void TestBisection()
        {
            var x = new Variable("x", 1);
            var f = x * x * x - 5 * x + 3;
            var solver = new BisectionSolver();
            var status = solver.Solve(f, x, 1.0, 2.0, 20, 1e-4);
        }

        static void Main(string[] args)
        {
            //  CanSolveFlashBubblePoint();
            // FlashVLE();
            // Console.WriteLine("\n\n\n");
            //FlashLLE();

            // TestIKCAPE();

            //TestNeuralNet();
            //TestBisection();
            TestColumnInit();
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();

        }
    }
}
