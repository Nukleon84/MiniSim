using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Interfaces;
using MiniSim.Core.Numerics;
using MiniSim.Core.Reporting;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.Thermodynamics.Routines;
using MiniSim.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.ModelLibrary
{
    public enum StageEfficiencyType { None, Murphree, ExtendedMurphree, BausaMurphree };

    public class StageConnectivity
    {
        MaterialStream _stream;
        int _stage;
        Variable _factor = new Variable("F", () => 0, SI.none) { Superscript = "Side", IsFixed = true };
        PhaseState _phase = PhaseState.Liquid;

        public MaterialStream Stream
        {
            get
            {
                return _stream;
            }

            set
            {
                _stream = value;
            }
        }

        public int Stage
        {
            get
            {
                return _stage;
            }

            set
            {
                _stage = value;
            }
        }

        public Variable Factor
        {
            get
            {
                return _factor;
            }

            set
            {
                _factor = value;
            }
        }

        public PhaseState Phase
        {
            get
            {
                return _phase;
            }

            set
            {
                _phase = value;
            }
        }


    }

    public class EquilibriumTray
    {
        public Variable T;
        public Variable TV;
        public Variable eps;
        public Variable p;
        public Variable Q;
        public Variable L;
        public Variable V;
        public Variable W;
        public Variable U;
        public Variable RV;
        public Variable RL;
        public Variable F;
        public Variable HL;
        public Variable HV;
        public Variable HF;
        public Variable[] x;
        public Variable[] y;
        public Variable[] yeq;
        public Variable[] z;

        public Variable[] K;
        //Variable[] EPS;
        public Variable DP;
        int _number = -1;
        ThermodynamicSystem _system;

        public int Number
        {
            get
            {
                return _number;
            }

            set
            {
                _number = value;
            }
        }

        public EquilibriumTray(int number, ThermodynamicSystem system)
        {
            _system = system;
            Number = number;
            var numString = number.ToString();
            T = system.VariableFactory.CreateVariable("T", numString, "Stage Temperature", PhysicalDimension.Temperature);
            TV = system.VariableFactory.CreateVariable("TV", numString, "Stage Vapor Temperature", PhysicalDimension.Temperature);
            DP = system.VariableFactory.CreateVariable("DP", numString, "Pressure Drop", PhysicalDimension.Pressure);
            DP.LowerBound = 0;
            DP.SetValue(0);

            p = system.VariableFactory.CreateVariable("P", numString, "Stage Pressure", PhysicalDimension.Pressure);
            Q = system.VariableFactory.CreateVariable("Q", numString, "Heat Duty", PhysicalDimension.HeatFlow);

            L = system.VariableFactory.CreateVariable("L", numString, "Liquid molar flow", PhysicalDimension.MolarFlow);
            V = system.VariableFactory.CreateVariable("V", numString, "Vapor molar flow", PhysicalDimension.MolarFlow);
            U = system.VariableFactory.CreateVariable("U", numString, "Liquid molar flow", PhysicalDimension.MolarFlow);
            W = system.VariableFactory.CreateVariable("W", numString, "Vapor molar flow", PhysicalDimension.MolarFlow);
            F = system.VariableFactory.CreateVariable("F", numString, "Feed molar flow", PhysicalDimension.MolarFlow);

            RL = system.VariableFactory.CreateVariable("RL", numString, "Liquid sidestream fraction", PhysicalDimension.Dimensionless);
            RV = system.VariableFactory.CreateVariable("RV", numString, "Vapor sidestream fraction", PhysicalDimension.Dimensionless);
            RV.LowerBound = 0;
            RL.LowerBound = 0;

            HF = system.VariableFactory.CreateVariable("HF", numString, "Feed specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);
            HL = system.VariableFactory.CreateVariable("HL", numString, "Liquid specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);
            HV = system.VariableFactory.CreateVariable("HV", numString, "Vapor specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);

            eps = system.VariableFactory.CreateVariable("eps", numString, "Stage efficiency", PhysicalDimension.Dimensionless);
            eps.SetValue(1);
            eps.LowerBound = 0;
            eps.UpperBound = 1;

            K = new Variable[system.Components.Count];
            x = new Variable[system.Components.Count];
            y = new Variable[system.Components.Count];
            yeq = new Variable[system.Components.Count];
            z = new Variable[system.Components.Count];
            for (int i = 0; i < system.Components.Count; i++)
            {
                K[i] = system.VariableFactory.CreateVariable("K", numString + ", " + system.Components[i].ID, "Equilibrium partition coefficient", PhysicalDimension.Dimensionless);
                K[i].SetValue(1.2);
                K[i].LowerBound = 1e-8;
                K[i].UpperBound = 1e6;
                x[i] = system.VariableFactory.CreateVariable("x", numString + ", " + system.Components[i].ID, "Liquid molar fraction", PhysicalDimension.MolarFraction);
                y[i] = system.VariableFactory.CreateVariable("y", numString + ", " + system.Components[i].ID, "Vapor molar fraction", PhysicalDimension.MolarFraction);
                yeq[i] = system.VariableFactory.CreateVariable("yeq", numString + ", " + system.Components[i].ID, "Equilibrium Vapor molar fraction", PhysicalDimension.MolarFraction);
                z[i] = system.VariableFactory.CreateVariable("z", numString + ", " + system.Components[i].ID, "Feed molar fraction", PhysicalDimension.MolarFraction);

            }
        }
    }

    public class EquilibriumStageSection : ProcessUnit
    {
        int _numberOfTrays = -1;
        StageEfficiencyType _efficiencyType = StageEfficiencyType.None;

        List<EquilibriumTray> _trays = new List<EquilibriumTray>();
        List<StageConnectivity> _feeds = new List<StageConnectivity>();
        List<StageConnectivity> _sidestream = new List<StageConnectivity>();
        List<Substance> _ignoredComponents = new List<Substance>();

        public int NumberOfTrays
        {
            get
            {
                return _numberOfTrays;
            }

            set
            {
                _numberOfTrays = value;
            }
        }

        public StageEfficiencyType EfficiencyType
        {
            get
            {
                return _efficiencyType;
            }

            set
            {
                _efficiencyType = value;
            }
        }

        public List<Substance> IgnoredComponents
        {
            get
            {
                return _ignoredComponents;
            }

            set
            {
                _ignoredComponents = value;
            }
        }

        public EquilibriumStageSection(string name, ThermodynamicSystem system, int numberOfTrays) : base(name, system)
        {
            Class = "TraySection";
            NumberOfTrays = numberOfTrays;
            Icon.IconType = IconTypes.ColumnSection;


            MaterialPorts.Add(new Port<MaterialStream>("Feeds", PortDirection.In, -1) { WidthFraction = 0, HeightFraction = 0.5, Normal = PortNormal.Left });
            MaterialPorts.Add(new Port<MaterialStream>("VIn", PortDirection.In, 1) { WidthFraction = 1, HeightFraction = 0.95, Normal = PortNormal.Right });
            MaterialPorts.Add(new Port<MaterialStream>("LIn", PortDirection.In, 1) { WidthFraction = 1, HeightFraction = 0.05, Normal = PortNormal.Right });
            MaterialPorts.Add(new Port<MaterialStream>("VOut", PortDirection.Out, 1) { WidthFraction = 0.5, HeightFraction = -0.075, Normal = PortNormal.Up });
            MaterialPorts.Add(new Port<MaterialStream>("LOut", PortDirection.Out, 1) { WidthFraction = 0.5, HeightFraction = 1.075, Normal = PortNormal.Down });
            MaterialPorts.Add(new Port<MaterialStream>("Sidestreams", PortDirection.Out, -1) { WidthFraction = 1, HeightFraction = 0.5, Normal = PortNormal.Right });

            for (int i = 0; i < NumberOfTrays; i++)
            {
                var tray = new EquilibriumTray(i + 1, system);
                _trays.Add(tray);
            }

            AddVariables(_trays.Select(t => t.T).ToArray());
            AddVariables(_trays.Select(t => t.TV).ToArray());

            AddVariables(_trays.Select(t => t.p).ToArray());
            AddVariables(_trays.Select(t => t.DP).ToArray());
            AddVariables(_trays.Select(t => t.Q).ToArray());
            AddVariables(_trays.Select(t => t.F).ToArray());
            AddVariables(_trays.Select(t => t.L).ToArray());
            AddVariables(_trays.Select(t => t.V).ToArray());
            AddVariables(_trays.Select(t => t.U).ToArray());
            AddVariables(_trays.Select(t => t.W).ToArray());
            AddVariables(_trays.Select(t => t.RL).ToArray());
            AddVariables(_trays.Select(t => t.RV).ToArray());
            AddVariables(_trays.Select(t => t.eps).ToArray());
            AddVariables(_trays.Select(t => t.HF).ToArray());
            AddVariables(_trays.Select(t => t.HL).ToArray());
            AddVariables(_trays.Select(t => t.HV).ToArray());

            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].K);
            }
            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].x);
            }
            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].y);
            }
            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].yeq);
            }

            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].z);
            }


            int NC = System.Components.Count;
            for (var tray = 0; tray < NumberOfTrays; tray++)
            {
                _trays[tray].U.Fix(0);
                _trays[tray].W.Fix(0);
                _trays[tray].RL.Fix(0);
                _trays[tray].RV.Fix(0);
                _trays[tray].F.Fix(0);
                _trays[tray].HF.Fix(0);

                for (var comp = 0; comp < NC; comp++)
                {
                    _trays[tray].z[comp].Fix(0);
                }
            }

        }


        public double[] GetProfile(string type, string component = null)
        {
            int cindex = 0;
            if (component != null)
                cindex = System.GetComponentIndex(component);

            switch (type)
            {
                case "T":
                    return _trays.Select(t => t.T.DisplayValue).ToArray();
                case "TV":
                    return _trays.Select(t => t.TV.DisplayValue).ToArray();
                case "P":
                    return _trays.Select(t => t.p.DisplayValue).ToArray();
                case "V":
                    return _trays.Select(t => t.V.DisplayValue).ToArray();
                case "L":
                    return _trays.Select(t => t.L.DisplayValue).ToArray();
                case "x":
                    {
                        return _trays.Select(t => t.x[cindex].DisplayValue).ToArray();
                    }
                case "y":
                    {
                        return _trays.Select(t => t.y[cindex].DisplayValue).ToArray();
                    }
                case "yeq":
                    {
                        return _trays.Select(t => t.yeq[cindex].DisplayValue).ToArray();
                    }
            }

            return new double[0];
        }
        public EquilibriumStageSection ConnectFeed(MaterialStream stream, int stage, PhaseState phase = PhaseState.LiquidVapor)
        {
            _feeds.Add(new StageConnectivity() { Stage = stage, Stream = stream, Phase = phase });
            Connect("Feeds", stream);
            return this;
        }

        public EquilibriumStageSection ConnectLiquidSidestream(MaterialStream stream, int stage, double factor)
        {
            var con = new StageConnectivity() { Stage = stage, Stream = stream, Phase = PhaseState.Liquid };
            con.Factor.SetValue(factor);
            _sidestream.Add(con);
            Connect("Sidestreams", stream);
            return this;
        }

        public EquilibriumStageSection ConnectVaporSidestream(MaterialStream stream, int stage, double factor)
        {
            var con = new StageConnectivity() { Stage = stage, Stream = stream, Phase = PhaseState.Vapor };
            con.Factor.SetValue(factor);
            _sidestream.Add(con);
            Connect("Sidestreams", stream);
            return this;
        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("Feeds");
            var VIn = FindMaterialPort("VIn").Streams[0];
            var LIn = FindMaterialPort("LIn").Streams[0];
            var VOut = FindMaterialPort("VOut").Streams[0];
            var LOut = FindMaterialPort("LOut").Streams[0];
            var Sidestreams = FindMaterialPort("Sidestreams");

            var pscale = 1e4;
            var hscale = 1e3;
            var L0 = LIn.Bulk.TotalMolarflow;
            var x0 = LIn.Bulk.ComponentMolarFraction;
            var VNT = VIn.Bulk.TotalMolarflow;
            var yNT = VIn.Bulk.ComponentMolarFraction;

            ApplyIgnoredComponents();

            /* foreach (var vari in Variables)
             {   
                 if(vari.IsBound)
                     vari.Unbind();
             }*/

            for (var i = 0; i < NumberOfTrays; i++)
            {
                var tray = _trays[i];

                //(M)ass Balance
                var Mi = Sym.Par(1 + _trays[i].RV) * tray.V + Sym.Par(1 + tray.RL) * _trays[i].L;
                if (i == 0)
                    AddEquationToEquationSystem(problem, ((_trays[i + 1].V + L0 + tray.F)) - (Mi));
                else if (i == NumberOfTrays - 1)
                    AddEquationToEquationSystem(problem, ((VNT + _trays[i - 1].L + tray.F)) - (Mi));
                else
                    AddEquationToEquationSystem(problem, ((_trays[i + 1].V + _trays[i - 1].L + tray.F)) - (Mi));

                //Component Massbalance
                for (var comp = 0; comp < NC; comp++)
                {
                    if (!IgnoredComponents.Contains(System.Components[comp]))
                    {
                        var nij = Sym.Par(1 + tray.RV) * tray.V * tray.y[comp] + Sym.Par(1 + tray.RL) * tray.L * tray.x[comp];
                        if (i == 0)
                            AddEquationToEquationSystem(problem, ((L0 * x0[comp] + _trays[i + 1].V * _trays[i + 1].y[comp] + tray.F * tray.z[comp])) - (nij));
                        else if (i == NumberOfTrays - 1)
                            AddEquationToEquationSystem(problem, ((VNT * yNT[comp] + _trays[i - 1].L * _trays[i - 1].x[comp] + tray.F * tray.z[comp])) - (nij));
                        else
                            AddEquationToEquationSystem(problem, ((_trays[i + 1].V * _trays[i + 1].y[comp] + _trays[i - 1].L * _trays[i - 1].x[comp] + tray.F * tray.z[comp])) - (nij));
                    }
                }

                //(E)quilibrium
                for (var comp = 0; comp < NC; comp++)
                {
                    System.EquationFactory.EquilibriumCoefficient(System, tray.K[comp], tray.T, tray.p, tray.x.ToList(), tray.yeq.ToList(), comp);
                    if (!IgnoredComponents.Contains(System.Components[comp]))
                    {
                        AddEquationToEquationSystem(problem, (tray.yeq[comp]) - (tray.K[comp] * tray.x[comp]));
                    }

                }

                //(S)ummation                
                AddEquationToEquationSystem(problem, Sym.Sum(0, NC, (j) => Sym.Par(tray.x[j] - tray.yeq[j])));

                //Ent(H)alpy               
                tray.HL.BindTo(new EnthalpyRoute(System, tray.T, tray.p, tray.x.ToList(), PhaseState.Liquid));
                tray.HV.BindTo(new EnthalpyRoute(System, tray.TV, tray.p, tray.y.ToList(), PhaseState.Vapor));

                var Hi = Sym.Par(Sym.Par(1 + tray.RV) * tray.V * tray.HV + Sym.Par(1 + tray.RL) * tray.L * tray.HL) / hscale;
                if (i == 0)
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i + 1].V * _trays[i + 1].HV + L0 * LIn.Bulk.SpecificEnthalpy + tray.F * tray.HF + tray.Q) / hscale) - (Hi));
                else if (i == NumberOfTrays - 1)
                    AddEquationToEquationSystem(problem, (Sym.Par(VNT * VIn.Bulk.SpecificEnthalpy + _trays[i - 1].L * _trays[i - 1].HL + tray.F * tray.HF + tray.Q) / hscale) - (Hi));
                else
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i + 1].V * _trays[i + 1].HV + _trays[i - 1].L * _trays[i - 1].HL + tray.F * tray.HF + tray.Q) / hscale) - (Hi));


                if (EfficiencyType == StageEfficiencyType.None)
                {
                    AddEquationToEquationSystem(problem, (tray.T / 100) - (tray.TV / 100));
                    for (var comp = 0; comp < NC; comp++)
                    {                      
                        AddEquationToEquationSystem(problem, tray.yeq[comp] - tray.y[comp]);
                    }
                }

                if (EfficiencyType == StageEfficiencyType.Murphree)
                {
                    AddEquationToEquationSystem(problem, (tray.T / 100) - (tray.TV / 100));
                    //tray.TV.BindTo(tray.T);

                    for (var comp = 0; comp < NC; comp++)
                    {
                        tray.yeq[comp].Unbind();
                        if (!IgnoredComponents.Contains(System.Components[comp]))
                        {
                            if (i < NumberOfTrays - 1)
                            {
                                Expression yIN = _trays[i + 1].y[comp];

                                if (_feeds.Any(f => f.Stage == i + 2))
                                {
                                    yIN = (_trays[i + 1].y[comp] * _trays[i + 1].V + _trays[i].z[comp] * _trays[i].F) / (_trays[i + 1].V + _trays[i].F);
                                }

                                AddEquationToEquationSystem(problem, (tray.y[comp]) - (yIN + tray.eps * Sym.Par(_trays[i].yeq[comp] - yIN)));
                            }
                            else
                                AddEquationToEquationSystem(problem, (tray.y[comp]) - (VIn.Bulk.ComponentMolarFraction[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - VIn.Bulk.ComponentMolarFraction[comp])));
                        }
                    }
                }

                if (EfficiencyType == StageEfficiencyType.ExtendedMurphree)
                {
                    // tray.TV.Unbind();
                    if (i < NumberOfTrays - 1)
                    {
                        AddEquationToEquationSystem(problem, (tray.TV) - (_trays[i + 1].TV + tray.eps * Sym.Par(_trays[i].T - _trays[i + 1].TV)));
                    }
                    else
                        AddEquationToEquationSystem(problem, (tray.TV) - (VIn.Temperature + tray.eps * Sym.Par(_trays[i].T - VIn.Temperature)));

                    for (var comp = 0; comp < NC; comp++)
                    {
                        tray.yeq[comp].Unbind();
                        if (!IgnoredComponents.Contains(System.Components[comp]))
                        {
                            if (i < NumberOfTrays - 1)
                                AddEquationToEquationSystem(problem, (tray.y[comp]) - (_trays[i + 1].y[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - _trays[i + 1].y[comp])));
                            else
                                AddEquationToEquationSystem(problem, (tray.y[comp]) - (VIn.Bulk.ComponentMolarFraction[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - VIn.Bulk.ComponentMolarFraction[comp])));
                        }
                    }
                }

                if (EfficiencyType == StageEfficiencyType.BausaMurphree)
                {
                    // tray.TV.Unbind();
                    if (i < NumberOfTrays - 1)
                    {
                        Expression TIN = _trays[i + 1].TV;

                        if (_feeds.Any(f => f.Stage == i + 1 && f.Phase == PhaseState.Vapor))
                        {
                            var nextFeed = _feeds.FirstOrDefault(f => f.Stage == i + 1 && f.Phase == PhaseState.Vapor);
                            TIN = (_trays[i + 1].T * _trays[i + 1].V + nextFeed.Stream.Temperature * _trays[i].F) / (_trays[i + 1].V + _trays[i].F);
                        }
                        AddEquationToEquationSystem(problem, (tray.TV) - (TIN + tray.eps * Sym.Par(_trays[i].T - TIN)));
                    }
                    else
                        AddEquationToEquationSystem(problem, (tray.TV) - (VIn.Temperature + tray.eps * Sym.Par(_trays[i].T - VIn.Temperature)));

                    for (var comp = 0; comp < NC; comp++)
                    {
                        tray.yeq[comp].Unbind();
                        if (!IgnoredComponents.Contains(System.Components[comp]))
                        {
                            if (i < NumberOfTrays - 1)
                            {
                                Expression yIN = _trays[i + 1].y[comp];

                                if (_feeds.Any(f => f.Stage == i + 1 && f.Phase == PhaseState.Vapor))
                                {
                                    yIN = (_trays[i + 1].y[comp] * _trays[i + 1].V + _trays[i].z[comp] * _trays[i].F) / (_trays[i + 1].V + _trays[i].F);
                                }
                                AddEquationToEquationSystem(problem, (tray.y[comp]) - (yIN + tray.eps * Sym.Par(_trays[i].yeq[comp] - yIN)));
                            }
                            else
                                AddEquationToEquationSystem(problem, (tray.y[comp]) - (VIn.Bulk.ComponentMolarFraction[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - VIn.Bulk.ComponentMolarFraction[comp])));
                        }
                    }
                }


                //Pressure profile
                if (i == NumberOfTrays - 1)
                    AddEquationToEquationSystem(problem, (tray.p / pscale) - ((VIn.Pressure - _trays[NumberOfTrays - 1].DP) / pscale));
                else
                    AddEquationToEquationSystem(problem, (tray.p / pscale) - (Sym.Par(_trays[i + 1].p - _trays[i].DP) / pscale));

            }

            for (var comp = 0; comp < NC; comp++)
            {
                AddEquationToEquationSystem(problem, VOut.Bulk.ComponentMolarflow[comp] - (_trays[0].V * _trays[0].y[comp]));
                AddEquationToEquationSystem(problem, LOut.Bulk.ComponentMolarflow[comp] - (_trays[NumberOfTrays - 1].L * _trays[NumberOfTrays - 1].x[comp]));
            }
            AddEquationToEquationSystem(problem, (VOut.Bulk.SpecificEnthalpy / 1e3) - (_trays[0].HV / 1e3));
            AddEquationToEquationSystem(problem, VOut.Pressure - (_trays[0].p));
            AddEquationToEquationSystem(problem, LOut.Temperature - (_trays[NumberOfTrays - 1].T));
            AddEquationToEquationSystem(problem, LOut.Pressure - (_trays[NumberOfTrays - 1].p));

            foreach (var feed in _feeds)
            {
                _trays[feed.Stage - 1].HF.Unfix();
                _trays[feed.Stage - 1].F.Unfix();
                _trays[feed.Stage - 1].HF.IsConstant = false;
                _trays[feed.Stage - 1].F.IsConstant = false;

                _trays[feed.Stage - 1].HF.SetValue(feed.Stream.Bulk.SpecificEnthalpy.Val());
                _trays[feed.Stage - 1].F.SetValue(feed.Stream.Bulk.TotalMolarflow.Val());

                AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].F) - (feed.Stream.Bulk.TotalMolarflow));
                AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].HF / 1e4) - (feed.Stream.Bulk.SpecificEnthalpy / 1e4));


                for (var comp = 0; comp < NC; comp++)
                {
                    _trays[feed.Stage - 1].z[comp].Unfix();
                    _trays[feed.Stage - 1].z[comp].IsConstant = false;

                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].z[comp]) - (feed.Stream.Bulk.ComponentMolarFraction[comp]));
                    _trays[feed.Stage - 1].z[comp].SetValue(feed.Stream.Bulk.ComponentMolarFraction[comp].Val());

                }
            }

            foreach (var feed in _sidestream)
            {
                if (feed.Phase == PhaseState.Liquid)
                {

                    _trays[feed.Stage - 1].W.Unfix();
                    _trays[feed.Stage - 1].RL.Unfix();
                    _trays[feed.Stage - 1].RL.SetValue(feed.Factor.Val());
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].RL) - (feed.Factor));
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].W) - (_trays[feed.Stage - 1].RL * _trays[feed.Stage - 1].L));
                    _trays[feed.Stage - 1].W.SetValue((_trays[feed.Stage - 1].RL.Val() * _trays[feed.Stage - 1].L.Val()));
                    _trays[feed.Stage - 1].RL.SetValue(feed.Factor.Val());

                    AddEquationToEquationSystem(problem, feed.Stream.Pressure - (_trays[feed.Stage - 1].p));
                    AddEquationToEquationSystem(problem, feed.Stream.Temperature - (_trays[feed.Stage - 1].T));
                    for (var comp = 0; comp < NC; comp++)
                    {
                        AddEquationToEquationSystem(problem, feed.Stream.Bulk.ComponentMolarflow[comp] - (_trays[feed.Stage - 1].W * _trays[feed.Stage - 1].x[comp]));
                    }
                }
            }
            base.CreateEquations(problem);
        }

        public EquilibriumStageSection Ignore(params string[] components)
        {
            foreach (var compId in components)
            {
                var comp = System.Components.FirstOrDefault(c => c.ID == compId);
                if (comp != null)
                    IgnoredComponents.Add(comp);
            }
            return this;
        }
        public EquilibriumStageSection MakeAdiabatic()
        {
            foreach (var tray in _trays)
            {
                tray.Q.Fix(0);
            }

            return this;
        }

        public EquilibriumStageSection MakeIsobaric()
        {
            foreach (var tray in _trays)
            {
                tray.DP.Fix(0);
            }
            return this;
        }

        public EquilibriumStageSection FixStageEfficiency(double value)
        {
            foreach (var tray in _trays)
            {
                tray.eps.Fix(value);
            }
            return this;
        }


        void ApplyIgnoredComponents()
        {
            int NT = (int)(NumberOfTrays);

            for (int c = 0; c < System.Components.Count; c++)
            {
                if (IgnoredComponents.Contains(System.Components[c]))
                {
                    for (int i = 0; i < NT; i++)
                    {
                        _trays[i].x[c].Fix(0);
                        _trays[i].y[c].Fix(0);
                        _trays[i].yeq[c].Fix(0);

                        _trays[i].x[c].IsConstant = true;
                        _trays[i].y[c].IsConstant = true;
                        _trays[i].yeq[c].IsConstant = true;
                    }
                }
            }
        }

        void InitAbsorber()
        {
            /* int NC = System.Components.Count;

             var VIn = FindMaterialPort("VIn");
             var LIn = FindMaterialPort("LIn");
             var VOut = FindMaterialPort("VOut");
             var LOut = FindMaterialPort("LOut");

             var TTop = LIn.Streams[0].Temperature.Val();
             var TBot = VIn.Streams[0].Temperature.Val();

             for (int i = 0; i < NumberOfTrays; i++)
             {
                 _trays[i].T.SetValue(TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i);
                 _trays[i].TV.SetValue(TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i);

                 if (i == 0)
                     _trays[i].L.SetValue(LIn.Streams[0].Bulk.TotalMolarflow.Val());
                 else
                     _trays[i].L.SetValue(_trays[i - 1].L.Val());

                 _trays[i].V.SetValue(VIn.Streams[0].Bulk.TotalMolarflow.Val());
                 _trays[i].HV.SetValue(VIn.Streams[0].Bulk.SpecificEnthalpy.Val());
                 _trays[i].HL.SetValue(LIn.Streams[0].Bulk.SpecificEnthalpy.Val());

                 for (int j = 0; j < System.Components.Count; j++)

                 {
                     _trays[i].x[j].SetValue(LIn.Streams[0].Bulk.ComponentMolarFraction[j].Val());
                     _trays[i].y[j].SetValue(VIn.Streams[0].Bulk.ComponentMolarFraction[j].Val());
                 }
             }

             for (int i = NumberOfTrays - 1; i >= 0; i--)
             {
                 if (i < NumberOfTrays - 1)
                     _trays[i].p.SetValue(_trays[i + 1].p.Val() - _trays[i + 1].DP.Val());
                 else
                     _trays[i].p.SetValue(VIn.Streams[0].Pressure.Val());
             }

             VOut.Streams[0].Temperature.SetValue(_trays[0].T.Val());
             VOut.Streams[0].Pressure.SetValue(_trays[0].p.Val() - _trays[0].DP.Val());

             LOut.Streams[0].Temperature.SetValue(_trays[NumberOfTrays - 1].T.Val());
             LOut.Streams[0].Pressure.SetValue(_trays[NumberOfTrays - 1].p.Val());

             for (int j = 0; j < System.Components.Count; j++)
             {
                 VOut.Streams[0].Bulk.ComponentMolarflow[j].SetValue(_trays[0].V.Val() * _trays[0].y[j].Val());
                 LOut.Streams[0].Bulk.ComponentMolarflow[j].SetValue(_trays[NumberOfTrays - 1].L.Val() * _trays[NumberOfTrays - 1].x[j].Val());
             }

             InitOutlets();*/

            int NC = System.Components.Count;
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");
            var Sidestreams = FindMaterialPort("Sidestreams");

            var feedcopy = new MaterialStream("FC", System);
            feedcopy.CopyFrom(LIn.Streams[0]);
            for (int i = 0; i < System.GetNumberOfComponents(); i++)
            {
                feedcopy.Bulk.ComponentMolarflow[i].SetValue(feedcopy.Bulk.ComponentMolarflow[i].Val() + VIn.Streams[0].Bulk.ComponentMolarflow[i].Val());
            }
            feedcopy.InitializeFromMolarFlows();
            feedcopy.VaporFraction.SetValue(0.01);
            feedcopy.FlashPZ();

            double[] a = new double[NC];
            double Kmin = feedcopy.KValues.Min(v => v.Val());

            for (int i = 0; i < NC; i++)
            {
                a[i] = feedcopy.KValues[i].Val() / Kmin;
            }

            // var F = In.Streams[0].Bulk.TotalMolarflow.Val();
            //  var D = F * distillateToFeed;
            var V = VIn.Streams[0].Bulk.TotalMolarflow.Val();
            var Lr = LIn.Streams[0].Bulk.TotalMolarflow.Val();

            for (int i = 0; i < NumberOfTrays; i++)
            {
                _trays[i].V.SetValue(V);

                _trays[i].L.SetValue(Lr);


                for (int c = 0; c < NC; c++)
                {
                    _trays[i].x[c].SetValue(1.0 / (double)NC);
                    _trays[i].y[c].SetValue(1.0 / (double)NC);
                }

            }
            _trays[0].F.SetValue(Lr);
            _trays[NumberOfTrays - 1].F.SetValue(V);
            for (var comp = 0; comp < NC; comp++)
            {
                _trays[0].z[comp].SetValue(LIn.Streams[0].Bulk.ComponentMolarFraction[comp].Val());
                _trays[NumberOfTrays - 1].z[comp].SetValue(VIn.Streams[0].Bulk.ComponentMolarFraction[comp].Val());

            }

            var eq = new AlgebraicSystem("Column Init");

            for (int j = 0; j < NumberOfTrays; j++)
            {
                var sumax = Sym.Sum(0, NC, (k) => a[k] * _trays[j].x[k]);

                for (var i = 0; i < NC; i++)
                {
                    Expression Vip1 = 0;
                    Expression Lim1 = 0;

                    if (j < NumberOfTrays - 1)
                        Vip1 = _trays[j + 1].V * a[i] * _trays[j + 1].x[i] / Sym.Sum(0, NC, (k) => a[k] * _trays[j + 1].x[k]);
                    else
                        Vip1 = 0;// VIn.Streams[0].Bulk.TotalMolarflow * VIn.Streams[0].Bulk.ComponentMolarFraction[i];

                    if (j > 0)
                        Lim1 = _trays[j - 1].L * _trays[j - 1].x[i];
                    else
                        Lim1 = 0;// LIn.Streams[0].Bulk.TotalMolarflow * LIn.Streams[0].Bulk.ComponentMolarFraction[i];


                    var Mij = _trays[j].F * _trays[j].z[i] - _trays[j].V * a[i] * _trays[j].x[i] / (sumax) - _trays[j].L * _trays[j].x[i];

                    eq.AddEquation(new Equation(Vip1 + Lim1 + Mij));
                    // _trays[j].x[i].LowerBound = -1000;
                    // _trays[j].x[i].UpperBound = +1000;
                }
                eq.AddVariables(_trays[j].x);
            }
            var solver = new DecompositionSolver(new NoLogger());
            var status = solver.Solve(eq);

            if (status)
            {
                for (int j = 0; j < NumberOfTrays; j++)
                {
                    var sumax = Sym.Sum(0, NC, (k) => a[k] * _trays[j].x[k]);

                    for (var i = 0; i < NC; i++)
                    {
                        _trays[j].y[i].SetValue(a[i] * _trays[j].x[i].Val() / (sumax.Val()));
                        _trays[j].yeq[i].SetValue(a[i] * _trays[j].x[i].Val() / (sumax.Val()));

                        feedcopy.Bulk.ComponentMolarFraction[i].SetValue(_trays[j].x[i].Val());
                    }
                    feedcopy.Pressure.SetValue(_trays[j].p.Val());
                    feedcopy.FlashPZ();
                    _trays[j].T.SetValue(feedcopy.Temperature.Val());
                    _trays[j].TV.SetValue(feedcopy.Temperature.Val());
                }
                _trays[0].F.SetValue(0);
                _trays[NumberOfTrays - 1].F.SetValue(0);

            }
            InitOutlets();
        }

        void InitRectifaction()
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("Feeds");
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");
            var Sidestreams = FindMaterialPort("Sidestreams");

            #region Old Style
            /*
            var TTop = LIn.Streams[0].Temperature.Val();
            var TBot = VIn.Streams[0].Temperature.Val();


            var feedcopy = new MaterialStream("FC", In.Streams[0].System);
            feedcopy.CopyFrom(In.Streams[0]);
            feedcopy.VaporFraction.SetValue(0.01);
            feedcopy.FlashPZ();
            TTop = feedcopy.Temperature.Val();
            feedcopy.VaporFraction.SetValue(0.99);
            feedcopy.FlashPZ();
            TBot = feedcopy.Temperature.Val();

            feedcopy.VaporFraction.SetValue(0.2);
            feedcopy.FlashPZ();

            foreach (var feed in _feeds)
            {
                _trays[feed.Stage - 1].HF.SetValue(feed.Stream.Bulk.SpecificEnthalpy.Val());
                _trays[feed.Stage - 1].F.SetValue(feed.Stream.Bulk.TotalMolarflow.Val());
                _trays[feed.Stage - 1].HF.IsConstant = false;
                _trays[feed.Stage - 1].F.IsConstant = false;
                for (var comp = 0; comp < NC; comp++)
                {
                    _trays[feed.Stage - 1].z[comp].SetValue(feed.Stream.Bulk.ComponentMolarFraction[comp].Val());
                    _trays[feed.Stage - 1].z[comp].IsConstant = false;
                }
            }

            for (int i = 0; i < NumberOfTrays; i++)
            {
                _trays[i].T.SetValue(TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i);
                _trays[i].TV.SetValue(TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i);

                if (i == 0)
                    _trays[i].L.SetValue(LIn.Streams[0].Bulk.TotalMolarflow.Val());
                else
                    _trays[i].L.SetValue(_trays[i - 1].L.Val() + _trays[i].F.Val());

                _trays[i].V.SetValue(VIn.Streams[0].Bulk.TotalMolarflow.Val());

                for (int j = 0; j < System.Components.Count; j++)
                {
                    _trays[i].x[j].SetValue(feedcopy.Liquid.ComponentMolarFraction[j].Val());
                    _trays[i].y[j].SetValue(feedcopy.Vapor.ComponentMolarFraction[j].Val());
                    _trays[i].yeq[j].SetValue(feedcopy.Vapor.ComponentMolarFraction[j].Val());
                }
            }

            for (int i = NumberOfTrays - 1; i >= 0; i--)
            {
                if (i < NumberOfTrays - 1)
                    _trays[i].p.SetValue(_trays[i + 1].p.Val() - _trays[i + 1].DP.Val());
                else
                    _trays[i].p.SetValue(VIn.Streams[0].Pressure.Val());
            }
            */
            #endregion

            InitOutlets();

        }

        void InitOutlets()
        {
            int NC = System.Components.Count;



            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");

            VOut.Streams[0].Temperature.SetValue(_trays[0].T.Val());
            VOut.Streams[0].Pressure.SetValue(_trays[0].p.Val() - _trays[0].DP.Val());

            LOut.Streams[0].Temperature.SetValue(_trays[NumberOfTrays - 1].T.Val());
            LOut.Streams[0].Pressure.SetValue(_trays[NumberOfTrays - 1].p.Val());

            for (int j = 0; j < System.Components.Count; j++)
            {
                VOut.Streams[0].Bulk.ComponentMolarflow[j].SetValue(_trays[0].V.Val() * _trays[0].y[j].Val());
                LOut.Streams[0].Bulk.ComponentMolarflow[j].SetValue(_trays[NumberOfTrays - 1].L.Val() * _trays[NumberOfTrays - 1].x[j].Val());
            }

            LOut.Streams[0].InitializeFromMolarFlows();
            VOut.Streams[0].InitializeFromMolarFlows();

            VOut.Streams[0].VaporFraction.SetValue(1.0);
            LOut.Streams[0].VaporFraction.SetValue(0.0);
            VOut.Streams[0].FlashPT();
            LOut.Streams[0].FlashPT();

            // VOut.Streams[0].State = PhaseState.DewPoint;
            LOut.Streams[0].State = PhaseState.BubblePoint;

            foreach (var feed in _sidestream)
            {
                if (feed.Phase == PhaseState.Liquid)
                {
                    _trays[feed.Stage - 1].RL.SetValue(feed.Factor.Val());
                    _trays[feed.Stage - 1].W.SetValue((_trays[feed.Stage - 1].RL.Val() * _trays[feed.Stage - 1].L.Val()));

                    feed.Stream.Temperature.SetValue(_trays[feed.Stage - 1].T.Val());
                    feed.Stream.Pressure.SetValue(_trays[feed.Stage - 1].p.Val());
                    feed.Stream.Bulk.TotalMolarflow.SetValue(_trays[feed.Stage - 1].W.Val());
                    for (var comp = 0; comp < NC; comp++)
                    {
                        feed.Stream.Bulk.ComponentMolarflow[comp].SetValue(_trays[feed.Stage - 1].x[comp].Val() * _trays[feed.Stage - 1].W.Val());
                    }
                    feed.Stream.InitializeFromMolarFlows();
                    feed.Stream.FlashPT();
                }
            }
        }

        public ProcessUnit Initialize(double refluxRatio, double distillateToFeed, ILogger logger)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("Feeds");
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");
            var Sidestreams = FindMaterialPort("Sidestreams");

            var feedcopy = new MaterialStream("FC", In.Streams[0].System);
            feedcopy.CopyFrom(In.Streams[0]);
            feedcopy.VaporFraction.SetValue(0.01);
            feedcopy.FlashPZ();

            double[] a = new double[NC];
            double Kmin = feedcopy.KValues.Min(v => v.Val());

            for (int i = 0; i < NC; i++)
            {
                a[i] = feedcopy.KValues[i].Val() / Kmin;
            }

            var F = In.Streams[0].Bulk.TotalMolarflow.Val();
            var D = F * distillateToFeed;
            var V = (refluxRatio + 1) * D;
            var Lr = refluxRatio * D;
            var Ls = Lr + F;

            for (int i = 0; i < NumberOfTrays; i++)
            {
                _trays[i].V.SetValue(V);
                if (i < _feeds.First().Stage - 1)
                    _trays[i].L.SetValue(Lr);
                else
                    _trays[i].L.SetValue(Ls);

                for (int c = 0; c < NC; c++)
                {
                    _trays[i].x[c].SetValue(1.0 / (double)NC);
                    _trays[i].y[c].SetValue(1.0 / (double)NC);
                }

            }

            foreach (var feed in _feeds)
            {
                _trays[feed.Stage - 1].F.SetValue(feed.Stream.Bulk.TotalMolarflow.Val());
                _trays[feed.Stage - 1].F.IsConstant = true;

                for (var comp = 0; comp < NC; comp++)
                {
                    _trays[feed.Stage - 1].z[comp].SetValue(feed.Stream.Bulk.ComponentMolarFraction[comp].Val());
                    _trays[feed.Stage - 1].z[comp].IsConstant = true;
                }
            }

            _trays[0].F.SetValue(Lr);
            _trays[NumberOfTrays - 1].F.SetValue(V);
            for (var comp = 0; comp < NC; comp++)
            {
                _trays[0].z[comp].SetValue(LIn.Streams[0].Bulk.ComponentMolarFraction[comp].Val());
                _trays[NumberOfTrays - 1].z[comp].SetValue(VIn.Streams[0].Bulk.ComponentMolarFraction[comp].Val());

            }

            var eq = new AlgebraicSystem("Column Init");

            for (int j = 0; j < NumberOfTrays; j++)
            {
                var sumax = Sym.Sum(0, NC, (k) => a[k] * _trays[j].x[k]);

                for (var i = 0; i < NC; i++)
                {
                    Expression Vip1 = 0;
                    Expression Lim1 = 0;

                    if (j < NumberOfTrays - 1)
                        Vip1 = _trays[j + 1].V * a[i] * _trays[j + 1].x[i] / Sym.Sum(0, NC, (k) => a[k] * _trays[j + 1].x[k]);
                    else
                        Vip1 = 0;// VIn.Streams[0].Bulk.TotalMolarflow * VIn.Streams[0].Bulk.ComponentMolarFraction[i];

                    if (j > 0)
                        Lim1 = _trays[j - 1].L * _trays[j - 1].x[i];
                    else
                        Lim1 = 0;// LIn.Streams[0].Bulk.TotalMolarflow * LIn.Streams[0].Bulk.ComponentMolarFraction[i];


                    var Mij = _trays[j].F * _trays[j].z[i] - _trays[j].V * a[i] * _trays[j].x[i] / (sumax) - _trays[j].L * _trays[j].x[i];

                    eq.AddEquation(new Equation(Vip1 + Lim1 + Mij));
                    // _trays[j].x[i].LowerBound = -1000;
                    // _trays[j].x[i].UpperBound = +1000;
                }
                eq.AddVariables(_trays[j].x);
            }
            var solver = new DecompositionSolver(logger);
            var status = solver.Solve(eq);

            if (status)
            {
                for (int j = 0; j < NumberOfTrays; j++)
                {
                    var sumax = Sym.Sum(0, NC, (k) => a[k] * _trays[j].x[k]);

                    for (var i = 0; i < NC; i++)
                    {
                        _trays[j].y[i].SetValue(a[i] * _trays[j].x[i].Val() / (sumax.Val()));
                        _trays[j].yeq[i].SetValue(a[i] * _trays[j].x[i].Val() / (sumax.Val()));

                        feedcopy.Bulk.ComponentMolarFraction[i].SetValue(_trays[j].x[i].Val());
                    }
                    feedcopy.Pressure.SetValue(_trays[j].p.Val());
                    feedcopy.FlashPZ();
                    _trays[j].T.SetValue(feedcopy.Temperature.Val());
                    _trays[j].TV.SetValue(feedcopy.Temperature.Val());
                }
                _trays[0].F.SetValue(0);
                _trays[NumberOfTrays - 1].F.SetValue(0);

            }
            InitOutlets();
            return this;
        }

        public override ProcessUnit Initialize()
        {

            int NC = System.Components.Count;
            var In = FindMaterialPort("Feeds");
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");
            var Sidestreams = FindMaterialPort("Sidestreams");

            if (LIn.IsConnected == false)
                throw new InvalidOperationException("LIn must be connected");
            if (VIn.IsConnected == false)
                throw new InvalidOperationException("VIn must be connected");
            if (LOut.IsConnected == false)
                throw new InvalidOperationException("LOut must be connected");
            if (VOut.IsConnected == false)
                throw new InvalidOperationException("VOut must be connected");

            if (In.IsConnected)
                InitRectifaction();
            else
                InitAbsorber();

            return this;


        }
    }
}
