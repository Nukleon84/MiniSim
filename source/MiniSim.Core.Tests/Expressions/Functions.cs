using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniSim.Core.Expressions;

namespace MiniSim.Core.Tests.Expressions
{
    [TestClass]
    public class Functions
    {
        [TestMethod]
        public void CanSqrt()
        {
            var x = new Variable("x",  4);            

            var z = Sym.Sqrt(x);

            z.GradientValue = 1.0f;
            Assert.AreEqual(2, z.Val());
            Assert.AreEqual(1.0/(4.0*Math.Sqrt(4)), z.Diff(x), 1e-6);
            

        }
    }
}
