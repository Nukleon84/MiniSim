using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Interfaces;
using MiniSim.Core.ModelLibrary;
using MiniSim.Core.Numerics;
using MiniSim.Core.PropertyDatabase;
using MiniSim.Core.Reporting;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;

namespace MiniSim.Core.Tests.Process_Units
{
    [TestClass]
    public class FlashTest
    {
        ThermodynamicSystem sys;
        ILogger logger;

        [TestInitialize]
        public void Setup()
        {
            var db = new ChemSepAdapter();

            var subst1 = db.FindComponent("Water").RenameID("H2O");
            var subst2 = db.FindComponent("Ethanol").RenameID("EtOH");
            var subst3 = db.FindComponent("Methanol").RenameID("MeOH");

            sys = new ThermodynamicSystem("sys", "Ideal")
                        .AddComponent(subst1)
                        .AddComponent(subst2)
                        .AddComponent(subst3);

            db.FillBIPs(sys);
            sys.VariableFactory.SetTemperatureLimits(273, 373);

            logger = new StringBuilderLogger();

        }

        [TestMethod]
        public void CanSolveFlashTwoPhase()
        {
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
            splt.Specify("VF", 0.5);
            splt.Initialize();

            var flowsheet = new Flowsheet("Test: Splitter");
            flowsheet.AddMaterialStreams(s01, s02, s03);
            flowsheet.AddUnits(splt);
            var status = solver.Solve(flowsheet);


            Assert.IsTrue(status);


        }

        [TestMethod]
        public void CanSolveFlashBubblePoint()
        {
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
            splt.Specify("VF", 0.001);
            splt.Initialize();

            var flowsheet = new Flowsheet("Test: Splitter");
            flowsheet.AddMaterialStreams(s01, s02, s03);
            flowsheet.AddUnits(splt);
            var status = solver.Solve(flowsheet);


            Assert.IsTrue(status);
        }

        [TestMethod]
        public void CanSolveFlashDewPoint()
        {
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
            splt.Specify("VF", 0.9999);
            splt.Initialize();

            var flowsheet = new Flowsheet("Test: Splitter");
            flowsheet.AddMaterialStreams(s01, s02, s03);
            flowsheet.AddUnits(splt);
            var status = solver.Solve(flowsheet);


            Assert.IsTrue(status);
        }


        [TestMethod]
        public void CanSolveFlashOnlyLiquidPhase()
        {
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
            splt.Specify("T", 25, METRIC.C);
            splt.Initialize();

            var flowsheet = new Flowsheet("Test: Splitter");
            flowsheet.AddMaterialStreams(s01, s02, s03);
            flowsheet.AddUnits(splt);
            var status = solver.Solve(flowsheet);


            Assert.IsTrue(status);
        }


    }
}
