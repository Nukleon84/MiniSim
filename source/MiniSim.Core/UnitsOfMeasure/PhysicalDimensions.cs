namespace MiniSim.Core.UnitsOfMeasure
{
    /* Description = "Unit set according to the SI",
                TemperatureUnit = SI.K,
                PressureUnit = SI.Pa,
                MolarFlowUnit = SI.mol/SI.s,
                MassFlowUnit = SI.kg/SI.s,
                MassDensityUnit = SI.kg/(SI.m ^ 3),
                MolarDensityUnit = SI.mol/(SI.m ^ 3),
                HeatFlowUnit = SI.J/SI.s,
                CurrencyUnit = Currency.Euro*/
    /// <summary>
    /// Enumeration of all Physical dimensions supported by OpenFMSL
    /// </summary>
    public enum PhysicalDimension
    {
        Temperature,
        TemperatureDifference,
        Pressure,
        MolarFlow,
        MassFlow,
        VolumeFlow,
        MolarDensity,
        MassDensity,
        HeatFlow,
        Energy,
        Power,
        Work,
        Currency,
        Length,
        Area,
        Volume,
        Time,
        Mass,
        Mole,
        Enthalpy,
        HeatCapacity,
        MolarVolume,
        Dimensionless,
        SpecificMolarEnthalpy,
        SpecificMassEnthalpy,
        HeatTransferCoefficient,
        SurfaceTension,
        HeatConductivity,
        DiffusionCoefficient,
        MolarFraction,
        MassFraction,
        SpecificArea,
        MassTransferCoefficient,
        DynamicViscosity,
        Velocity,
        MolarWeight


    }
}
