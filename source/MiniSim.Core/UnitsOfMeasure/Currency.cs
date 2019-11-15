namespace MiniSim.Core.UnitsOfMeasure
{
    public static class Currency
    {
        public static Unit Dollar = new Unit("$", "US-Dollar", new double[] {0, 0, 0, 0, 0, 0, 0, 1});

        public static Unit Euro = new Unit("€", "Euro", Dollar, 1/0.76, 0);
    }
}