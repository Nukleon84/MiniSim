using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Expressions
{
    public class Equation
    {
        List<Variable> _variables;
        public Expression Expression;
        public string Name;
        string _description;
        string _modelName;
        string _modelClass;
        string _group;
             

        public string ModelName { get => _modelName; set => _modelName = value; }
        public string ModelClass { get => _modelClass; set => _modelClass = value; }
        public string Group { get => _group; set => _group = value; }
        public string Description { get => _description; set => _description = value; }

        public IReadOnlyCollection<Variable> Variables
        {
            get { return _variables; }
        }

    
        public Equation( Expression residual):this("", residual)
        {
        }



        public Equation(string name, Expression residual)
        {
            Name = name;
            Expression = residual;
            Reset();

            _variables = new List<Variable>();
            FillVariables(Expression);
        }

        void FillVariables(Expression exp)
        {
            var variable = exp as Variable;
            if (variable != null && !_variables.Contains(variable) && !variable.IsConstant)
                _variables.Add(variable);

            foreach (var parent in exp.Children)
                FillVariables(parent);
        }


        public double Residual()
        {
            return Expression.Val();
        }


        public string Pretty()
        {
            return "  $" + Expression.Pretty()+" = 0$";
        }


        public override string ToString()
        {
            return Name + " >> 0 := " + Expression +" = "+Residual()+"";
        }
       
        public void Reset()
        {
            Expression.Reset();           
         
        }
        
    }
}
