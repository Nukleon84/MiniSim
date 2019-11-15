using MiniSim.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    /*
     * Sample taken from: https://github.com/alexshtf/autodiff/blob/master/AutoDiff/Samples/GradientDescentSample/Program.cs
     * 
       
     */
    public class ScalarGradientDescent
    {
        public static double Solve(Expression func, List<Variable> x, double stepSize, int maxIterations, double tolerance = 1e-6)
        {
            // clone the initial argument

            double f = Double.MaxValue;
            double f0 = Double.MaxValue;
            var gradient = new double[x.Count];
            int i = 0;
            // perform the iterations
            for (i = 0; i < maxIterations; ++i)
            {
                func.Reset();
                func.GradientValue = 1.0;

                // compute the gradient - fill the gradient array
                //func.Differentiate(x, gradient);
                f = func.Val();
                // perform a descent step
                for (int j = 0; j < x.Count; ++j)
                {
                    gradient[j] = x[j].Diff(x[j]);
                    x[j].SetValue(x[j].Val() - stepSize * gradient[j]);
                }

                Console.WriteLine("Iter : {0,3} {1,-20} {2,-20}", i, f, String.Join("|", gradient));

                if (Math.Abs(f0 - f) < tolerance)
                {
                    Console.WriteLine("Problem converged.");
                    break;
                }
                f0 = f;
            }

            if (i == maxIterations)
                Console.WriteLine("Maximum number of iterations exceeded.");


            return f;
        }

    }
}
