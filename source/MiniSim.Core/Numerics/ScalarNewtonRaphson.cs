using MiniSim.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    public class ScalarNewtonRaphson
    {
        public static bool Solve(Expression func, Variable x, int maxIterations = 10, double tolerance=1e-6)
        {
            var i = 0;
            for (i = 0; i <= maxIterations; i++) // perform maxIterations iterations
            {
                func.Reset();
                func.GradientValue = 1.0;
                
                // perform differentiation
                var fx = func.Val();
                var dfx = func.Diff(x);

                // extract function value + derivative (the first element of the gradient)
                
                // newton-raphson iteration: x <- x - f(x) / f'(x)
                var delta= - fx / dfx;

                //Console.WriteLine("Iter : {0,3} {1,-20} {2,-20}", i, x.Val(), delta);

                x.AddDelta(delta);

                if (Math.Abs(delta) < tolerance)
                {
                    //Console.WriteLine("Problem converged.");
                    break;
                }
            }

            if (i == maxIterations)
                return false;
            //    Console.WriteLine("Maximum number of iterations exceeded.");

            return true;
        }

    }
}
