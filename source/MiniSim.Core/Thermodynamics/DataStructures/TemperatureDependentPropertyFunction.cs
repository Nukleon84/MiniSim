using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.UnitsOfMeasure;
using MiniSim.Core.Expressions;

namespace MiniSim.Core.Thermodynamics
{
    public class TemperatureDependentPropertyFunction
    {
        EvaluatedProperties _property;
        UnitsOfMeasure.Unit _xUnit;
        UnitsOfMeasure.Unit _yUnit;
        FunctionType _type;
        List<Variable> _coefficients = new List<Variable>();
        Variable _xReference;

        Variable _lowerBound;
        Variable _upperBound;


        public int NumberOfCoefficients
        {
            get { return Coefficients.Count; }
        }

        public Unit XUnit
        {
            get
            {
                return _xUnit;
            }

            set
            {
                _xUnit = value;
            }
        }

        public Unit YUnit
        {
            get
            {
                return _yUnit;
            }

            set
            {
                _yUnit = value;
            }
        }

        public FunctionType Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        public EvaluatedProperties Property
        {
            get
            {
                return _property;
            }

            set
            {
                _property = value;
            }
        }

        public List<Variable> Coefficients
        {
            get
            {
                return _coefficients;
            }

            set
            {
                _coefficients = value;
            }
        }


        public Variable ReferenceX
        {
            get
            {
                return _xReference;
            }

            set
            {
                _xReference = value;
            }
        }

        public Variable MinimumX
        {
            get
            {
                return _lowerBound;
            }

            set
            {
                _lowerBound = value;
            }
        }

        public Variable MaximumX
        {
            get
            {
                return _upperBound;
            }

            set
            {
                _upperBound = value;
            }
        }

        
    }
}
