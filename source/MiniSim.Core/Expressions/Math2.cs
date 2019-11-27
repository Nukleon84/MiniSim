using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Expressions
{
    public static class Math2
    {
        public static double Sec(double x)
        {
            return 1.0 / Math.Cos(x);
        }
        public static double Sech(double x)
        {
            return 2 / (Math.Exp(x) + Math.Exp(-x));
        }
        public static double Cosech(double x)
        {
            return 2 / (Math.Exp(x) - Math.Exp(-x));
        }
        public static double Coth(double x)
        {
            return (Math.Exp(x) + Math.Exp(-x)) / (Math.Exp(x) - Math.Exp(-x));            
        }
    }
}
