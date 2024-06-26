using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSimulation
{
    internal class RoofSupport
    {
        private int num { get; set; }
        private PointF center { get; set; }
        private float angle { get; set; }

        private Image RS = Properties.Resources.RS;

        public int getNum()
        {
            return num;
        }
        public void setNum(int _num) {
            num = _num;
        }

        public PointF getCenter() {
            return center;
        }

        public void setCenter(PointF _center)
        {
            center = _center;
        }

        public float getAngle()
        {
            return angle;
        }
        public void setAngle(float _angle)
        {
            angle = _angle;
        }

        public void rotate(float _angle)
        {
            angle += _angle;
        }

        public void move(float _dx, float _dy) 
        {
            center = new PointF(center.X + _dx, center.Y + _dy);
        }

        public void draw(Graphics g)
        {
            // Set the scale factor for the image
            float scaleFactor = 70;

            float scaledWidth = RS.Width / scaleFactor;
            float scaledHeight = RS.Height / scaleFactor;

            // Calculate the rotation point
            float centerX = scaledWidth / 2;
            float centerY = scaledHeight / 2;

            var state = g.Save();

            // Apply the transformations
            g.TranslateTransform(center.X, center.Y); // Move to the rotation point
            g.RotateTransform(angle); // Rotate
            g.TranslateTransform(-centerX, -centerY); // Move back by the rotation point offsets

            // Draw the image at the translated and rotated position
            g.DrawImage(RS, 0, 0, scaledWidth, scaledHeight);

            // Restore the graphics object state
            g.Restore(state);

            drawText(g);
        }

        private void drawText(Graphics g)
        {
            string fontName = "Arial"; // Default font
            FontStyle fontStyle = FontStyle.Regular;
            using (Font font = new Font(
               new FontFamily($"{fontName}"),
               (float)0.8,
               FontStyle.Regular,
               GraphicsUnit.Pixel))
            using (Brush brush = new SolidBrush(Color.White))
            {
                StringAlignment alignment = StringAlignment.Center;
                PointF position = center;

                // Save the current state of the graphics object
                GraphicsState state = g.Save();

                // Translate to the position where you want to draw the text
                g.TranslateTransform(position.X, position.Y);

                // Rotate the coordinate system 180 degrees (vertically invert)

                g.RotateTransform(angle - 90);
                g.TranslateTransform((float)-1, (float)2.5);
                g.ScaleTransform(1, -1);
                // Draw the text at the origin of the rotated coordinate system
                g.DrawString("#" + num.ToString(), font, brush, 0, 0);

                // Restore the graphics object to its original state
                g.Restore(state);
            }
        }
    }
}
