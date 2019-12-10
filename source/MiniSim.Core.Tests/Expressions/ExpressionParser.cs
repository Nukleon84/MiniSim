using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniSim.Core.Expressions;

namespace MiniSim.Core.Tests.Expressions
{
    [TestClass]
    public class ExpressionParser
    {
        [TestMethod]
        public void CanParseAddition()
        {
            var x = new Variable("x", 1);
            var y = new Variable("y", 1);

            var z = x + y;

            Assert.AreEqual(2, z.Val());
            Assert.AreEqual(1, z.Diff(x));
            Assert.AreEqual(1, z.Diff(y));

            var parser = new SymbolicExpressionParser();
            parser.RegisterVariable(x);
            parser.RegisterVariable(y);

            var zp = parser.ParseExpression("x+y");

            Assert.AreEqual(2, zp.Val());
            Assert.AreEqual(1, zp.Diff(x));
            Assert.AreEqual(1, zp.Diff(y));
        }

        [TestMethod]
        public void CanParseSubtraction()
        {
            var x = new Variable("x", 1);
            var y = new Variable("y", 1);

            var z = x - y;

            Assert.AreEqual(0, z.Val());
            Assert.AreEqual(1, z.Diff(x));
            Assert.AreEqual(-1, z.Diff(y));

            var parser = new SymbolicExpressionParser();
            parser.RegisterVariable(x);
            parser.RegisterVariable(y);

            var zp = parser.ParseExpression("x-y");

            Assert.AreEqual(0, zp.Val());
            Assert.AreEqual(1, zp.Diff(x));
            Assert.AreEqual(-1, zp.Diff(y));
        }


        [TestMethod]
        public void CanParseMultiplication()
        {
            var x = new Variable("x", 3);
            var y = new Variable("y", 2);

            var z = x * y;

            Assert.AreEqual(6, z.Val());
            Assert.AreEqual(2, z.Diff(x));
            Assert.AreEqual(3, z.Diff(y));

            var parser = new SymbolicExpressionParser();
            parser.RegisterVariable(x);
            parser.RegisterVariable(y);

            var zp = parser.ParseExpression("x*y");

            Assert.AreEqual(6, zp.Val());
            Assert.AreEqual(2, zp.Diff(x));
            Assert.AreEqual(3, zp.Diff(y));
        }

        [TestMethod]
        public void CanParseDivision()
        {
            var x = new Variable("x", 1);
            var y = new Variable("y", 2);

            var z = x / y;

            z.GradientValue = 1.0f;
            Assert.AreEqual(0.5, z.Val());
            Assert.AreEqual(0.5, z.Diff(x));
            Assert.AreEqual(-0.25, z.Diff(y));

            var parser = new SymbolicExpressionParser();
            parser.RegisterVariable(x);
            parser.RegisterVariable(y);

            var zp = parser.ParseExpression("x/y");

            Assert.AreEqual(0.5, zp.Val());
            Assert.AreEqual(0.5, zp.Diff(x));
            Assert.AreEqual(-0.25, zp.Diff(y));
        }


      

        [TestMethod]
        public void CanParseExp()
        {
            var x = new Variable("x", 2);

            var z = Sym.Exp(x);
            
            Assert.AreEqual(Math.Exp(2), z.Val());            

            var parser = new SymbolicExpressionParser();
            parser.RegisterVariable(x);

            var zp = parser.ParseExpression("exp(x)");

            Assert.AreEqual(Math.Exp(2), zp.Val());
                       
        }

    }
}
