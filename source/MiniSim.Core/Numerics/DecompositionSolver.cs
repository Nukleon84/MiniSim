using CSparse.Ordering;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Interfaces;
using MiniSim.Core.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    public class DecompositionSolver
    {
        AlgebraicSystem problemData;

        ILogger _logger;
        Flowsheet _flowsheet;

        bool _supressNewtonLogging = true;
        bool _doLinesearch = true;
        bool _showStatistics = true;

        double _newtonTolerance = 1e-6;
        int _newtonMaxIter = 70;
        double _minNewtonLambda = 0.2;
        private bool _activateInit = false;
        private bool _isInsufficientRank = false;
        private bool _isOverconstrained = false;
        int _minimumSubsystemSize = 1;



        public AlgebraicSystem ProblemData
        {
            get { return problemData; }
            set { problemData = value; }
        }

        public IList<AlgebraicSystem> Subproblems
        {
            get { return _decomposedNlps; }
            set { _decomposedNlps = value; }
        }

        public bool IsInsufficientRank
        {
            get { return _isInsufficientRank; }
            set { _isInsufficientRank = value; }
        }

        public bool IsOverconstrained
        {
            get { return _isOverconstrained; }
            set { _isOverconstrained = value; }
        }
        public bool ActivateInit
        {
            get { return _activateInit; }
            set { _activateInit = value; }
        }

        public int MinimumSubsystemSize
        {
            get
            {
                return _minimumSubsystemSize;
            }

            set
            {
                _minimumSubsystemSize = value;
            }
        }

        public bool SuppressNewtonLogging
        {
            get
            {
                return _supressNewtonLogging;
            }

            set
            {
                _supressNewtonLogging = value;
            }
        }

        public bool DoLinesearch
        {
            get
            {
                return _doLinesearch;
            }

            set
            {
                _doLinesearch = value;
            }
        }

        public double NewtonTolerance
        {
            get
            {
                return _newtonTolerance;
            }

            set
            {
                _newtonTolerance = value;
            }
        }

        public int NewtonMaxIter
        {
            get
            {
                return _newtonMaxIter;
            }

            set
            {
                _newtonMaxIter = value;
            }
        }

        public double MinNewtonLambda
        {
            get
            {
                return _minNewtonLambda;
            }

            set
            {
                _minNewtonLambda = value;
            }
        }

        public bool ShowStatistics { get => _showStatistics; set => _showStatistics = value; }

        void Log(string message)
        {

            _logger.Log(message);
        }


        private void LogDebug(string message)
        {
            _logger.Debug(message);

        }
        private void LogSuccess(string message)
        {

            _logger.Succcess(message);
        }
        private void LogInfo(string message)
        {

            _logger.Info(message);

        }
        private void LogWarning(string message)
        {

            _logger.Warning(message);
        }
        private void LogError(string message)
        {
            _logger.Error(message);
        }

        private IList<AlgebraicSystem> _decomposedNlps;


        public DecompositionSolver(ILogger logger)
        {
            _logger = logger;
        }
        public string GetDebugInfo()
        {
            StringBuilder sb = new StringBuilder();

            if (IsOverconstrained)
            {
                sb.AppendLine("Error: System has an overconstrained part. Consider removing one of the following constraints:");

                foreach (var eq in Subproblems.First().Equations)
                {
                    sb.AppendLine(String.Format("{1,-20} {2,-10} {3,-15} ( {0} )", eq, eq.ModelClass, eq.ModelName, eq.Group));
                }
            }

            if (IsInsufficientRank)
            {
                sb.AppendLine("Warning: System has an underspecified part. Consider fixing one of the following variables.");
                foreach (var variable in Subproblems.Last().Variables)
                    sb.AppendLine(String.Format("{0,-20}", variable.FullName));
            }

            return sb.ToString();
        }

        public void Decompose(AlgebraicSystem problem)
        {
            this.ProblemData = problem;
            _isInsufficientRank = false;
            _isOverconstrained = false;

            var A = CSparseWrapper.ConvertSparsityJacobian(ProblemData);
            var dm = DulmageMendelsohn.Generate(A, 1);

            LogInfo(String.Format("Decomposition Result: V={0}, E={1}, Blocks={2}, Singletons={3}", problem.NumberOfVariables, problem.NumberOfEquations, dm.Blocks, dm.Singletons));
            if (dm.StructuralRank != problem.NumberOfEquations)
                LogInfo("Structural Rank : " + dm.StructuralRank);
            if (dm.StructuralRank < problem.NumberOfEquations)
                IsInsufficientRank = true;

            _decomposedNlps = new List<AlgebraicSystem>();
            var currentSystem = new AlgebraicSystem("Subproblem 1");
            var lastVarCount = 0;

            for (int i = dm.Blocks - 1; i >= 0; i--)
            {
                var varcount = dm.BlockColumnPointers[i + 1] - dm.BlockColumnPointers[i];

                if (varcount > 1 || (varcount == 1 && lastVarCount > 1) || currentSystem.Variables.Count >= MinimumSubsystemSize)
                {
                    if (currentSystem.Variables.Count > 0 || currentSystem.Equations.Count > 0)
                        _decomposedNlps.Add(currentSystem);
                    currentSystem = new AlgebraicSystem("Subproblem " + (_decomposedNlps.Count + 1));
                }

                for (int j = 0; j < varcount; j++)
                {
                    var vari = dm.ColumnPermutation[dm.BlockColumnPointers[i] + j];
                    currentSystem.AddVariables(ProblemData.Variables[vari]);
                }

                var eqcount = dm.BlockRowPointers[i + 1] - dm.BlockRowPointers[i];
                for (int j = 0; j < eqcount; j++)
                {
                    var index = dm.RowPermutation[dm.BlockRowPointers[i] + j];
                    currentSystem.AddEquation(ProblemData.Equations[index]);
                }
                lastVarCount = varcount;

            }

            _decomposedNlps.Add(currentSystem);

            if (_decomposedNlps.First().NumberOfEquations != _decomposedNlps.First().Variables.Count)
                IsOverconstrained = true;

            if (_decomposedNlps.Last().NumberOfEquations != _decomposedNlps.Last().Variables.Count)
                IsInsufficientRank = true;

            if (IsInsufficientRank || IsOverconstrained)
                LogError(GetDebugInfo());

            foreach (var subproblem in _decomposedNlps.ToArray())
            {
                if (subproblem.Variables.Count == 0)
                    _decomposedNlps.Remove(subproblem);
            }
        }

        public bool Solve(Flowsheet flowsheet)
        {
            _flowsheet = flowsheet;

            var eq = new AlgebraicSystem("NLAES");
            _flowsheet.CreateEquations(eq);

            return Solve(eq);
        }


        public bool Solve(AlgebraicSystem problem)
        {
            if (Subproblems != null)
                Subproblems.Clear();

            this.ProblemData = problem;

            ProblemData.CreateIndex();
            ProblemData.GenerateJacobian();

            Decompose(problem);

            if (IsOverconstrained && IsInsufficientRank)
                return false;

            BasicNewtonSolver newtonSubsolver = null;

            if (SuppressNewtonLogging)
                newtonSubsolver = new BasicNewtonSolver(new NoLogger());
            else
                newtonSubsolver = new BasicNewtonSolver(_logger);

            newtonSubsolver.MaximumIterations = NewtonMaxIter;
            newtonSubsolver.Tolerance = NewtonTolerance;


            if (ShowStatistics)
            {
                var groupedBlocks = _decomposedNlps.GroupBy(s => s.NumberOfEquations).OrderBy(s => s.Key);
                var numBlocks = _decomposedNlps.Count;
                LogInfo("Block Statistics:");
                LogInfo(String.Format("{0,8} {1,8} {2,8}", "# Var", "# Blocks", "% Blocks"));
                foreach (var group in groupedBlocks)
                {
                    LogInfo(String.Format("{0,8} {1,8} {2,8}", group.Key, group.Count(), (group.Count() / (double)numBlocks).ToString("P2")));
                }


            }

            int i = 1;
            // LogInfo("Solving decomposed sub-problems...");      
            bool hasError = false;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var decomposedNlp in _decomposedNlps)
            {
                if (decomposedNlp.Variables.Count == 0)
                    continue;

                var statusmessage = "Solving problem " + i + " of " + _decomposedNlps.Count + " (Size: " + decomposedNlp.Variables.Count + ")";
                var status = newtonSubsolver.Solve(decomposedNlp);

                if (!status)
                {
                    // PublishStatus("Solving problem " + decomposedNlp.Name + " failed!");
                    hasError = true;
                    LogError("Solving problem " + decomposedNlp.Name + " (Size: " + decomposedNlp.Variables.Count + ") failed!");
                    LogError("The 10 most problematic constraints are:");
                    foreach (var eq in decomposedNlp.Equations.OrderByDescending(c => Math.Abs(c.Residual())).Take(Math.Min(10, decomposedNlp.Equations.Count)))
                    {
                        LogError(String.Format("{2,-20} {3,-10} {4,-15} {0,20} ( {1} )", eq.Residual().ToString("G8"), eq, eq.ModelClass, eq.ModelName, eq.Group));
                    }

                    if(decomposedNlp.Equations.Count==1)
                    {
                        LogError("Solved for =>" + decomposedNlp.Variables.First().WriteReport());
                        LogError("Other Variables:");
                        foreach (var vari in decomposedNlp.Equations.First().Variables)
                            LogError(vari.WriteReport());
                    }
                    LogError("");
                    break;
                }
                else
                {
                    //LogSuccess("Problem " + decomposedNlp.Name + " solved in " + subsolver.Iterations + " iterations.");                    
                }
                i++;
            }
            watch.Stop();

            if (!hasError)
            {
                LogSuccess("Problem " + problem.Name + " was successfully solved (" + watch.Elapsed.TotalSeconds.ToString("0.00") + " seconds)");
                return true;
            }
            else
            {
                LogError("Problem " + problem.Name + " was not successfully solved (Result = " + ")");
                return false;
            }
        }
    }

}
