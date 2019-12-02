
using CSparse.Storage;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    public class BasicNewtonSolver : ISolver
    {
        double _tolerance = 1e-6;
        int _maximumIterations = 20;
        int _currentIterations = -1;
        double _brakeFactor = 1;
        bool _enableScaling = true;
        private readonly ILogger _logger;

        public int MaximumIterations { get => _maximumIterations; set => _maximumIterations = value; }
        public bool EnableScaling { get => _enableScaling; set => _enableScaling = value; }
        public double BrakeFactor { get => _brakeFactor; set => _brakeFactor = value; }
        public double Tolerance { get => _tolerance; set => _tolerance = value; }
        public int CurrentIterations { get => _currentIterations; set => _currentIterations = value; }

        Flowsheet _flowsheet;

        public BasicNewtonSolver(ILogger logger)
        {
            _logger = logger;
        }

        public bool Solve(Flowsheet flowsheet)
        {
            _flowsheet = flowsheet;

            var eq = new AlgebraicSystem("NLAES");
            _flowsheet.CreateEquations(eq);

            return Solve(eq);

        }

        public bool Solve(AlgebraicSystem system)
        {
            if (system.NumberOfEquations != system.NumberOfVariables)
            {
                _logger.Error("The BasicNewton-Solver can only solve square problems. E = " + system.NumberOfEquations + " , V = " + system.NumberOfVariables);
                return false;
            }
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var scalingLogSum = new MatrixScalingLogSum(2);
            double[] U = null;
            double[] V = null;
            var delta = new Vector(system.NumberOfVariables, 0);
            var b = new Vector(system.NumberOfEquations, 0);           
            bool status = false;
            string error = null;
            double lambda = 1.0;
            system.CreateIndex();
            system.GenerateJacobian();

            _logger.Info(String.Format("{0,-4} {1,-15} {2,-15} {3,-7} {4}", "Iter", "Step Length", "Infeasibility", "Damping", "Notes"));
           
            for (CurrentIterations = 0; CurrentIterations <= MaximumIterations; CurrentIterations++)
            {
                //Evaluate residual and Jacobian
                
                var A = CSparseWrapper.FillResidualAndJacobian(system, ref b);               

                //Termination checked versus max of unscaled residuals AND 2-norm of change in variables
                var variableNorm = delta.GetNorm();
                var equationNorm = b.ToDouble().Select(e => Math.Abs(e)).Max();
                              
                if (CurrentIterations == 0)
                    variableNorm = 0;

                _logger.Log(String.Format("{0,-4} {1,-15} {2,-15} {3,-7} {4}", CurrentIterations, variableNorm.ToString("G6"), equationNorm.ToString("G6"), lambda, error));

                if (equationNorm < Tolerance)
                {
                    _logger.Succcess("Problem " + system.Name + " was solved. Constraint violation is below tolerance (" + CurrentIterations + " iter, " + watch.Elapsed.TotalMilliseconds.ToString("0.00") + " ms, NV = " + system.Variables.Count + ", NZ = " + system.Jacobian.Count + ", NZ% = " + (system.Jacobian.Count / (double)(system.NumberOfVariables * system.NumberOfEquations) * 100).ToString(".00") + "%)");
                    return true;
                }
                if (equationNorm > 1e16)
                {
                    _logger.Error("!!! Problem diverged!");
                    return false;
                }

                if (EnableScaling)
                {
                    if (CurrentIterations % 5 == 0)
                        scalingLogSum.GetMatrixScalingFactors(A, out U, out V);

                    for (int i = 0; i < system.NumberOfEquations; i++)
                    {
                        b[i] = b[i] * U[i];
                    }
                    var UM = CSparseWrapper.CreateDiagonal(system.NumberOfEquations, U);
                    var VM = CSparseWrapper.CreateDiagonal(system.NumberOfEquations, V);
                    A = UM.Multiply(A);
                    A = A.Multiply(VM);
                }

                delta = CSparseWrapper.SolveLinearSystem(A, delta, -b, out status, out error);
                //Check for Steepest Descent Step
                if (status == false)
                {
                    error += " | SD";
                    A.Multiply((-b).ToDouble(), delta.ToDouble());
                    delta *= 0.01;
                }
                
                if (EnableScaling)
                {
                    for (int i = 0; i < delta.Size; i++)
                    {
                        delta[i] = delta[i] * V[i];
                    }
                }
                
                lambda = 1.0;
                var distanceToBound = 1.0;
                Variable closestVar = null;
                double closestStep = 0;
                //Check for possible bounds violation and reduce delta to avoid crossing bounds.
                for (int i = 0; i < delta.Size; i++)
                {
                    var distanceToBoundVari = 1.0;
                    var vari = system.Variables[i];
                    var step = delta[i];
                    if (step > 0 && vari.Val() < vari.UpperBound - 1e-3)                   
                        distanceToBoundVari = vari.UpperBound - vari.Val();                        

                    if (step < 0 && vari.Val() > vari.LowerBound + 1e-3)                    
                        distanceToBoundVari = vari.Val() - vari.LowerBound;                       
                    
                    var boundFraction = distanceToBoundVari / Math.Abs(step);
                    if (boundFraction < distanceToBound)
                    {
                        distanceToBound = boundFraction;
                        closestVar = vari;
                        closestStep = step;
                    }
                }
                lambda = distanceToBound*BrakeFactor;
                lambda = Math.Max(0.001, lambda);
               
                //Apply Newton Step
                for (int i = 0; i < delta.Size; i++)
                {
                    //Check for possible bounds violation and reduce delta to avoid crossing bounds.
                    if (!Double.IsNaN(delta[i]))
                    {
                        var vari = system.Variables[i];
                        var step = lambda*delta[i];
                        vari.AddDelta(step);
                    }                    
                }
            }         

            _logger.Warning("Maximum number of iterations exceeded!");

            return false;
        }
    }
}
