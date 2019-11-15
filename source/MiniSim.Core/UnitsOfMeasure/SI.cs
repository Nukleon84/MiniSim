namespace MiniSim.Core.UnitsOfMeasure
{
    public static class SI
    {
        //Base Units
        public static Unit none = new Unit("", "Dimensionless", new double[] { 0, 0, 0, 0, 0, 0, 0, 0 });

        public static Unit m = new Unit("m", "Meter", new double[] { 1, 0, 0, 0, 0, 0, 0, 0 });
        public static Unit kg = new Unit("kg", "Kilogram", new double[] { 0, 1, 0, 0, 0, 0, 0, 0 });
        public static Unit s = new Unit("s", "Second", new double[] { 0, 0, 1, 0, 0, 0, 0, 0 });
        public static Unit K = new Unit("K", "Kelvin", new double[] { 0, 0, 0, 0, 1, 0, 0, 0 });
        public static Unit mol = new Unit("mol", "Mol", new double[] { 0, 0, 0, 0, 0, 1, 0, 0 });

        //Derived Units
        public static Unit N = new Unit("N", "Newton", kg * m / (s ^ 2));
        public static Unit J = new Unit("J", "Joule", N * m);
        public static Unit Pa = new Unit("Pa", "Pascal", N / (m ^ 2));

        public static Unit W = new Unit("W", "Watt", J / s, 1, 0);

        public static Unit h = new Unit("h", "hour", s, 3600, 0);

        public static Unit TK = new Unit("°C (Δ)", "Celsius (Delta)", K, 1, 0);
        public static Unit DK = new Unit("K (Δ)", "Kelvin (Delta)", K, 1, 0);

        public static Unit min = new Unit("min", "Minute", s, 60, 0);
        public static Unit cum = new Unit("cum", "Cubic-meter", (SI.m ^ 3), 1, 0);
        public static Unit sqm = new Unit("sqm", "Square-meter", (SI.m ^ 2), 1, 0);


        public static Unit g = new Unit("g", "gram", kg, 0.001, 0);
        //Prefixes

        public static Unit micro = new Unit("µ", "Micro", 1e-6);
        public static Unit milli = new Unit("m", "Milli", 1e-3);
        public static Unit centi = new Unit("c", "Centi", 1e-2);
        public static Unit kilo = new Unit("k", "Kilo", 1e3);
        public static Unit mega = new Unit("M", "Mega", 1e6);
        public static Unit giga = new Unit("G", "Giga", 1e9);
        public static Unit tera = new Unit("T", "Tera", 1e12);

        //Convenient units
        public static Unit kJ = new Unit("kJ", "KiloJoule", kilo * J);

        public static Unit kW = new Unit("kW", "KiloWatt", kilo * W);
        public static Unit MW = new Unit("MW", "MegaWatt", mega * W);

        public static Unit kPa = new Unit("kPa", "KiloPascal", kilo * Pa);
        public static Unit kmol = new Unit("kmol", "KiloMol", kilo * mol);
    }
}