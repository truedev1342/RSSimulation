using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSimulation
{
    internal class Carrier
    {
        private PointF center { get; set; }
        private float angle { get; set; }

        private float telemeter { get; set; }

        private Image lcc = Properties.Resources.LCC;

        public PointF getCenter()
        {
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

        public float getTelemeter()
        {
            return telemeter;
        }

        public void setTelemeter(float _telemeter)
        {
            telemeter = _telemeter;
        }

        public void rotate(float _angle)
        {
            angle += _angle;
        }

        public void move(float _dx, float _dy)
        {
            center = new PointF(center.X + _dx, center.Y + _dy);
            if (telemeter != -1)
            {
                increaseTelemeter((float)Math.Sqrt(_dx * _dx + _dy * _dy));
            }
        }

        public void increaseTelemeter(float d)
        {
            telemeter += d;
        }

        public void draw(Graphics g)
        {

            // Set the scale factor for the image
            float scaleFactor = 70;

            // Calculate the size of the image after scaling
            float scaledWidth = lcc.Width / scaleFactor;
            float scaledHeight = lcc.Height / scaleFactor;

            // Calculate the rotation point
            float centerX = scaledWidth / 2;
            float centerY = scaledHeight / 2;

            // Save the current state of the graphics object
            var state = g.Save();

            // Apply the transformations
            g.TranslateTransform(center.X, center.Y); // Move to the rotation point
            g.RotateTransform(angle - 180); // Rotate 45 degrees
            g.TranslateTransform(-centerX, -centerY); // Move back by the rotation point offsets

            // Draw the image at the translated and rotated position
            g.DrawImage(lcc, 0, 0, scaledWidth, scaledHeight);

            // Restore the graphics object state
            g.Restore(state);
        }
    }
}
