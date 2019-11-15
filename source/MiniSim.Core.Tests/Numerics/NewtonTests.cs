using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniSim.Core.Expressions;
using MiniSim.Core.Numerics;
using MiniSim.Core.Reporting;

namespace MiniSim.Core.Tests.Numerics
{
    [TestClass]
    public class NewtonTests
    {
        [TestMethod]
        public void DifficultFunction3Variables()
        {
            var problem = new AlgebraicSystem("Test 1: Difficult Function");
            var x1 = new Variable("x1", 1);
            var x2 = new Variable("x2", 1);
            var x3 = new Variable("x3", 1);
            problem.AddVariables(x1, x2, x3);
            problem.AddEquation(new Equation("eq1", (3 * x1 - Sym.Cos(x2 * x3) - 3.0 / 2.0)));
            problem.AddEquation(new Equation("eq2", 4 * Sym.Pow(x1, 2) - 625 * Sym.Pow(x2, 2) + 2 * x2 - 1));
            problem.AddEquation(new Equation("eq3", Sym.Exp(-x1 * x2) + 20 * x3 + (10 * Math.PI - 3.0) / 3.0));

            var logger = new ColoredConsoleLogger();            
            var solver = new BasicNewtonSolver(logger);
            solver.Solve(problem);

            Assert.AreEqual(0.833196581863439, x1.Val(), 1e-6);
            Assert.AreEqual(0.0549436583091183, x2.Val(), 1e-6);
            Assert.AreEqual(-0.521361434378159, x3.Val(), 1e-6);

        }
    }
}
