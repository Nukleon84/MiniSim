namespace MiniSim.Core.UnitsOfMeasure
{
    public static class USENG
    {
        public static Unit F = new Unit("F", "Fahrenheit", SI.K, 1 / 1.8, 459.67 / 1.8);
        public static Unit psi = new Unit("psi", "psi", SI.Pa, 0.0689476e5, 0);
        public static Unit USGallon = new Unit("US-Gal", "US-Gallon", SI.cum, 0.00378541, 0);

      
    }

}