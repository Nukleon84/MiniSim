using MiniSim.Core.Expressions;
using MiniSim.Core.Numerics;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public class BaseSimulationElement : FlowsheetNode
    {

        string _description;
        string _class;
        List<Variable> _variables = new List<Variable>();
        ThermodynamicSystem _system;

        public string Description { get => _description; set => _description = value; }
        public string Class { get => _class; set => _class = value; }
        public List<Variable> Variables { get => _variables; set => _variables = value; }
        public ThermodynamicSystem System { get => _system; protected set => _system = value; }




        public virtual void CreateEquations(AlgebraicSystem problem)
        {
            foreach (var vari in Variables)
            {
                if (vari.IsFixed)
                {
                    // AddEquationToEquationSystem(problem, vari - vari.Val(), "Fixed variable specification");
                }
                else if (vari.IsBound)
                {

                }
                else
                    problem.AddVariable(vari);
            }
        }

        public Variable GetVariable(string name)
        {
            return Variables.FirstOrDefault(v => v.FullName == name);
        }


        public BaseSimulationElement Specify(string variable, int value)
        {
            Specify(variable, value, null);
            return this;
        }
        public BaseSimulationElement Init(string variable, int value)
        {
            Init(variable, value, null);
            return this;
        }


        public BaseSimulationElement Specify(string variable, double value)
        {
            Specify(variable, value, null);
            return this;
        }
        public BaseSimulationElement Init(string variable, double value)
        {
            Init(variable, value, null);
            return this;
        }
        public BaseSimulationElement Init(string variable, int value, Unit unit)
        {
            return Init(variable, (double)value, unit);
        }

        public BaseSimulationElement Init(string variable, double value, Unit unit)
        {
            var vari = GetVariable(variable);
            if (vari != null)
            {
                if (unit != null)
                    vari.Init(value, unit);
                else
                    vari.Init(value);
            }
            else
                throw new InvalidOperationException("Unknown variable " + variable + " in object " + Name);

            return this;
        }


        public BaseSimulationElement Specify(string variable, int value, Unit unit)
        {
            return Specify(variable, (double)value, unit);
        }

        public BaseSimulationElement Specify(string variable, double value, Unit unit)
        {
            var vari = GetVariable(variable);
            if (vari != null)
            {
                if (unit != null)
                    vari.Fix(value, unit);
                else
                    vari.Fix(value);
            }
            else
                throw new InvalidOperationException("Unknown variable " + variable + " in object " + Name);

            return this;
        }

        public BaseSimulationElement Unspecify(string variable)
        {
            var vari = GetVariable(variable);
            if (vari != null)
            {
                vari.Unfix();
            }
            else
                throw new InvalidOperationException("Unknown variable " + variable + " in object " + Name);

            return this;
        }

        protected BaseSimulationElement AddVariables(params Variable[] variables)
        {
            foreach (var vari in variables)
            {
                if (!Variables.Contains(vari))
                {
                    vari.ModelClass = Class;
                    vari.ModelName = Name;
                    Variables.Add(vari);
                }
            }
            return this;
        }

        protected BaseSimulationElement AddVariables(IEnumerable<Variable> variables)
        {
            foreach (var vari in variables)
            {
                if (!Variables.Contains(vari))
                {
                    vari.ModelClass = Class;
                    vari.ModelName = Name;
                    Variables.Add(vari);
                }
            }
            return this;
        }



        protected BaseSimulationElement AddVariable(Variable vari)
        {
            if (!Variables.Contains(vari))
            {
                vari.ModelClass = Class;
                vari.ModelName = Name;
                Variables.Add(vari);
            }
            return this;
        }

        protected void AddEquationToEquationSystem(AlgebraicSystem system, Expression res, string group = "")
        {
            AddEquationToEquationSystem(system, new Equation(res), group);
        }

        protected void AddEquationToEquationSystem(AlgebraicSystem system, Equation eq, string group = "")
        {
            eq.ModelName = Name;
            eq.ModelClass = Class;
            eq.Group = group;

            if (String.IsNullOrEmpty(eq.Name))
                eq.Name = "EQ" + (system.Equations.Count + 1).ToString("000000");
            system.AddEquation(eq);
        }



    }
}
