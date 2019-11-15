using MiniSim.Core.Expressions;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting
{
    public class Phase
    {
        private readonly ThermodynamicSystem _system;

        private string _name;
     
        private Variable _density;
        private Variable _densityMolar;
        private Variable _totalMolarVolume;
        private Variable _specificEnthalpy;
        private Variable _totalEnthalpy;
        private Variable _totalVolumeflow;
        private Variable _totalMolarflow;
        private Variable _totalMassflow;

        List<Variable> _componentMolarFraction = new List<Variable>();
        List<Variable> _componentMassFraction = new List<Variable>();

        List<Variable> _componentMolarflow = new List<Variable>();
        List<Variable> _componentMassflow = new List<Variable>();
        List<Variable> _componentMolarVolume = new List<Variable>();
        List<Variable> _componentEnthalpy = new List<Variable>();

        List<Variable> _variables = new List<Variable>();
        #region Properties
        public Variable TotalMassflow
        {
            get
            {
                return _totalMassflow;
            }

            set
            {
                _totalMassflow = value;
            }
        }

        public Variable TotalMolarflow
        {
            get
            {
                return _totalMolarflow;
            }

            set
            {
                _totalMolarflow = value;
            }
        }

        public Variable TotalVolumeflow
        {
            get
            {
                return _totalVolumeflow;
            }

            set
            {
                _totalVolumeflow = value;
            }
        }

        public Variable SpecificEnthalpy
        {
            get
            {
                return _specificEnthalpy;
            }

            set
            {
                _specificEnthalpy = value;
            }
        }

      

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public Variable TotalEnthalpy
        {
            get
            {
                return _totalEnthalpy;
            }

            set
            {
                _totalEnthalpy = value;
            }
        }

        public List<Variable> ComponentMolarFraction
        {
            get
            {
                return _componentMolarFraction;
            }

            set
            {
                _componentMolarFraction = value;
            }
        }

        public List<Variable> ComponentMassFraction
        {
            get
            {
                return _componentMassFraction;
            }

            set
            {
                _componentMassFraction = value;
            }
        }

        public List<Variable> ComponentMolarflow
        {
            get
            {
                return _componentMolarflow;
            }

            set
            {
                _componentMolarflow = value;
            }
        }

        public List<Variable> ComponentMassflow
        {
            get
            {
                return _componentMassflow;
            }

            set
            {
                _componentMassflow = value;
            }
        }

        public List<Variable> ComponentMolarVolume
        {
            get
            {
                return _componentMolarVolume;
            }

            set
            {
                _componentMolarVolume = value;
            }
        }

        public List<Variable> ComponentEnthalpy
        {
            get
            {
                return _componentEnthalpy;
            }

            set
            {
                _componentEnthalpy = value;
            }
        }

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

        public Variable TotalMolarVolume
        {
            get
            {
                return _totalMolarVolume;
            }

            set
            {
                _totalMolarVolume = value;
            }
        }

        public Variable Density
        {
            get
            {
                return _density;
            }

            set
            {
                _density = value;
            }
        }

        public Variable DensityMolar
        {
            get
            {
                return _densityMolar;
            }

            set
            {
                _densityMolar = value;
            }
        }
        #endregion

        public Phase(string symbol, ThermodynamicSystem system)
        {
            Name = symbol;
            _system = system;

             SpecificEnthalpy = _system.VariableFactory.CreateVariable("h" + symbol, "Molar specific enthalpy", PhysicalDimension.SpecificMolarEnthalpy);
            TotalEnthalpy = _system.VariableFactory.CreateVariable("H" + symbol, "Total enthalpy", PhysicalDimension.Enthalpy);
            TotalVolumeflow = _system.VariableFactory.CreateVariable("V" + symbol, "Volume flow", PhysicalDimension.VolumeFlow);
            TotalMolarflow = _system.VariableFactory.CreateVariable("n" + symbol, "Total molar flow", PhysicalDimension.MolarFlow);
            TotalMassflow = _system.VariableFactory.CreateVariable("m" + symbol, "Total mass flow", PhysicalDimension.MassFlow);
            Density = _system.VariableFactory.CreateVariable("rho" + symbol, "Density", PhysicalDimension.MassDensity);
            DensityMolar = _system.VariableFactory.CreateVariable("rhom" + symbol, "Density", PhysicalDimension.MolarDensity);
            TotalMolarVolume = _system.VariableFactory.CreateVariable("v" + symbol, "Total molar volume", PhysicalDimension.MolarVolume);
                                
            Variables.Add(SpecificEnthalpy);
            Variables.Add(Density);
            Variables.Add(DensityMolar);
            Variables.Add(TotalEnthalpy);
            Variables.Add(TotalVolumeflow);
            Variables.Add(TotalMolarflow);
            Variables.Add(TotalMassflow);

            foreach (var comp in system.Components)
            {
                ComponentMolarFraction.Add(_system.VariableFactory.CreateVariable("x" + symbol, comp.ID, "Molar Fraction of component " + comp.ID, PhysicalDimension.MolarFraction));
                ComponentMassFraction.Add(_system.VariableFactory.CreateVariable("w" + symbol, comp.ID, "Molar Fraction of component " + comp.ID, PhysicalDimension.MassFraction));
                ComponentMolarflow.Add(_system.VariableFactory.CreateVariable("n" + symbol, comp.ID, "Molar flow of component " + comp.ID, PhysicalDimension.MolarFlow));
                ComponentMassflow.Add(_system.VariableFactory.CreateVariable("m" + symbol, comp.ID, "Mass flow of component " + comp.ID, PhysicalDimension.MassFlow));
                ComponentMolarVolume.Add(_system.VariableFactory.CreateVariable("v" + symbol, comp.ID, "Molar volume of component " + comp.ID, PhysicalDimension.MolarVolume));
                ComponentEnthalpy.Add(_system.VariableFactory.CreateVariable("h" + symbol, comp.ID, "Specific Molar Enthalpy of component " + comp.ID, PhysicalDimension.SpecificMolarEnthalpy));

            }

            Variables.AddRange(ComponentMolarFraction);
            Variables.AddRange(ComponentMassFraction);
            Variables.AddRange(ComponentMolarflow);
            Variables.AddRange(ComponentMassflow);
            Variables.AddRange(ComponentMolarVolume);
            Variables.AddRange(ComponentEnthalpy);
        }
    }
}
