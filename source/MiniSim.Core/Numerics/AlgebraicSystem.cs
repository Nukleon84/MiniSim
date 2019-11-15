using MiniSim.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    public class AlgebraicSystem
    {
        string _name;
        List<Variable> _variables;
        List<Equation> _equations;

        public List<Variable> Variables { get => _variables; set => _variables = value; }
        public List<Equation> Equations { get => _equations; set => _equations = value; }
        public string Name { get => _name; set => _name = value; }

        List<JacobianElement> _jacobian = new List<JacobianElement>();
        private Dictionary<Variable, int> variableIndex = new Dictionary<Variable, int>();



        public int NumberOfVariables
        {
            get { return Variables.Count; }
        }
        public int NumberOfEquations
        {
            get { return Equations.Count; }
        }
        public List<JacobianElement> Jacobian
        {
            get
            {
                return _jacobian;
            }

            set
            {
                _jacobian = value;
            }
        }

        public Dictionary<Variable, int> VariableIndex { get => variableIndex;  }

        public AlgebraicSystem(string name)
        {
            Name = name;
            Variables = new List<Variable>();
            Equations = new List<Equation>();
        }

        public AlgebraicSystem AddVariable(Variable variable)
        {
            if (!Variables.Contains(variable))
                Variables.Add(variable);
            else
                throw new ArgumentException("Variable named " + variable.Name + " already included in equation system " + Name);

            return this;
        }

        public AlgebraicSystem AddVariables(params Variable[] variables)
        {
            foreach (var variable in variables)
            {
                if (!Variables.Contains(variable))
                    Variables.Add(variable);
                else
                    throw new ArgumentException("Variable named " + variable.Name + " already included in equation system " + Name);
            }
            return this;
        }


        public AlgebraicSystem AddEquation(Equation equation)
        {
            if (!Equations.Contains(equation))
                Equations.Add(equation);
            else
                throw new ArgumentException("Equation named " + equation.Name + " already included in equation system " + Name);

            return this;
        }



        public void CreateIndex()
        {
            VariableIndex.Clear();
            for (int i = 0; i < Variables.Count; i++)
            {
                VariableIndex.Add(Variables[i], i);
            }
        }

        public virtual void GenerateJacobian()
        {
            int i = 0;
            Jacobian.Clear();
            foreach (var equation in Equations)
            {
                foreach (var variable in equation.Variables)
                {
                    int j = -1;
                        if (VariableIndex.TryGetValue(variable, out j))
                        {
                            Jacobian.Add(new JacobianElement() { EquationIndex = i, VariableIndex = j, Value = 1.0 });
                        }
                    
                }
                i++;
            }
        }


    }
}
