using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiniSim.Core.Expressions;

namespace MiniSim.Core.Numerics
{
    public class BisectionSolver
    {
        public bool Solve(Expression func, Variable x, double a, double b, int maxIterations = 10, double tolerance = 1e-6)
        {
            double x1 = a;
            double x2 = b;
            double mid = 0.5 * (x1 + x2);
            double x0 = x.Val();
            
            //Calculate function values at the borders
            x.SetValue(x1);
            func.Reset();
            double fa = func.Val();
                        
            x.SetValue(x2);
            func.Reset();
            double fb = func.Val();
            
            //Only perform Bisection method if root is bracketed in the interval
            if (Math.Sign(fa) == Math.Sign(fb))
            {
                x.SetValue(x0);
                return false;
            }

            var i = 0;
            for (i = 0; i <= maxIterations; i++) // perform maxIterations iterations
            {
                mid = 0.5 * (x1 + x2);
                //Console.WriteLine($"x{i} = {mid}");
                x.SetValue(mid);
                func.Reset();
                if (fb * func.Val() > 0)
                    x2 = mid;
                else
                    x1 = mid;

                var delta = x2 - x1;
                if (Math.Abs(delta) < tolerance)
                {

                    x.SetValue(x1);
                    func.Reset();
                    var fx1 = func.Val();

                    x.SetValue(x2);
                    func.Reset();
                    var fx2 = func.Val();

                    //Console.WriteLine($"x1 = {x1}");
                    //Console.WriteLine($"x2 = {x2}");
                    //Console.WriteLine($"fx1 = {fx1}");
                    //Console.WriteLine($"fx2 = {fx2}");
                  //  Console.WriteLine($"x = {x.Val()}");
                    if (Math.Abs(fx2-fx1) > 0)
                    {
                        x.SetValue(x2 - (x2 - x1) * fx2 / (fx2 - fx1));
                    }                   

                    break;
                }
            }

            if (i == maxIterations)
                return false;

            return true;
        }


    }
}
