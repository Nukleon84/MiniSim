using MiniSim.Core.Expressions;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics
{
    public class VariableFactory
    {
        UnitSet _internal = UnitSet.CreateSI();
        UnitSet _input = UnitSet.CreateSI();
        UnitSet _output = UnitSet.CreateSI();

        double _temperatureLowerBound = 1;
        double _temperatureUpperBound = 1000;

        double _pressureLowerBound = 100;
        double _pressureUpperBound = 1e9;
        public void SetTemperatureLimits(double lower, double upper)
        {
            _temperatureLowerBound = lower;
            _temperatureUpperBound = upper;
        }

        public void SetPressureLimits(double lower, double upper)
        {
            _pressureLowerBound = lower;
            _pressureUpperBound = upper;
        }

        public UnitSet Input
        {
            get
            {
                return _input;
            }

            set
            {
                _input = value;
            }
        }

        public UnitSet Output
        {
            get
            {
                return _output;
            }

            set
            {
                _output = value;
            }
        }

        public UnitSet Internal
        {
            get
            {
                return _internal;
            }            
        }

        public void SetInputDimensions(UnitSet set)
        {
            _input = set;
        }
        public void SetOutputDimensions(UnitSet set)
        {
            _output = set;
        }

        public void SetInputDimension(PhysicalDimension type, Unit unit)
        {
            SetDimension(_input, type, unit);

        }
        public void SetOutputDimension(PhysicalDimension type, Unit unit)
        {
            SetDimension(_output, type, unit);

        }
        void SetDimension(UnitSet set, PhysicalDimension type, Unit unit)
        {
            set.UnitDictionary[type] = unit;
        }
        public Variable CreateVariable(string name, string description, PhysicalDimension type)
        {
            return CreateVariable(name, "", description, type);
        }

        public Variable CreateVariable(string name, string subscript, string description, PhysicalDimension type)
        {
            var variable = new Variable(name, 0, _internal.UnitDictionary[type]);                        
            variable.DisplayUnit = _output.UnitDictionary[type];
            variable.Dimension = type;
            variable.Description = description;
            variable.Subscript = subscript;

            switch (type)
            {
                case PhysicalDimension.Temperature:
                    variable.SetValue(273.15);
                    variable.LowerBound = _temperatureLowerBound;
                    variable.UpperBound = _temperatureUpperBound;
                    break;
                case PhysicalDimension.Pressure:
                    variable.SetValue(1e5);
                    variable.LowerBound = _pressureLowerBound;
                    variable.UpperBound = _pressureUpperBound;
                    break;
                case PhysicalDimension.MassDensity:
                case PhysicalDimension.MolarDensity:
                case PhysicalDimension.MolarFlow:
                case PhysicalDimension.MassFlow:
                    variable.LowerBound = 0;
                    variable.UpperBound = 1e9;
                    break;
                case PhysicalDimension.Area:
                case PhysicalDimension.Volume:
                    variable.LowerBound = 0;
                    variable.UpperBound = 1e20;
                    break;
                case PhysicalDimension.VolumeFlow:
                    variable.LowerBound = 0;
                    variable.UpperBound = 1e6;
                    break;
                case PhysicalDimension.MolarVolume:
                    variable.LowerBound = 0;
                    variable.UpperBound = 1e6;
                    break;

                case PhysicalDimension.HeatTransferCoefficient:
                    variable.LowerBound = 1e-6;
                    variable.UpperBound = 1e6;
                    break;


                case PhysicalDimension.MolarFraction:
                case PhysicalDimension.MassFraction:
                    variable.LowerBound =0;
                    variable.UpperBound = 1;
                    break;
            }

            return variable;
        }
    }
}
