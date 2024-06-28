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
        private HaDesDxf _haDesDxf = null;
        SizeF DrawAreaSize;

        private BoundingBox initialRect = new BoundingBox();
        private ZoomFactor zoomFactor = new ZoomFactor(1.0f, 0.1f, 0.1f, 100.0f);
        private PointF offsetPos = new PointF();
        private Point moveStartPoint = new Point();

        private DateTime _curTime = DateTime.Now;
        private DateTime SimulationTime = DateTime.Now;
        private int SimulationSpeed = 3600;
        private int RSCounts = 0;
        private int EFrameBuiltTime = 48;
        private int _elapsed = 0;
        private int _total = 0;
        private bool _started = false;

        private LinePos initRSPos = new LinePos();
        private PointF eFramePoint = Point.Empty;
        private PointF _basePoint = new PointF();

        private RoofSupport[] _rsData = new RoofSupport[202];
        private Dozer _dozer = new Dozer();
        
        private List<int> _available = new List<int>();
        private List<int> _unavailable = new List<int>();

        private Queue<Command> _commands = new Queue<Command>();

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
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (RSCounts == 0)
            {
                EFramePosBtn.Enabled = false;
                EFramePosBtn.CheckOnClick = true;
                StopSimulationBtn.Enabled = false;
                SimulationTimeSB.Text = SimulationTime.ToString();
                ElapsedTimeSB.Text = "00:00:00";
            }
        }
        private void initForms()
        {
            _dxfDocument = null;
            _haDesDxf = null;
            initialRect = new BoundingBox();
            zoomFactor = new ZoomFactor(1.0f, 0.1f, 0.1f, 100.0f);
            for (int i = 0; i < 202; i++)
            {
                _rsData[i] = new RoofSupport();
            }
        }
        private void LoadDXFFile(string strPath)
        {
            try
            {
                _dxfDocument = DxfDocument.Load(strPath);
                _haDesDxf = new HaDesDxf();
                initialRect = _haDesDxf.ReadDxf(_dxfDocument);
                initRSPos = _haDesDxf.getPrimaryLinePos();
                //initialRect = HaDesMath.GetBoundingBox(_dxfDocument, ref initRSPos);
                DrawAreaSize = HaDesMath.UpdateDrawAreaSize(initialRect, zoomFactor);
                DrawArea.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to render DXF file: {ex.Message}");
            }
        }         
        private void InitializeRSandDozer()
        {
            _basePoint = HaDesMath.CalcBasePoint(initRSPos, eFramePoint);
            float c = HaDesMath.CalcDistancePoints(initRSPos.StartPos, initRSPos.EndPos);
            float alpha = (float)(Math.Acos((initRSPos.EndPos.X - initRSPos.StartPos.X) / c) * 180 / Math.PI);

            float dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * 5 / c);
            float dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * 5 / c);
            float positionX = _basePoint.X + dx;
            float positionY = _basePoint.Y - dy;
            dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * 1.7 / c);
            dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * 1.7 / c);
            positionX = positionX - dx;
            positionY = positionY - dy;
            _dozer.setAngle(alpha - 90);
            _dozer.setCenter(new PointF(positionX, positionY));


            for (int i = 0; i < RSCounts; i++)
            {
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * 8.5 / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * 8.5 / c);
                positionX = _basePoint.X + dx;
                positionY = _basePoint.Y - dy;
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * (2 + i * 2) / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * (2 + i * 2) / c);
                positionX = positionX - dx;
                positionY = positionY - dy;

                _rsData[RSCounts - i].setNum(RSCounts - i);
                _rsData[RSCounts - i].setCenter(new PointF(positionX, positionY));
                _rsData[RSCounts - i].setAngle(alpha - 90);
            }
        }
        private void DrawArea_Paint(object sender, PaintEventArgs e)
        {            
            if (_dxfDocument == null || _haDesDxf == null)
                return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            g.ScaleTransform(zoomFactor.Value, zoomFactor.Value);
            g.TranslateTransform(-offsetPos.X, -offsetPos.Y);

            RectangleF visibleArea = HaDesMath.GetVisibleAreaOfDrawArea(DrawArea, zoomFactor.Value, initialRect, offsetPos);

            _haDesDxf.DrawDxf(g);
            _dozer.draw(g);
            for (int i = 1; i <= RSCounts; i++)
            {
                _rsData[i].draw(g);
            }
        }                 
        private void processSimulation()
        {
            while (_commands.Count > 0 && _commands.Peek()._enTime <= _elapsed)
            {
                _commands.Dequeue();
            }
            if (_commands.Count == 0)
            {
                return;
            }
            Command _cmd = _commands.Peek();
            int dt = SimulationSpeed * timer1.Interval / 1000;
            if (_elapsed + dt >= _cmd._enTime) {
                if (_cmd._type == 0) // rotating...
                {
                    _dozer.setAngle(_cmd._enAngle);
                } else if (_cmd._type == 1) // moving...
                {
                    _dozer.setCenter(_cmd._enPos);
                }
            } else
            {
                int cmdLeft = (_cmd._enTime - _elapsed) / dt;
                if (_cmd._type == 0) // rotating...
                {
                    float _ang = Math.Abs(_cmd._enAngle - _dozer.getAngle());
                    /*if (_ang < 0)
                    {
                        _ang += 360;
                    } else if (_ang > 360)
                    {
                        _ang -= 360;
                    }*/
                    _dozer.rotate((float)_cmd._rotDir * _ang / cmdLeft);
                }
                else if (_cmd._type == 1) // moving...
                {
                    float dx = (_cmd._enPos.X - _dozer.getCenter().X) / cmdLeft;
                    float dy = (_cmd._enPos.Y - _dozer.getCenter().Y) / cmdLeft;
                    _dozer.move(dx, dy);
                }
            }
            if (_cmd._loadedRS)
            {
                int id = _cmd._rsNum;
                float x1 = _dozer.getCenter().X + (float)(3.5 * Math.Cos(_dozer.getAngle() * Math.PI / 180));
                float y1 = _dozer.getCenter().Y + (float)(3.5 * Math.Sin(_dozer.getAngle() * Math.PI / 180));
                _rsData[id].setCenter(new PointF(x1, y1));
                _rsData[id].setAngle(_dozer.getAngle());
            }
        }

        private bool isSalvage(int id)
        {
            for (int i = 0; i < _unavailable.Count; i++)
            {
                if (_unavailable[i] == id)
                {
                    return true;
                }
            }
            return false;
        }

        private int getNextMoveId(int id)
        {
            for (int i = id - 1; i >= 0; i--)
            {
                if (!isSalvage(i)) return i;
            }
            return 0;
        }

        private void makeCommandList()
        {
            _commands.Clear();
            int curTime = 0;
            int step = EFrameBuiltTime * 3600 / 18;
            Command _cmd;
            int carried = 0;
            float c = HaDesMath.CalcDistancePoints(initRSPos.StartPos, initRSPos.EndPos);
            float dx, dy, x1, y1;
            PointF tempPos = _dozer.getCenter();
            float tempAngle = _dozer.getAngle();
            int nextId;
            for (int i = RSCounts; i > 0; i --)
            {
                if (carried == 3)
                {
                    break;
                }
                if (isSalvage(i)) continue;
                // move down
                x1 = tempPos.X - (float)((4 - 2 * carried) * Math.Cos(tempAngle * Math.PI / 180));
                y1 = tempPos.Y - (float)((4 - 2 * carried) * Math.Sin(tempAngle * Math.PI / 180));
                _cmd = new Command(curTime, curTime + step, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, true, i);
                _commands.Enqueue(_cmd);
                tempPos = new PointF(x1, y1);
                curTime += step;

                // rotate 90 degree
                _cmd = new Command(curTime, curTime + step, 0, 1, tempPos, tempPos, tempAngle, tempAngle + 90, true, i);
                _commands.Enqueue(_cmd);
                tempAngle += 90;
                curTime += step;

                // move to target position
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * (1 + carried * 2) / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * (1 + carried * 2) / c);
                x1 = _basePoint.X + dx;
                y1 = _basePoint.Y - dy;
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * 6 / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * 6 / c);
                x1 = x1 - dx;
                y1 = y1 - dy;
                _cmd = new Command(curTime, curTime + step, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, true, i);
                _commands.Enqueue(_cmd);
                tempPos = new PointF(x1, y1);
                curTime += step;

                // move to next position
                nextId = getNextMoveId(i);
                if (carried == 2 && _unavailable.Count > 0)
                {
                    nextId = _unavailable[0];
                }
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * (1 + 2 * carried) / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * (1 + 2 * carried) / c);
                x1 = _basePoint.X + dx;
                y1 = _basePoint.Y - dy;
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * (1.7 + (RSCounts - nextId) * 2) / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * (1.7 + (RSCounts - nextId) * 2) / c);
                x1 = x1 - dx;
                y1 = y1 - dy;
                _cmd = new Command(curTime, curTime + step, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, false, 0);
                _commands.Enqueue(_cmd);
                tempPos = new PointF(x1, y1);
                curTime += step;

                // rotate 90 degree
                _cmd = new Command(curTime, curTime + step, 0, -1, tempPos, tempPos, tempAngle, tempAngle - 90, false, 0);
                _commands.Enqueue(_cmd);
                tempAngle -= 90;
                curTime += step;

                // move up
                x1 = tempPos.X + (float)((4 - 2 * carried) * Math.Cos(tempAngle * Math.PI / 180));
                y1 = tempPos.Y + (float)((4 - 2 * carried) * Math.Sin(tempAngle * Math.PI / 180));
                if (carried == 2)
                {
                    _cmd = new Command(curTime, EFrameBuiltTime * 3600, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, false, 0);
                    _commands.Enqueue(_cmd);
                    tempPos = new PointF(x1, y1);
                    curTime  = EFrameBuiltTime * 3600;
                } else
                {
                    _cmd = new Command(curTime, curTime + step, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, false, 0);
                    _commands.Enqueue(_cmd);
                    tempPos = new PointF(x1, y1);
                    curTime += step;
                }

                carried++;
            }

            for (int i = 0; i < _unavailable.Count; i++)
            {
                // move down
                x1 = tempPos.X - (float)(3 * Math.Cos(tempAngle * Math.PI / 180));
                y1 = tempPos.Y - (float)(3 * Math.Sin(tempAngle * Math.PI / 180));
                _cmd = new Command(curTime, curTime + 1800, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, true, _unavailable[i]);
                _commands.Enqueue(_cmd);
                tempPos = new PointF(x1, y1);
                curTime += 1800;

                // rotate 90 degree
                _cmd = new Command(curTime, curTime + 1800, 0, 1, tempPos, tempPos, tempAngle, tempAngle + 90, true, _unavailable[i]);
                _commands.Enqueue(_cmd);
                tempAngle += 90;
                curTime += 1800;

                // move to LCC
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * 3 / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * 3 / c);
                x1 = initRSPos.StartPos.X + dx;
                y1 = initRSPos.StartPos.Y - dy;
                float d = HaDesMath.CalcDistancePoints(new PointF(x1, y1), tempPos);
                step = (int)d * 180;
                _cmd = new Command(curTime, curTime + step, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, true, _unavailable[i]);
                _commands.Enqueue(_cmd);
                tempPos = new PointF(x1, y1);
                curTime += step;

                // move to next position
                if (i + 1 == _unavailable.Count)
                {
                    nextId = _unavailable[i];
                } else
                {
                    nextId = _unavailable[i + 1];
                }
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * 3 / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * 3 / c);
                x1 = _basePoint.X + dx;
                y1 = _basePoint.Y - dy;
                dx = (float)((initRSPos.EndPos.X - initRSPos.StartPos.X) * (1.7 + (RSCounts - nextId) * 2) / c);
                dy = (float)((initRSPos.EndPos.Y - initRSPos.StartPos.Y) * (1.7 + (RSCounts - nextId) * 2) / c);
                x1 = x1 - dx;
                y1 = y1 - dy;
                d = HaDesMath.CalcDistancePoints(new PointF(x1, y1), tempPos);
                step = (int)d * 180;
                _cmd = new Command(curTime, curTime + step, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, false, 0);
                _commands.Enqueue(_cmd);
                tempPos = new PointF(x1, y1);
                curTime += step;

                // rotate 90 degree
                _cmd = new Command(curTime, curTime + 1800, 0, -1, tempPos, tempPos, tempAngle, tempAngle - 90, false, 0);
                _commands.Enqueue(_cmd);
                tempAngle -= 90;
                curTime += 1800;

                // move up
                x1 = tempPos.X + (float)(3 * Math.Cos(tempAngle * Math.PI / 180));
                y1 = tempPos.Y + (float)(3 * Math.Sin(tempAngle * Math.PI / 180));
                _cmd = new Command(curTime, curTime + 1800, 1, 0, tempPos, new PointF(x1, y1), tempAngle, tempAngle, false, 0);
                _commands.Enqueue(_cmd);
                tempPos = new PointF(x1, y1);
                curTime += 1800;

            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_commands.Count == 0)
            {
                timer1.Enabled = false;
                StopSimulationBtn.Enabled = false;
                StartSimulationBtn.Enabled = true;
                return;
            }
            _elapsed += SimulationSpeed * timer1.Interval / 1000;
            _curTime = _curTime + TimeSpan.FromSeconds(SimulationSpeed * timer1.Interval / 1000);
            SimulationTimeSB.Text = _curTime.ToString();
            ElapsedTimeSB.Text = HaDesMath.ConvertSecToTimeStr(_elapsed);
            processSimulation();
            DrawArea.Invalidate();
        }
        ///Mouse Event process Start
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
                    //eFramePoint = HaDesGraphics.GetDxfPosF(e.Location, initialRect, offsetPos, zoomFactor);
                    eFramePoint = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    EFramePosBtn.Checked = false;
                    _elapsed = 0;
                    _curTime = SimulationTime;
                    InitializeRSandDozer();
                    DrawArea.Invalidate();
                }
                else
                {
                    moveStartPoint = e.Location;
                }
            }
        }
        private void DrawArea_MouseUp(object sender, MouseEventArgs e)
        {
            moveStartPoint = e.Location;
            if (moveStartPoint != e.Location)
            {
                Point currentPoint = e.Location;
                // Calculate the delta (difference) from the starting point
                int deltaX = currentPoint.X - moveStartPoint.X;
                int deltaY = currentPoint.Y - moveStartPoint.Y;

                //HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(deltaX, deltaY), zoomFactor.Value);

                HaDesDxf.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(deltaX, deltaY), zoomFactor.Value);
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
                int deltaX = currentPoint.X - moveStartPoint.X;
                int deltaY = currentPoint.Y - moveStartPoint.Y;

                //HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(deltaX, deltaY), zoomFactor.Value);
                HaDesDxf.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(deltaX, deltaY), zoomFactor.Value);
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
                //HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(delta, 0), zoomFactor.Value);
                HaDesDxf.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(delta, 0), zoomFactor.Value);
                DrawArea.Invalidate();
                return;
            }
            // Adjust vertical scroll
            //HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(0, delta), zoomFactor.Value);
            HaDesDxf.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(0, delta), zoomFactor.Value);
            DrawArea.Invalidate();
            return;
        }
        ///Mouse Event Process End
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
        private void onMoveLeftSMenu(object sender, EventArgs e)
        {
            HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(10, 0), zoomFactor.Value);
            MousePositionSB.Text = $"Offset:({offsetPos.X}, {offsetPos.Y}), " +
                $"AreaSize:({DrawAreaSize.Width},{DrawAreaSize.Height})";
            DrawArea.Invalidate();
        }
        private void onMoveRightsMenu(object sender, EventArgs e)
        {
            HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(-10, 0), zoomFactor.Value);
            MousePositionSB.Text = $"Offset:({offsetPos.X}, {offsetPos.Y}), " +
                $"AreaSize:({DrawAreaSize.Width},{DrawAreaSize.Height})";
            DrawArea.Invalidate();
        }
        private void onMoveUpSMenu(object sender, EventArgs e)
        {
            HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(0, 10), zoomFactor.Value);
            MousePositionSB.Text = $"Offset:({offsetPos.X}, {offsetPos.Y}), " +
                $"AreaSize:({DrawAreaSize.Width},{DrawAreaSize.Height})";
            DrawArea.Invalidate();
        }
        private void onMoveDownSMenu(object sender, EventArgs e)
        {
            HaDesGraphics.UpdateOffsetPos(ref offsetPos, DrawArea, DrawAreaSize, new Point(0, -10), zoomFactor.Value);
            MousePositionSB.Text = $"Offset:({offsetPos.X}, {offsetPos.Y}), " +
                $"AreaSize:({DrawAreaSize.Width},{DrawAreaSize.Height})";
            DrawArea.Invalidate();
        }
        private void onSimulationProperiesSMenu(object sender, EventArgs e)
        {
            SettingsForm sfrm = new SettingsForm();

            sfrm.EFrameBuiltTime = EFrameBuiltTime;
            sfrm.SimulationSpeed = SimulationSpeed;
            sfrm.RSCounts = RSCounts;
            sfrm.SimulationTime = SimulationTime;


            if (sfrm.ShowDialog() == DialogResult.OK)
            {
                EFrameBuiltTime = sfrm.EFrameBuiltTime;
                SimulationSpeed = sfrm.SimulationSpeed;
                RSCounts = sfrm.RSCounts;
                SimulationTime = sfrm.SimulationTime;

                if (RSCounts > 0 && RSCounts < 150)
                {
                    _available.Clear();
                    _unavailable.Clear();
                    for (int i = 1; i <= RSCounts; i ++)
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
            svfrm.RSCounts = RSCounts;
            svfrm._available = _available;
            svfrm._unavailable = _unavailable;
            if (svfrm.ShowDialog() == DialogResult.OK)
            {
                _available = svfrm._available;
                _unavailable = svfrm._unavailable;
                RSCounts = svfrm.RSCounts;
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
            if (SimulationSpeed <= 0) return;
            _started = true;
            _elapsed = 0;
            _curTime = SimulationTime;
            StartSimulationBtn.Enabled = false;
            StopSimulationBtn.Enabled = true;
            InitializeRSandDozer();
            makeCommandList();
            timer1.Interval = 100;
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

    public class Command
    {
        public int _stTime; // command start time.
        public int _enTime; // command end time.
        public int _type;   // command type 0: rotate, 1: move.
        public int _rotDir; // rotation direction 1: clockwise, -1: anti-clockwise
        public PointF _stPos;  // start position when move
        public PointF _enPos;  // end position when move
        public float _stAngle; // start angle when rotate
        public float _enAngle; // end angle when rotate
        public bool _loadedRS;         // rs loaded
        public int _rsNum;

        public Command()
        {
            _stTime = 0;
            _enTime = 0;
            _type = 0;
            _rotDir = -1;
            _stPos = PointF.Empty;
            _enPos = PointF.Empty;
            _stAngle = 0;
            _enAngle = 0;
            _loadedRS = false;
            _rsNum = 0;
        }
        public Command(int _stt, int _ent, int _tp, int _rd, PointF _stp, PointF _enp, float _sta, float _ena, bool _lrs, int _rsn)
        {
            _stTime = _stt;
            _enTime = _ent;
            _type = _tp;
            _rotDir = _rd;
            _stPos = _stp;
            _enPos = _enp;
            _stAngle = _sta;
            _enAngle = _ena;
            _loadedRS = _lrs;
            _rsNum = _rsn;
        }
    }
}
