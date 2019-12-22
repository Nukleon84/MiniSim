using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Interfaces;
using MiniSim.Core.ModelLibrary;
using MiniSim.Core.Numerics;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Reporting
{
    public class Generator
    {
        private readonly ILogger _logger;

        public Generator(ILogger logger)
        {
            _logger = logger;
        }

        public void Report(AlgebraicSystem system, bool showEquations = false)
        {
            _logger.Info("System      : " + system.Name);

            if (showEquations)
            {
                _logger.Info("Equations # : " + system.NumberOfEquations);
                foreach (var eq in system.Equations)
                    _logger.Log(eq.ToString());
            }

            _logger.Info("Variables # : " + system.NumberOfVariables);

            foreach (var vari in system.Variables)
                _logger.Log(String.Format("{0,-15} {1,-15} {2,-15} {3,15} {4,-10}", vari.ModelClass, vari.ModelName, vari.FullName, vari.DisplayValue.ToString("0.000"), vari.DisplayUnit));



        }

        public void Report(Variable variable)
        {
            var formatter = System.Globalization.NumberFormatInfo.InvariantInfo;
            _logger.Log(String.Format("{0, -25} = {1} {2,-12} [{3}]", variable.ModelName + "." + variable.FullName, String.Format(formatter, "{0,12:0.0000}", variable.DisplayValue), variable.DisplayUnit, variable.Description));
        }

        public void Report(MaterialStream stream)
        {
            
            foreach (var vari in stream.Variables)
                vari.Reset();

            _logger.Info("Stream      : " + stream.Name);
            _logger.Log("Property    : " + stream.System.Name);
            _logger.Log("Molar Weight: " + stream.MolarWeight.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.MolarWeight.DisplayUnit);
            _logger.Log("Temperature : " + stream.Temperature.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Temperature.DisplayUnit);
            _logger.Log("Pressure    : " + stream.Pressure.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Pressure.DisplayUnit);
            _logger.Log("Density     : " + stream.Bulk.Density.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Bulk.Density.DisplayUnit);
            _logger.Log("Density mol.: " + stream.Bulk.DensityMolar.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Bulk.DensityMolar.DisplayUnit);

            _logger.Log("Vapor Frac. : " + stream.VaporFraction.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.VaporFraction.DisplayUnit);

            if (stream.TwoLiquidPhases)
                _logger.Log("Phi         : " + stream.Phi.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Phi.DisplayUnit);
            _logger.Log("State       : " + stream.State);
            _logger.Log("");
            _logger.Log("Mass Flow   : " + stream.Bulk.TotalMassflow.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Bulk.TotalMassflow.DisplayUnit);
            _logger.Log("Molar Flow  : " + stream.Bulk.TotalMolarflow.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Bulk.TotalMolarflow.DisplayUnit);
            _logger.Log("Vol. Flow   : " + stream.Bulk.TotalVolumeflow.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Bulk.TotalVolumeflow.DisplayUnit);


            _logger.Log("");
            _logger.Log("Enthalpy    : " + stream.Bulk.SpecificEnthalpy.DisplayValue.ToString("0.000").PadLeft(10) + " " + stream.Bulk.SpecificEnthalpy.DisplayUnit);
            _logger.Log("");
            _logger.Log("Molar Flows");
            _logger.Log("");
            var phases = new List<Phase> { stream.Bulk, stream.Liquid, stream.Vapor };


            var molarFlowUnit = stream.System.VariableFactory.Output.UnitDictionary[PhysicalDimension.MolarFlow];
            var molarFracUnit = stream.System.VariableFactory.Output.UnitDictionary[PhysicalDimension.MolarFraction];

            var massFlowUnit = stream.System.VariableFactory.Output.UnitDictionary[PhysicalDimension.MassFlow];
            var massFracUnit = stream.System.VariableFactory.Output.UnitDictionary[PhysicalDimension.MassFraction];

            if (stream.TwoLiquidPhases)
            {
                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10} {7,-10} {8,-10}", "ID", "Mixed", " ", "Liquid", " ", "Liquid2", " ", "Vapor", " "));
                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10} {7,-10} {8,-10}", "", molarFlowUnit, molarFracUnit, molarFlowUnit, molarFracUnit, molarFlowUnit, molarFracUnit, molarFlowUnit, molarFracUnit));

                for (int i = 0; i < stream.System.Components.Count; i++)
                {

                    _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10} {7,-10} {8,-10}",
                        stream.System.Components[i].ID,
                        stream.Bulk.ComponentMolarflow[i].DisplayValue.ToString("0.000"), stream.Bulk.ComponentMolarFraction[i].DisplayValue.ToString("0.000"),
                        stream.Liquid.ComponentMolarflow[i].DisplayValue.ToString("0.000"), stream.Liquid.ComponentMolarFraction[i].DisplayValue.ToString("0.000"),
                        stream.Liquid2.ComponentMolarflow[i].DisplayValue.ToString("0.000"), stream.Liquid2.ComponentMolarFraction[i].DisplayValue.ToString("0.000"),
                        stream.Vapor.ComponentMolarflow[i].DisplayValue.ToString("0.000"), stream.Vapor.ComponentMolarFraction[i].DisplayValue.ToString("0.000")));
                }
                _logger.Log("");
                _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10} {7,-10} {8,-10}", "Total", 
                    stream.Bulk.TotalMolarflow.DisplayValue.ToString("0.000"), "", 
                    stream.Liquid.TotalMolarflow.DisplayValue.ToString("0.000"), "",
                    stream.Liquid2.TotalMolarflow.DisplayValue.ToString("0.000"), "",
                    stream.Vapor.TotalMolarflow.DisplayValue.ToString("0.000"), ""));
                _logger.Log("");
                _logger.Log("Mass Flows");
                _logger.Log("");
                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10} {7,-10} {8,-10}", "ID", "Mixed", " ", "Liquid", " ", "Liquid2", " ", "Vapor", " "));
                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10} {7,-10} {8,-10}", "", massFlowUnit, massFracUnit, massFlowUnit, massFracUnit, massFlowUnit, massFracUnit, massFlowUnit, massFracUnit));

                for (int i = 0; i < stream.System.Components.Count; i++)
                {
                    _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10} {7,-10} {8,-10}",
                      stream.System.Components[i].ID,
                      stream.Bulk.ComponentMassflow[i].DisplayValue.ToString("0.000"), stream.Bulk.ComponentMassFraction[i].DisplayValue.ToString("0.000"),
                      stream.Liquid.ComponentMassflow[i].DisplayValue.ToString("0.000"), stream.Liquid.ComponentMassFraction[i].DisplayValue.ToString("0.000"),
                      stream.Liquid2.ComponentMassflow[i].DisplayValue.ToString("0.000"), stream.Liquid2.ComponentMassFraction[i].DisplayValue.ToString("0.000"),
                      stream.Vapor.ComponentMassflow[i].DisplayValue.ToString("0.000"), stream.Vapor.ComponentMassFraction[i].DisplayValue.ToString("0.000")));
                }
                _logger.Log("");
                _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10}  {7,-10} {8,-10}", "Total", stream.Bulk.TotalMassflow.DisplayValue.ToString("0.000"), "", stream.Liquid.TotalMassflow.DisplayValue.ToString("0.000"), "", stream.Liquid2.TotalMassflow.DisplayValue.ToString("0.000"), "", stream.Vapor.TotalMassflow.DisplayValue.ToString("0.000"), ""));

            }
            else
            {

                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10}", "ID", "Mixed", " ", "Liquid", " ", "Vapor", " "));
                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10}", "", molarFlowUnit, molarFracUnit, molarFlowUnit, molarFracUnit, molarFlowUnit, molarFracUnit));

                for (int i = 0; i < stream.System.Components.Count; i++)
                {

                    _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10}",
                        stream.System.Components[i].ID,
                        stream.Bulk.ComponentMolarflow[i].DisplayValue.ToString("0.000"), stream.Bulk.ComponentMolarFraction[i].DisplayValue.ToString("0.000"),
                        stream.Liquid.ComponentMolarflow[i].DisplayValue.ToString("0.000"), stream.Liquid.ComponentMolarFraction[i].DisplayValue.ToString("0.000"),
                        stream.Vapor.ComponentMolarflow[i].DisplayValue.ToString("0.000"), stream.Vapor.ComponentMolarFraction[i].DisplayValue.ToString("0.000")));
                }
                _logger.Log("");
                _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10}", "Total", stream.Bulk.TotalMolarflow.DisplayValue.ToString("0.000"), "", stream.Liquid.TotalMolarflow.DisplayValue.ToString("0.000"), "", stream.Vapor.TotalMolarflow.DisplayValue.ToString("0.000"), ""));
                _logger.Log("");
                _logger.Log("Mass Flows");
                _logger.Log("");
                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10}", "ID", "Mixed", " ", "Liquid", " ", "Vapor", " "));
                _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10} {5,-10} {6,-10}", "", massFlowUnit, massFracUnit, massFlowUnit, massFracUnit, massFlowUnit, massFracUnit));

                for (int i = 0; i < stream.System.Components.Count; i++)
                {
                    _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10}",
                      stream.System.Components[i].ID,
                      stream.Bulk.ComponentMassflow[i].DisplayValue.ToString("0.000"), stream.Bulk.ComponentMassFraction[i].DisplayValue.ToString("0.000"),
                      stream.Liquid.ComponentMassflow[i].DisplayValue.ToString("0.000"), stream.Liquid.ComponentMassFraction[i].DisplayValue.ToString("0.000"),
                      stream.Vapor.ComponentMassflow[i].DisplayValue.ToString("0.000"), stream.Vapor.ComponentMassFraction[i].DisplayValue.ToString("0.000")));
                }
                _logger.Log("");
                _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10}", "Total", stream.Bulk.TotalMassflow.DisplayValue.ToString("0.000"), "", stream.Liquid.TotalMassflow.DisplayValue.ToString("0.000"), "", stream.Vapor.TotalMassflow.DisplayValue.ToString("0.000"), ""));
            }
        }

        #region Flowsheet
        public void Report(Flowsheet flowsheet, int columns = -1, bool showPhases = true)
        {
            _logger.Log("");
            _logger.Info("Report for flowsheet " + flowsheet.Name);
            _logger.Info("================================================");
            _logger.Log("");
            _logger.Info("Material Streams");

            var groups = flowsheet.MaterialStreams.GroupBy(s => s.System);

            var formatter = System.Globalization.NumberFormatInfo.InvariantInfo;

            var lineFormat = "{0,-25} {1,-10} {2}";
            var lineFormat2 = "{0,25} {1,-10} {2}";
            _logger.Log("");
            foreach (var group in groups)
            {
                _logger.Info(String.Format(lineFormat, "System", group.Key.Name, ""));
                _logger.Log("");
                int batches = 1;
                if (columns != -1)
                    batches = (int)Math.Ceiling(group.Count() / (double)columns);


                for (int i = 0; i < batches; i++)
                {

                    IEnumerable<MaterialStream> currentStreamBatch = group;

                    foreach (var stream in group)
                        foreach (var vari in stream.Variables)
                            vari.Reset();



                    if (columns != -1)
                        currentStreamBatch = group.Skip(i * columns).Take(columns);


                    _logger.Info(String.Format(lineFormat, "Property", "Unit", String.Join(" ", currentStreamBatch.Select(s => String.Format("{0,12}", s.Name)))));
                    _logger.Log("");

                    Func<PhysicalDimension, Unit> unitFor = d => group.Key.VariableFactory.Output.UnitDictionary[d];
                    Func<MaterialStream, string, string> valueSelector = (s, v) => String.Format(formatter, "{0,12:0.0000}", s.GetVariable(v).DisplayValue);


                    _logger.Log(String.Format(lineFormat, "Temperature", unitFor(PhysicalDimension.Temperature), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "T")))));
                    _logger.Log(String.Format(lineFormat, "Pressure", unitFor(PhysicalDimension.Pressure), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "P")))));
                    _logger.Log(String.Format(lineFormat, "Vapor Fraction", unitFor(PhysicalDimension.MolarFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "VF")))));
                    _logger.Log(String.Format(lineFormat, "Specific Enthalpy", unitFor(PhysicalDimension.SpecificMolarEnthalpy), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "h")))));
                    _logger.Log(String.Format(lineFormat, "Phase", "", String.Join(" ", currentStreamBatch.Select(s => String.Format("{0,12}", s.State)))));

                    _logger.Log(String.Format(lineFormat, "Density", unitFor(PhysicalDimension.MolarDensity), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "rhom")))));
                    _logger.Log(String.Format(lineFormat, "Mass Density", unitFor(PhysicalDimension.MassDensity), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "rho")))));
                    _logger.Log(String.Format(lineFormat, "Volume Flow", unitFor(PhysicalDimension.VolumeFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "V")))));
                    _logger.Log(String.Format(lineFormat, "Molar Weight", unitFor(PhysicalDimension.MolarWeight), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "MW")))));

                    _logger.Log("");
                    _logger.Info(String.Format(lineFormat, "Total Molar Flow", unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "n")))));

                    var compExists = new Dictionary<Substance, bool>();

                    foreach (var c in group.Key.Components)
                    {
                        var test = currentStreamBatch.Select(s => s.GetVariable("n[" + c.ID + "]").DisplayValue).Sum();
                        if (test > 1e-8)
                            compExists.Add(c, true);
                        else
                            compExists.Add(c, false);

                        if (compExists[c])
                            _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "n[" + c.ID + "]")))));
                    }

                    _logger.Info(String.Format(lineFormat, "Molar Composition", "", ""));
                    foreach (var c in group.Key.Components)
                    {
                        if (compExists[c])
                            _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "x[" + c.ID + "]")))));
                    }

                    _logger.Log("");
                    _logger.Info(String.Format(lineFormat, "Total Mass Flow", unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "m")))));
                    foreach (var c in group.Key.Components)
                    {
                        if (compExists[c])
                            _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "m[" + c.ID + "]")))));
                    }

                    _logger.Info(String.Format(lineFormat, "Mass Composition", "", ""));
                    foreach (var c in group.Key.Components)
                    {
                        if (compExists[c])
                            _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "w[" + c.ID + "]")))));
                    }
                    _logger.Log("");
                    _logger.Log("");
                    if (showPhases)
                    {
                        _logger.Info(String.Format(lineFormat, "Liquid Molar Flow", unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "nL")))));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "nL[" + c.ID + "]")))));
                        }
                        _logger.Info(String.Format(lineFormat, "Liquid Molar Composition", "", ""));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "xL[" + c.ID + "]")))));
                        }


                        _logger.Info(String.Format(lineFormat, "Liquid Mass Flow", unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mL")))));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mL[" + c.ID + "]")))));
                        }
                        _logger.Info(String.Format(lineFormat, "Liquid Mass Composition", "", ""));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "wL[" + c.ID + "]")))));
                        }


                        if(group.Key.EquilibriumMethod.AllowedPhases== AllowedPhases.VLLE)
                        {
                            _logger.Log("");
                            _logger.Log("");
                            _logger.Info(String.Format(lineFormat, "Liquid2 Molar Flow", unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "nL2")))));
                            foreach (var c in group.Key.Components)
                            {
                                if (compExists[c])
                                    _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "nL2[" + c.ID + "]")))));
                            }
                            _logger.Info(String.Format(lineFormat, "Liquid2 Molar Composition", "", ""));
                            foreach (var c in group.Key.Components)
                            {
                                if (compExists[c])
                                    _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "xL2[" + c.ID + "]")))));
                            }


                            _logger.Info(String.Format(lineFormat, "Liquid2 Mass Flow", unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mL2")))));
                            foreach (var c in group.Key.Components)
                            {
                                if (compExists[c])
                                    _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mL2[" + c.ID + "]")))));
                            }
                            _logger.Info(String.Format(lineFormat, "Liquid2 Mass Composition", "", ""));
                            foreach (var c in group.Key.Components)
                            {
                                if (compExists[c])
                                    _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "wL2[" + c.ID + "]")))));
                            }
                        }

                        _logger.Log("");
                        _logger.Log("");
                        _logger.Info(String.Format(lineFormat, "Vapor Molar Flow", unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "nV")))));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "nV[" + c.ID + "]")))));
                        }
                        _logger.Info(String.Format(lineFormat, "Vapor Molar Composition", "", ""));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "xV[" + c.ID + "]")))));
                        }

                        _logger.Info(String.Format(lineFormat, "Vapor Mass Flow", unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mV")))));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mV[" + c.ID + "]")))));
                        }
                        _logger.Info(String.Format(lineFormat, "Vapor Mass Composition", "", ""));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                _logger.Log(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "wV[" + c.ID + "]")))));
                        }
                        _logger.Log("");

                    }
                    _logger.Log("");

                }



                _logger.Log("");
                _logger.Info("Design Specifications");
                _logger.Log("");

                _logger.Log(String.Format("{0,-15} {1,-30} {2,-15} {3,-20} {4,15} {5}", "Name", "Model", "Class", "Group", "Residual", "Equation"));

                foreach (var eq in flowsheet.DesignSpecifications)
                {
                    _logger.Log(String.Format("{0,-15} {1,-30} {2,-15} {3,-20} {4,15} {5}", eq.Name, eq.ModelName, eq.ModelClass, eq.Group, eq.Residual().ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), eq.ToString()));
                }
                _logger.Log("");

            }
        }

        public void Report(EquilibriumStageSection sec, bool onlyOverview = true)
        {
            foreach (var vari in sec.Variables)
                vari.Reset();

            _logger.Log(String.Format("{0,10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10} {7,10} {8,10} {9,10} {10,10}", "Stage", "T", "TV", "P", "Q", "L", "V", "F", "W", "RL", "EPS"));


            _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10} {7,10} {8,10} {9,10} {10,10}",
                "",
                sec.GetVariable("T[1]").DisplayUnit,
                sec.GetVariable("TV[1]").DisplayUnit,
                sec.GetVariable("P[1]").DisplayUnit,
                sec.GetVariable("Q[1]").DisplayUnit,
                sec.GetVariable("L[1]").DisplayUnit,
                sec.GetVariable("V[1]").DisplayUnit,
                sec.GetVariable("F[1]").DisplayUnit,
                sec.GetVariable("W[1]").DisplayUnit,
                sec.GetVariable("RL[1]").DisplayUnit,
                "[-]"));

            for (int i = 0; i < sec.NumberOfTrays; i++)
            {
                var j = i + 1;
                _logger.Log(String.Format("{0,-10} {1,10} {2,10} {3,10} {4,10} {5,10} {6,10} {7,10} {8,10} {9,10} {10,10}",
                    j,
                    sec.GetVariable("T[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("TV[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("P[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("Q[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("L[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("V[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("F[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("W[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("RL[" + j + "]").DisplayValue.ToString("0.000"),
                    sec.GetVariable("eps[" + j + "]").DisplayValue.ToString("0.000")));
            }


            if (onlyOverview)
                return;
            _logger.Log(String.Format("{0,10} {1,10} ", "Stage", String.Join(" ", sec.System.Components.Select(c => String.Format("{0,10}", "x[" + c.ID + "]")))));
            for (int i = 0; i < sec.NumberOfTrays; i++)
            {
                var j = i + 1;
                _logger.Log(String.Format("{0,10} {1,10} ", j, String.Join(" ", sec.System.Components.Select(c => String.Format("{0,10}", sec.GetVariable("x[" + j + ", "+ c.ID + "]").DisplayValue.ToString("0.000"))))));
            }

            _logger.Log(String.Format("{0,10} {1,10} ", "Stage", String.Join(" ", sec.System.Components.Select(c => String.Format("{0,10}", "y[" + c.ID + "]")))));
            for (int i = 0; i < sec.NumberOfTrays; i++)
            {
                var j = i + 1;
                _logger.Log(String.Format("{0,10} {1,10} ", j, String.Join(" ", sec.System.Components.Select(c => String.Format("{0,10}", sec.GetVariable("y[" + j + ", " + c.ID + "]").DisplayValue.ToString("0.000"))))));
            }
            _logger.Log(String.Format("{0,10} {1,10} ", "Stage", String.Join(" ", sec.System.Components.Select(c => String.Format("{0,10}", "z[" + c.ID + "]")))));
            for (int i = 0; i < sec.NumberOfTrays; i++)
            {
                var j = i + 1;
                _logger.Log(String.Format("{0,10} {1,10} ", j, String.Join(" ", sec.System.Components.Select(c => String.Format("{0,10}", sec.GetVariable("z[" + j + ", " + c.ID + "]").DisplayValue.ToString("0.000"))))));
            }
            /*


    print '{0:10} {1:10} '.format('Stage', ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format('x['+c.ID+']'))))
    for i in range(trays):
        j=str(i+1)
        print '{0:10} {1:10}'.format(j, ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format( sec.GetVariable( 'x['+ j +', '+c.ID+']' ).ValueInSI ) ) ))
    print '{0:10} {1:10} '.format('Stage', ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format('y['+c.ID+']'))))
    for i in range(trays):
        j=str(i+1)
        print '{0:10} {1:10}'.format(j, ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format( sec.GetVariable( 'y['+ j +', '+c.ID+']' ).ValueInSI ) ) ))
    print '{0:10} {1:10} '.format('Stage', ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format('y*['+c.ID+']'))))
    for i in range(trays):
        j=str(i+1)
        print '{0:10} {1:10}'.format(j, ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format( sec.GetVariable( 'yeq['+ j +', '+c.ID+']' ).ValueInSI ) ) ))
    print '{0:10} {1:10} '.format('Stage', ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format('z['+c.ID+']'))))
    for i in range(trays):
        j=str(i+1)
        print '{0:10} {1:10}'.format(j, ' '.join(sec.System.Components.Select(lambda c: '{0:10}'.format( sec.GetVariable( 'z['+ j +', '+c.ID+']' ).ValueInSI ) ) ))

    return

              */
        }


        public void Report(ProcessUnit unit)
        {
            _logger.Log("");
            _logger.Log("Report for unit " + unit.Name + "[" + unit.Class + "]");
            _logger.Log("================================================");
            _logger.Log("Material Ports");
            _logger.Log("");

            _logger.Log(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", "Name", "Direction", "Multi", "Num", "Streams"));

            foreach (var port in unit.MaterialPorts)
            {
                _logger.Log(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", port.Name, port.Direction, port.Multiplicity, port.NumberOfStreams, String.Join(", ", port.Streams.Select(s => s.Name))));
            }

            if (unit.HeatPorts.Count > 0)
            {
                _logger.Log("");
                _logger.Log("Heat Ports");
                _logger.Log("");
                _logger.Log(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", "Name", "Direction", "Multi", "Num", "Streams"));
                foreach (var port in unit.HeatPorts)
                {
                    _logger.Log(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", port.Name, port.Direction, port.Multiplicity, port.NumberOfStreams, String.Join(", ", port.Streams.Select(s => s.Name))));
                }
            }

            _logger.Log("");
            _logger.Info("Variables");
            _logger.Log("");
            _logger.Log(String.Format("{0,-15} {1,-0}{2,-0}{3,-0} {4,10} {5,-10} {6,-10} {7,-10} {8,-15} {9,-15}", "Name", "", "", "", "Value", "Unit", "Min", "Max", "Dimension", "Description"));
            foreach (var vari in unit.Variables)
            {
                _logger.Log(String.Format("{0,-15} {1,-0}{2,-0}{3,-0} {4,10} {5,-10} {6,-10} {7,-10} {8,-15} {9,-15}",
                    vari.FullName,
                    "",//vari.ModelName,
                    "",//vari.ModelClass,
                    "",
                    vari.DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    vari.DisplayUnit.Symbol,
                    Unit.Convert(vari.InternalUnit, vari.DisplayUnit, vari.LowerBound).ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    Unit.Convert(vari.InternalUnit, vari.DisplayUnit, vari.UpperBound).ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    vari.Dimension,
                    vari.Description));
            }
            _logger.Log("");
        }
        #endregion

        #region Thermodynamics System
        public void Report(ThermodynamicSystem system)
        {
            _logger.Info("");
            _logger.Info("Report for thermodynamic system " + system.Name);
            _logger.Info("================================================");

            _logger.Log("");
            _logger.Info("Equilibrium");
            _logger.Log("");

            _logger.Log("VLEQ Method      : " + system.EquilibriumMethod.EquilibriumApproach);
            _logger.Log("Activity Method  : " + system.EquilibriumMethod.Activity);
            _logger.Log("Fugacity Method  : " + system.EquilibriumMethod.Fugacity);
            _logger.Log("Henry Method     : " + system.EquilibriumMethod.AllowHenryComponents);

            _logger.Log("");
            _logger.Info("Unit of Measure");
            _logger.Log("");
            _logger.Log(String.Format("{0,-25} {1,-15} {2,-15}", "Dimension", "Input", "Output"));

            foreach (var unit in system.VariableFactory.Internal.UnitDictionary)
            {
                _logger.Log(String.Format("{0,-25} {1,-15} {2,-15}", unit.Key, unit.Value, system.VariableFactory.Output.UnitDictionary[unit.Key]));

            }

            _logger.Log("");
            _logger.Info("Components");
            _logger.Log("");

            _logger.Log(String.Format("{0,-25} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15} {6,-15}", "Name", "ID", "CAS-No", "Inert", "MOLW", "TC", "PC"));
            foreach (var comp in system.Components)
            {
                _logger.Log(String.Format("{0,-25} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15} {6,-15}",
                    comp.Name,
                    comp.ID,
                    comp.CasNumber,
                    comp.IsInert,
                    comp.GetConstant(ConstantProperties.MolarWeight).DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    comp.GetConstant(ConstantProperties.CriticalTemperature).DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    comp.GetConstant(ConstantProperties.CriticalPressure).DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo)));
            }
            _logger.Log("");
            _logger.Info("Enthalpy Functions");
            _logger.Log("");
            _logger.Log(String.Format("{0,-15} {1,-10} {2,-12} {3,-8} {4,-8} {5,-5}", "Comp", "Phase", "Href", "Tref", "TPc", "Fixed Phase Change"));
            foreach (var enth in system.EnthalpyMethod.PureComponentEnthalpies)
            {
                _logger.Log(String.Format("{0,-15} {1,-10} {2,-12} {3,-8} {4,-8} {5,-5}",
                    enth.Component.ID,
                    enth.ReferenceState,
                    enth.Href.DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    enth.Tref.DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    enth.TPhaseChange.DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    !enth.PhaseChangeAtSystemTemperature));

            }


            _logger.Log("");
            _logger.Info("Property Functions");
            _logger.Log("");
            _logger.Log(String.Format("{0,-15} {1,-25} {2,-15} {3,-8} {4,-8} {5,-5} {6,-25}", "Comp", "Property", "Form", "Min T", "Max T", "Coeff", "Equation"));
            foreach (var comp in system.Components)
            {
                foreach (var func in comp.Functions)
                {
                    _logger.Log(String.Format("{0,-15} {1,-25} {2,-15} {3,-8} {4,-8} {5,-5} {6,-25}",
                        comp.ID,
                        func.Property,
                        func.Type,
                        func.MinimumX.DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                        func.MaximumX.DisplayValue.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                        func.Coefficients.Count,
                        "Y = " + system.CorrelationFactory.CreateExpression(func.Type, func, new Variable("T", 1), new Variable("TC", 1), new Variable("PC", 1)).ToString()));

                }
            }

            _logger.Log("");
            _logger.Info("Chemistry blocks");
            _logger.Log("");
            _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-40} {4,-20} ", "Label", "Type", "DHR", "Reaction", "Stoichiometry"));


            foreach (var chem in system.ChemistryBlocks)
            {
                foreach (var reac in chem.Reactions)
                {
                    _logger.Log(String.Format("{0,-10} {1,-10} {2,-10} {3,-40} {4,-20} ",
                        chem.Label,
                        reac.Type,
                        reac.ReactionEnthalpy,
                        GetReactionFunction(reac),
                        GetReactionStoichiometry(reac)
                        ));

                }
            }

        }

        string GetReactionFunction(Reaction reaction)
        {
            Func<double, string> formatter = (x) => x.ToString("G4", System.Globalization.NumberFormatInfo.InvariantInfo);

            switch (reaction.Type)
            {
                case ReactionType.EQLA:
                case ReactionType.EQLM:
                case ReactionType.EQVM:
                    var expr = "K=exp("
                        + ((reaction.Coefficients.Count > 0) ? formatter(reaction.Coefficients[0]) : "")
                        + ((reaction.Coefficients.Count > 1) ? " + " + formatter(reaction.Coefficients[1]) + "/T" : "")
                    + ((reaction.Coefficients.Count > 2) ? " + " + formatter(reaction.Coefficients[2]) + "*ln(T)" : "")
                    + ((reaction.Coefficients.Count > 3) ? " + " + formatter(reaction.Coefficients[3]) + "*T^2" : "")
                    + ((reaction.Coefficients.Count > 4) ? " + " + formatter(reaction.Coefficients[4]) + "*T^3" : "");
                    return expr + ")";
                default:
                    return "K=1";
            }

        }
        string GetReactionStoichiometry(Reaction reaction)
        {
            string educts = "";
            string products = "";
            Func<double, string> formatter = (x) => x.ToString("G4", System.Globalization.NumberFormatInfo.InvariantInfo);

            switch (reaction.Type)
            {
                case ReactionType.EQLA:
                    foreach (var stoic in reaction.Stoichiometry)
                    {
                        if (stoic.StoichiometricFactor < 0)
                        {
                            if (educts != "")
                                educts += " + ";
                            educts += formatter(Math.Abs(stoic.StoichiometricFactor)) + " " + stoic.Component.ID;
                        }
                        if (stoic.StoichiometricFactor > 0)
                        {
                            if (products != "")
                                products += " + ";
                            products += formatter(stoic.StoichiometricFactor) + " " + stoic.Component.ID;
                        }
                    }
                    return educts + " <=> " + products;
            }

            return "";
        }
        #endregion
    }
}
