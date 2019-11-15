namespace MiniSim.Core.UnitsOfMeasure
{
    public static class METRIC
    {
        public static Unit C = new Unit("°C", "Celsius", SI.K, 1, 273.15);
        public static Unit bar = new Unit("bar", "Bar", SI.Pa, 1E5, 0);
        public static Unit mbar = new Unit("mbar", "Millibar", SI.Pa, 1E2, 0);

        public static Unit ton = new Unit("t", "Metric ton", SI.kg, 1E3, 0);

        public static Unit weightpercent = new Unit("w-%", "Weight percent", SI.kg/SI.kg, 1E-2, 0);
        public static Unit molpercent = new Unit("mol-%", "Mol percent", SI.mol / SI.mol, 1E-2, 0);

    }
}