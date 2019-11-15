using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Numerics;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting.Documentation;

namespace MiniSim.Core.Flowsheeting
{
    public class Flowsheet : BaseSimulationElement
    {
        List<ProcessUnit> _units = new List<ProcessUnit>();
        List<MaterialStream> _materialStreams = new List<MaterialStream>();
        List<HeatStream> _heatStreams = new List<HeatStream>();
        List<Equation> _designSpecifications = new List<Equation>();
        List<DocumentationElement> _documentation = new List<DocumentationElement>();


        public List<ProcessUnit> Units
        {
            get
            {
                return _units;
            }

            set
            {
                _units = value;
            }
        }

        public List<MaterialStream> MaterialStreams
        {
            get
            {
                return _materialStreams;
            }

            set
            {
                _materialStreams = value;
            }
        }

        public List<HeatStream> HeatStreams
        {
            get
            {
                return _heatStreams;
            }

            set
            {
                _heatStreams = value;
            }
        }

        public List<Equation> DesignSpecifications
        {
            get
            {
                return _designSpecifications;
            }

            set
            {
                _designSpecifications = value;
            }
        }

        public List<DocumentationElement> Documentation { get => _documentation; set => _documentation = value; }

        public Flowsheet(string name) 
        {
            Name = name;
        }

        public Flowsheet AddDesignSpecification(string name, Equation eq)
        {
            eq.Name = name;
            eq.Group = "Design Spec";

            AddDesignSpecification(eq);
            return this;
        }
        public Flowsheet AddDesignSpecification(string name, Equation eq, string description)
        {
            eq.Name = name;
            eq.Group = "Design Spec";
            eq.Description = description;
            AddDesignSpecification(eq);
            return this;
        }

        public Flowsheet AddDesignSpecification(Equation eq)
        {
            if (!DesignSpecifications.Contains(eq))
            {
                eq.ModelClass = "Flowsheet";
                DesignSpecifications.Add(eq);
            }
            else
                throw new InvalidOperationException("Design spec " + eq.Name + " already included in flowsheet");

            return this;
        }

        public Flowsheet ReplaceDesignSpecification(string name, Equation eq)
        {
            var spec = DesignSpecifications.FirstOrDefault(s => s.Name == name);
            if (spec != null)
            {
                DesignSpecifications.Remove(spec);
                eq.Name = name;
                eq.Group = "Design Spec";
                AddDesignSpecification(eq);
            }

            return this;
        }


        public Flowsheet RemoveDesignSpecification(string name)
        {
            var spec = DesignSpecifications.FirstOrDefault(eq => eq.Name == name);
            if (spec != null)
                DesignSpecifications.Remove(spec);

            return this;
        }
        public Flowsheet RemoveDesignSpecification(Equation eq)
        {
            if (DesignSpecifications.Contains(eq))
                DesignSpecifications.Remove(eq);
            return this;
        }

       
        public Flowsheet AddUnits(params ProcessUnit[] units)
        {
            foreach (var unit in units)
                AddUnit(unit);

            return this;
        }

        public Flowsheet AddUnit(ProcessUnit unit)
        {
            if (!Units.Contains(unit))
                Units.Add(unit);
            else
                throw new InvalidOperationException("Unit " + unit.Name + " already included in flowsheet");
            return this;
        }

        public Flowsheet AddMaterialStreams(params MaterialStream[] streams)
        {
            foreach (var stream in streams)
                AddMaterialStream(stream);
            return this;
        }

        public Flowsheet RemoveMaterialStream(MaterialStream stream)
        {
            if (MaterialStreams.Contains(stream))
                MaterialStreams.Remove(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " not included in flowsheet");
            return this;

        }
        public Flowsheet AddMaterialStream(MaterialStream stream)
        {
            if (!MaterialStreams.Contains(stream))
                MaterialStreams.Add(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " already included in flowsheet");
            return this;
        }
        public Flowsheet AddHeatStreams(params HeatStream[] streams)
        {
            foreach (var stream in streams)
                AddHeatStream(stream);
            return this;
        }
        public Flowsheet AddHeatStream(HeatStream stream)
        {
            if (!HeatStreams.Contains(stream))
                HeatStreams.Add(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " already included in flowsheet");
            return this;
        }

        public Flowsheet Merge(Flowsheet other)
        {
            foreach (var unit in other.Units)
                AddUnit(unit);
            foreach (var stream in other.MaterialStreams)
                AddMaterialStream(stream);
            foreach (var stream in other.HeatStreams)
                AddHeatStream(stream);
            foreach (var spec in other.DesignSpecifications)
                AddDesignSpecification(spec);
            return this;
        }
        public ProcessUnit GetUnit(string name)
        {
            return Units.FirstOrDefault(v => v.Name == name);
        }
        public MaterialStream GetMaterialStream(string name)
        {
            return MaterialStreams.FirstOrDefault(v => v.Name == name);
        }
        public override void CreateEquations(AlgebraicSystem problem)
        {
            foreach (var stream in MaterialStreams)
                stream.CreateEquations(problem);

            foreach (var stream in HeatStreams)
                stream.CreateEquations(problem);

            foreach (var unit in Units)
                unit.CreateEquations(problem);

            foreach (var spec in DesignSpecifications)
                AddEquationToEquationSystem(problem, spec);

            base.CreateEquations(problem);
        }
    }
}
