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
        private float RSWidth = 350.0f / 70.0f;
        private float RSHeight = 140.0f / 70.0f;
        private PointF RSCenter = new PointF(2.5f, 1.0f);
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
            /*// Set the scale factor for the image
            float scaleFactor = 70.0f;

            float scaledWidth = RS.Width / scaleFactor;
            float scaledHeight = RS.Height / scaleFactor;

            // Calculate the rotation point
            float centerX = scaledWidth / 2.0f;
            float centerY = scaledHeight / 2.0f;*/

            var state = g.Save();

            // Apply the transformations
            g.TranslateTransform(center.X, center.Y); // Move to the rotation point
            g.RotateTransform(angle); // Rotate
            g.TranslateTransform(-RSCenter.X, -RSCenter.Y); // Move back by the rotation point offsets

            // Draw the image at the translated and rotated position
            g.DrawImage(RS, 0, 0, RSWidth, RSHeight);

            // Restore the graphics object state
            g.Restore(state);

            drawText(g);
        }

        private void drawText(Graphics g)
        {
            string fontName = "Arial"; // Default font
            using (Font font = new Font(
               new FontFamily($"{fontName}"),
               0.7f,
               FontStyle.Regular,
               GraphicsUnit.Pixel))
            using (Brush brush = new SolidBrush(Color.White))
            {
                PointF position = center;
                // Save the current state of the graphics object
                GraphicsState state = g.Save();
                // Translate to the position where you want to draw the text
                g.TranslateTransform(position.X, position.Y);
                // Rotate the coordinate system 180 degrees (vertically invert)
                g.RotateTransform(angle + 90);
                g.TranslateTransform(-0.9f, -2.5f);
                //g.ScaleTransform(1, -1);
                // Draw the text at the origin of the rotated coordinate system
                g.DrawString("#" + num.ToString(), font, brush, 0, 0);
                // Restore the graphics object to its original state
                g.Restore(state);
            }
        }
    }
}
