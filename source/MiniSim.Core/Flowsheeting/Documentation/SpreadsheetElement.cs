using MiniSim.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting.Documentation
{
    public class SpreadsheetElement : DocumentationElement
    {
        List<Variable> _variables = new List<Variable>();

        public List<Variable> Variables
        {
            get
            {
                return _variables;
            }

            set
            {
                _variables = value;
            }
        }

        public SpreadsheetElement(string name)
        {
            Name = name;
            Icon.IconType = IconTypes.Spreadsheet;
            SetColors("Silver", "GhostWhite");
        }

        public SpreadsheetElement AddVariables(params Variable[] variables)
        {
            foreach (var variable in variables)
                AddVariable(variable);
            return this;
        }

        public SpreadsheetElement RemoveVariable(Variable variable)
        {
            if (Variables.Contains(variable))
                Variables.Remove(variable);
            else
                throw new InvalidOperationException("Variable " + variable.Name + " not included in spreadsheet");
            return this;

        }
        public SpreadsheetElement AddVariable(Variable variable)
        {
            if (!Variables.Contains(variable))
                Variables.Add(variable);
            else
                throw new InvalidOperationException("Variable " + variable.Name + " already included in spreadsheet");
            return this;
        }


    }
}
