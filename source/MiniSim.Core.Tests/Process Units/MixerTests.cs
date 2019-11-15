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
    public class MixerTests
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

            logger = new ColoredConsoleLogger();

        }


        [TestMethod]
        public void CanSolveSubcooledMixer()
        {
            var solver = new BasicNewtonSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("T", 25, METRIC.C);
            s01.Specify("P", 2, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 0.5);
            s01.Specify("x[EtOH]", 0.5);
            s01.Specify("x[MeOH]", 0);
            s01.InitializeFromMolarFractions();
            s01.FlashPT();

            var s02 = new MaterialStream("S02", sys);
            s02.Specify("T", 35, METRIC.C);
            s02.Specify("P", 2, METRIC.bar);
            s02.Specify("n", 1, SI.kmol / SI.h);
            s02.Specify("x[H2O]", 0);
            s02.Specify("x[EtOH]", 0);
            s02.Specify("x[MeOH]", 1);
            s02.InitializeFromMolarFractions();
            s02.FlashPT();

            var s03 = new MaterialStream("S03", sys);

            var mixer = new Mixer("Mix", sys);
            mixer.Connect("In", s01);
            mixer.Connect("In", s02);
            mixer.Connect("Out", s03);
            mixer.Specify("DP", 1, METRIC.mbar);
            mixer.Initialize();

            var flowsheet = new Flowsheet("Test: Mixer");
            flowsheet.AddMaterialStreams(s01, s02, s03);
            flowsheet.AddUnits(mixer);
            var status = solver.Solve(flowsheet);

            Assert.IsTrue(status);
        }
    }
}
