using System;
using System.Linq;

namespace MiniSim.Core.UnitsOfMeasure
{
    /// <summary>
    ///     The Base class for units of measurement. Provides helper classes for dimensional analysis
    /// </summary>
   
    public class Unit
    {
        #region Fields

        private string _name;
        private string _symbol;
        private double[] _dimensions;
        private double _factor = 1;
        private double _offset=0;       
        #endregion

        #region Properties

        /// <summary>
        ///     Name of the unit of measurement, e.g. Kelvin
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        ///     Short symbol of the unit of measurement, e.g. K
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        /// <summary>
        ///     Array describing the dimensions of the unit of measurement.
        ///     The 8 basic SI dimensions are { "L", "NumberOfEquations", "t", "I", "T", "NumberOfVariables", "J", "$" };
        ///     Length, Mass, time, Current, Temperature, Moles (Number), Energy, Currency
        ///     All other units are derived by dimensional analysis.
        ///     The molar flow unit is defined by the array {0, 0, -1, 0, 0, 1, 0, 0} = mol/s
        /// </summary>
        public double[] Dimensions
        {
            get { return _dimensions; }
            set { _dimensions = value; }
        }

        /// <summary>
        ///     The linear scaling factor of the affine transformation for a value in a base-unit into a derived unit
        ///     The affine transform for Celsius = 1*[Kelvin] - 273.15, with Factor =1.
        /// </summary>
        public double Factor
        {
            get { return _factor; }
            set { _factor = value; }
        }

        /// <summary>
        ///     The constant offset of the affine transformation for a value in a base-unit into a derived unit
        ///     The affine transform for Celsius = 1*[Kelvin] - 273.15, with Offset =-273.15.
        /// </summary>
        public double Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }
        

        #endregion

        #region Operator Overloading

        public static Unit operator *(Unit u1, Unit u2)
        {
            var newDimensions = new double[8];
            for (var i = 0; i < 8; i++)
            {
                newDimensions[i] = u1.Dimensions[i] + u2.Dimensions[i];
            }

            Unit newUnit;

            if (u1.PrintDimensions() == "")
                newUnit = new Unit(u1.Symbol + "" + u2.Symbol, "Undefined", newDimensions);
            else
                newUnit = new Unit(u1.Symbol + "*" + u2.Symbol, "Undefined", newDimensions);
           
            newUnit.Factor = u1.Factor*u2.Factor;
            newUnit.Offset = u1.Offset + u2.Offset;
            return newUnit;
        }

        public static Unit operator /(Unit u1, Unit u2)
        {
            var newDimensions = new double[8];
            for (var i = 0; i < 8; i++)
            {
                newDimensions[i] = u1.Dimensions[i] - u2.Dimensions[i];
            }
            var newUnit = new Unit(u1.Symbol + "/" + u2.Symbol, "Undefined", newDimensions)
            {
             
                Factor = u1.Factor/u2.Factor,
                Offset = u1.Offset - u2.Offset
            };
            return newUnit;
        }

        public static Unit operator ^(Unit u1, double power)
        {
            var newDimensions = new double[8];
            for (var i = 0; i < 8; i++)
            {
                newDimensions[i] = u1.Dimensions[i]*power;
            }
            var newUnit = new Unit(u1.Symbol + "^" + power, "Undefined", newDimensions);
            return newUnit;
        }

        #endregion

        #region Helper

        public static bool AreEquivalent(Unit u1, Unit u2)
        {
            var firstLevel = AreSameDimension(u1, u2);

            if (firstLevel)
            {
                var secondLevel =   u1.Factor == u2.Factor && u1.Offset == u2.Offset;
                return secondLevel;
            }
            return false;
        }

        public static bool AreSameDimension(Unit u1, Unit u2)
        {
            return u1.Dimensions.SequenceEqual(u2.Dimensions) || u1 == SI.none || u2 == SI.none;
        }

        public static double Convert(Unit source, Unit destination, double value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");

            //if (!Enumerable.SequenceEqual(source.Dimensions, destination.Dimensions))
            //   throw new Exception("Source and destination units do not match dimensionally.");

            var baseValue = source.Factor*value + source.Offset;
            return (baseValue - destination.Offset)/destination.Factor;
            
        }

        public static double GetConversionFactor(Unit source, Unit destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");

            //if (!Enumerable.SequenceEqual(source.Dimensions, destination.Dimensions))
            //   throw new Exception("Source and destination units do not match dimensionally.");

            // var baseValue = source.Factor * value + source.Offset;
            return source.Factor/destination.Factor;
            
        }

        public string PrintDimensions()
        {
            string[] dimensionSymbols = {"L", "M", "t", "I", "T", "N", "J", "$"};
            var dimensionString = "";
            for (var i = 0; i < 8; i++)
            {
                //Comparison for double values is ok, as the values are directly assigned from code and assume exact values as given by the literals
                if (Dimensions[i] != 0)
                {
                    if (Dimensions[i] != 1)
                        dimensionString += dimensionSymbols[i] + "^" + Dimensions[i];
                    else
                        dimensionString += dimensionSymbols[i];

                    if (i < 7)
                        dimensionString += " ";
                }
            }
            return dimensionString.TrimEnd(' ');
        }

        public override string ToString()
        {
            return Symbol;//PrintBaseUnits();
        }

        public string PrintBaseUnits()
        {
            string[] baseUnits = {"m", "kg", "s", "A", "K", "mol", "cd", "$"};
            var dimensionString = "";
            var dimensionStringDenom = "";

            for (var i = 0; i < 8; i++)
            {
                if (Dimensions[i] != 0)
                {
                    if (Dimensions[i] > 0)
                    {
                        if (Dimensions[i] != 1)
                            dimensionString += baseUnits[i] + "^" + Dimensions[i];
                        else
                            dimensionString += baseUnits[i];
                        if (i < 7)
                            dimensionString += " ";
                    }

                    if (Dimensions[i] < 0)
                    {
                        if (Dimensions[i] != 1)
                            dimensionStringDenom += baseUnits[i] + "^" + Math.Abs(Dimensions[i]);
                        else
                            dimensionStringDenom += baseUnits[i];

                        if (i < 7)
                            dimensionStringDenom += " ";
                    }
                }
            }

            if (dimensionStringDenom != "")
                return dimensionString.TrimEnd(' ') + " / (" + dimensionStringDenom.TrimEnd(' ') + ")";
            return dimensionString.TrimEnd(' ');
        }

        #endregion

        #region Constructors

        public Unit()
        {
            Dimensions = new double[8];

        }
        public Unit(string symbol, string name, double factor)
        {
            Symbol = symbol;
            Name = name;
            Factor = factor;
            Dimensions = new double[8];

        }


        public Unit(string symbol, string name, double[] dimensions)
        {
            Symbol = symbol;
            Name = name;
            Dimensions = dimensions;

            if (Dimensions == null)
                Dimensions = new double[8];

            if (Dimensions.Length != 8)
                throw new ArgumentException("Argument _dimensions must be an array with 8 elements");
        }

        public Unit(string symbol, string name, Unit baseUnit)
        {
            Symbol = symbol;
            Name = name;
            Dimensions = baseUnit.Dimensions;
            Factor = baseUnit.Factor;
            Offset = baseUnit.Offset;
        }

        public Unit(string symbol, string name, Unit baseUnit, double factor, double offset)
        {
            Symbol = symbol;
            Name = name;
            Dimensions = baseUnit.Dimensions;
            Factor = baseUnit.Factor*factor;
            Offset = baseUnit.Offset + offset;
        }

        #endregion
    }
}