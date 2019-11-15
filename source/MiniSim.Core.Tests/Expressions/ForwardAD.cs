using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniSim.Core.Expressions;

namespace MiniSim.Core.Tests.Expressions
{
    [TestClass]
    public class ForwardAD
    {
        [TestClass]
        public class BasicTests
        {
            [TestMethod]
            public void CanAdd()
            {
                var x = new Variable("x", 1);
                var y = new Variable("y", 1);

                var z = x + y;

                z.GradientValue = 1.0f;
                Assert.AreEqual(2, z.Val());
                Assert.AreEqual(1, z.Diff(x));
                Assert.AreEqual(1, z.Diff(y));

            }

            [TestMethod]
            public void CanSubtract()
            {
                var x = new Variable("x", 1);
                var y = new Variable("y", 1);

                var z = x - y;

                z.GradientValue = 1.0f;
                Assert.AreEqual(0, z.Val());       
                Assert.AreEqual(1, z.Diff(x));
                Assert.AreEqual(-1, z.Diff(y));

            }

            [TestMethod]
            public void CanMultiply()
            {
                var x = new Variable("x", 2);
                var y = new Variable("y", 1);

                var z = x * y;

                z.GradientValue = 1.0f;
                Assert.AreEqual(2, z.Val());     

                Assert.AreEqual(1, z.Diff(x));
                Assert.AreEqual(2, z.Diff(y));



            }


            [TestMethod]
            public void CanDivide()
            {
                var x = new Variable("x", 1);
                var y = new Variable("y", 2);

                var z = x / y;

                z.GradientValue = 1.0f;
                Assert.AreEqual(0.5, z.Val()); 

                Assert.AreEqual(0.5, z.Diff(x));
                Assert.AreEqual(-0.25, z.Diff(y));


            }


            [TestMethod]
            public void CanSubtractPrecendence()
            {
                var x = new Variable("x", 2);
                var y1 = new Variable("y", 1);
                var y2 = new Variable("y2", 1);

                var z = x - Sym.Par(y1 + y2);

                z.GradientValue = 1.0f;
                Assert.AreEqual(0, z.Val());
       

                Assert.AreEqual(1, z.Diff(x));
                Assert.AreEqual(-1, z.Diff(y1));
                Assert.AreEqual(-1, z.Diff(y2));


            }



        }
    }
}
