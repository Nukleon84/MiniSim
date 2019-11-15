using MiniSim.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Numerics
{
    public class JacobianElement
    {
        int _equationIndex = -1;
        int _variableIndex = -1;
        double _value = Double.NaN;

        public int EquationIndex
        {
            get
            {
                return _equationIndex;
            }

            set
            {
                _equationIndex = value;
            }
        }

        public int VariableIndex
        {
            get
            {
                return _variableIndex;
            }

            set
            {
                _variableIndex = value;
            }
        }

        public double Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }
    }

    public class HessianElement
    {
        int _equationIndex = -1;
        int _variable1Index = -1;
        int _variable2Index = -1;
        Expression _expression;
        double _value = Double.NaN;
        int _structuralIndex;
        public int EquationIndex
        {
            get
            {
                return _equationIndex;
            }

            set
            {
                _equationIndex = value;
            }
        }

        public int Variable1Index
        {
            get
            {
                return _variable1Index;
            }

            set
            {
                _variable1Index = value;
            }
        }

        public double Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        public Expression Expression
        {
            get
            {
                return _expression;
            }

            set
            {
                _expression = value;
            }
        }

        public int Variable2Index
        {
            get
            {
                return _variable2Index;
            }

            set
            {
                _variable2Index = value;
            }
        }

        public int StructuralIndex
        {
            get
            {
                return _structuralIndex;
            }

            set
            {
                _structuralIndex = value;
            }
        }

    }

    public class HessianStructureEntry
    {
        int _var1;
        int _var2;
       

        public int Var1
        {
            get { return _var1; }
            set { _var1 = value; }
        }

        public int Var2
        {
            get { return _var2; }
            set { _var2 = value; }
        }

       
    }
}
