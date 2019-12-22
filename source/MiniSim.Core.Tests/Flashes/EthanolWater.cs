using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Interfaces;
using MiniSim.Core.Numerics;
using MiniSim.Core.PropertyDatabase;
using MiniSim.Core.Reporting;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;

namespace MiniSim.Core.Tests.Flashes
{
    [TestClass]
    public class EthanolWater
    {
        StringBuilderLogger logger;

        ThermodynamicSystem sys;
        ThermodynamicSystem sys2;

        [TestInitialize]
        public void Setup()
        {
            var db = new ChemSepAdapter();

            var subst1 = db.FindComponent("Water").RenameID("H2O");
            var subst2 = db.FindComponent("Ethanol").RenameID("EtOH");

            sys = new ThermodynamicSystem("sys", "Ideal")
                        .AddComponent(subst1)
                        .AddComponent(subst2);
            db.FillBIPs(sys);
            sys.VariableFactory.SetTemperatureLimits(273, 373);


            sys2 = new ThermodynamicSystem("sys2", "NRTL")
                      .AddComponent(subst1)
                      .AddComponent(subst2);
            db.FillBIPs(sys2);
            sys2.VariableFactory.SetTemperatureLimits(273, 373);

            logger = new StringBuilderLogger();
        }
        [TestMethod]
        public void CanSolveEtOHWaterTwoPhaseIdeal_T()
        {
            var solver = new BasicNewtonSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("T", 87, METRIC.C);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 0.5);
            s01.Specify("x[EtOH]", 0.5);
            s01.InitializeFromMolarFractions();
            s01.FlashPT();

            var eq = new AlgebraicSystem("test");
            s01.CreateEquations(eq);

            var status = solver.Solve(eq);
            Assert.AreEqual(0.05829, s01.VaporFraction.Val(), 1e-5);
            Assert.IsTrue(status);


        }


        [TestMethod]
        public void CanSolveEtOHWaterTwoPhaseIdeal_VF()
        {

            var solver = new BasicNewtonSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("VF", 0.5);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 0.5);
            s01.Specify("x[EtOH]", 0.5);
            s01.InitializeFromMolarFractions();
            s01.FlashPZ();

            var eq = new AlgebraicSystem("test");
            s01.CreateEquations(eq);

            var status = solver.Solve(eq);

            Assert.IsTrue(status);
            Assert.AreEqual(88.843, s01.Temperature.DisplayValue, 1e-3);

        }


        [TestMethod]
        public void CanSolveEtOHWaterTwoPhaseNRTL_VF()
        {
            var solver = new DecompositionSolver(logger);

            var s01 = new MaterialStream("S01", sys2);
            s01.Specify("VF", 0.5);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 0.5);
            s01.Specify("x[EtOH]", 0.5);
            s01.InitializeFromMolarFractions();
            s01.FlashPZ();

            var eq = new AlgebraicSystem("test");
            s01.CreateEquations(eq);

            var status = solver.Solve(eq);
            var log = logger.Flush();

            Assert.IsTrue(status);
            Assert.AreEqual(80.184, s01.Temperature.DisplayValue, 1e-3);

        }



        [TestMethod]
        public void CanSolveEtOHWaterTwoPhaseIdeal_DP()
        {
            var solver = new DecompositionSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("VF", 0.999);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 0.5);
            s01.Specify("x[EtOH]", 0.5);
            s01.InitializeFromMolarFractions();
            s01.FlashPZ();

            var eq = new AlgebraicSystem("test");
            s01.CreateEquations(eq);

            var status = solver.Solve(eq);
            var log = logger.Flush();

            Assert.IsTrue(status);
            Assert.AreEqual(90.902, s01.Temperature.DisplayValue, 1e-3);

        }

        [TestMethod]
        public void CanSolveEtOHWaterTwoPhaseIdeal_BP()
        {
            var solver = new DecompositionSolver(logger);

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("VF", 0.001);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 0.5);
            s01.Specify("x[EtOH]", 0.5);
            s01.InitializeFromMolarFractions();
            s01.FlashPZ();

            var eq = new AlgebraicSystem("test");
            s01.CreateEquations(eq);

            var status = solver.Solve(eq);

            Assert.IsTrue(status);
            Assert.AreEqual(86.777, s01.Temperature.DisplayValue, 1e-3);

        }

        [TestMethod]
        public void CanSolveEtOHWaterTwoPhaseIdeal_Region()
        {            
            var solver = new DecompositionSolver(logger);

            var T = new List<double>();
            var VF = new List<double>();

            var s01 = new MaterialStream("S01", sys);
            s01.Specify("VF", 0.01);
            s01.Specify("P", 1, METRIC.bar);
            s01.Specify("n", 1, SI.kmol / SI.h);
            s01.Specify("x[H2O]", 0.5);
            s01.Specify("x[EtOH]", 0.5);
            s01.InitializeFromMolarFractions();
            s01.FlashPZ();

            int steps = 15;

            for (int i = 0; i < steps; i++)
            {
                var vfVal = 0.01 + i / ((double)steps - 1.0) * 0.98;
                s01.Specify("VF", vfVal);
           
                var eq = new AlgebraicSystem("VLE");
                s01.CreateEquations(eq);
                var status = solver.Solve(eq);
                var log = logger.Flush();
                Assert.IsTrue(status);
                T.Add(s01.Temperature.DisplayValue);
                VF.Add(s01.VaporFraction.DisplayValue);
            }


            Assert.AreEqual(86.812, T[0], 1e-3);
            Assert.AreEqual(87.086, T[1], 1e-3);
            Assert.AreEqual(87.367, T[2], 1e-3);
            Assert.AreEqual(87.654, T[3], 1e-3);
            Assert.AreEqual(87.947, T[4], 1e-3);
            Assert.AreEqual(88.244, T[5], 1e-3);
            Assert.AreEqual(88.543, T[6], 1e-3);
            Assert.AreEqual(88.843, T[7], 1e-3);
            Assert.AreEqual(89.144, T[8], 1e-3);
            Assert.AreEqual(89.442, T[9], 1e-3);
            Assert.AreEqual(89.738, T[10], 1e-3);
            Assert.AreEqual(90.030, T[11], 1e-3);
            Assert.AreEqual(90.315, T[12], 1e-3);
            Assert.AreEqual(90.595, T[13], 1e-3);
            Assert.AreEqual(90.867, T[14], 1e-3);

        }

    }
}
