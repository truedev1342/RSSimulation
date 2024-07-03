using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using netDxf;
using Point = System.Drawing.Point;

namespace RSSimulation
{
    public partial class MainForm : Form
    {
        private string filePath = "2013.dxf";
        private DxfDocument _dxfDocument = null;
        private HaDesDxf _haDesDxf = null;
        SizeF DrawAreaSize = new SizeF();

        private BoundingBox initialRect = new BoundingBox();
        private ZoomFactor zoomFactor = new ZoomFactor(1.0f, 0.1f, 0.1f, 100.0f);
        private PointF offsetPos = new PointF();
        private Point moveStartPoint = new Point();

        private DateTime _curTime = DateTime.Now;
        private DateTime SimulationTime = DateTime.Now;
        private int SimulationSpeed = 3600; 
        private int RSCounts = 149;
        private int EFrameBuiltTime = 48;   //unit : hour
        private int SalvageTime = 60;       //unit : min
        private int DozerSpeed = 1;        //unit : meter/min
        private int LCCSpeed = 2;          //unit : meter/min
        private int LCCLoadingTime = 60;    //unit : min
        private int LCCDropTime = 60;       //unit : min
        private int LCCCounts = 3;


        private int _elapsed = 0;
        private int _total = 0;
        private bool _started = false;

        private LinePos initRSPos = new LinePos();
        private PointF eFramePoint = PointF.Empty;
        private PointF _basePoint = PointF.Empty;

        private RoofSupport[] _rsData = new RoofSupport[202];
        private Dozer _dozer = new Dozer();
        private Carrier[] _lcc = new Carrier[3];

        private int setPosBtnId = -1;
        
        private List<int> _available = new List<int>();
        private List<int> _unavailable = new List<int>();


        private PointF locAPosSt = PointF.Empty;
        private PointF locAPosEn = PointF.Empty;

        private PointF locBPosSt = PointF.Empty;
        private PointF locBPosEn = PointF.Empty;

        private PointF locShuntPos = PointF.Empty;

        private List<PointF> routeAPoints = new List<PointF>();
        private List<PointF> routeBPoints = new List<PointF>();
        private List<PointF> routeDozerPoints = new List<PointF>();
        private PointF routeAcurPos = PointF.Empty;
        private PointF routeBcurPos = PointF.Empty;
        private PointF routeDozercurPos = PointF.Empty;
        private PointF lastRSPosInA = PointF.Empty;
        private PointF lastRSPosInB = PointF.Empty;

        private int movedSalvageRS = 0;
        private int waitingLCCId = -1;

        private Queue<Command> _cmdDozer = new Queue<Command>();
        private Queue<Command>[] _cmdLCC = new Queue<Command>[3];
        private float shuntDistance = 50.0f;
        private List<int> rsInLocA = new List<int>();
        private List<int> rsInLocB = new List<int>();

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
        private void initForms()
        {
            _dxfDocument = null;
            _haDesDxf = null;
            initialRect = new BoundingBox();
            zoomFactor = new ZoomFactor(1.0f, 0.1f, 0.1f, 100.0f);
            offsetPos = new PointF();
            DrawAreaSize = new SizeF();
            _unavailable = new List<int>();
            _available = new List<int>();
            for (int i = 1; i <= RSCounts; i++)
            {
                _available.Add(i);
            }
            SimulationTimeSB.Text = SimulationTime.ToString();
            ElapsedTimeSB.Text = "00:00:00";
            enableSimulation(false);
            for (int i = 0; i < 202; i++)
            {
                _rsData[i] = new RoofSupport();
            }
            for (int i = 0; i < _lcc.Length; i ++)
            {
                _lcc[i] = new Carrier();
                _cmdLCC[i] = new Queue<Command>();
                _cmdLCC[i].Clear();
            }
            DrawArea.Invalidate();
        }
        private void LoadDXFFile(string strPath)
        {
            try
            {
                _dxfDocument = DxfDocument.Load(strPath);
                _haDesDxf = new HaDesDxf();
                initialRect = _haDesDxf.ReadDxf(_dxfDocument);
                if( _haDesDxf.getPrimaryLinePos(ref initRSPos))
                    enableSimulation(true);
                //initialRect = HaDesMath.GetBoundingBox(_dxfDocument, ref initRSPos);
                DrawAreaSize = HaDesMath.UpdateDrawAreaSize(initialRect, zoomFactor);
                DrawArea.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to render DXF file: {ex.Message}");
            }
        }        
        
        private void InitializeLCCs()
        {
            PointF curPos = PointF.Empty;
            float tempAng = -90;
            if (routeDozerPoints.Count > 0)
            {
                curPos = routeDozerPoints[routeDozerPoints.Count - 1];
            }
            for (int i = 0; i < LCCCounts; i ++)
            {
                curPos.Y += 5;
                _lcc[i].setCenter(curPos);
                _lcc[i].setAngle(tempAng);
                _lcc[i].setTelemeter(-1);
                _cmdLCC[i].Clear();
            }
            waitingLCCId = 0;
            rsInLocA.Clear();
            rsInLocB.Clear();
            lastRSPosInA = locAPosSt;
            lastRSPosInB = locBPosSt;
        }

        private void InitializeRSandDozer()
        {
            if (eFramePoint == PointF.Empty)
            {
                return;
            }
            _basePoint = HaDesMath.CalcBasePoint(initRSPos, eFramePoint);
            float alpha = HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos);

            int idx = RSCounts - getNextMoveId(RSCounts + 1);

            PointF pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 2.5f);
            pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 1.7f + 2.0f * idx);
            _dozer.setAngle(alpha - 90);
            _dozer.setCenter(pos);

            for (int i = 0; i < RSCounts; i++)
            {
                pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 8.5f);
                pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 2.0f + 2.0f * i);

                _rsData[RSCounts - i].setNum(RSCounts - i);
                _rsData[RSCounts - i].setCenter(pos);
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


            g.DrawEllipse(new Pen(Color.DarkBlue), new RectangleF(new PointF(locShuntPos.X - 10, locShuntPos.Y - 10), new Size(20, 20)));
            g.FillEllipse(new SolidBrush(Color.Red), new RectangleF(new PointF(locShuntPos.X - 10, locShuntPos.Y - 10), new Size(20, 20)));

            _haDesDxf.DrawDxf(g);

            g.DrawLine(new Pen(Color.Red, 0.1f), locAPosSt, locAPosEn);
            g.DrawLine(new Pen(Color.Blue, 0.1f), locBPosSt, locBPosEn);

            // draw dozer route
            for (int i = 0; i < routeDozerPoints.Count; i ++)
            {
                if (i <  routeDozerPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Yellow, 0.1f), routeDozerPoints[i], routeDozerPoints[i + 1]);
                } else
                {
                    g.DrawLine(new Pen(Color.Yellow, 0.1f), routeDozerPoints[i], routeDozercurPos);
                }
            }

            // draw route A
            for (int i = 0; i < routeAPoints.Count; i++)
            {
                if (i < routeAPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), routeAPoints[i], routeAPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), routeAPoints[i], routeAcurPos);
                }
            }

            // draw route B
            for (int i = 0; i < routeBPoints.Count; i++)
            {
                if (i < routeBPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), routeBPoints[i], routeBPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), routeBPoints[i], routeBcurPos);
                }
            }

            // draw LCC
            for (int i = 0; i < LCCCounts; i++)
            {
                _lcc[i].draw(g);
            }

            _dozer.draw(g);  // draw dozer

            // draw RS
            for (int i = 1; i <= RSCounts; i++)
            {
                _rsData[i].draw(g);
            }
        }
        private void processDozerSimulation()
        {
            while (_cmdDozer.Count > 0 && _cmdDozer.Peek().enTime <= _elapsed)
            {
                _cmdDozer.Dequeue();
            }
            if (_cmdDozer.Count == 0 || _cmdDozer.Peek().stTime > _elapsed)
            {
                return;
            }
            Command _cmd = _cmdDozer.Peek();
            if (_cmd.type == 1) // waiting to load LCC
            {
                if (waitingLCCId != -1 && _cmdLCC[waitingLCCId].Count == 0)
                {
                    _cmdDozer.Dequeue();
                    loadRSToLCC(_cmd.loadedRS, _elapsed);
                    movedSalvageRS++;
                    salvageNextRS(_dozer.getCenter(), _dozer.getAngle(), _elapsed);
                }
            }
            int dt = SimulationSpeed * timer1.Interval / 1000;
            if (_elapsed + dt >= _cmd.enTime) {
                if (_cmd.direction == 0) // moving...
                {
                    _dozer.setCenter(_cmd.enPos);
                }
                else // rotating...
                {
                    _dozer.setAngle(_cmd.enAngle);
                }
            } else
            {
                int cmdLeft = (_cmd.enTime - _elapsed) / dt;
                if (_cmd.direction == 0) // moving...
                {
                    float dx = (_cmd.enPos.X - _dozer.getCenter().X) / cmdLeft;
                    float dy = (_cmd.enPos.Y - _dozer.getCenter().Y) / cmdLeft;
                    _dozer.setAngle(_cmd.enAngle);
                    _dozer.move(dx, dy);
                }
                else // rotating...
                {
                    float _ang = Math.Abs(_cmd.enAngle - _dozer.getAngle());
                    _dozer.rotate((float)_cmd.direction * _ang / cmdLeft);
                    _dozer.setCenter(_cmd.enPos);
                }
            }
            if (_cmd.loadedRS != -1)
            {
                int id = _cmd.loadedRS;
                _rsData[id].setCenter(HaDesMath.movePointByDisAndAngle(_dozer.getCenter(), _dozer.getAngle(), 3.5f));
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

        private void makeEFrameCommand()
        {
            int curTime = 0;
            int step = EFrameBuiltTime * 3600 / 18;
            Command _cmd;
            int carried = 0;
            float c = HaDesMath.CalcDistancePoints(initRSPos.StartPos, initRSPos.EndPos);
            float dx, dy, x1, y1;
            PointF curPos = _dozer.getCenter(), pos;
            float curAngle = _dozer.getAngle();
            float alpha = HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos);
            int nextId;
            for (int i = RSCounts; i > 0; i --)
            {
                if (carried == 3)
                {
                    break;
                }
                if (isSalvage(i)) continue;

                if (carried == 0) // in first RS moving move up to RS
                {
                    // move up
                    pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.5f);
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    _cmdDozer.Enqueue(_cmd);
                    curPos = pos;
                    curTime += step;
                }

                // move down
                pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.0f * carried - 4.0f);
                _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, i);
                _cmdDozer.Enqueue(_cmd);
                curPos = pos;
                curTime += step;

                // rotate 90 degree
                _cmd = new Command(curTime, curTime + step, curPos, curPos, curAngle, curAngle + 90, 1, i);
                _cmdDozer.Enqueue(_cmd);
                curAngle += 90;
                curTime += step;

                // move to target position
                pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 1.0f + carried * 2.0f);
                pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 6.0f);
                _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, i);
                _cmdDozer.Enqueue(_cmd);
                curPos = pos;
                curTime += step;

                if (carried == 2)
                {
                    // move to complete eframe position
                    pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 5.0f);
                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 10.0f);
                    _cmd = new Command(curTime, EFrameBuiltTime * 3600, curPos, pos, curAngle, curAngle, 0, -1);
                    _cmdDozer.Enqueue(_cmd);
                    curPos = pos;
                    curTime = EFrameBuiltTime * 3600;
                    
                } else
                {
                    // move to next position
                    nextId = getNextMoveId(i);
                    pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 1.0f + carried * 2.0f);
                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 1.7f + 2.0f * (RSCounts - nextId));
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    _cmdDozer.Enqueue(_cmd);
                    curPos = pos;
                    curTime += step;

                    // rotate 90 degree
                    _cmd = new Command(curTime, curTime + step, curPos, curPos, curAngle, curAngle - 90, -1, -1);
                    _cmdDozer.Enqueue(_cmd);
                    curAngle -= 90;
                    curTime += step;

                    // move up
                    pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 4.0f - 2.0f * carried);
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    _cmdDozer.Enqueue(_cmd);
                    curPos = pos;
                    curTime += step;
                }
                carried++;
            }
            makeFirstSalvageCommand(curPos, curAngle, curTime);
        }

        private void makeFirstSalvageCommand(PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime;
            if (_unavailable.Count == 0) return;
            int idx = _unavailable[0];
            Command _cmd;
            PointF curPos = lastPos, pos;
            float curAngle = lastAng, d, alpha = HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos);

            pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 5.0f);
            pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 1.7f + (RSCounts - idx) * 2.0f);

            d = HaDesMath.CalcDistancePoints(pos, curPos);
            int step = (int)(d * 60.0f / DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
            _cmdDozer.Enqueue(_cmd);
            curPos = pos;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + 1200, curPos, curPos, curAngle, curAngle - 90, -1, -1);
            _cmdDozer.Enqueue(_cmd);
            curAngle -= 90;
            curTime += 1200;

            // move down

            pos = HaDesMath.movePointByDisAndAngle(curPos, alpha - 90, -2);
            _cmd = new Command(curTime, curTime + 1200, curPos, pos, curAngle, curAngle, 0, idx);
            _cmdDozer.Enqueue(_cmd);
            curPos = pos;
            curTime += 1200;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + 1200, curPos, curPos, curAngle, curAngle + 90, 1, idx);
            _cmdDozer.Enqueue(_cmd);
            curAngle += 90;
            curTime += 1200;

            gotoLCCStartPos(curPos, curAngle, curTime, idx);
        }

        private void gotoLCCStartPos(PointF lastPos, float lastAng, int lastTime, int idx)
        {
            if (routeDozerPoints.Count < 2)
            {
                return;
            }
            int curTime = lastTime;
            PointF curPos = lastPos;
            float curAngle = lastAng;
            Command _cmd;
            float d = HaDesMath.CalcDistancePoints(curPos, routeDozerPoints[1]);
            float alpha = HaDesMath.getAngleByPoints(routeDozerPoints[1], curPos);

            int step = (int)(d * 60.0f / DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, routeDozerPoints[1], alpha, alpha, 0, idx);
            _cmdDozer.Enqueue(_cmd);
            curPos = routeDozerPoints[1];
            curTime += step;

            for (int i = 2; i < routeDozerPoints.Count; i ++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeDozerPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeDozerPoints[i], curPos);
                step = (int)(d * 60.0f / DozerSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeDozerPoints[i], alpha, alpha, 0, idx);
                _cmdDozer.Enqueue(_cmd);
                curPos = routeDozerPoints[i];
                curTime += step;
            }
            _cmd = new Command(curTime, int.MaxValue, curPos, curPos, alpha, alpha, 0, idx, 1); // insert waiting command
            _cmdDozer.Enqueue(_cmd);
        }

        private void salvageNextRS(PointF lastPos, float lastAng, int lastTime)
        {
            if (movedSalvageRS == _unavailable.Count)
            {
                return;
            }

            // move to next RS
            int idx = _unavailable[movedSalvageRS];

            int curTime = lastTime;
            PointF curPos = lastPos, pos;
            float curAngle = lastAng;
            Command _cmd;
            float d, alpha, dx, dy, x1, y1;
            int step;
            for (int i = routeDozerPoints.Count - 2; i > 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeDozerPoints[i]);
                alpha = HaDesMath.getAngleByPoints(curPos, routeDozerPoints[i]);

                step = (int)(d * 60.0f / DozerSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeDozerPoints[i], alpha, alpha, 0, -1);
                _cmdDozer.Enqueue(_cmd);
                curPos = routeDozerPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            pos = HaDesMath.movePointByDisAndAngle(_basePoint, HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos) - 90, 3.0f);
            pos = HaDesMath.movePointByDisAndAngle(pos, HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos) - 180, 1.7f + (RSCounts - idx) * 2.0f);

            d = HaDesMath.CalcDistancePoints(pos, curPos);
            alpha = HaDesMath.getAngleByPoints(curPos, pos);
            step = (int)(d * 60.0f / DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
            _cmdDozer.Enqueue(_cmd);
            curPos = pos;
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + 900, curPos, curPos, curAngle, curAngle - 90, -1, -1);
            _cmdDozer.Enqueue(_cmd);
            curAngle -= 90;
            curTime += 900;

            // move up
            pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.0f);
            _cmd = new Command(curTime, curTime + 900, curPos, pos, curAngle, curAngle, 0, -1);
            _cmdDozer.Enqueue(_cmd);
            curPos = pos;
            curTime += 900;

            // move down
            pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, -2.0f);
            _cmd = new Command(curTime, curTime + 900, curPos, pos, curAngle, curAngle, 0, idx);
            _cmdDozer.Enqueue(_cmd);
            curPos = pos;
            curTime += 900;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + 900, curPos, curPos, curAngle, curAngle + 90, 1, idx);
            _cmdDozer.Enqueue(_cmd);
            curAngle += 90;
            curTime += 900;

            gotoLCCStartPos(curPos, curAngle, curTime, idx);
        }
        private void processLCCSimulation()
        {
            for (int i = 0; i < LCCCounts; i ++)
            {
                while (_cmdLCC[i].Count > 0 && _cmdLCC[i].Peek().enTime <= _elapsed)
                {
                    _cmdLCC[i].Dequeue();
                }
                if (_cmdLCC[i].Count == 0 || _cmdLCC[i].Peek().stTime > _elapsed)
                {
                    continue;
                }
                Command _cmd = _cmdLCC[i].Peek();
                if (_cmd.type == 1) // waiting to check shunt(before carry)
                {
                    if (checkShunt((i + LCCCounts - 1) % LCCCounts))
                    {
                        _cmdLCC[i].Dequeue();
                        if ((int)(HaDesMath.CalcDistancePoints(locAPosSt, locAPosEn) / 2.0f) > rsInLocA.Count)
                        {
                            carryRSToLocA(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _cmd.loadedRS);
                        } else
                        {
                            carryRSToLocB(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _cmd.loadedRS);
                        }
                        waitingLCCId = (waitingLCCId + 1) % LCCCounts;
                    }
                    continue;
                }
                else if (_cmd.type == 2) // wait to load new RS (after carry)
                {
                    _cmdLCC[i].Dequeue();
                    gotoWaitingRoom(i);
                    continue;
                }
                int dt = SimulationSpeed * timer1.Interval / 1000;
                if (_elapsed + dt >= _cmd.enTime)
                {
                    if (_lcc[i].getTelemeter() != -1)
                    {
                        _lcc[i].increaseTelemeter(HaDesMath.CalcDistancePoints(_lcc[i].getCenter(), _cmd.enPos));
                    }
                    _lcc[i].setAngle(_cmd.enAngle);
                    _lcc[i].setCenter(_cmd.enPos);
                }
                else
                {
                    int cmdLeft = (_cmd.enTime - _elapsed) / dt;
                    if (_cmd.direction == 0) // moving...
                    {
                        float dx = (_cmd.enPos.X - _lcc[i].getCenter().X) / cmdLeft;
                        float dy = (_cmd.enPos.Y - _lcc[i].getCenter().Y) / cmdLeft;
                        _lcc[i].setAngle(_cmd.enAngle);
                        _lcc[i].move(dx, dy);
                    }
                    else // rotating...
                    {
                        float _ang = Math.Abs(_cmd.enAngle - _lcc[i].getAngle());
                        _lcc[i].rotate((float)_cmd.direction * _ang / cmdLeft);
                        _lcc[i].setCenter(_cmd.enPos);
                    }
                }
                if (_cmd.loadedRS != -1)
                {
                    int id = _cmd.loadedRS;
                    float x1 = _lcc[i].getCenter().X + (float)(2.5 * Math.Cos(_lcc[i].getAngle() * Math.PI / 180));
                    float y1 = _lcc[i].getCenter().Y + (float)(2.5 * Math.Sin(_lcc[i].getAngle() * Math.PI / 180));
                    _rsData[id].setCenter(new PointF(x1, y1));
                    _rsData[id].setAngle(_lcc[i].getAngle());
                }
            }
        }

        private void carryRSToLocA(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime;
            PointF curPos = lastPos, pos;
            float curAngle = lastAng, alpha;
            Command _cmd;
            float d, vect = HaDesMath.vectorMulti(locAPosSt, locAPosEn, routeAPoints[routeAPoints.Count - 1]);
            float beta = HaDesMath.getAngleByPoints(locAPosSt, locAPosEn);
            int step;
            if (vect < 0)
            {
                beta += 90;
            }
            else
            {
                beta -= 90;
            }

            // move in route A: carry
            for (int i = 1; i < routeAPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);

                step = (int)(d * 60.0f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, idRS);
                _cmdLCC[id].Enqueue(_cmd);
                curPos = routeAPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            // go to location A load pos
            pos = HaDesMath.movePointByDisAndAngle(lastRSPosInA, beta, -5.0f);
            d = HaDesMath.CalcDistancePoints(curPos, pos);
            alpha = HaDesMath.getAngleByPoints(pos, curPos);

            step = (int)(d * 60.0f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, idRS);
            _cmdLCC[id].Enqueue(_cmd);
            curPos = pos;
            curAngle = alpha;
            curTime += step;

            // drop RS
            if (beta < curAngle)
            {
                _cmd = new Command(curTime, curTime + LCCDropTime * 20, curPos, curPos, curAngle, beta, -1, idRS);
            } else
            {
                _cmd = new Command(curTime, curTime + LCCDropTime * 20, curPos, curPos, curAngle, beta, 1, idRS);
            }
            _cmdLCC[id].Enqueue(_cmd);
            curAngle = beta;
            curTime += LCCDropTime * 20;

            // move +5m to drop
            pos = HaDesMath.movePointByDisAndAngle(curPos, beta, 5.0f);
            _cmd = new Command(curTime, curTime + LCCDropTime * 20, curPos, pos, curAngle, curAngle, 0, idRS);
            _cmdLCC[id].Enqueue(_cmd);
            curPos = pos;
            curTime += LCCDropTime * 20;

            // move back -5m to back
            pos = HaDesMath.movePointByDisAndAngle(curPos, beta, -5.0f);
            _cmd = new Command(curTime, curTime + LCCDropTime * 20, curPos, pos, curAngle, curAngle, 0, -1);
            _cmdLCC[id].Enqueue(_cmd);
            curPos = pos;
            curTime += LCCDropTime * 20;


            // move in route A: back
            for (int i = routeAPoints.Count - 1; i >= 0 ; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);

                step = (int)(d * 60.0f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, -1);
                _cmdLCC[id].Enqueue(_cmd);
                curPos = routeAPoints[i];
                curAngle = alpha;
                curTime += step;
            }


            _cmd = new Command(curTime, int.MaxValue, curPos, curPos, curAngle, curAngle, 0, -1, 2); // insert waiting command
            _cmdLCC[id].Enqueue(_cmd);
            rsInLocA.Add(idRS);

            d = HaDesMath.CalcDistancePoints(locAPosSt, locAPosEn);
            lastRSPosInA.X += (locAPosEn.X - locAPosSt.X) * 2 / d;
            lastRSPosInA.Y += (locAPosEn.Y - locAPosSt.Y) * 2 / d;
        }


        private void carryRSToLocB(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            ////////////TODO after done carry A
        }

        private void gotoWaitingRoom(int id)
        {
            int prev = (id + LCCCounts - 1) % LCCCounts;
            PointF curPos = routeDozerPoints[routeDozerPoints.Count - 1];
            if (_lcc[prev].getTelemeter() == -1)
            {
                curPos = _lcc[prev].getCenter();
            }
            curPos.Y += 5;
            _lcc[id].setCenter(curPos);
            _lcc[id].setAngle(-90);
            _lcc[id].setTelemeter(-1);
        }

        private bool checkShunt(int id)
        {
            if (_lcc[id].getTelemeter() == -1 || _lcc[id].getTelemeter() >= shuntDistance) return true;
            return false;
        }

        private void loadRSToLCC(int idRS, int lastTime)
        {
            Command _cmd;
            int idx;
            int step = (int)(5.0f * 60.0f / DozerSpeed);
            for (int i = 0; i < LCCCounts; i ++)
            {
                idx = (waitingLCCId + i) % LCCCounts;
                if (_lcc[idx].getTelemeter() != -1)
                {
                    break;
                }
                _cmd = new Command(lastTime, lastTime + step, _lcc[idx].getCenter(), new PointF(_lcc[idx].getCenter().X, _lcc[idx].getCenter().Y - 5), _lcc[idx].getAngle(), _lcc[idx].getAngle(), 0, -1);
                _cmdLCC[idx].Enqueue(_cmd);
            }
            idx = waitingLCCId;

            float alpha = HaDesMath.getAngleByPoints(routeDozerPoints[routeDozerPoints.Count - 1], routeDozerPoints[routeDozerPoints.Count - 2]);
            float beta = HaDesMath.getAngleByPoints(routeAPoints[0], routeAPoints[1]) - 180;
            int dir;
            if (_lcc[idx].getAngle() < alpha) { dir = 1; }
            else { dir = -1; }
            PointF pos = routeDozerPoints[routeDozerPoints.Count-1];
            _cmd = new Command(lastTime + step, lastTime + step + LCCLoadingTime * 30, pos, pos, _lcc[idx].getAngle(), alpha, dir, -1);
            _cmdLCC[idx].Enqueue(_cmd);
            if (alpha < beta) { dir = 1; }
            else { dir = -1; }
            _cmd = new Command(lastTime + step + LCCLoadingTime * 30, lastTime + step + LCCLoadingTime * 60, pos, pos, alpha, beta, dir, idRS);
            _cmdLCC[idx].Enqueue(_cmd);

            _cmd = new Command(lastTime + step + LCCLoadingTime * 60, int.MaxValue, pos, pos, beta, beta, 0, idRS, 1);
            _cmdLCC[idx].Enqueue(_cmd);
            _lcc[idx].setTelemeter(0);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            _elapsed += SimulationSpeed * timer1.Interval / 1000;
            _curTime = _curTime + TimeSpan.FromSeconds(SimulationSpeed * timer1.Interval / 1000);
            SimulationTimeSB.Text = _curTime.ToString();
            ElapsedTimeSB.Text = HaDesMath.ConvertSecToTimeStr(_elapsed);
            processDozerSimulation();
            processLCCSimulation();
            DrawArea.Invalidate();
        }
        //
        private void enableSimulation(bool state = true)
        {
            SimulationPropertiesBtn.Enabled = state;
            SimulationPropertiesSMenu.Enabled = state;
            SalvageSequenceBtn.Enabled = state;
            SalvageSequenceSMenu.Enabled = state;
            SetLocationEFrameBtn.Enabled = state;
            SetLocationABtn.Enabled = state;
            SetLocationBBtn.Enabled = state;
            SetRouteABtn.Enabled = state;
            SetRouteBBtn.Enabled = state;
            SetRouteDozerBtn.Enabled = state;
            StartSimulationBtn.Enabled = state;
            StopSimulationBtn.Enabled = state;
            SetLocationShuntBtn.Enabled = state;
        }
        ///Mouse Event process Start
        private void DrawArea_MouseDown(object sender, MouseEventArgs e)
        {
            Point currentPoint = DrawPanel.PointToClient(DrawArea.PointToScreen(e.Location));
            if (e.Button == MouseButtons.Right)
            {
                moveStartPoint = currentPoint;
                DrawArea.Cursor = Cursors.SizeAll;

                if (SetRouteDozerBtn.Checked)
                {
                    SetRouteDozerBtn.Checked = false;
                    routeDozercurPos = routeDozerPoints[routeDozerPoints.Count - 1];
                    InitializeLCCs();
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                switch (setPosBtnId)
                {
                    case 0: // set e-frame point
                        eFramePoint = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        setUncheckRest(-1);
                        _elapsed = 0;
                        _curTime = SimulationTime;
                        InitializeRSandDozer();
                        DrawArea.Invalidate();
                        break;
                    case 1: // set location A
                        if (locAPosSt == PointF.Empty)
                        {
                            locAPosSt = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        }
                        else
                        {
                            locAPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                            DrawArea.Invalidate();
                            setUncheckRest(-1);
                        }
                        break;
                    case 2: // set location B
                        if (locBPosSt == PointF.Empty)
                        {
                            locBPosSt = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        }
                        else
                        {
                            locBPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                            DrawArea.Invalidate();
                            setUncheckRest(-1);
                        }
                        break;
                    case 3: // set route A
                        routeAPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                        DrawArea.Invalidate();
                        break;
                    case 4: // set route B
                        routeBPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                        DrawArea.Invalidate();
                        break;
                    case 5: // set dozer route
                        routeDozerPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                        DrawArea.Invalidate();
                        break;
                    case 6:
                        locShuntPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        shuntDistance = HaDesMath.calcShuntDistance(locShuntPos, routeAPoints);
                        setUncheckRest(-1);
                        DrawArea.Invalidate();
                        break;
                    default:
                        DrawArea.Cursor = Cursors.Cross;
                        moveStartPoint = e.Location;
                        break;
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
            if (setPosBtnId == -1)
            {
                DrawArea.Cursor = Cursors.Default;
            }
            else
            {
                DrawArea.Cursor = Cursors.Cross;
            }
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
            switch (setPosBtnId)
            {
                case 1: // set location A
                    if (locAPosSt != PointF.Empty)
                    {
                        locAPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        DrawArea.Invalidate();
                    }
                    break;
                case 2: // set location B
                    if (locBPosSt != PointF.Empty)
                    {
                        locBPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        DrawArea.Invalidate();
                    }
                    break;
                case 3: // set route A
                    routeAcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                case 4: // set route B
                    routeBcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                case 5: // set dozer route
                    routeDozercurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                default:
                    moveStartPoint = e.Location;
                    break;

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
            enableSimulation(false);
            initForms();
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
            System.Windows.Forms.Application.Exit();
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

            sfrm.SimulationTime = SimulationTime;
            sfrm.SimulationSpeed = SimulationSpeed;
            sfrm.RSCounts = RSCounts;
            sfrm.EFrameBuiltTime = EFrameBuiltTime;
            sfrm.SalvageTime = SalvageTime;
            sfrm.DozerSpeed = DozerSpeed;
            sfrm.LCCSpeed = LCCSpeed;
            sfrm.LCCLoadingTime = LCCLoadingTime;
            sfrm.LCCDropTime = LCCDropTime;
            sfrm.LCCCounts  = LCCCounts;

            if (sfrm.ShowDialog() == DialogResult.OK)
            {
                EFrameBuiltTime = sfrm.EFrameBuiltTime;
                SimulationSpeed = sfrm.SimulationSpeed;
                RSCounts = sfrm.RSCounts;
                SimulationTime = sfrm.SimulationTime;
                SalvageTime = sfrm.SalvageTime;
                DozerSpeed = sfrm.DozerSpeed;
                LCCSpeed = sfrm.LCCSpeed;
                LCCLoadingTime = sfrm.LCCLoadingTime;
                LCCDropTime = sfrm.LCCDropTime;
                LCCCounts = sfrm.LCCCounts;

                _lcc = new Carrier[LCCCounts];
                for (int i = 0; i < LCCCounts; i ++)
                {
                    _lcc[i] = new Carrier();
                    _cmdLCC[i] = new Queue<Command>();
                    _cmdLCC[i].Clear();
                }

                if (RSCounts > 0 && RSCounts < 150)
                {
                    _available.Clear();
                    _unavailable.Clear();
                    for (int i = 1; i <= RSCounts; i ++)
                    {
                        _available.Add(i);
                    }
                    SetLocationEFrameBtn.Enabled = true;
                    SetLocationEFrameBtn.Checked = false;
                }
                else
                {
                    SetLocationEFrameBtn.Enabled = false;
                    SetLocationEFrameBtn.Checked = false;
                }
                InitializeRSandDozer();
                DrawArea.Invalidate();
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
                InitializeRSandDozer();
                DrawArea.Invalidate();
            }
        }
        private void onViewHelpSMenu(object sender, EventArgs e)
        {

        }
        /// Menubar Event Process End
        /// Toolbar Event process Start
        private void onSetLocationABtn(object sender, EventArgs e)
        {
            if (SetLocationABtn.Checked)
            {
                setUncheckRest(1);
                locAPosSt = PointF.Empty;
                locAPosEn = PointF.Empty;
                DrawArea.Invalidate();
            } else
            {
                setUncheckRest(-1);
            }
        }
        private void onSetLocationBBtn(object sender, EventArgs e)
        {
            if (SetLocationBBtn.Checked)
            {
                setUncheckRest(2);
                locBPosSt = PointF.Empty;
                locBPosEn = PointF.Empty;
                DrawArea.Invalidate();
            } else
            {
                setUncheckRest(-1);
            }
        }

        private void setUncheckRest(int id)
        {
            SetLocationEFrameBtn.Checked = (id == 0);
            SetLocationABtn.Checked = (id == 1);
            SetLocationBBtn.Checked = (id == 2);
            SetRouteABtn.Checked = (id == 3);
            SetRouteBBtn.Checked = (id == 4);
            SetRouteDozerBtn.Checked = (id == 5);
            SetLocationShuntBtn.Checked = (id == 6);

            setPosBtnId = id;
            if (id == -1)
            {
                DrawArea.Cursor = Cursors.Default;
            } else
            {
                DrawArea.Cursor = Cursors.Cross;
            }
        }
        private void onSetLocationEFrameBtn(object sender, EventArgs e)
        {
            if (SetLocationEFrameBtn.Checked)
            {
                setUncheckRest(0);
            }
            else
            {
                setUncheckRest(-1);
            }
        }
        private void onSetRouteABtn(object sender, EventArgs e)
        {
            if (SetRouteABtn.Checked)
            {
                if (routeDozerPoints.Count == 0)
                {
                    MessageBox.Show("Please set Dozer route.");
                    setUncheckRest(-1);
                    return;
                }
                routeAPoints.Clear();
                routeAPoints.Add(routeDozerPoints[routeDozerPoints.Count - 1]);
                routeAcurPos = routeAPoints[0];
                DrawArea.Invalidate();
                setUncheckRest(3);
            } else
            {
                routeAcurPos = routeAPoints[routeAPoints.Count - 1];
                shuntDistance = HaDesMath.calcShuntDistance(locShuntPos, routeAPoints);
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }
        private void onSetRouteBBtn(object sender, EventArgs e)
        {
            if (SetRouteBBtn.Checked)
            {
                if (routeDozerPoints.Count == 0)
                {
                    MessageBox.Show("Please set Dozer route.");
                    setUncheckRest(-1);
                    return;
                }
                routeBPoints.Clear();
                routeBPoints.Add(routeDozerPoints[routeDozerPoints.Count - 1]);
                routeBcurPos = routeBPoints[0];
                DrawArea.Invalidate();
                setUncheckRest(4);
            } else
            {
                routeBcurPos = routeBPoints[routeBPoints.Count - 1];
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }
        private void onSetRouteDozerBtn(object sender, EventArgs e)
        {
            if (SetRouteDozerBtn.Checked)
            {
                routeDozerPoints.Clear();
                float alpha = HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos);
                PointF pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 3.0f);

                routeDozerPoints.Add(pos);

                pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 2.0f * RSCounts + 3.0f);

                routeDozerPoints.Add(pos);
                routeDozercurPos = pos;

                routeDozercurPos = PointF.Empty;
                DrawArea.Invalidate();
                setUncheckRest(5);
            } else
            {
                routeDozercurPos = routeDozerPoints[routeDozerPoints.Count - 1];
                InitializeLCCs();
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
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
            movedSalvageRS = 0;
            InitializeRSandDozer();
            InitializeLCCs();
            _cmdDozer.Clear();
            makeEFrameCommand();
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

        private void SetLocationShuntBtn_Click(object sender, EventArgs e)
        {
            if (SetLocationShuntBtn.Checked)
            {
                setUncheckRest(6);
            }
            else
            {
                setUncheckRest(-1);
            }
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
        public int stTime; // command start time.
        public int enTime; // command end time.
        public PointF stPos;  // start position when move
        public PointF enPos;  // end position when move
        public float stAngle; // start angle when rotate
        public float enAngle; // end angle when rotate
        public int direction; // move direction 1: rotate clockwise, 0: move forword, -1: rotate anti-clockwise
        public int loadedRS;  // roaded RS numb -1: not loaded
        public int type;   // command type:  used to determine next command

        public Command()
        {
            stTime = 0;
            enTime = 0;
            stPos = PointF.Empty;
            enPos = PointF.Empty;
            stAngle = 0;
            enAngle = 0;
            direction = -1;
            loadedRS = -1;
            type = 0;
        }
        public Command(int _stTime, int _enTime, PointF _stPos, PointF _enPos, float _stAngle, float _enAngle, int _direction = 0, int _loadedRS = -1, int _type = 0)
        {
            stTime = _stTime;
            enTime = _enTime;
            stPos = _stPos;
            enPos = _enPos;
            stAngle = _stAngle;
            enAngle = _enAngle;
            direction = _direction;
            loadedRS = _loadedRS;
            type = _type;
        }
    }
}
