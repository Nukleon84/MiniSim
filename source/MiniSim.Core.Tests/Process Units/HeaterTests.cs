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
    public class HeaterTests
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
            sys.VariableFactory.SetTemperatureLimits(273, 573);

            logger = new ColoredConsoleLogger();
        }
        [TestMethod]
        public void CanTestSubCooledToSubCooledHeater()
        {          
            var solver = new BasicNewtonSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("T", 25, METRIC.C);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 1);
            s01.Specify("x[EtOH]", 0);
            s01.Specify("x[MeOH]", 0);          
            s01.InitializeFromMolarFractions();
            s01.FlashPT();

            var s02 = new MaterialStream("S02", sys);

            var unit = new Heater("Mix", sys);
            unit.Connect("In", s01);
            unit.Connect("Out", s02);
            unit.Specify("DP", 0, METRIC.mbar);
            unit.Specify("T", 50, METRIC.C);
            unit.Initialize();


            var flowsheet = new Flowsheet("Test: Heater");
            flowsheet.AddMaterialStreams(s01, s02);
            flowsheet.AddUnits(unit);

            var status=solver.Solve(flowsheet);

            Assert.IsTrue(status);
        }

        [TestMethod]
        public void CanTestSubCooledToTwoPhaseHeater()
        {
            var solver = new BasicNewtonSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("T", 25, METRIC.C);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n[H2O]", 1, SI.kmol / SI.h);
            s01.Specify("n[EtOH]", 1, SI.kmol / SI.h);
            s01.Specify("n[MeOH]", 0, SI.kmol / SI.h);
            s01.InitializeFromMolarFlows();
            s01.FlashPT();

            var s02 = new MaterialStream("S02", sys);

            var unit = new Heater("Mix", sys);
            unit.Connect("In", s01);
            unit.Connect("Out", s02);
            unit.Specify("DP", 0, METRIC.mbar);
            unit.Specify("VF", 0.5);
            unit.Initialize();


            var flowsheet = new Flowsheet("Test: Heater");
            flowsheet.AddMaterialStreams(s01, s02);
            flowsheet.AddUnits(unit);

            var status = solver.Solve(flowsheet);

            Assert.IsTrue(status);
            Assert.AreEqual(PhaseState.LiquidVapor, s02.State);
        }
        [TestMethod]
        public void CanTestSubCooledToSuperheatedHeater()
        {
            var solver = new DecompositionSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("T", 25, METRIC.C);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n[H2O]", 1, SI.kmol / SI.h);
            s01.Specify("n[EtOH]", 1, SI.kmol / SI.h);
            s01.Specify("n[MeOH]", 0, SI.kmol / SI.h);
            s01.InitializeFromMolarFlows();
            s01.FlashPT();

            var s02 = new MaterialStream("S02", sys);

            var unit = new Heater("Mix", sys);
            unit.Connect("In", s01);
            unit.Connect("Out", s02);
            unit.Specify("DP", 0, METRIC.mbar);
            unit.Specify("T", 120, METRIC.C);
            unit.Initialize();


            var flowsheet = new Flowsheet("Test: Heater");
            flowsheet.AddMaterialStreams(s01, s02);
            flowsheet.AddUnits(unit);

            var status = solver.Solve(flowsheet);

            Assert.IsTrue(status);
        }


        [TestMethod]
        public void CanTestCondenserHeater()
        {
            var solver = new BasicNewtonSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("VF", 1);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n[H2O]", 1, SI.kmol / SI.h);
            s01.Specify("n[EtOH]", 1, SI.kmol / SI.h);
            s01.Specify("n[MeOH]", 0, SI.kmol / SI.h);
            s01.InitializeFromMolarFlows();
            s01.FlashPZ();

            var s02 = new MaterialStream("S02", sys);

            var unit = new Heater("Mix", sys);
            unit.Connect("In", s01);
            unit.Connect("Out", s02);
            unit.Specify("DP", 0, METRIC.mbar);
            unit.Specify("VF", 0);
            unit.Initialize();


            var flowsheet = new Flowsheet("Test: Heater (Condenser)");
            flowsheet.AddMaterialStreams(s01, s02);
            flowsheet.AddUnits(unit);

            var status = solver.Solve(flowsheet);

            Assert.IsTrue(status);
        }

    }
}
