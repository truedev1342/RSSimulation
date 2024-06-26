using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSimulation
{
    internal class Dozer
    {
        private PointF center { get; set; }
        private float angle { get; set; }

        private Image dz = Properties.Resources.Dozer;

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

            // Calculate the size of the image after scaling
            float scaledWidth = dz.Width / scaleFactor;
            float scaledHeight = dz.Height / scaleFactor;

            // Calculate the rotation point
            float centerX = scaledWidth / 2;
            float centerY = scaledHeight / 2;

            // Save the current state of the graphics object
            var state = g.Save();

            // Apply the transformations
            g.TranslateTransform(center.X, center.Y); // Move to the rotation point
            g.RotateTransform(angle); // Rotate 45 degrees
            g.TranslateTransform(-centerX, -centerY); // Move back by the rotation point offsets

            // Draw the image at the translated and rotated position
            g.DrawImage(dz, 0, 0, scaledWidth, scaledHeight);

            // Restore the graphics object state
            g.Restore(state);
        }
    }
}
