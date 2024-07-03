using netDxf.Entities;
using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using netDxf.Collections;
using netDxf;
using Point = System.Drawing.Point;
using System.Windows.Forms;

namespace RSSimulation
{
    internal class HaDesGraphics
    {
        public static Tuple<Vector3, Vector3> GetBoundingBox(DrawingEntities entities)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var line in entities.Lines)
            {
                minX = Math.Min(minX, Math.Min(line.StartPoint.X, line.EndPoint.X));
                minY = Math.Min(minY, Math.Min(line.StartPoint.Y, line.EndPoint.Y));
                maxX = Math.Max(maxX, Math.Max(line.StartPoint.X, line.EndPoint.X));
                maxY = Math.Max(maxY, Math.Max(line.StartPoint.Y, line.EndPoint.Y));
            }
            return new Tuple<Vector3, Vector3>(new Vector3(minX, minY, 0), new Vector3(maxX, maxY, 0));
        }
        public static void DrawLine(Graphics g, netDxf.Entities.Line line)
        {
            using (Pen pen = new Pen(HaDesGraphics.ToColor(line.Layer.Color), 0.01f))
            {
                //if (line.Layer.Linetype.Description != "Solid line")
                {
                    //pen.DashStyle = DashStyle.Dash;
                }
                g.DrawLine(pen, HaDesGraphics.ToPointF(line.StartPoint), HaDesGraphics.ToPointF(line.EndPoint));
            }
        }
        public static void DrawText(Graphics g, netDxf.Entities.Text text)
        {
            string fontName = "Arial"; // Default font
            if (text.Style != null && !string.IsNullOrEmpty(text.Style.FontFamilyName))
            {
                fontName = text.Style.FontFamilyName;
            }
            using (Font font = new Font(
               new FontFamily($"{fontName}"),
               (float)text.Height,
               FontStyle.Regular,
               GraphicsUnit.Pixel))
            using (Brush brush = new SolidBrush(ToColor(text.Layer.Color)))
            {
                StringAlignment alignment = (StringAlignment)text.Alignment;
                PointF position = ToPointF(text.Position);

                // Save the current state of the graphics object
                GraphicsState state = g.Save();

                // Translate to the position where you want to draw the text
                g.TranslateTransform(position.X, position.Y);

                // Rotate the coordinate system 180 degrees (vertically invert)

                g.RotateTransform((float)text.Rotation);
                g.TranslateTransform(0, (float)text.Height / 2);
                g.ScaleTransform(1, -1);
                // Draw the text at the origin of the rotated coordinate system
                g.DrawString(text.Value, font, brush, 0, 0);

                // Restore the graphics object to its original state
                g.Restore(state);
            }
        }
        public static void DrawMText(Graphics g, netDxf.Entities.MText mtext)
        {
            // Extract the font name and set default values
            string fontName = "Arial"; // Default font
            if (mtext.Style != null && !string.IsNullOrEmpty(mtext.Style.FontFamilyName))
            {
                fontName = mtext.Style.FontFamilyName;
            }

            // Determine the font size
            float fontSize = (float)mtext.Height;

            // Create the font
            using (Font font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                // Create a brush with the text color
                using (Brush brush = new SolidBrush(Color.FromArgb(55, 55, 55)/*ToColor(mtext.Color)*/))
                {
                    // Save the graphics state
                    GraphicsState state = g.Save();

                    // Apply transformation for rotation
                    g.TranslateTransform((float)mtext.Position.X, (float)mtext.Position.Y);
                    g.RotateTransform((float)mtext.Rotation);
                    g.TranslateTransform(0, -fontSize / 4);
                    g.ScaleTransform(1, -1);
                    // Calculate the bounding box for the text
                    SizeF textSize = g.MeasureString(mtext.Value, font);

                    // Apply alignment
                    float xOffset = 0;
                    float yOffset = 0;
                    /*switch (mtext.AttachmentPoint)
                    {
                        case MTextAttachmentPoint.TopLeft:
                            yOffset = -textSize.Height;
                            break;
                        case MTextAttachmentPoint.TopCenter:
                            xOffset = -textSize.Width / 2;
                            yOffset = -textSize.Height;
                            break;
                        case MTextAttachmentPoint.TopRight:
                            xOffset = -textSize.Width;
                            yOffset = -textSize.Height;
                            break;
                        case MTextAttachmentPoint.MiddleLeft:
                            yOffset = -textSize.Height / 2;
                            break;
                        case MTextAttachmentPoint.MiddleCenter:
                            xOffset = -textSize.Width / 2;
                            yOffset = -textSize.Height / 2;
                            break;
                        case MTextAttachmentPoint.MiddleRight:
                            xOffset = -textSize.Width;
                            yOffset = -textSize.Height / 2;
                            break;
                        case MTextAttachmentPoint.BottomLeft:
                            // No offset needed
                            break;
                        case MTextAttachmentPoint.BottomCenter:
                            xOffset = -textSize.Width / 2;
                            break;
                        case MTextAttachmentPoint.BottomRight:
                            xOffset = -textSize.Width;
                            break;
                    }*/

                    // Draw the text
                    g.DrawString(ExtractString(mtext.Value), font, brush, xOffset, yOffset);

                    // Restore the graphics state
                    g.Restore(state);
                }
            }
        }
        public static string ExtractString(string input)
        {
            // Find the position of the semicolon
            int semicolonIndex = input.IndexOf(';');

            // If the semicolon is found, extract the number
            if (semicolonIndex != -1)
            {
                // Extract the substring after the semicolon
                string numberPart = input.Substring(semicolonIndex + 1);

                // Remove the closing brace if it exists
                if (numberPart.EndsWith("}"))
                {
                    numberPart = numberPart.TrimEnd('}');
                }

                return numberPart;
            }

            // If the semicolon is not found, return an empty string or handle accordingly
            return input;
        }
        public static void DrawPolyline(Graphics g, netDxf.Entities.Polyline2D polyline)
        {
            using (Pen pen = new Pen(ToColor(polyline.Layer.Color), 0.01f))
            {
                for (int i = 0; i < polyline.Vertexes.Count - 1; i++)
                {
                    g.DrawLine(pen, ToPointF(polyline.Vertexes[i].Position), ToPointF(polyline.Vertexes[i + 1].Position));
                }
                if (polyline.IsClosed)
                {
                    g.DrawLine(pen, ToPointF(polyline.Vertexes[polyline.Vertexes.Count - 1].Position), ToPointF(polyline.Vertexes[0].Position));
                }
            }
        }
        public static void DrawPolyline(Graphics g, netDxf.Entities.Polyline3D polyline)
        {
            using (Pen pen = new Pen(ToColor(polyline.Layer.Color), 0.01f))
            {
                for (int i = 0; i < polyline.Vertexes.Count - 1; i++)
                {
                    g.DrawLine(pen, ToPointF(polyline.Vertexes[i]), ToPointF(polyline.Vertexes[i + 1]));
                }
                if (polyline.IsClosed)
                {
                    g.DrawLine(pen, ToPointF(polyline.Vertexes[polyline.Vertexes.Count - 1]), ToPointF(polyline.Vertexes[0]));
                }
            }
        }
        public static void DrawHatch(Graphics g, Hatch hatch)
        {
            //Brush hatchBrush = Brushes.Gray; // Example hatch brush color
            HatchBrush hatchBrush = new HatchBrush(System.Drawing.Drawing2D.HatchStyle.Cross, ToColor(hatch.Color), ToColor(hatch.Color));

            using (Pen boundaryPen = new Pen(ToColor(hatch.Color), 0.01f))
                foreach (var boundaryPath in hatch.BoundaryPaths)
                {
                    GraphicsPath hatchPath = new GraphicsPath();

                    // Loop through edges of the boundary path
                    foreach (var edge in boundaryPath.Edges)
                    {
                        switch (edge)
                        {
                            case netDxf.Entities.HatchBoundaryPath.Line line:
                                hatchPath.AddLine((float)line.Start.X, (float)line.Start.Y,
                                          (float)line.End.X, (float)line.End.Y);
                                break;
                            case netDxf.Entities.HatchBoundaryPath.Arc arc:
                                hatchPath.AddArc((float)arc.Center.X - (float)arc.Radius, (float)arc.Center.Y - (float)arc.Radius,
                                         (float)arc.Radius * 2, (float)arc.Radius * 2,
                                         (float)arc.StartAngle, (float)(arc.EndAngle - arc.StartAngle));
                                break;
                            case netDxf.Entities.HatchBoundaryPath.Polyline pline:
                                for (int i = 0; i < pline.Vertexes.Length - 1; i++)
                                {
                                    var startVertex = pline.Vertexes[i];
                                    var endVertex = pline.Vertexes[i + 1];

                                    hatchPath.AddLine((float)startVertex.X, (float)startVertex.Y,
                                                      (float)endVertex.X, (float)endVertex.Y);
                                }

                                // Close the polyline if it is closed
                                if (pline.IsClosed)
                                {
                                    var startVertex = pline.Vertexes[pline.Vertexes.Length - 1];
                                    var endVertex = pline.Vertexes[0];
                                    hatchPath.AddLine((float)startVertex.X, (float)startVertex.Y,
                                                      (float)endVertex.X, (float)endVertex.Y);
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    // Draw the hatch using the brush and pen
                    g.FillPath(hatchBrush, hatchPath);
                    g.DrawPath(boundaryPen, hatchPath);
                }
        }
        public static PointF ToPointF(Vector2 vector)
        {
            return new PointF((float)vector.X, (float)vector.Y);
        }
        public static PointF ToPointF(Vector3 vector)
        {
            return new PointF((float)vector.X, (float)vector.Y);
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
        public static Point GetDxfPos(Point currentPos, BoundingBox initialRect, PointF offsetPos, ZoomFactor zoomFactor)
        {
            Point cnvPos = new Point();
            cnvPos.X = (int)(initialRect.MinX + offsetPos.X + currentPos.X / zoomFactor.Value);
            cnvPos.Y = (int)(initialRect.MaxY - offsetPos.Y - currentPos.Y / zoomFactor.Value);
            return cnvPos;
        }
        public static PointF GetDxfPosF(Point currentPos, BoundingBox initialRect, PointF offsetPos, ZoomFactor zoomFactor)
        {
            PointF cnvPos = new PointF();
            cnvPos.X = (float)(initialRect.MinX + offsetPos.X + currentPos.X / zoomFactor.Value);
            cnvPos.Y = (float)(initialRect.MaxY - offsetPos.Y - currentPos.Y / zoomFactor.Value);
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
    }
}
