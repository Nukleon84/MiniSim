using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Flowsheeting.Documentation
{
    public class StreamTableElement : DocumentationElement
    {
        List<MaterialStream> _streams = new List<MaterialStream>();
        bool _removeMissingComponents = true;
        bool _printMolarVariables = false;
        public List<MaterialStream> Streams
        {
            get
            {
                return _streams;
            }

            set
            {
                _streams = value;
            }
        }

        public DataView StreamTableView
        {
            get
            {
                if (_streamTableData != null)
                    return _streamTableData.DefaultView;
                else
                    return Render().DefaultView;
            }
        }

        public bool RemoveMissingComponents
        {
            get
            {
                return _removeMissingComponents;
            }

            set
            {
                _removeMissingComponents = value;
            }
        }

        public bool PrintMolarVariables
        {
            get
            {
                return _printMolarVariables;
            }

            set
            {
                _printMolarVariables = value;
            }
        }

        DataTable _streamTableData;



        string GetStreamProperty(string property, MaterialStream stream)
        {
            // var connection = visual.GetStreamByName(stream.Name);

            switch (property)
            {
                case "Name":
                    return stream.Name;
                /* case "From Unit":
                     if (connection != null)
                     {
                         if (connection.Source == null)
                             return "";
                         if (connection.Source.Owner != null)
                             return connection.Source.Owner.Name;
                     }
                     return "";
                 case "From Port":
                     if (connection != null)
                     {
                         if (connection.Source != null)
                             return connection.Source.Name;
                     }
                     return "";
                 case "To Unit":
                     if (connection != null)
                     {
                         if (connection.Sink == null)
                             return "";
                         if (connection.Sink.Owner != null)
                             return connection.Sink.Owner.Name;
                     }
                     return "";
                 case "To Port":
                     if (connection != null)
                     {
                         if (connection.Sink != null)
                             return connection.Sink.Name;
                     }
                     return "";
                     */
                case "Temperature":
                    return stream.Temperature.DisplayValue.ToString("G4");
                case "Pressure":
                    return stream.Pressure.DisplayValue.ToString("G4");
                case "Vapour Fraction":
                    return stream.VaporFraction.DisplayValue.ToString("P1");
                case "Enthalpy":
                    return stream.Bulk.SpecificEnthalpy.DisplayValue.ToString("G4");

            }

            return "";
        }


        public DataTable Render()
        {
            _streamTableData = new DataTable();

            if (Streams.Count == 0)
                return _streamTableData;


            // _streamTableData.Columns.Add("Number", typeof(Int32));
            _streamTableData.Columns.Add("Property");
            _streamTableData.Columns.Add("Unit");

            var system = Streams.First().System;

            foreach (var stream in Streams.Distinct())
            {
                var col = _streamTableData.Columns.Add(stream.Name);
            }

            List<string> properties = new List<string> { "Temperature", "Pressure", "Enthalpy", "Vapour Fraction" };
            //List<string> properties = new List<string> { "Name", "From Unit", "From Port", "To Unit", "To Port", "", "Temperature", "Pressure", "Enthalpy", "Vapour Fraction" };
            List<string> propertyUnits = new List<string> {
                system.VariableFactory.Output.UnitDictionary[UnitsOfMeasure.PhysicalDimension.Temperature].Symbol,
                system.VariableFactory.Output.UnitDictionary[UnitsOfMeasure.PhysicalDimension.Pressure].Symbol,
                system.VariableFactory.Output.UnitDictionary[UnitsOfMeasure.PhysicalDimension.SpecificMolarEnthalpy].Symbol, "" };

            int i = 0;
            DataRow workRow;

            for (i = 0; i < properties.Count; i++)
            {
                workRow = _streamTableData.NewRow();

                //workRow["Number"] = i;
                workRow["Property"] = properties[i];
                workRow["Unit"] = propertyUnits[i];

                foreach (var stream in Streams)
                {
                    workRow[stream.Name] = GetStreamProperty(properties[i], stream);
                }
                _streamTableData.Rows.Add(workRow);
            }

            workRow = _streamTableData.NewRow();
            // workRow["Number"] = i++;
            workRow["Property"] = "Phase";
            workRow["Unit"] = "";
            foreach (var stream in Streams)
            {
                workRow[stream.Name] = stream.State;
            }
            _streamTableData.Rows.Add(workRow);

            workRow = _streamTableData.NewRow();
            // workRow["Number"] = i++;
            _streamTableData.Rows.Add(workRow);

            var components = system.Components;

            // Molar flows
            if (PrintMolarVariables)
            {
                workRow = _streamTableData.NewRow();
                //   workRow["Number"] = i++;
                workRow["Property"] = "Total Molar Flow";
                workRow["Unit"] = system.VariableFactory.Output.UnitDictionary[UnitsOfMeasure.PhysicalDimension.MolarFlow].Symbol;
                foreach (var stream in Streams)
                {
                    workRow[stream.Name] = stream.Bulk.TotalMolarflow.DisplayValue.ToString("0.000");
                }
                _streamTableData.Rows.Add(workRow);

                workRow = _streamTableData.NewRow();
                // workRow["Number"] = i++;
                workRow["Property"] = "";// "Component Molar Flows";
                _streamTableData.Rows.Add(workRow);


                for (int j = 0; j < components.Count; j++)
                {
                    workRow = _streamTableData.NewRow();
                    //   workRow["Number"] = i++;
                    workRow["Property"] = "ṅ [" + components[j].ID + "]";
                    workRow["Unit"] = system.VariableFactory.Output.UnitDictionary[UnitsOfMeasure.PhysicalDimension.MolarFlow].Symbol;
                    foreach (var stream in Streams)
                    {
                        var value = stream.Bulk.ComponentMolarflow[j].DisplayValue;
                        if (value > 1e-8)
                            workRow[stream.Name] = value.ToString("0.000");
                        else
                            workRow[stream.Name] = null;
                    }
                    _streamTableData.Rows.Add(workRow);
                }

                //Molar fractions
                workRow = _streamTableData.NewRow();
                //  workRow["Number"] = i++;
                workRow["Property"] = "";// "Component Molar Fractions";
                _streamTableData.Rows.Add(workRow);


                for (int j = 0; j < components.Count; j++)
                {
                    workRow = _streamTableData.NewRow();
                    //    workRow["Number"] = i++;
                    workRow["Property"] = "x [" + components[j].ID + "]";
                    workRow["Unit"] = "mol/mol";
                    foreach (var stream in Streams)
                    {
                       // workRow[stream.Name] = stream.Mixed.ComponentMolarFraction[j].ValueInSI.ToString("0.0000");

                        var value = stream.Bulk.ComponentMolarFraction[j].Val();
                        if (value > 1e-8)
                            workRow[stream.Name] = value.ToString("0.000");
                        else
                            workRow[stream.Name] = null;


                    }
                    _streamTableData.Rows.Add(workRow);
                }

                workRow = _streamTableData.NewRow();
                //  workRow["Number"] = i++;
                _streamTableData.Rows.Add(workRow);
            }



            // Mass flows
            workRow = _streamTableData.NewRow();
            // workRow["Number"] = i++;
            workRow["Property"] = "Total Mass Flow";
            workRow["Unit"] = system.VariableFactory.Output.UnitDictionary[UnitsOfMeasure.PhysicalDimension.MassFlow].Symbol;
            foreach (var stream in Streams)
            {
                workRow[stream.Name] = stream.Bulk.TotalMassflow.DisplayValue.ToString("0.000");
            }
            _streamTableData.Rows.Add(workRow);

            workRow = _streamTableData.NewRow();
            //  workRow["Number"] = i++;
            workRow["Property"] = "";//"Component Mass Flows";
            _streamTableData.Rows.Add(workRow);

            for (int j = 0; j < components.Count; j++)
            {
                var sumForCompJ = Streams.Sum(s => s.Bulk.ComponentMassflow[j].DisplayValue);

                if (sumForCompJ > 1e-6 || !RemoveMissingComponents)
                {
                    workRow = _streamTableData.NewRow();
                    //       workRow["Number"] = i++;
                    workRow["Property"] = "" + components[j].ID + "";
                    workRow["Unit"] = system.VariableFactory.Output.UnitDictionary[UnitsOfMeasure.PhysicalDimension.MassFlow].Symbol;
                    foreach (var stream in Streams)
                    {
                       // workRow[stream.Name] = stream.Mixed.ComponentMassflow[j].ValueInOutputUnit.ToString("0.000");

                        var value = stream.Bulk.ComponentMassflow[j].DisplayValue;
                        if (value > 1e-8)
                            workRow[stream.Name] = value.ToString("0.000");
                        else
                            workRow[stream.Name] = null;


                    }
                    _streamTableData.Rows.Add(workRow);
                }
            }

            //Mass fractions
            workRow = _streamTableData.NewRow();
            //   workRow["Number"] = i++;
            workRow["Property"] = "";//"Component Mass Fractions";
            _streamTableData.Rows.Add(workRow);
            for (int j = 0; j < components.Count; j++)
            {
                var sumForCompJ = Streams.Sum(s => s.Bulk.ComponentMassFraction[j].Val());

                if (sumForCompJ > 1e-6 || !RemoveMissingComponents)
                {
                    workRow = _streamTableData.NewRow();
                    //          workRow["Number"] = i++;
                    workRow["Property"] = "" + components[j].ID + "";
                    workRow["Unit"] = "w-%";
                    foreach (var stream in Streams)
                    {
                        //workRow[stream.Name] = stream.Mixed.ComponentMassFraction[j].ValueInSI.ToString("P2");

                        var value = stream.Bulk.ComponentMassFraction[j].Val();
                        if (value > 1e-8)
                            workRow[stream.Name] = value.ToString("0.000");
                        else
                            workRow[stream.Name] = null;


                        
                    }
                    _streamTableData.Rows.Add(workRow);
                }
            }
            return _streamTableData;
        }



        public StreamTableElement(string name, params MaterialStream[] streams)
        {
            Name = name;
            Icon.IconType = IconTypes.StreamTable;
            SetColors("Silver", "GhostWhite");
            foreach (MaterialStream stream in streams)
                Streams.Add(stream);
        }

        public StreamTableElement AddMaterialStreams(params MaterialStream[] streams)
        {
            foreach (var stream in streams)
                AddMaterialStream(stream);
            return this;
        }

        public StreamTableElement RemoveMaterialStream(MaterialStream stream)
        {
            if (Streams.Contains(stream))
                Streams.Remove(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " not included in streamtable");
            return this;

        }
        public StreamTableElement AddMaterialStream(MaterialStream stream)
        {
            if (!Streams.Contains(stream))
                Streams.Add(stream);
            else
                throw new InvalidOperationException("Stream " + stream.Name + " already included in streamtable");
            return this;
        }

    }
}
