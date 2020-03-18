using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Numerics;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;

namespace MiniSim.Core.ModelLibrary
{
    public class Source : ProcessUnit
    {
        MaterialStream _stream;

        public Source(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Source";

            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1) { WidthFraction = 1, HeightFraction = 0.5, Normal = PortNormal.Right });

            _stream = new MaterialStream("VLEQ", system);

            AddVariables(_stream.Variables);

            Parameters.Add(_stream.Temperature);
            Parameters.Add(_stream.Pressure);
            Parameters.Add(_stream.VaporFraction);
            Parameters.AddRange(_stream.Bulk.ComponentMolarflow);
        }


        public override ProcessUnit ApplyDefaultSpecifications()
        {
            _stream.Temperature.Fix(298.15);
            _stream.Pressure.Fix();
            foreach (var flow in _stream.Bulk.ComponentMolarflow)
                flow.Fix(1);

            return this;
        }


        public override void CreateEquations(AlgebraicSystem problem)
        {
            foreach (var vari in Variables)
            {
                vari.Children.Clear();
            }

            _stream.CreateEquations(problem);

            int NC = System.Components.Count;
            var Out = FindMaterialPort("Out");

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem, Out.Streams[0].Bulk.ComponentMolarflow[cindex] - _stream.Bulk.ComponentMolarflow[cindex], "Mass Balance");
            }
            AddEquationToEquationSystem(problem, (Out.Streams[0].Pressure / 1e4) - (Out.Streams[0].Pressure / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Out.Streams[0].Temperature / 1e3) - (Out.Streams[0].Temperature / 1e3), "Temperature Balance");
            // base.CreateEquations(problem);

        }

        public override ProcessUnit Initialize()
        {

            var Out = FindMaterialPort("Out");
            int NC = System.Components.Count;

            if (_stream.Bulk.ComponentMolarflow.Any(v => v.IsFixed))
                _stream.InitializeFromMolarFlows();
            else if (_stream.Bulk.ComponentMassflow.Any(v => v.IsFixed))
                _stream.InitializeFromMassFlows();
            else
                _stream.InitializeFromMolarFractions();

            _stream.FlashPT();

            Out.Streams[0].Pressure.SetValue(_stream.Pressure.Val());
            Out.Streams[0].Temperature.SetValue(_stream.Temperature.Val());

            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Bulk.ComponentMolarflow[i].SetValue(_stream.Bulk.ComponentMolarflow[i].Val());
            }
            Out.Streams[0].InitializeFromMolarFlows();
            Out.Streams[0].FlashPT();

            return this;
        }

    }
}
