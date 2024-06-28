using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static netDxf.Entities.HatchBoundaryPath;
using Point = System.Drawing.Point;

namespace RSSimulation
{
    class LineData
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Pen pen;
        public LineData() { 
            startPos = new Vector3();
            endPos = new Vector3();
            pen = null;
        }
        public LineData(Vector3 startPos, Vector3 endPos, Pen pen, string lineStyle = "Solid line")
        {
            this.startPos = startPos;
            this.endPos = endPos;
            this.pen = pen;
            if(lineStyle != "Solid line")
                pen.DashStyle = DashStyle.Dash;
        }
    }
    class Polyline2DData
    {
        public Polyline2D polyline;
        public Pen pen;
        public Polyline2DData(Polyline2D polyline, Pen pen)
        {
            this.polyline = polyline;
            this.pen = pen;
        }
    }
    class Polyline3DData
    {
        public Polyline3D polyline;
        public Pen pen;
        public Polyline3DData(Polyline3D polyline, Pen pen)
        {
            this.polyline = polyline;
            this.pen = pen;
        }
    }
    class CircleData
    {
        public Vector3 center;
        public double radius;
        public Pen pen;
        public CircleData(Vector3 center, double radius, Pen pen)
        {
            this.center = center;
            this.radius = radius;
            this.pen = pen;
        }
    }
    class TextData
    {
        public string text;
        public Vector3 position;
        public Brush brush;
        public Font font;
        public TextData(string text, Vector3 position, Brush brush, Font font)
        {
            this.text = text;
            this.position = position;
            this.brush = brush;
            this.font = font;
        }
    }
    struct HatchPathData
    {
        public GraphicsPath hatchPath;
        public HatchBrush hatchBrush;
        public Pen pen;
        public HatchPathData(GraphicsPath hatchPath, HatchBrush hatchBrush, Pen pen)
        {
            this.hatchPath = hatchPath;
            this.hatchBrush = hatchBrush;
            this.pen = pen;
        }
    }
    internal class HaDesDxf
    {
        public LineData primaryLine;
        public List<LineData> lines;
        public List<CircleData> circles;
        public List<HatchPathData> hatchPaths;
        public List<Polyline2DData> polyline2Ds;
        public List<Polyline3DData> polyline3Ds;
        public List<TextData> texts;
        public List<Hatch> hatchs;
        private BoundingBox initRect;
        public HaDesDxf() {            
            primaryLine = null;
            lines = new List<LineData>();
            circles = new List<CircleData>();
            hatchPaths = new List<HatchPathData>();
            polyline2Ds = new List<Polyline2DData>();
            polyline3Ds = new List<Polyline3DData>();
            texts = new List<TextData>();
            hatchs = new List<Hatch>();

            initRect = new BoundingBox();
        }
        public LinePos getPrimaryLinePos()
        {
            return new LinePos(ToPointF(primaryLine.startPos), ToPointF(primaryLine.endPos));
        }
        public void setPrimaryLin(LineData line)
        {
            primaryLine = line;
        }
        public void Add(LineData line) {
            lines.Add(line);
        }
        public void Add(CircleData circle)
        {
            circles.Add(circle);
        }
        public void Add(HatchPathData hatchPath) {
            hatchPaths.Add(hatchPath);
        }
        public void Add(Polyline2DData polyline) { 
            polyline2Ds.Add(polyline);
        }
        public void Add(Polyline3DData polyline)
        {
            polyline3Ds.Add(polyline);
        }
        public void Add(TextData text)
        {
            texts.Add(text);
        }
        public BoundingBox ReadDxf(DxfDocument dxf)
        {
            initRect = GetBoundingBox(dxf);
            update();
            return initRect;
        }
        private BoundingBox GetBoundingBox(DxfDocument dxf)
        {
            BoundingBox boundingBox = new BoundingBox(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);

            if (dxf != null)
            {
                foreach (var entity in dxf.Entities.All)
                {
                    switch (entity)
                    {
                        case netDxf.Entities.Line line:
                            LineData lineData = new LineData(line.StartPoint, line.EndPoint,
                                new Pen(ToColor(line.Layer.Color), 0.01f), line.Layer.Linetype.Description);
                            Add(lineData);
                            if (line.Handle as string == "10526")
                            {
                                setPrimaryLin(lineData);
                            }
                            UpdateBoundingBox(ref boundingBox, line.StartPoint);
                            UpdateBoundingBox(ref boundingBox, line.EndPoint);
                            break;
                        case Circle circle:
                            CircleData circleData = new CircleData(circle.Center, circle.Radius,
                                new Pen(ToColor(circle.Layer.Color), 0.01f));
                            Add(circleData);
                            UpdateBoundingBox(ref boundingBox, circle.Center, circle.Radius);
                            break;
                        case Polyline2D polyline2D:
                            Polyline2DData polyline2DData = new Polyline2DData(polyline2D,
                                new Pen(ToColor(polyline2D.Layer.Color), 0.01f));
                            Add(polyline2DData);
                            foreach (var vertex in polyline2D.Vertexes)
                                UpdateBoundingBox(ref boundingBox, vertex.Position);
                            break;
                        case Polyline3D polyline3D:
                            Polyline3DData polyline3DData = new Polyline3DData(polyline3D,
                                new Pen(ToColor(polyline3D.Layer.Color), 0.01f));
                            Add(polyline3DData);
                            foreach (var vertex in polyline3D.Vertexes)
                                UpdateBoundingBox(ref boundingBox, vertex);
                            break;
                        case Text text:
                            TextData textData = new TextData(text.Value, text.Position,
                                new SolidBrush(ToColor(text.Layer.Color)),
                                new Font(
                                   new FontFamily($"Arial"),
                                   (float)text.Height,
                                   FontStyle.Regular,
                                   GraphicsUnit.Pixel));
                            Add(textData);
                            break;
                        case Hatch hatch:
                            hatchs.Add(hatch);
                            break;
                            // Add cases for other entity types as needed
                    }
                }
            }
            return boundingBox;
        }
        private static void UpdateBoundingBox(ref BoundingBox boundingBox, Vector2 point, double radius = 0)
        {
            boundingBox.MinX = Math.Min(boundingBox.MinX, point.X - radius);
            boundingBox.MinY = Math.Min(boundingBox.MinY, point.Y - radius);
            boundingBox.MaxX = Math.Max(boundingBox.MaxX, point.X + radius);
            boundingBox.MaxY = Math.Max(boundingBox.MaxY, point.Y + radius);
        }
        private static void UpdateBoundingBox(ref BoundingBox boundingBox, Vector3 point, double radius = 0)
        {
            boundingBox.MinX = Math.Min(boundingBox.MinX, point.X - radius);
            boundingBox.MinY = Math.Min(boundingBox.MinY, point.Y - radius);
            boundingBox.MaxX = Math.Max(boundingBox.MaxX, point.X + radius);
            boundingBox.MaxY = Math.Max(boundingBox.MaxY, point.Y + radius);
        }
        public void update(float scale = 1.0f)
        {
            foreach (LineData line in lines)
            {
                line.startPos = updatePos(line.startPos, initRect);
                line.endPos = updatePos(line.endPos, initRect);
            }
            foreach (CircleData circle in circles)
            {
                circle.center = updatePos(circle.center, initRect);
            }
            foreach (Polyline2DData polyline2D in polyline2Ds)
            {
                for (int i = 0; i < polyline2D.polyline.Vertexes.Count; i++)
                {
                    polyline2D.polyline.Vertexes[i].Position = updatePos(polyline2D.polyline.Vertexes[i].Position, initRect);
                }
            }
            foreach (Polyline3DData polyline3D in polyline3Ds)
            {
                for (int i = 0; i < polyline3D.polyline.Vertexes.Count; i++)
                {
                    polyline3D.polyline.Vertexes[i] = updatePos(polyline3D.polyline.Vertexes[i], initRect);
                }
            }
            foreach (Hatch hatch in hatchs)
            {
                HatchBrush hatchBrush = new HatchBrush(System.Drawing.Drawing2D.HatchStyle.Cross, ToColor(hatch.Color), ToColor(hatch.Color));
                Pen pen = new Pen(ToColor(hatch.Color), 0.01f);
                foreach (var boundaryPath in hatch.BoundaryPaths)
                {
                    GraphicsPath hatchPath = new GraphicsPath();

                    // Loop through edges of the boundary path
                    foreach (var edge in boundaryPath.Edges)
                    {
                        switch (edge)
                        {
                            case netDxf.Entities.HatchBoundaryPath.Line line:
                                hatchPath.AddLine(updatePosF(line.Start, initRect), updatePosF(line.End, initRect));
                                break;
                            case netDxf.Entities.HatchBoundaryPath.Arc arc:
                                hatchPath.AddArc(updateRectF(arc.Center, arc.Radius, initRect), (float)arc.StartAngle, (float)(arc.EndAngle - arc.StartAngle));
                                break;
                            case netDxf.Entities.HatchBoundaryPath.Polyline pline:
                                for (int i = 0; i < pline.Vertexes.Length - 1; i++)
                                {
                                    var startVertex = pline.Vertexes[i];
                                    var endVertex = pline.Vertexes[i + 1];
                                    hatchPath.AddLine(updatePosF(startVertex, initRect), updatePosF(endVertex, initRect));                                    
                                }
                                // Close the polyline if it is closed
                                if (pline.IsClosed)
                                {
                                    var startVertex = pline.Vertexes[pline.Vertexes.Length - 1];
                                    var endVertex = pline.Vertexes[0];
                                    hatchPath.AddLine(updatePosF(startVertex, initRect), updatePosF(endVertex, initRect));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    HatchPathData hatchPathData = new HatchPathData(hatchPath, hatchBrush, pen);
                    hatchPaths.Add(hatchPathData);
                }
            }
        }
        public Vector3 updatePos(Vector3 pos, BoundingBox initRect)
        {
            Vector3 newPos = pos;
            newPos.X -= initRect.MinX;
            newPos.Y = (initRect.MinY + initRect.height()) - newPos.Y;
            return newPos;
        }
        public Vector2 updatePos(Vector2 pos, BoundingBox initRect)
        {
            Vector2 newPos = pos;
            newPos.X -= initRect.MinX;
            newPos.Y = (initRect.MinY + initRect.height()) - newPos.Y;
            return newPos;
        }
        public PointF updatePosF(Vector3 pos, BoundingBox initRect)
        {
            Vector3 newPos = pos;
            newPos.X -= initRect.MinX;
            newPos.Y = (initRect.MinY + initRect.height()) - newPos.Y;
            return ToPointF(newPos);
        }
        public PointF updatePosF(Vector2 pos, BoundingBox initRect)
        {
            Vector2 newPos = pos;
            newPos.X -= initRect.MinX;
            newPos.Y = (initRect.MinY + initRect.height()) - newPos.Y;
            return ToPointF(newPos);
        }
        public RectangleF updateRectF(Vector2 center, double radius, BoundingBox initRect)
        {
            Vector2 newCenter = updatePos(center, initRect);
            return ToRectangleF(newCenter, radius);
        }
        public static PointF ToPointF(Vector2 vector)
        {
            return new PointF((float)vector.X, (float)vector.Y);
        }
        public static PointF ToPointF(Vector3 vector)
        {
            return new PointF((float)vector.X, (float)vector.Y);
        }
        public static RectangleF ToRectangleF(Vector2 center, double radius)
        {
            float diameter = (float)(radius * 2);
            return new RectangleF((float)(center.X - radius), (float)(center.Y - radius), diameter, diameter);
        }
        public static RectangleF ToRectangleF(Vector3 center, double radius)
        {
            float diameter = (float)(radius * 2);
            return new RectangleF((float)(center.X - radius), (float)(center.Y - radius), diameter, diameter);
        }
        public static Color ToColor(AciColor color)
        {
            return Color.FromArgb(color.ToColor().R, color.ToColor().G, color.ToColor().B);
        }
        public static Point GetDxfPos(Point currentPos, PointF offsetPos, ZoomFactor zoomFactor)
        {
            Point cnvPos = new Point();
            cnvPos.X = (int)(offsetPos.X + currentPos.X / zoomFactor.Value);
            cnvPos.Y = (int)(offsetPos.Y + currentPos.Y / zoomFactor.Value);
            return cnvPos;
        }
        public static PointF GetDxfPosF(Point currentPos,PointF offsetPos, ZoomFactor zoomFactor)
        {
            PointF cnvPos = new PointF();
            cnvPos.X = (float)(offsetPos.X + currentPos.X / zoomFactor.Value);
            cnvPos.Y = (float)(offsetPos.Y + currentPos.Y / zoomFactor.Value);
            return cnvPos;
        }
        public static void UpdateOffsetPos(ref PointF offsetPos, PictureBox DrawArea, SizeF DrawAreaSize, Point delta, float zoomValue)
        {
            // Adjust horizontal scroll
            float newX = offsetPos.X - delta.X / zoomValue;
            newX = Math.Min((DrawAreaSize.Width - DrawArea.Width) / zoomValue, newX); // Ensure not more than maximum
            newX = Math.Max(0, newX); // Ensure not less than 0
            offsetPos.X = newX;

            // Adjust vertical scroll
            float newY = offsetPos.Y - delta.Y / zoomValue;
            newY = Math.Min((DrawAreaSize.Height - DrawArea.Height) / zoomValue, newY); // Ensure not more than maximum
            newY = Math.Max(0, newY); // Ensure not less than 0
            offsetPos.Y = newY;
        }
        public void DrawDxf(Graphics g)
        {
            DrawHatch(g);
            DrawLine(g);
            DrawPolyline2D(g);
            DrawPolyline3D(g);
        }
        private void DrawLine(Graphics g)
        {
            foreach(LineData line in lines)
            {
                g.DrawLine(line.pen, ToPointF(line.startPos), ToPointF(line.endPos));
            }                
        }
        private void DrawPolyline2D(Graphics g)
        {
            foreach (Polyline2DData polyline2D in polyline2Ds)
            {
                for (int i = 0; i < polyline2D.polyline.Vertexes.Count - 1; i++)
                {
                    g.DrawLine(polyline2D.pen, ToPointF(polyline2D.polyline.Vertexes[i].Position), ToPointF(polyline2D.polyline.Vertexes[i + 1].Position));
                }
                if (polyline2D.polyline.IsClosed)
                {
                    g.DrawLine(polyline2D.pen, ToPointF(polyline2D.polyline.Vertexes[polyline2D.polyline.Vertexes.Count - 1].Position), ToPointF(polyline2D.polyline.Vertexes[0].Position));
                }
            }
                
        }
        private void DrawPolyline3D(Graphics g)
        {
            foreach (Polyline3DData polyline3D in polyline3Ds)
            {
                for (int i = 0; i < polyline3D.polyline.Vertexes.Count - 1; i++)
                {
                    g.DrawLine(polyline3D.pen, ToPointF(polyline3D.polyline.Vertexes[i]), ToPointF(polyline3D.polyline.Vertexes[i + 1]));
                }
                if (polyline3D.polyline.IsClosed)
                {
                    g.DrawLine(polyline3D.pen, ToPointF(polyline3D.polyline.Vertexes[polyline3D.polyline.Vertexes.Count - 1]), ToPointF(polyline3D.polyline.Vertexes[0]));
                }
            }
        }
        private void DrawHatch(Graphics g)
        {
            foreach (HatchPathData hatchPathData in hatchPaths)
            {
                g.FillPath(hatchPathData.hatchBrush, hatchPathData.hatchPath);
                g.DrawPath(hatchPathData.pen, hatchPathData.hatchPath);
            }                
        }
    }
}
