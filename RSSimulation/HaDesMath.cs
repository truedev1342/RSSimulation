using netDxf;
using netDxf.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static netDxf.Entities.HatchBoundaryPath;
using Point = System.Drawing.Point;

namespace RSSimulation
{
    struct BoundingBox
    {
        public double MinX;
        public double MinY;
        public double MaxX;
        public double MaxY;
        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }
        public double width()
        {
            return MaxX - MinX;
        }
        public double height()
        {
            return MaxY - MinY;
        }
    }
    struct ZoomFactor
    {
        public float Value;
        public float Step;
        public float Min;
        public float Max;
        public ZoomFactor(float value, float step, float min, float max)
        {
            Value = value;
            Step = step;
            Min = min;
            Max = max;
        }
    }
    struct LinePos
    {
        public PointF StartPos;
        public PointF EndPos;
        public LinePos(PointF startPos, PointF endPos)
        {
            StartPos = startPos;
            EndPos = endPos;
        }
    }
    internal class HaDesMath
    {
        public static SizeF UpdateDrawAreaSize(BoundingBox boundingBox, ZoomFactor zoomFactor)
        {
            // Compute scale factors to fit the bounding box into the drawing area
            float width = (float)boundingBox.width() * zoomFactor.Value;
            float height = (float)boundingBox.height() * zoomFactor.Value;
            
            return new SizeF(width, height); 

            //drawArea.Size = new Size(width, height);
            //drawArea.Location = new Point(0, 0);
        }
        public static BoundingBox GetBoundingBox(DxfDocument dxf, ref LinePos initRSPos)
        {
            BoundingBox boundingBox = new BoundingBox(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);

            if (dxf != null)
            {
                foreach (var entity in dxf.Entities.All)
                {
                    switch (entity)
                    {
                        case netDxf.Entities.Line line:
                            if (line.Handle as string == "10526")
                            {
                                initRSPos.StartPos = HaDesGraphics.ToPointF(line.StartPoint);
                                initRSPos.EndPos = HaDesGraphics.ToPointF(line.EndPoint);
                            }
                            UpdateBoundingBox(ref boundingBox, line.StartPoint);
                            UpdateBoundingBox(ref boundingBox, line.EndPoint);
                            break;
                        case Circle circle:
                            UpdateBoundingBox(ref boundingBox, circle.Center, circle.Radius);
                            break;
                        case Polyline2D polyline2D:
                            foreach (var vertex in polyline2D.Vertexes)
                                UpdateBoundingBox(ref boundingBox, vertex.Position);
                            break;
                        case Polyline3D polyline3D:
                            foreach (var vertex in polyline3D.Vertexes)
                                UpdateBoundingBox(ref boundingBox, vertex);
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
        public static RectangleF GetVisibleAreaOfDrawArea(PictureBox DrawArea, float zoomFactor, BoundingBox boundingBox, PointF offsetPos)
        {
            // Get the current size of the DrawPanel
            int panelWidth = DrawArea.ClientSize.Width;
            int panelHeight = DrawArea.ClientSize.Height;

            // Calculate the scaled size considering the zoom factor
            float scaledWidth = panelWidth / zoomFactor;
            float scaledHeight = panelHeight / zoomFactor;

            float startX = (float)(boundingBox.MinX + offsetPos.X);
            float startY = (float)(boundingBox.MaxY - offsetPos.Y) - scaledHeight;

            // Calculate the visible rectangle in the panel's coordinate system
            RectangleF visibleRect = new RectangleF(
                startX,            // X-coordinate
                startY,            // Y-coordinate
                scaledWidth,         // Width
                scaledHeight         // Height
            );

            return visibleRect;
        }
        public static RectangleF GetLineBounds(netDxf.Entities.Line line)
        {
            float minX = Math.Min((float)line.StartPoint.X, (float)line.EndPoint.X);
            float minY = Math.Min((float)line.StartPoint.Y, (float)line.EndPoint.Y);
            float maxX = Math.Max((float)line.StartPoint.X, (float)line.EndPoint.X);
            float maxY = Math.Max((float)line.StartPoint.Y, (float)line.EndPoint.Y);

            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }
        public static RectangleF GetHatchBounds(netDxf.Entities.Hatch hatch)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var boundaryPath in hatch.BoundaryPaths)
            {
                foreach (var edge in boundaryPath.Edges)
                {
                    switch (edge)
                    {
                        case netDxf.Entities.HatchBoundaryPath.Line line:
                            minX = Math.Min(minX, (float)Math.Min(line.Start.X, line.End.X));
                            minY = Math.Min(minY, (float)Math.Min(line.Start.Y, line.End.Y));
                            maxX = Math.Max(maxX, (float)Math.Max(line.Start.X, line.End.X));
                            maxY = Math.Max(maxY, (float)Math.Max(line.Start.Y, line.End.Y));
                            break;
                        case netDxf.Entities.HatchBoundaryPath.Arc arc:
                            var arcBounds = GetArcBounds(arc);
                            minX = Math.Min(minX, arcBounds.Left);
                            minY = Math.Min(minY, arcBounds.Top);
                            maxX = Math.Max(maxX, arcBounds.Right);
                            maxY = Math.Max(maxY, arcBounds.Bottom);
                            break;
                        case netDxf.Entities.HatchBoundaryPath.Polyline pline:
                            foreach (var vertex in pline.Vertexes)
                            {
                                minX = Math.Min(minX, (float)vertex.X);
                                minY = Math.Min(minY, (float)vertex.Y);
                                maxX = Math.Max(maxX, (float)vertex.X);
                                maxY = Math.Max(maxY, (float)vertex.Y);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }
        private static RectangleF GetArcBounds(netDxf.Entities.HatchBoundaryPath.Arc arc)
        {
            var startPoint = new Vector2(
                arc.Center.X + arc.Radius * Math.Cos(arc.StartAngle * Math.PI / 180),
                arc.Center.Y + arc.Radius * Math.Sin(arc.StartAngle * Math.PI / 180)
            );

            var endPoint = new Vector2(
                arc.Center.X + arc.Radius * Math.Cos(arc.EndAngle * Math.PI / 180),
                arc.Center.Y + arc.Radius * Math.Sin(arc.EndAngle * Math.PI / 180)
            );

            float minX = Math.Min((float)Math.Min(startPoint.X, endPoint.X), (float)(arc.Center.X - arc.Radius));
            float minY = Math.Min((float)Math.Min(startPoint.Y, endPoint.Y), (float)(arc.Center.Y - arc.Radius));
            float maxX = Math.Max((float)Math.Max(startPoint.X, endPoint.X), (float)(arc.Center.X + arc.Radius));
            float maxY = Math.Max((float)Math.Max(startPoint.Y, endPoint.Y), (float)(arc.Center.Y + arc.Radius));

            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }
        public static RectangleF GetPolylineBounds(netDxf.Entities.Polyline2D polyline)
        {
            if (polyline == null || polyline.Vertexes.Count == 0)
                return RectangleF.Empty;

            // Initialize with the first vertex position
            float minX = (float)polyline.Vertexes[0].Position.X;
            float minY = (float)polyline.Vertexes[0].Position.Y;
            float maxX = minX;
            float maxY = minY;

            // Iterate over the vertexes to find the min and max bounds
            foreach (var vertex in polyline.Vertexes)
            {
                if (vertex.Position.X < minX) minX = (float)vertex.Position.X;
                if (vertex.Position.X > maxX) maxX = (float)vertex.Position.X;
                if (vertex.Position.Y < minY) minY = (float)vertex.Position.Y;
                if (vertex.Position.Y > maxY) maxY = (float)vertex.Position.Y;
            }

            // Create a rectangle from the min and max bounds
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
        public static RectangleF GetPolylineBounds(netDxf.Entities.Polyline3D polyline)
        {
            if (polyline == null || polyline.Vertexes.Count == 0)
                return RectangleF.Empty;

            // Initialize with the first vertex position
            float minX = (float)polyline.Vertexes[0].X;
            float minY = (float)polyline.Vertexes[0].Y;
            float maxX = minX;
            float maxY = minY;

            // Iterate over the vertexes to find the min and max bounds
            foreach (var vertex in polyline.Vertexes)
            {
                if (vertex.X < minX) minX = (float)vertex.X;
                if (vertex.X > maxX) maxX = (float)vertex.X;
                if (vertex.Y < minY) minY = (float)vertex.Y;
                if (vertex.Y > maxY) maxY = (float)vertex.Y;
            }

            // Create a rectangle from the min and max bounds
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
        public static float CalcDistancePoints(PointF firstPos, PointF secondPos)
        {
            return (float)Math.Sqrt((firstPos.X - secondPos.X) * (firstPos.X - secondPos.X) +
                (firstPos.Y - secondPos.Y) * (firstPos.Y - secondPos.Y));
        }
        public static PointF CalcBasePoint(LinePos linePos, PointF eFramePos)
        {
            PointF start = linePos.StartPos;
            PointF end = linePos.EndPos;

            // Vector from start to end
            PointF lineVec = new PointF(end.X - start.X, end.Y - start.Y);
            // Vector from start to point
            PointF pointVec = new PointF(eFramePos.X - start.X, eFramePos.Y - start.Y);

            // Calculate the dot product of lineVec and pointVec
            float dotProduct = pointVec.X * lineVec.X + pointVec.Y * lineVec.Y;
            // Calculate the squared length of the lineVec
            float lineLengthSq = lineVec.X * lineVec.X + lineVec.Y * lineVec.Y;

            // Projection scale factor
            float scale = dotProduct / lineLengthSq;

            // Calculate the base point coordinates
            PointF basePoint = new PointF(
                start.X + scale * lineVec.X,
                start.Y + scale * lineVec.Y
            );

            return basePoint;
        }

        public static float calcShuntDistance(PointF shuntPos, List<PointF> routePoints)
        {
            if (routePoints.Count == 0) return 0.0f;
            return 50.0f;
        }
        public static string ConvertSecToTimeStr(int sec)
        {
            string result = "";
            int h = sec / 3600;
            if (h < 10) result += "0";
            result += h.ToString();
            result += ":";
            int m = (sec % 3600) / 60;
            if (m < 10) result += "0";
            result += m.ToString();
            result += ":";
            int s = sec % 60;
            if (s < 10) result += "0";
            result += s.ToString();
            return result;
        }
    }
}
