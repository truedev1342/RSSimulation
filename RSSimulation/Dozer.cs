using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;


namespace RSSimulation
{
    internal class Dozer
    {
        private PointF center { get; set; }
        private float angle { get; set; }
        private int status { get; set; }
        private int loadedRS { get; set; }
        private Image dz = Properties.Resources.Dozer;
        private Queue<Command> _cmdList;

        public Dozer()
        {
            center = PointF.Empty;
            angle = 0.0f;
            status = 0;
            loadedRS = -1;
            _cmdList = new Queue<Command>();
        }
        public PointF getCenter() { return center; }
        public void setCenter(PointF _center) { center = _center; }
        public float getAngle() { return angle; }
        public void setAngle(float _angle) { angle = _angle; }
        public int getStatus() { return status; }
        public void setStatus(int _status) { status = _status; }
        public int getLoadedRS() { return loadedRS; }
        public void setLoadedRS(int _loadedRS) { loadedRS = _loadedRS; }

        public Command getCommand()
        {
            if (_cmdList.Count == 0) return null;
            return _cmdList.Peek();
        }

        public Command dequeueCommand()
        {
            if (_cmdList.Count == 0) { return null; }
            return _cmdList.Dequeue();
        }

        public void clearCommand()
        {
            _cmdList.Clear();
        }
        public void addCommand(Command cmd)
        {
            _cmdList.Enqueue(cmd);
        }
        public void addCommand(int _stTime, int _enTime, PointF _stPos, PointF _enPos, float _stAngle, float _enAngle, int _direction = 0, int _loadedRS = -1, int _type = 0)
        {
            _cmdList.Enqueue(new Command(_stTime, _enTime, _stPos, _enPos, _stAngle, _enAngle, _direction, _loadedRS, _type));
        }

        public int runCommand(int _curTime, int dt)
        {
            while(getCommand() != null && getCommand().enTime <= _curTime)
            {
                dequeueCommand();
            }
            if (getCommand() == null || getCommand().stTime > _curTime)
            {
                return 0;
            }
            Command _cmd = getCommand();
            setLoadedRS(_cmd.loadedRS);
            if (_curTime + dt >= _cmd.enTime)
            {
                setCenter(_cmd.enPos);
                setAngle(_cmd.enAngle);
                dequeueCommand();
                return _cmd.type;
            }
            else
            {
                int cmdLeft = (_cmd.enTime - _curTime) / dt;
                if (_cmd.direction == 0) // moving...
                {
                    float dx = (_cmd.enPos.X - getCenter().X) / cmdLeft;
                    float dy = (_cmd.enPos.Y - getCenter().Y) / cmdLeft;
                    setAngle(_cmd.enAngle);
                    move(dx, dy);
                }
                else // rotating...
                {
                    float _ang = Math.Abs(_cmd.enAngle - getAngle());
                    rotate((float)_cmd.direction * _ang / cmdLeft);
                    setCenter(_cmd.enPos);
                }
            }
            return 0;
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
