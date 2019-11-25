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

            var sys = new ThermodynamicSystem("sys", "NRTL","Default")
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
            s01.Specify("T", 40,METRIC.C);
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
        static void Main(string[] args)
        {
            //FlashVLE();
           // Console.WriteLine("\n\n\n");
            FlashLLE();
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();

        }
    }
}
