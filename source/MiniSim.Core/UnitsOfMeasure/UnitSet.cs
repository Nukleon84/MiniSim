using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.UnitsOfMeasure
{
    public class UnitSet
    {
        string _name;
        string _description;
        Dictionary<PhysicalDimension, Unit> _unitDictionary;

        public UnitSet()
        {
            UnitDictionary = new Dictionary<PhysicalDimension, Unit>();
        }
        
        /// <summary>
        /// Dictionary used for storing the mapping between physical dimenions and a concrete unit of measure
        /// </summary>
        public Dictionary<PhysicalDimension, Unit> UnitDictionary
        {
            get
            {
                return _unitDictionary;
            }

            set
            {
                _unitDictionary = value;
            }
        }

        /// <summary>
        /// Name of the unit set
        /// </summary>
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
        /// <summary>
        /// Description of the unit set
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                _description = value;
            }
        }

        /// <summary>
        /// Create a new instance of the basic SI unit set
        /// </summary>
        /// <returns>An instance of UnitSet initialized with SI units</returns>
        public static UnitSet CreateSI()
        {
            var set = new UnitSet();
            set.Name = "SI";
            set.Description = "Unit set according to the SI";
            set.UnitDictionary.Add(PhysicalDimension.Dimensionless, SI.none);
            set.UnitDictionary.Add(PhysicalDimension.Temperature, SI.K);
            set.UnitDictionary.Add(PhysicalDimension.Pressure, SI.Pa);

            set.UnitDictionary.Add(PhysicalDimension.Mass, SI.kg);
            set.UnitDictionary.Add(PhysicalDimension.Mole, SI.mol);


            set.UnitDictionary.Add(PhysicalDimension.Length, SI.m);
            set.UnitDictionary.Add(PhysicalDimension.Area, SI.sqm);
            set.UnitDictionary.Add(PhysicalDimension.Volume, SI.cum);

            set.UnitDictionary.Add(PhysicalDimension.MassFlow, SI.kg / SI.s);
            set.UnitDictionary.Add(PhysicalDimension.MolarFlow, SI.mol / SI.s);
            set.UnitDictionary.Add(PhysicalDimension.HeatFlow, SI.J / SI.s);
            set.UnitDictionary.Add(PhysicalDimension.Enthalpy, SI.J / SI.s);
            set.UnitDictionary.Add(PhysicalDimension.VolumeFlow, SI.cum / SI.s);


            set.UnitDictionary.Add(PhysicalDimension.MolarWeight, SI.kg / SI.mol);

            set.UnitDictionary.Add(PhysicalDimension.MolarVolume,  SI.cum/SI.mol);
            set.UnitDictionary.Add(PhysicalDimension.MassDensity, SI.kg / SI.cum);
            set.UnitDictionary.Add(PhysicalDimension.MolarDensity, SI.mol / SI.cum);

            set.UnitDictionary.Add(PhysicalDimension.SpecificMolarEnthalpy, SI.J / SI.mol);
            set.UnitDictionary.Add(PhysicalDimension.SpecificMassEnthalpy, SI.J / SI.kg);
            set.UnitDictionary.Add(PhysicalDimension.HeatCapacity, SI.J / SI.mol / SI.K);

            set.UnitDictionary.Add(PhysicalDimension.HeatTransferCoefficient, SI.W / SI.sqm / SI.K);
            set.UnitDictionary.Add(PhysicalDimension.MassTransferCoefficient, SI.m / SI.s);

            set.UnitDictionary.Add(PhysicalDimension.MolarFraction, SI.mol / SI.mol);
            set.UnitDictionary.Add(PhysicalDimension.MassFraction, SI.kg / SI.kg);
            set.UnitDictionary.Add(PhysicalDimension.SpecificArea, SI.sqm / SI.cum);

            set.UnitDictionary.Add(PhysicalDimension.DynamicViscosity, SI.Pa * SI.s);
            set.UnitDictionary.Add(PhysicalDimension.Velocity, SI.m / SI.s);

            return set;
        }

        public static UnitSet CreateDefault()
        {
            var set = new UnitSet();
            set.Name = "Default";
            set.Description = "Default unit set according to the common european engineering practice";
            set.UnitDictionary.Add(PhysicalDimension.Dimensionless, SI.none);
            set.UnitDictionary.Add(PhysicalDimension.Temperature, METRIC.C);
            set.UnitDictionary.Add(PhysicalDimension.Pressure, METRIC.mbar);

            set.UnitDictionary.Add(PhysicalDimension.Mass, SI.kg);
            set.UnitDictionary.Add(PhysicalDimension.Mole, SI.mol);


            set.UnitDictionary.Add(PhysicalDimension.Length, SI.m);
            set.UnitDictionary.Add(PhysicalDimension.Area, SI.sqm);
            set.UnitDictionary.Add(PhysicalDimension.Volume, SI.cum);

            set.UnitDictionary.Add(PhysicalDimension.MassFlow, SI.kg / SI.h);
            set.UnitDictionary.Add(PhysicalDimension.MolarFlow, SI.kmol / SI.h);
            set.UnitDictionary.Add(PhysicalDimension.Enthalpy, SI.kW);
            set.UnitDictionary.Add(PhysicalDimension.HeatFlow, SI.kW);
            set.UnitDictionary.Add(PhysicalDimension.VolumeFlow, SI.cum / SI.h);

            set.UnitDictionary.Add(PhysicalDimension.MolarWeight, SI.kg / SI.kmol);
            set.UnitDictionary.Add(PhysicalDimension.MolarVolume, SI.cum / SI.kmol);
            set.UnitDictionary.Add(PhysicalDimension.MassDensity, SI.kg / SI.cum);
            set.UnitDictionary.Add(PhysicalDimension.MolarDensity, SI.kmol / SI.cum);

            set.UnitDictionary.Add(PhysicalDimension.SpecificMolarEnthalpy, SI.kJ / SI.kmol);
            set.UnitDictionary.Add(PhysicalDimension.SpecificMassEnthalpy, SI.kJ / SI.kg);
            set.UnitDictionary.Add(PhysicalDimension.HeatCapacity, SI.kJ / SI.kmol / SI.K);

            set.UnitDictionary.Add(PhysicalDimension.HeatTransferCoefficient, SI.W / SI.sqm / SI.K);
            set.UnitDictionary.Add(PhysicalDimension.MassTransferCoefficient, SI.m / SI.s);

            set.UnitDictionary.Add(PhysicalDimension.MolarFraction, SI.mol / SI.mol);
            set.UnitDictionary.Add(PhysicalDimension.MassFraction, SI.kg / SI.kg);
            //set.UnitDictionary.Add(PhysicalDimension.MolarFraction, METRIC.molpercent);
            //set.UnitDictionary.Add(PhysicalDimension.MassFraction, METRIC.weightpercent);

            set.UnitDictionary.Add(PhysicalDimension.SpecificArea, SI.sqm / SI.cum);

            set.UnitDictionary.Add(PhysicalDimension.DynamicViscosity, SI.Pa *SI.s);
            set.UnitDictionary.Add(PhysicalDimension.Velocity, SI.m / SI.s);
            return set;
        }

    }
}
