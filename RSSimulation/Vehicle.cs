using System.Collections.Generic;
using System.Drawing;

namespace RSSimulation
{
    internal class Vehicle
    {
        protected PointF center { get; set; }
        protected float angle { get; set; }
        protected int status { get; set; }
        protected int loadedRS { get; set; }
        protected Image imgV;
        protected Queue<Command> _cmdList;

        public Vehicle()
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
        public int runCommand(int _curTime)
        {
            if (getCommand() == null || getCommand().stTime > _curTime)
            {
                return -1;
            }
            Command _cmd = getCommand();
            setLoadedRS(_cmd.loadedRS);
            if (_curTime >= _cmd.enTime)
            {
                setCenter(_cmd.enPos);
                setAngle(_cmd.enAngle);
                setStatus(_cmd.type);
                return _cmd.enTime;
            }
            else
            {
                if (_cmd.direction == 0) // moving...
                {
                    float x = _cmd.stPos.X + (_cmd.enPos.X - _cmd.stPos.X) * (_curTime - _cmd.stTime) / (_cmd.enTime - _cmd.stTime);
                    float y = _cmd.stPos.Y + (_cmd.enPos.Y - _cmd.stPos.Y) * (_curTime - _cmd.stTime) / (_cmd.enTime - _cmd.stTime);
                    setAngle(_cmd.enAngle);
                    setCenter(new PointF(x, y));
                }
                else // rotating...
                {
                    float _ang = _cmd.stAngle + (_cmd.enAngle - _cmd.stAngle) * (_curTime - _cmd.stTime) / (_cmd.enTime - _cmd.stTime);
                    setAngle(_ang);
                    setCenter(_cmd.enPos);
                }
                if (_cmd.type > 100)
                {
                    setStatus(_cmd.type);
                }
            }
            return _curTime;
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
            float scaledWidth = imgV.Width / scaleFactor;
            float scaledHeight = imgV.Height / scaleFactor;

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
            g.DrawImage(imgV, 0, 0, scaledWidth, scaledHeight);

            // Restore the graphics object state
            g.Restore(state);
        }
    }
}
