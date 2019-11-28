using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Flowsheeting.Documentation;
using MiniSim.Core.ModelLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.FlowsheetDrawing
{
    public class FlowsheetDrawer
    {
        DrawingOptions _options = new DrawingOptions();

        public DrawingOptions Options { get => _options; set => _options = value; }

        public string ConvertToBase64String(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }


        public byte[] ConvertToBase64(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] imageBytes = ms.ToArray();
                return imageBytes;
            }
        }

        public byte[] DrawAsBase64(Flowsheet flowsheet)
        {
            var image = Draw(flowsheet);
            return ConvertToBase64(image);
        }

        public Image Draw(Flowsheet flowsheet)
        {
            var maxX = flowsheet.Units.Select(u => u.Icon.X + u.Icon.Width).Max() + 100;
            var maxY = flowsheet.Units.Select(u => u.Icon.Y + u.Icon.Height).Max() + 100;

            var maxSX = flowsheet.MaterialStreams.Select(u => u.Icon.X + u.Icon.Width).Max() + 100;
            var maxSY = flowsheet.MaterialStreams.Select(u => u.Icon.Y + u.Icon.Height).Max() + 100;

            Image image = new Bitmap((int)Math.Max(maxX, maxSX), (int)Math.Max(maxY, maxSY));
            Graphics graph = Graphics.FromImage(image);
            graph.Clear(Color.White);

            drawUnits(flowsheet, graph);
            drawStreams(flowsheet, graph);
            drawDecorations(flowsheet, graph);
            return image;

        }

        private void drawDecorations(Flowsheet flowsheet, Graphics graph)
        {
            foreach (var unit in flowsheet.Documentation)
            {
                switch (unit.Icon.IconType)
                {
                    case IconTypes.Spreadsheet:
                        {
                            var spreadsheet = unit as SpreadsheetElement;
                            if (spreadsheet != null)
                            {
                                var maxLength = spreadsheet.Variables.Select(v => (v.ModelName + "." + v.FullName).Length).Max() + 2;

                                var sb = new StringBuilder();


                                sb.AppendLine(unit.Name);
                                Font f = new Font("Consolas", 11);
                                foreach (var vari in spreadsheet.Variables)
                                {
                                    var valueString = vari.DisplayValue.ToString("0.00");
                                    sb.AppendLine($"{(vari.ModelName + "." + vari.FullName).PadRight(maxLength, ' ')} {valueString.PadLeft(10)} {vari.DisplayUnit}");
                                }
                                var label = sb.ToString();
                                DrawString(graph, (int)unit.Icon.X, (int)unit.Icon.Y, label, f, false);

                                var size = MeasureString(graph, f, label);

                                DrawLine(graph, (int)unit.Icon.X, (int)unit.Icon.Y + 16, size.Width);
                            }


                            break;
                        }
                }
            }

        }


        private void drawStreams(Flowsheet flowsheet, Graphics graph)
        {
            Font f = new Font("Arial", 11);

            foreach (var stream in flowsheet.MaterialStreams)
            {
                var source = flowsheet.Units.Where(u => u.MaterialPorts.Any(p => p.Streams.Contains(stream) && p.Direction == PortDirection.Out)).FirstOrDefault();
                var sink = flowsheet.Units.Where(u => u.MaterialPorts.Any(p => p.Streams.Contains(stream) && p.Direction == PortDirection.In)).FirstOrDefault();
                double startX = 0, startY = 0, endX = 0, endY = 0, startXO = 0, startYO = 0, endXO = 0, endYO = 0;

                var sourceNormal = PortNormal.Right;
                var sinkNormal = PortNormal.Left;

                if (source == null)
                {
                    DrawPointedRectangle(graph, stream.Icon.X, stream.Icon.Y, stream.Icon.Width, stream.Icon.Height);
                    DrawString(graph, stream.Icon.X + stream.Icon.Width * 0.66 / 2.0, stream.Icon.Y + stream.Icon.Height / 2.0f, stream.Name, f);

                    var sinkCon = sink.MaterialPorts.FirstOrDefault(p => p.Streams.Contains(stream) && p.Direction == PortDirection.In);
                    sinkNormal = sinkCon.Normal;

                    startX = stream.Icon.X + stream.Icon.Width;
                    startY = stream.Icon.Y + stream.Icon.Height / 2.0;

                    endX = sink.Icon.X + sinkCon.WidthFraction * sink.Icon.Width;
                    endY = sink.Icon.Y + sinkCon.HeightFraction * sink.Icon.Height;
                }

                if (sink == null)
                {
                    DrawPointedRectangle(graph, stream.Icon.X, stream.Icon.Y, stream.Icon.Width, stream.Icon.Height);
                    DrawString(graph, stream.Icon.X + stream.Icon.Width * 0.66 / 2.0, stream.Icon.Y + stream.Icon.Height / 2.0f, stream.Name, f);

                    var sourceCon = source.MaterialPorts.FirstOrDefault(p => p.Streams.Contains(stream) && p.Direction == PortDirection.Out);
                    sourceNormal = sourceCon.Normal;

                    startX = source.Icon.X + sourceCon.WidthFraction * source.Icon.Width;
                    startY = source.Icon.Y + sourceCon.HeightFraction * source.Icon.Height;

                    endX = stream.Icon.X;
                    endY = stream.Icon.Y + stream.Icon.Height / 2.0;

                }

                if (source != null && sink != null)
                {
                    var sourceCon = source.MaterialPorts.FirstOrDefault(p => p.Streams.Contains(stream) && p.Direction == PortDirection.Out);
                    var sinkCon = sink.MaterialPorts.FirstOrDefault(p => p.Streams.Contains(stream) && p.Direction == PortDirection.In);

                    sourceNormal = sourceCon.Normal;
                    sinkNormal = sinkCon.Normal;

                    startX = source.Icon.X + sourceCon.WidthFraction * source.Icon.Width;
                    startY = source.Icon.Y + sourceCon.HeightFraction * source.Icon.Height;

                    endX = sink.Icon.X + sinkCon.WidthFraction * sink.Icon.Width;
                    endY = sink.Icon.Y + sinkCon.HeightFraction * sink.Icon.Height;

                }

                int relationalType = 0;

                if (startX < endX && startY >= endY)
                    relationalType = 1;

                if (startX < endX && startY < endY)
                    relationalType = 2;


                if (startX > endX && startY > endY)
                    relationalType = 3;

                if (startX > endX && startY < endY)
                    relationalType = 4;

                var pointList = new List<Point>();
                var offset = 20;

                //Source Connector+ Normal
                switch (sourceNormal)
                {
                    case PortNormal.Up:
                        startXO = startX;
                        startYO = startY - offset;
                        break;
                    case PortNormal.Right:
                        startXO = startX + offset;
                        startYO = startY;
                        break;
                    case PortNormal.Down:
                        startXO = startX;
                        startYO = startY + offset;
                        break;
                    case PortNormal.Left:
                        startXO = startX - offset;
                        startYO = startY;
                        break;
                }
                //Sink Connector +Normal
                switch (sinkNormal)
                {
                    case PortNormal.Up:
                        endXO = endX;
                        endYO = endY - offset;
                        break;
                    case PortNormal.Right:
                        endXO = endX + offset;
                        endYO = endY;
                        break;
                    case PortNormal.Down:
                        endXO = endX;
                        endYO = endY + offset;
                        break;
                    case PortNormal.Left:
                        endXO = endX - offset;
                        endYO = endY;
                        break;
                }


                //Source Connector
                pointList.Add(new System.Drawing.Point((int)startX, (int)startY));
                pointList.Add(new System.Drawing.Point((int)startXO, (int)startYO));

                double centerX = startX;
                double centerY = startY;

                //Intermedate
                if (sourceNormal == PortNormal.Right && sinkNormal == PortNormal.Left)
                {
                    if (relationalType == 1 || relationalType == 2)
                    {
                        pointList.Add(new System.Drawing.Point((int)((startXO + endXO) / 2.0), (int)startYO));
                        pointList.Add(new System.Drawing.Point((int)((startXO + endXO) / 2.0), (int)endYO));

                        centerX = (startXO + endXO) / 2.0;
                        centerY = (startYO + endYO) / 2.0;
                    }
                }
                else if (sourceNormal == PortNormal.Right && sinkNormal == PortNormal.Right)
                {
                    if (relationalType == 1)
                    {
                        pointList.Add(new System.Drawing.Point((int)((endXO)), (int)startYO));

                        centerX = (endXO);
                        centerY = (startYO);
                    }

                    if (relationalType == 3)
                    {
                        pointList.Add(new System.Drawing.Point((int)((startXO)), (int)endYO));

                        centerX = (startXO);
                        centerY = (endYO);
                    }


                    if (relationalType == 4)
                    {
                        pointList.Add(new System.Drawing.Point((int)((startXO)), (int)endYO));

                        centerX = (startXO);
                        centerY = (endYO);
                    }
                }
                else if (sourceNormal == PortNormal.Up && sinkNormal == PortNormal.Left)
                {
                    if (relationalType == 1)
                    {
                        pointList.Add(new System.Drawing.Point((int)startXO, (int)endYO));
                        centerX = (startXO);
                        centerY = (endYO);
                    }
                }
                else if (sourceNormal == PortNormal.Up && sinkNormal == PortNormal.Right)
                {
                    if (relationalType == 3)
                    {
                        pointList.Add(new System.Drawing.Point((int)startXO, (int)endYO));

                        centerX = (startXO);
                        centerY = (endYO);
                    }

                    if (relationalType == 4)
                    {
                        pointList.Add(new System.Drawing.Point((int)endXO, (int)startYO));

                        centerX = (startXO);
                        centerY = (endYO);
                    }

                }
                else if (sourceNormal == PortNormal.Down && sinkNormal == PortNormal.Left)
                {
                    if (relationalType == 1)
                    {
                        pointList.Add(new System.Drawing.Point((int)((startXO + endXO) / 2.0), (int)startYO));
                        pointList.Add(new System.Drawing.Point((int)((startXO + endXO) / 2.0), (int)endYO));
                        centerX = (startXO + endXO) / 2.0;
                        centerY = (startYO + endYO) / 2.0;
                    }
                    if (relationalType == 2)
                    {
                        pointList.Add(new System.Drawing.Point((int)startXO, (int)endYO));
                        centerX = (startXO);
                        centerY = (endYO);
                    }

                    if (relationalType == 3)
                    {
                        pointList.Add(new System.Drawing.Point((int)((endXO)), (int)startYO));
                        //pointList.Add(new System.Drawing.Point((int)((endXO) / 2.0), (int)endYO));

                        centerX = (startXO + endXO) / 2.0;
                        centerY = (startYO);
                    }

                    if (relationalType == 4)
                    {
                        pointList.Add(new System.Drawing.Point((int)((endXO)), (int)startYO));

                        centerX = (startXO + endXO) / 2.0;
                        centerY = (startYO);
                    }

                }
                else if (sourceNormal == PortNormal.Down && sinkNormal == PortNormal.Right)
                {
                    if (relationalType == 1)
                    {
                        pointList.Add(new System.Drawing.Point((int)endXO, (int)startYO));
                        centerX = (endXO);
                        centerY = (startYO);
                    }

                    if (relationalType == 2)
                    {
                        pointList.Add(new System.Drawing.Point((int)startXO, (int)((startYO + endYO) / 2)));
                        pointList.Add(new System.Drawing.Point((int)endXO, (int)((startYO + endYO) / 2)));
                        centerX = (int)((startXO + endXO) / 2);
                        centerY = (int)((startYO + endYO) / 2);
                    }

                    if (relationalType == 3)
                    {
                        pointList.Add(new System.Drawing.Point((int)endXO, (int)startYO));
                        centerX = (startXO);
                        centerY = (endYO);
                    }

                    if (relationalType == 4)
                    {
                        pointList.Add(new System.Drawing.Point((int)startXO, (int)endYO));
                        centerX = (startXO);
                        centerY = (endYO);
                    }

                }
                else
                {
                    pointList.Add(new System.Drawing.Point((int)((startXO + endXO) / 2.0), (int)startYO));
                    pointList.Add(new System.Drawing.Point((int)((startXO + endXO) / 2.0), (int)endYO));
                    centerX = (startXO + endXO) / 2.0;
                    centerY = (startYO + endYO) / 2.0;
                }

                pointList.Add(new System.Drawing.Point((int)endXO, (int)endYO));
                //Sink Connector
                pointList.Add(new System.Drawing.Point((int)endX, (int)endY));

                bool dashed = false;
                if (stream.GetVariable("VF").Val() > 0.5)
                    dashed = true;

                DrawLinesPoint(graph, pointList.ToArray(), dashed);

                string label = $"{stream.Name}\n";

                if (Options.ShowTemperature)
                    label += $"{stream.Temperature.DisplayValue:F2} {stream.Temperature.DisplayUnit}\n";
                if (Options.ShowPressure)
                    label += $"{stream.Pressure.DisplayValue:F2} {stream.Pressure.DisplayUnit}\n";
                if (Options.ShowMassFlow)
                    label += $"{stream.Bulk.TotalMassflow.DisplayValue:F2} {stream.Bulk.TotalMassflow.DisplayUnit}\n";

                DrawString(graph, (int)centerX, (int)centerY, label, f);
            }
        }

        private void drawUnits(Flowsheet flowsheet, Graphics graph)
        {
            Font f = new Font("Arial", 11, FontStyle.Bold);


            foreach (var unit in flowsheet.Units)
            {
                switch (unit.Icon.IconType)
                {
                    case IconTypes.Stream:
                        DrawRectangle(graph, unit.Icon.X, unit.Icon.Y, unit.Icon.Width, unit.Icon.Height);
                        DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height / 2.0, unit.Name, f);
                        break;
                    case IconTypes.Splitter:
                        DrawRectangle(graph, unit.Icon.X, unit.Icon.Y, unit.Icon.Width, unit.Icon.Height);
                        DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height / 2.0, "S", f);
                        DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height + 10, unit.Name, f);
                        break;
                    case IconTypes.Mixer:
                        DrawRectangle(graph, unit.Icon.X, unit.Icon.Y, unit.Icon.Width, unit.Icon.Height);
                        DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height / 2.0, "M", f);
                        DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height + 10, unit.Name, f);
                        break;

                    case IconTypes.ColumnSection:
                        DrawRectangle(graph, unit.Icon.X, unit.Icon.Y, unit.Icon.Width, unit.Icon.Height);
                        DrawUpperHalfCircle(graph, unit.Icon.X, unit.Icon.Y - 15, 40, 30);
                        DrawLowerHalfCircle(graph, unit.Icon.X, unit.Icon.Y + unit.Icon.Height - 15, 40, 30);
                        var column = unit as EquilibriumStageSection;
                        if (column != null)
                            DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height / 2.0, "N=" + column.NumberOfTrays, f);

                        DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height + 30, unit.Name, f);
                        break;
                    case IconTypes.Heater:
                    case IconTypes.TwoPhaseFlash:
                        DrawCircle(graph, unit.Icon.X, unit.Icon.Y, unit.Icon.Width, unit.Icon.Height);
                        DrawString(graph, unit.Icon.X + unit.Icon.Width / 2.0, unit.Icon.Y + unit.Icon.Height + 10, unit.Name, f);
                        break;
                }

                foreach (var port in unit.MaterialPorts)
                {
                    if (port.IsConnected)
                        DrawBlueCircle(graph, unit.Icon.X + unit.Icon.Width * port.WidthFraction - 5, unit.Icon.Y + unit.Icon.Height * port.HeightFraction - 5, 10, 10);
                }
            }
        }

        SizeF MeasureString(Graphics g, Font f, string drawString)
        {

            SizeF stringSize = new SizeF();
            stringSize = g.MeasureString(drawString, f);
            return stringSize;
        }

        void DrawLinesPoint(Graphics g, Point[] points, bool dashed)
        {
            // Create pen.
            Pen pen = new Pen(Color.DimGray, 2);
            if (dashed && Options.ShowVaporStreams)
                pen.DashPattern = new float[] { 1.0f, 01.0f };
            //Draw lines to screen.
            g.DrawLines(pen, points);
        }


        void DrawString(Graphics g, double x, double y, string drawString, Font f, bool centered = true)
        {

            // Create font and brush.

            SolidBrush drawBrush = new SolidBrush(Color.Black);

            // Set format of string.
            StringFormat drawFormat = new StringFormat();
            // drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            float xs = (float)x;
            float ys = (float)y;


            if (centered)
            {
                var size = MeasureString(g, f, drawString);
                xs -= size.Width / 2.0f;
                ys -= size.Height / 2.0f;
            }

            // Draw string to screen.
            g.DrawString(drawString, f, drawBrush, xs, ys, drawFormat);
        }

        void DrawPointedRectangle(Graphics g, double x, double y, double width, double height)
        {
            Pen myPen = new Pen(Brushes.Black);

            myPen.Width = 3.0f;

            // Set the LineJoin property
            myPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            Point point1 = new Point((int)x, (int)y);
            Point point2 = new Point((int)(x + width * 0.66), (int)(y));
            Point point3 = new Point((int)(x + width), (int)(y + height * 0.5));
            Point point4 = new Point((int)(x + width * 0.66), (int)(y + height));
            Point point5 = new Point((int)x, (int)(y + height));
            Point point6 = new Point((int)x, (int)y);
            Point[] curvePoints =
                     {
                 point1,
                 point2,
                 point3,
                 point4,
                 point5,
                 point6
             };

            // Draw polygon to screen.
            g.DrawPolygon(myPen, curvePoints);

        }
        void DrawLine(Graphics g, double x, double y, double width)
        {
            Pen myPen = new Pen(Brushes.Black);

            myPen.Width = 2.0f;

            // Set the LineJoin property
            myPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            // Draw the rectangle
            g.DrawLine(myPen, (float)x, (float)y, (float)(x + width), (float)y);

        }

        void DrawRectangle(Graphics g, double x, double y, double width, double height)
        {
            Pen myPen = new Pen(Brushes.Black);

            myPen.Width = 3.0f;

            // Set the LineJoin property
            myPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            // Draw the rectangle
            g.DrawRectangle(myPen, new Rectangle((int)x, (int)y, (int)width, (int)height));
        }

        void DrawBlueCircle(Graphics g, double x, double y, double width, double height)
        {
            Pen myPen = new Pen(Brushes.Blue);

            myPen.Width = 2.0f;

            // Set the LineJoin property
            myPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            // Draw the rectangle
            g.DrawEllipse(myPen, new Rectangle((int)x, (int)y, (int)width, (int)height));
        }

        void DrawUpperHalfCircle(Graphics g, double x, double y, double width, double height)
        {
            Pen myPen = new Pen(Brushes.Black);

            myPen.Width = 3.0f;

            // Set the LineJoin property
            myPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            // Draw the rectangle
            g.DrawArc(myPen, new Rectangle((int)x, (int)y, (int)width, (int)height), 0, -180);
        }
        void DrawLowerHalfCircle(Graphics g, double x, double y, double width, double height)
        {
            Pen myPen = new Pen(Brushes.Black);

            myPen.Width = 3.0f;

            // Set the LineJoin property
            myPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            // Draw the rectangle
            g.DrawArc(myPen, new Rectangle((int)x, (int)y, (int)width, (int)height), -180, -180);
        }


        void DrawCircle(Graphics g, double x, double y, double width, double height)
        {
            Pen myPen = new Pen(Brushes.Black);

            myPen.Width = 3.0f;

            // Set the LineJoin property
            myPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            // Draw the rectangle
            g.DrawEllipse(myPen, new Rectangle((int)x, (int)y, (int)width, (int)height));
        }
    }

}
