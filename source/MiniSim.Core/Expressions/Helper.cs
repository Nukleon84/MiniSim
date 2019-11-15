using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Expressions
{
    /// <summary>
    /// Helper class for constructing symbolic expressions quickly
    /// </summary>
    public static class Sym
    {

        public static Expression Convert(Variable x, Unit targetUnit)
        {
            var factor = Unit.GetConversionFactor(x.InternalUnit, targetUnit);

            var z = new Expression("Convert", () => factor * x.Val(), (vari) => factor * x.Diff(vari));
            
            z.AddChildren(x);
            
            return z;
        }


        public static Expression Sin(Expression x)
        {
            var z = new Expression("sin", () => Math.Sin(x.Val()), (vari) => Math.Cos(x.Val()) * x.Diff(vari));
            z.AddChildren(x);
            return z;
        }
        public static Expression Cos(Expression x)
        {
            var z = new Expression("cos", () => Math.Cos(x.Val()), (vari) => -Math.Sin(x.Val()) * x.Diff(vari));
            z.AddChildren(x);
            return z;
        }

        public static Expression Exp(Expression x)
        {
            var z = new Expression("exp", () => Math.Exp(x.Val()), (vari) => Math.Exp(x.Val()) * x.Diff(vari));
            z.AddChildren(x);
            return z;
        }

        public static Expression Ln(Expression x)
        {
            var z = new Expression("ln", () => Math.Log(x.Val()), (vari) => 1 / x.Val() * x.Diff(vari));
            z.AddChildren(x);
            return z;
        }
        public static Expression Sqrt(Expression x)
        {
            var z = new Expression("sqrt", () => Math.Sqrt(x.Val()), (vari) => 1.0 / (2.0 * x.Val()) * x.Diff(vari));
            z.AddChildren(x);
            return z;
        }

        //v * Math.Pow(u, v - 1) * dudx + Math.Pow(u, v) * Math.Log(u) * dvdx
        public static Expression Pow(Expression x, Expression e)
        {
            var z = new Expression("^", () => Math.Pow(x.Val(), e.Val()), (vari) => e.Val() * Math.Pow(x.Val(), e.Val() - 1) * x.Diff(vari) + Math.Pow(x.Val(), e.Val()) * Math.Log(e.Val()) * e.Diff(vari));
            z.AddChildren(x);
            z.AddChildren(e);            
            return z;
        }





        public static Expression Par(Expression x)
        {
            var z = new Expression("", () => x.Val(), (vari) => x.Diff(vari));            
            z.AddChildren(x);
            return z;
        }


        public static Expression Min(Expression x, Expression y)
        {
            var z = new Expression("min", () => Math.Min(x.Val(), y.Val()), (vari) => x.Diff(vari) + y.Diff(vari));
            z.AddChildren(x);
            z.AddChildren(y);            
            return z;
        }
        public static Expression Max(Expression x, Expression y)
        {
            var z = new Expression("max", () => Math.Max(x.Val(), y.Val()), (vari) => x.Diff(vari) + y.Diff(vari));            
            z.AddChildren(x);
            z.AddChildren(y);
            return z;
        }


        public static Expression Sum(List<Variable> array)
        {
            var sum = (Expression)array[0];
            for (var i = 1; i < array.Count; i++)
            {
                sum += array[i];
            }
            return Par(sum);
        }
        public static Expression Sum(Variable[] array)
        {
            var sum = (Expression)array[0];
            for (var i = 1; i < array.Length; i++)
            {
                sum += array[i];
            }
            return Par(sum);
        }

        public static Expression Sum(Expression[] array)
        {
            var sum = array[0];
            for (var i = 1; i < array.Length; i++)
            {
                sum += array[i];
            }
            return Par(sum);
        }

        public static Expression Sum(int start, int end, Func<int, Expression> mapping)
        {
            var sum = mapping(start);
            for (var i = start + 1; i < end; i++)
            {
                sum += mapping(i);
            }
            return Par(sum);
        }


    }
}
