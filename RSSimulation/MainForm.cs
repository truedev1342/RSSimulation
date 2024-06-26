using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using netDxf;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Tables;
using static System.Net.Mime.MediaTypeNames;
using static netDxf.Entities.HatchBoundaryPath;
using FontStyle = System.Drawing.FontStyle;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;

namespace RSSimulation
{
    public partial class MainForm : Form
    {
        private string filePath = "2013.dxf";
        private DxfDocument _dxfDocument = null;
        SizeF DrawAreaSize;

        private BoundingBox initialRect = new BoundingBox();
        private ZoomFactor zoomFactor = new ZoomFactor(1.0f, 0.1f, 0.1f, 100.0f);
        private PointF offsetPos = new PointF();
        private Point moveStartPoint = Point.Empty;


        private DateTime _startTime = DateTime.Now;
        private DateTime _curTime = DateTime.Now;
        private int _scale = 0;
        private int _rsNum = 0;
        private int _necessaryTime = 0;
        private int _elapsed = 0;
        private int _total = 0;
        private bool _started = false;

        private PointF _stPoint = PointF.Empty;
        private PointF _enPoint = PointF.Empty;
        private PointF _basePoint = PointF.Empty;
        private PointF eFramePoint = Point.Empty;

        private RoofSupport[] _rsData = new RoofSupport[202];
        private Dozer _dozer = new Dozer();
        private float _tempDx = 0.0f;
        private float _tempDy = 0.0f;
        private int _timerInterval = 1000;
        
        private List<int> _available = new List<int>();
        private List<int> _unavailable = new List<int>();

        public MainForm()
        {
            InitializeComponent();
            initForms();
            
            //this.DrawArea.MouseWheel += new MouseEventHandler(DrawArea_MouseWheel);            
            DrawArea.Paint += new PaintEventHandler(DrawArea_Paint);
            this.DoubleBuffered = true;
            DrawArea.MouseDown += DrawArea_MouseDown;
            DrawArea.MouseUp += DrawArea_MouseUp;
            DrawArea.MouseMove += DrawArea_MouseMove;
            DrawArea.MouseWheel += new MouseEventHandler(DrawPanel_MouseWheel);
            for (int i = 0; i < 202; i ++)
            {
                _rsData[i] = new RoofSupport();
            }
        }
        private void initForms()
        {
            _dxfDocument = null;
            initialRect = new BoundingBox();
            zoomFactor = new ZoomFactor(1.0f, 0.1f, 0.1f, 100.0f);
        }
        private void LoadDXFFile(string strPath)
        {
            try
            {
                _dxfDocument = DxfDocument.Load(strPath);
                initialRect = HaDesMath.GetBoundingBox(_dxfDocument);
                DrawAreaSize = HaDesMath.UpdateDrawAreaSize(initialRect, zoomFactor);
                DrawArea.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to render DXF file: {ex.Message}");
            }
        }              
        private void DrawArea_MouseDown(object sender, MouseEventArgs e)
        {
            Point currentPoint = DrawPanel.PointToClient(DrawArea.PointToScreen(e.Location));
            if (e.Button == MouseButtons.Right)
            {
                moveStartPoint = currentPoint;
                DrawArea.Cursor = Cursors.Hand;
            }
            if (e.Button == MouseButtons.Left)
            {
                DrawArea.Cursor = Cursors.Cross;
                if (EFramePosBtn.Checked == true)
                {
                    //DrawArea.Cursor = Cursors.Default;
                    eFramePoint = HaDesGraphics.GetDxfPosF(e.Location, initialRect, offsetPos, zoomFactor);
                    EFramePosBtn.Checked = false;
                    _elapsed = 0;
                    _curTime = _startTime;
                    InitializeRSandDozer();
                    DrawArea.Invalidate();
                } else
                {
                    moveStartPoint = e.Location;
                }
            }
        }
        private void DrawArea_MouseUp(object sender, MouseEventArgs e)
        {
            moveStartPoint = e.Location;
            if(moveStartPoint != e.Location)
            {
                Point currentPoint = e.Location;
                // Calculate the delta (difference) from the starting point
                int deltaX = (int)((currentPoint.X - moveStartPoint.X) / zoomFactor.Value);
                int deltaY = (int)((currentPoint.Y - moveStartPoint.Y) / zoomFactor.Value);

                HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(deltaX, deltaY));
                // Update moveStartPoint for the next calculation
                moveStartPoint = currentPoint;

                MousePositionSB.Text = $"Offset:({offsetPos.X}, {offsetPos.Y}), " +
                    $"AreaSize:({DrawAreaSize.Width},{DrawAreaSize.Height})";
                DrawArea.Invalidate();
            }
            DrawArea.Cursor = Cursors.Default;
        }
        private void DrawArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point currentPoint = e.Location;
                // Calculate the delta (difference) from the starting point
                int deltaX = (int)((currentPoint.X - moveStartPoint.X) / zoomFactor.Value);
                int deltaY = (int)((currentPoint.Y - moveStartPoint.Y) / zoomFactor.Value);

                HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(deltaX, deltaY));
                // Update moveStartPoint for the next calculation
                moveStartPoint = currentPoint;

                MousePositionSB.Text = $"Offset:({offsetPos.X}, {offsetPos.Y}), " +
                    $"AreaSize:({DrawAreaSize.Width},{DrawAreaSize.Height})";
                DrawArea.Invalidate();
            }
        }
        private void DrawPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta;
            (e as HandledMouseEventArgs).Handled = true;
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control) // Ctrl key pressed
            {
                if (delta > 0)
                    zoomFactor.Value += zoomFactor.Step;
                else if (delta < 0)
                    zoomFactor.Value -= zoomFactor.Step;
                ZoomProcessing();
                return;
            }
            else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) // Shift key pressed
            {
                // Adjust horizontal scroll
                HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(delta, 0));
                DrawArea.Invalidate();
                return;
            }
            // Adjust vertical scroll
            HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(0, delta));
            DrawArea.Invalidate();
            return;
        }

        private float distancePoints(PointF a,  PointF b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private PointF calcBasePoint()
        {
            float a = distancePoints(_enPoint, eFramePoint);
            float b = distancePoints(_stPoint, eFramePoint);
            float c = distancePoints(_stPoint, _enPoint);
            float p = (a + b + c) / 2;
            float s = (float)Math.Sqrt(p * (p - a) * (p - b) * (p - c));
            float d = s * 2 / c;
            PointF basePoint = eFramePoint;
            float dy = Math.Abs((_enPoint.X - _stPoint.X) * d / c);
            float dx = Math.Abs((_enPoint.Y - _stPoint.Y) * d / c);
            basePoint.X = basePoint.X - dx;
            basePoint.Y = basePoint.Y - dy;
            return basePoint;
        }

        private void InitializeRSandDozer()
        {
            foreach (var entity in _dxfDocument.Entities.All)
            {
                if (entity.Type == EntityType.Line)
                {
                    var line = (netDxf.Entities.Line)entity;
                    if (line.Handle as string == "10526")
                    {
                        _stPoint = HaDesGraphics.ToPointF(line.StartPoint);
                        _enPoint = HaDesGraphics.ToPointF(line.EndPoint);
                    }
                }
            }
            _basePoint = calcBasePoint();
            float c = distancePoints(_stPoint, _enPoint);
            float alpha = (float)(Math.Acos((_enPoint.X - _stPoint.X) / c) * 180 / Math.PI);

            float dy = (float)((_enPoint.X - _stPoint.X) * 5 / c);
            float dx = (float)((_enPoint.Y - _stPoint.Y) * 5 / c);
            float positionX = _basePoint.X - dx;
            float positionY = _basePoint.Y + dy;
            dx = (float)((_enPoint.X - _stPoint.X) / c);
            dy = (float)((_enPoint.Y - _stPoint.Y) / c);
            positionX = positionX - dx;
            positionY = positionY - dy;
            _dozer.setAngle(90 - alpha);
            _dozer.setCenter(new PointF(positionX, positionY));


            for (int i = 0; i < _rsNum; i++)
            {
                dy = (float)((_enPoint.X - _stPoint.X) * 7.5 / c);
                dx = (float)((_enPoint.Y - _stPoint.Y) * 7.5 / c);
                positionX = _basePoint.X - dx;
                positionY = _basePoint.Y + dy;
                dx = (float)((_enPoint.X - _stPoint.X) * (1 + i * 2) / c);
                dy = (float)((_enPoint.Y - _stPoint.Y) * (1 + i * 2) / c);
                positionX = positionX - dx;
                positionY = positionY - dy;

                _rsData[_rsNum - i].setNum(_rsNum - i);
                _rsData[_rsNum - i].setCenter(new PointF(positionX, positionY));
                _rsData[_rsNum - i].setAngle(90 - alpha);
            }
        }


        private void DrawArea_Paint(object sender, PaintEventArgs e)
        {
            
            if (_dxfDocument == null)
                return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);
            //g.ScaleTransform(_zoomFactor, _zoomFactor);

            // Translate the coordinates to center the drawing
            var offsetX = -initialRect.MinX - offsetPos.X;
            var offsetY = -initialRect.MinY + offsetPos.Y;

            // Apply the transformation
            g.TranslateTransform(0, DrawAreaSize.Height);
            g.ScaleTransform(1, -1); // Invert Y-axis to match the usual coordinate system
            g.ScaleTransform(zoomFactor.Value, zoomFactor.Value);
            g.TranslateTransform((float)offsetX, (float)offsetY);
            //g.DrawImage(Dozer, 627459, 7570823, Dozer.Width/25, Dozer.Height/25);

            RectangleF visibleArea = HaDesMath.GetVisibleAreaOfDrawArea(DrawArea, zoomFactor.Value, initialRect, offsetPos);

            foreach (var entity in _dxfDocument.Entities.All)
            {
                /*if (entity. .Bounds != null && !visibleArea.IntersectsWith(entity.Bounds.Value))
                    continue; */// Skip drawing if entity is outside visible area
                switch (entity.Type)
                {
                    case EntityType.Hatch:
                        //if (visibleArea.IntersectsWith(HaDesMath.GetHatchBounds((netDxf.Entities.Hatch)entity)))
                            HaDesGraphics.DrawHatch(g, (netDxf.Entities.Hatch)entity);
                        break;
                    case EntityType.Line:
                        var line = (netDxf.Entities.Line)entity;
                        RectangleF lineBounds = HaDesMath.GetLineBounds(line);
                        //if (line.Handle as string != "10526" &&  !visibleArea.IntersectsWith(lineBounds))
                          //  break;
                        using (Pen pen = new Pen(HaDesGraphics.ToColor(line.Layer.Color), 1))
                        {
                            g.DrawLine(pen, HaDesGraphics.ToPointF(line.StartPoint), HaDesGraphics.ToPointF(line.EndPoint));
                        }
                        break;

                    case EntityType.Circle:
                        var circle = (netDxf.Entities.Circle)entity; 
                        using (Pen pen = new Pen(HaDesGraphics.ToColor(circle.Layer.Color), 1))
                        {
                            ;// g.DrawEllipse(pen, ToRectangleF(circle.Center, circle.Radius));
                        }
                        break;
                    case EntityType.Polyline2D :
                        //if (visibleArea.IntersectsWith(HaDesMath.GetPolylineBounds((netDxf.Entities.Polyline2D)entity)))
                        HaDesGraphics.DrawPolyline(g, (netDxf.Entities.Polyline2D)entity);
                        break;
                    case EntityType.Polyline3D:
                        //if (visibleArea.IntersectsWith(HaDesMath.GetPolylineBounds((netDxf.Entities.Polyline3D)entity)))
                        HaDesGraphics.DrawPolyline(g, (netDxf.Entities.Polyline3D)entity);
                        break;
                    case EntityType.Text:
                        var text = (netDxf.Entities.Text)entity;
                        HaDesGraphics.DrawText(g, text);
                        break;
                    case EntityType.MText:
                        var mtext = (netDxf.Entities.MText)entity;
                        HaDesGraphics.DrawMText(g, mtext);
                        break;
                    default:                        
                        break;

                        // Add more entity types as needed and handle their drawing and colors
                }
            }
            _dozer.draw(g);
            for (int i = 1; i <= _rsNum; i ++)
            {
                _rsData[i].draw(g);
            }

            /*foreach (var line in _dxfDocument.Entities.Lines)
            {
                g.DrawLine(Pens.Black, ToPointF(line.StartPoint), ToPointF(line.EndPoint));
            }*/
            //g.DrawLine(Pens.Black, ToPointF(new Vector3(50,50,0)), ToPointF(new Vector3(150, 150, 0)));
        }         
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (_rsNum == 0)
            {
                EFramePosBtn.Enabled = false;
                EFramePosBtn.CheckOnClick = true;
                StopSimulationBtn.Enabled = false;
                SimulationTimeSB.Text = _startTime.ToString();
                ElapsedTimeSB.Text = "00:00:00";
            }
        }
        private string convertSecToTimeStr(int sec)
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

        private void processSimulation()
        {
            int idx = _rsNum - _elapsed / (_total / 3);
            int perTime = _elapsed % (_total / 3);
            if (perTime == 0) perTime = _total / 3;
            int subTime = _total / 9;
            if (perTime <= subTime) // rotating...
            {
                float _angle = 90 / subTime ;
                _dozer.rotate(-_angle);
                _rsData[idx].rotate(-_angle);
                float x1 = (float)(_dozer.getCenter().X + 2.5 * Math.Cos(_dozer.getAngle() * Math.PI / 180));
                float y1 = (float)(_dozer.getCenter().Y + 2.5 * Math.Sin(_dozer.getAngle() * Math.PI / 180));
                _rsData[idx].setCenter(new PointF(x1, y1));
                if (perTime == subTime) // rotated
                {
                    float c = distancePoints(_stPoint, _enPoint);
                    float alpha = (float)(Math.Acos((_enPoint.X - _stPoint.X) / c) * 180 / Math.PI);
                    float dy = (float)((_enPoint.X - _stPoint.X) * (1 + (_rsNum - idx) * 2) / c);
                    float dx = (float)((_enPoint.Y - _stPoint.Y) * (1 + (_rsNum - idx) * 2) / c);
                    float positionXX = _basePoint.X - dx;
                    float positionYY = _basePoint.Y + dy;
                    dx = (float)((_enPoint.X - _stPoint.X) * 2.5 / c);
                    dy = (float)((_enPoint.Y - _stPoint.Y) * 2.5 / c);
                    positionXX = positionXX - dx;
                    positionYY = positionYY - dy;

                    _tempDx = (positionXX - _rsData[idx].getCenter().X) / subTime;
                    _tempDy = (positionYY - _rsData[idx].getCenter().Y) / subTime;
                }
            }
            else if (perTime <= 2 * subTime) // moving...
            {
                _rsData[idx].move(_tempDx, _tempDy);
                _dozer.move(_tempDx, _tempDy);
                if (perTime == 2 * subTime)
                {
                    _dozer.setAngle(_rsData[idx - 1].getAngle());
                    float x1 = _rsData[idx - 1].getCenter().X - (float)(2.5 * Math.Cos(_rsData[idx - 1].getAngle() * Math.PI / 180));
                    float y1 = _rsData[idx - 1].getCenter().Y - (float)(2.5 * Math.Sin(_rsData[idx - 1].getAngle() * Math.PI / 180));
                    _tempDx = (x1 - _dozer.getCenter().X) / subTime;
                    _tempDy = (y1 - _dozer.getCenter().Y) / subTime;
                }
            }
            else if (perTime <= 3 * subTime)
            {
                _dozer.move(_tempDx, _tempDy);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_elapsed >= _total)
            {
                timer1.Enabled = false;
                StopSimulationBtn.Enabled = false;
                StartSimulationBtn.Enabled = true;
                return;
            }
            _elapsed++;
            _curTime = _curTime + TimeSpan.FromSeconds(_scale * _timerInterval / 1000);
            SimulationTimeSB.Text = _curTime.ToString();
            ElapsedTimeSB.Text = convertSecToTimeStr(_elapsed * _timerInterval);
            processSimulation();
            DrawArea.Invalidate();
        }
        /// Menubar Event Process Start
        private void onNewSMenu(object sender, EventArgs e)
        {

        }
        private void onOpenSMenu(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Set the filter options
            openFileDialog.Filter = "DXF Files (*.dxf)|*.dxf";
            openFileDialog.Title = "Open AutoCAD File";
            if (filePath == null)
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            else
                openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                LoadDXFFile(filePath);
            }
        }
        private void onCloseSMenu(object sender, EventArgs e)
        {

        }
        private void onZoomInSMenu(object sender, EventArgs e)
        {
            zoomFactor.Value *= 2.0f;
            ZoomProcessing();
        }

        private void onZoomOutSMenu(object sender, EventArgs e)
        {
            zoomFactor.Value /= 2.0f;
            ZoomProcessing();
        }

        private void onZoomFitSMenu(object sender, EventArgs e)
        {
            zoomFactor.Value = 1.0f;
            ZoomProcessing();
        }
        private void ZoomProcessing()
        {
            if (zoomFactor.Value < zoomFactor.Min)
                zoomFactor.Value = zoomFactor.Min;
            if (zoomFactor.Value > zoomFactor.Max)
                zoomFactor.Value = zoomFactor.Max;
            ScaleSB.Text = $"{zoomFactor.Value}";
            DrawAreaSize = HaDesMath.UpdateDrawAreaSize(initialRect, zoomFactor);
            DrawArea.Invalidate();
        }
        private void onSimulationProperiesSMenu(object sender, EventArgs e)
        {
            SettingsForm sfrm = new SettingsForm();

            sfrm._necessaryTime = _necessaryTime;
            sfrm._scale = _scale;
            sfrm._rsNum = _rsNum;
            sfrm._startTime = _startTime;


            if (sfrm.ShowDialog() == DialogResult.OK)
            {
                _necessaryTime = sfrm._necessaryTime;
                _scale = sfrm._scale;
                _rsNum = sfrm._rsNum;
                _startTime = sfrm._startTime;

                if (_rsNum > 0 && _rsNum < 150)
                {
                    _available.Clear();
                    _unavailable.Clear();
                    for (int i = 1; i <= _rsNum; i ++)
                    {
                        _available.Add(i);
                    }
                    EFramePosBtn.Enabled = true;
                    EFramePosBtn.Checked = false;
                }
                else
                {
                    EFramePosBtn.Enabled = false;
                    EFramePosBtn.Checked = false;
                }
            }

        }
        private void onSalvageSequenceSMenu(object sender, EventArgs e)
        {
            SalvageForm svfrm = new SalvageForm();
            svfrm._available = _available;
            svfrm._unavailable = _unavailable;
            if (svfrm.ShowDialog() == DialogResult.OK)
            {
                _available = svfrm._available;
                _unavailable = svfrm._unavailable;
            }
        }
        private void onViewHelpSMenu(object sender, EventArgs e)
        {

        }
        /// Menubar Event Process End
        /// Toolbar Event process Start
        private void onSetALocationBtn(object sender, EventArgs e)
        {

        }
        private void onSetBLocationBtn(object sender, EventArgs e)
        {

        }
        private void onEFramePosBtn(object sender, EventArgs e)
        {
            if (EFramePosBtn.Checked)
            {
                DrawArea.Cursor = Cursors.Hand;
            }
            else
            {
                DrawArea.Cursor = Cursors.Default;
            }
        }
        private void onSetRouteABtn(object sender, EventArgs e)
        {

        }

        private void onSetRouteBBtn(object sender, EventArgs e)
        {

        }
        private void onStartSimulationBtn(object sender, EventArgs e)
        {
            if (eFramePoint == Point.Empty) return;
            if (_scale <= 0) return;
            _started = true;
            _elapsed = 0;
            _curTime = _startTime;
            StartSimulationBtn.Enabled = false;
            StopSimulationBtn.Enabled = true;
            InitializeRSandDozer();
            float _temp = (float)(_necessaryTime * 3600 / _scale / 9);
            int _intTemp = (int)_temp;
            _total = _intTemp * 9;
            _timerInterval = (int)(_temp * 1000 / _intTemp);
            timer1.Interval = _timerInterval;
            timer1.Enabled = true;
        }
        private void onStopSimulationBtn(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            _started = false;
            StartSimulationBtn.Enabled = true;
            StopSimulationBtn.Enabled = false;
        }
        /// Toolbar Event process End
    }
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }
}
