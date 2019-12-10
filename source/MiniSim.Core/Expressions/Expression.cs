using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Expressions
{
    [DebuggerDisplay("{Name} = {Value}")]
    public class Expression
    {
        protected double Value;
        public double GradientValue;
        protected Func<double> ValueFunc;
        protected Func<Variable, double> DiffFunc;
        protected Func<string> PrettyFunc;

        public List<Expression> Children;

        public string Name;

        public Expression(string name, Func<double> valueFunc, Func<Variable, double> diffFunc, Func<string> pretty) : this(name, valueFunc, diffFunc)
        {
            PrettyFunc = pretty;
        }

        public Expression(string name) : this(name, () => 0, (v) => 0)
        {
        }

        public Expression(string name, Func<double> valueFunc, Func<Variable, double> diffFunc)
        {
            Name = name;

            ValueFunc = valueFunc;
            DiffFunc = diffFunc;
            Value = Double.NaN;
            GradientValue = Double.NaN;

            Children = new List<Expression>();
        }

        public virtual string Pretty()
        {
            if (PrettyFunc != null)
                return PrettyFunc();
            else
            {
                if (Children.Count != 2)
                    return Name + (Children.Count > 0 ? "(" + String.Join(",", Children.Select(c => c.Pretty())) + ")" : "");
                else
                    return Children[0].Pretty() + " " + Name + " " + Children[1].Pretty();
            }
        }



        public override string ToString()
        {
            if (Children.Count != 2)
                return Name + (Children.Count > 0 ? "(" + String.Join(",", Children) + ")" : "");
            else
                return Children[0] + " " + Name + " " + Children[1];
        }


        public void SetValue(double value)
        {
            if (double.IsNaN(value))
            {
                throw new NotFiniteNumberException();
            }
            ValueFunc = () => value;
            Value = Double.NaN;

        }
        public double Val()
        {
            if (Double.IsNaN(Value))
            {
                Value = ValueFunc();
                if (Double.IsNaN(Value))
                {
                    Value = 0;
                }
            }

            return Value;
        }

        public double Diff(Variable var)
        {
            GradientValue = DiffFunc(var);

            return GradientValue;
        }


        public void Reset()
        {
            Value = Double.NaN;
            GradientValue = Double.NaN;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Reset();
            }

        }

        public void AddChildren(Expression z)
        {
            this.Children.Add(z);
        }

        public static Expression operator +(Expression u1, Expression u2)
        {
            var z = new Expression("+", () => u1.Val() + u2.Val(), (v) => u1.Diff(v) + u2.Diff(v));
            z.AddChildren(u1);
            z.AddChildren(u2);
            return z;
        }
        public static Expression operator -(Expression u1, Expression u2)
        {
            var z = new Expression("-", () => u1.Val() - u2.Val(), (v) => u1.Diff(v) - u2.Diff(v));
            z.AddChildren(u1);
            z.AddChildren(u2);
            return z;
        }

        public static Expression operator *(Expression u1, Expression u2)
        {
            var u1asVar = u1 as Variable;
            var u2asVar = u2 as Variable;

            var z = new Expression("*", () =>
            {
                var left = u1.Val();
                if (left != 0)
                    return left * u2.Val();
                else
                    return 0;
            },
            (vari) =>
            {
                return (Math.Abs(u1.Val()) > Double.Epsilon ? u1.Val() * u2.Diff(vari) : 0) + (Math.Abs(u2.Val()) > Double.Epsilon ? u2.Val() * u1.Diff(vari) : 0);
            }

            );
            z.AddChildren(u1);
            z.AddChildren(u2);
            return z;
        }

        public static Expression operator /(Expression u, Expression v)
        {
            var z = new Expression("/", () => u.Val() / v.Val(),
                (vari) =>
                {
                    return (v.Val() * u.Diff(vari) - u.Val() * v.Diff(vari)) / Math.Pow(v.Val(), 2);
                },
                () => "\\frac{" + u.Pretty() + "}{" + v.Pretty() + "}"
                );
            z.AddChildren(u);
            z.AddChildren(v);
            return z;
        }


        public static Expression operator -(Expression u)
        {
            var z = new Expression("-", () => -u.Val(), (vari) => -u.Diff(vari));
            z.AddChildren(u);
            return z;
        }


        public static implicit operator Expression(double d)
        {
            return new Expression(d.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), () => d, (vari) => 0);
        }

        public static implicit operator Expression(int d)
        {
            return new Expression(d.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), () => d, (vari) => 0);
        }
    }
}
