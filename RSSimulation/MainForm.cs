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
        private float DozerSpeed = 0.12f;        //unit : km/h
        private float LCCSpeed = 0.06f;          //unit : km/h
        private int LCCLoadingTime = 60;    //unit : min
        private int LCCDropTime = 60;       //unit : min
        private int LCCCounts = 3;
        private int LCCWaitingTime = 3;     //unit : min


        private int _elapsed = 0;

        private LinePos initRSPos = new LinePos();
        private PointF eFramePoint = PointF.Empty;
        private PointF _basePoint = PointF.Empty;

        private RoofSupport[] _rsData = new RoofSupport[202];
        private Dozer _dozer = new Dozer();
        private Carrier[] _lcc = new Carrier[3];
        private Carrier _surfLcc = new Carrier();

        private int setPosBtnId = -1;
        
        private List<int> _available = new List<int>();
        private List<int> _unavailable = new List<int>();


        private PointF locAPosSt = PointF.Empty;
        private PointF locAPosEn = PointF.Empty;

        private PointF locBPosSt = PointF.Empty;
        private PointF locBPosEn = PointF.Empty;

        private PointF locShuntPos = PointF.Empty;
        private PointF locSurfacePos = PointF.Empty;

        private List<PointF> routeAPoints = new List<PointF>();
        private List<PointF> routeBPoints = new List<PointF>();
        private List<PointF> routeDozerPoints = new List<PointF>();
        private List<PointF> routeSurfaceAPoints = new List<PointF>();
        private List<PointF> routeSurfaceBPoints = new List<PointF>();

        private PointF routeAcurPos = PointF.Empty;
        private PointF routeBcurPos = PointF.Empty;
        private PointF routeDozercurPos = PointF.Empty;
        private PointF routeSurfaceAcurPos = PointF.Empty;
        private PointF routeSurfaceBcurPos = PointF.Empty;

        private PointF lastRSPosInA = PointF.Empty;
        private PointF lastRSPosInB = PointF.Empty;

        private int movedSalvageRS = 0;

        //private Queue<Command> _cmdDozer = new Queue<Command>();
        //private Queue<Command>[] _cmdLCC = new Queue<Command>[3];
        //private Queue<Command> _cmdSLCC = new Queue<Command>();

        private float shuntDistanceA = 50.0f;
        private float shuntDistanceB = 50.0f;
        private List<int> rsInLocA = new List<int>();
        private List<int> rsInLocB = new List<int>();

        private List<int> rsInSurface = new List<int>();

        private int curStorage = 0; // 0: carry to storage A, 1: carry to storage B
        private int stockedRSinStorage = 0; // number of RSs that carried to storage
        private int curStorageInSurface = 0;  // 0: carry from storage A, 1: carry from storage B

        private List<int> waitingRoom = new List<int>();
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
            DrawArea.MouseDoubleClick += DrawArea_MouseDblClick;
            DrawArea.MouseClick += DrawArea_MouseClick;
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
            waitingRoom.Clear();
            for (int i = 0; i < LCCCounts; i ++)
            {
                curPos.Y += 5;
                _lcc[i].setCenter(curPos);
                _lcc[i].setAngle(tempAng);
                _lcc[i].clearCommand();
                _lcc[i].setStatus(0);
                _lcc[i].setLoadedRS(-1);
                waitingRoom.Add(i);
            }
            rsInLocA.Clear();
            rsInLocB.Clear();
            lastRSPosInA = HaDesMath.movePointByDisAndAngle(locAPosEn, HaDesMath.getAngleByPoints(locAPosEn, locAPosSt), 5.0f);
            lastRSPosInB = HaDesMath.movePointByDisAndAngle(locBPosEn, HaDesMath.getAngleByPoints(locBPosEn, locBPosSt), 5.0f);

            _surfLcc.setCenter(locSurfacePos);
            _surfLcc.setAngle(90);
            _surfLcc.setTelemeter(0);
            _surfLcc.setStatus(3);
            _surfLcc.setLoadedRS(-1);
            _surfLcc.clearCommand();
            rsInSurface.Clear();

            curStorage = 0;
            stockedRSinStorage = 0;
            curStorageInSurface = 0;
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
            _dozer.setStatus(0);
            _dozer.setLoadedRS(-1);
            _dozer.clearCommand();

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


            // draw shunt point
            g.DrawEllipse(new Pen(Color.DarkBlue), new RectangleF(new PointF(locShuntPos.X - 10, locShuntPos.Y - 10), new Size(20, 20)));
            g.FillEllipse(new SolidBrush(Color.Red), new RectangleF(new PointF(locShuntPos.X - 10, locShuntPos.Y - 10), new Size(20, 20)));

            // draw surface point
            g.DrawEllipse(new Pen(Color.DarkBlue), new RectangleF(new PointF(locSurfacePos.X - 2, locSurfacePos.Y - 2), new Size(4, 4)));
            g.FillEllipse(new SolidBrush(Color.Green), new RectangleF(new PointF(locSurfacePos.X - 2, locSurfacePos.Y - 2), new Size(4, 4)));

            _haDesDxf.DrawDxf(g);

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

            // draw storage A
            g.DrawLine(new Pen(Color.Green, 0.2f), locAPosSt, locAPosEn);
            float alpha = HaDesMath.getAngleByPoints(locAPosSt, locAPosEn);
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(locAPosSt, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(locAPosSt, alpha + 90, 1.5f));
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(locAPosEn, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(locAPosEn, alpha + 90, 1.5f));


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

            // draw storage B
            g.DrawLine(new Pen(Color.Green, 0.2f), locBPosSt, locBPosEn);
            alpha = HaDesMath.getAngleByPoints(locBPosSt, locBPosEn);
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(locBPosSt, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(locBPosSt, alpha + 90, 1.5f));
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(locBPosEn, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(locBPosEn, alpha + 90, 1.5f));

            // draw route A to Surface
            for (int i = 0; i < routeSurfaceAPoints.Count; i++)
            {
                if (i < routeSurfaceAPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), routeSurfaceAPoints[i], routeSurfaceAPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), routeSurfaceAPoints[i], routeSurfaceAcurPos);
                }
            }

            // draw route B to Surface
            for (int i = 0; i < routeSurfaceBPoints.Count; i++)
            {
                if (i < routeSurfaceBPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), routeSurfaceBPoints[i], routeSurfaceBPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), routeSurfaceBPoints[i], routeSurfaceBcurPos);
                }
            }

            // draw LCC
            for (int i = 0; i < LCCCounts; i++)
            {
                _lcc[i].draw(g);
            }

            _surfLcc.draw(g);

            _dozer.draw(g);  // draw dozer

            // draw RS
            for (int i = 1; i <= RSCounts; i++)
            {
                _rsData[i].draw(g);
            }
        }
        private void processDozerSimulation()
        {
            if (_dozer.getStatus() == 1)
            {
                if (waitingRoom.Count > 0)
                {
                    //_dozer.dequeueCommand();
                    loadRSToLCC(_dozer.getLoadedRS(), _elapsed);
                    movedSalvageRS++;
                    salvageNextRS(_dozer.getCenter(), _dozer.getAngle(), _elapsed);
                }
            }
            int status = _dozer.runCommand(_elapsed, SimulationSpeed * timer1.Interval / 1000);
            if (_dozer.getLoadedRS() != -1)
            {
                int id = _dozer.getLoadedRS();
                _rsData[id].setCenter(HaDesMath.movePointByDisAndAngle(_dozer.getCenter(), _dozer.getAngle(), 3.5f));
                _rsData[id].setAngle(_dozer.getAngle());
            }
            if (status == 1)
            {
                _dozer.setStatus(1);
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
                    _dozer.addCommand(_cmd);
                    curPos = pos;
                    curTime += step;
                }

                // move down
                pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.0f * carried - 4.0f);
                _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, i);
                _dozer.addCommand(_cmd);
                curPos = pos;
                curTime += step;

                // rotate 90 degree
                _cmd = new Command(curTime, curTime + step, curPos, curPos, curAngle, curAngle + 90, 1, i);
                _dozer.addCommand(_cmd);
                curAngle += 90;
                curTime += step;

                // move to target position
                pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 1.0f + carried * 2.0f);
                pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 6.0f);
                _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, i);
                _dozer.addCommand(_cmd);
                curPos = pos;
                curTime += step;

                if (carried == 2)
                {
                    // move to complete eframe position
                    pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 5.0f);
                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 10.0f);
                    _cmd = new Command(curTime, EFrameBuiltTime * 3600, curPos, pos, curAngle, curAngle, 0, -1);
                    _dozer.addCommand(_cmd);
                    curPos = pos;
                    curTime = EFrameBuiltTime * 3600;
                    
                } else
                {
                    // move to next position
                    nextId = getNextMoveId(i);
                    pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 1.0f + carried * 2.0f);
                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 1.7f + 2.0f * (RSCounts - nextId));
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    _dozer.addCommand(_cmd);
                    curPos = pos;
                    curTime += step;

                    // rotate 90 degree
                    _cmd = new Command(curTime, curTime + step, curPos, curPos, curAngle, curAngle - 90, -1, -1);
                    _dozer.addCommand(_cmd);
                    curAngle -= 90;
                    curTime += step;

                    // move up
                    pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 4.0f - 2.0f * carried);
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    _dozer.addCommand(_cmd);
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
            int step = (int)(d * 3.6f / DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
            _dozer.addCommand(_cmd);
            curPos = pos;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + SalvageTime * 20, curPos, curPos, curAngle, curAngle - 90, -1, -1);
            _dozer.addCommand(_cmd);
            curAngle -= 90;
            curTime += SalvageTime * 20;

            // move down

            pos = HaDesMath.movePointByDisAndAngle(curPos, alpha - 90, -2);
            _cmd = new Command(curTime, curTime + SalvageTime * 20, curPos, pos, curAngle, curAngle, 0, idx);
            _dozer.addCommand(_cmd);
            curPos = pos;
            curTime += SalvageTime * 20;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + SalvageTime * 20, curPos, curPos, curAngle, curAngle + 90, 1, idx);
            _dozer.addCommand(_cmd);
            curAngle += 90;
            curTime += SalvageTime * 20;

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

            int step = (int)(d * 3.6f / DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, routeDozerPoints[1], alpha, alpha, 0, idx);
            _dozer.addCommand(_cmd);
            curPos = routeDozerPoints[1];
            curTime += step;

            for (int i = 2; i < routeDozerPoints.Count; i ++)
            {
                int type = (i == routeDozerPoints.Count - 1) ? 1 : 0;
                d = HaDesMath.CalcDistancePoints(curPos, routeDozerPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeDozerPoints[i], curPos);
                step = (int)(d * 3.6f / DozerSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeDozerPoints[i], alpha, alpha, 0, idx, type);
                _dozer.addCommand(_cmd);
                curPos = routeDozerPoints[i];
                curTime += step;
            }
        }

        private void salvageNextRS(PointF lastPos, float lastAng, int lastTime)
        {
            if (movedSalvageRS >= _unavailable.Count)
            {
                return;
            }

            // move to next RS
            int idx = _unavailable[movedSalvageRS];

            int curTime = lastTime;
            PointF curPos = lastPos, pos;
            float curAngle = lastAng;
            Command _cmd;
            float d, alpha;
            int step;
            for (int i = routeDozerPoints.Count - 2; i > 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeDozerPoints[i]);
                alpha = HaDesMath.getAngleByPoints(curPos, routeDozerPoints[i]);

                step = (int)(d * 3.6f / DozerSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeDozerPoints[i], alpha, alpha, 0, -1);
                _dozer.addCommand(_cmd);
                curPos = routeDozerPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            pos = HaDesMath.movePointByDisAndAngle(_basePoint, HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos) - 90, 3.0f);
            pos = HaDesMath.movePointByDisAndAngle(pos, HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos) - 180, 1.7f + (RSCounts - idx) * 2.0f);

            d = HaDesMath.CalcDistancePoints(pos, curPos);
            alpha = HaDesMath.getAngleByPoints(curPos, pos);
            step = (int)(d * 3.6f / DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
            _dozer.addCommand(_cmd);
            curPos = pos;
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + SalvageTime * 15, curPos, curPos, curAngle, curAngle - 90, -1, -1);
            _dozer.addCommand(_cmd);
            curAngle -= 90;
            curTime += SalvageTime * 15;

            // move up
            pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.0f);
            _cmd = new Command(curTime, curTime + SalvageTime * 15, curPos, pos, curAngle, curAngle, 0, -1);
            _dozer.addCommand(_cmd);
            curPos = pos;
            curTime += SalvageTime * 15;

            // move down
            pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, -2.0f);
            _cmd = new Command(curTime, curTime + SalvageTime * 15, curPos, pos, curAngle, curAngle, 0, idx);
            _dozer.addCommand(_cmd);
            curPos = pos;
            curTime += SalvageTime * 15;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + SalvageTime * 15, curPos, curPos, curAngle, curAngle + 90, 1, idx);
            _dozer.addCommand(_cmd);
            curAngle += 90;
            curTime += SalvageTime * 15;

            gotoLCCStartPos(curPos, curAngle, curTime, idx);
        }

        /*
        private void insertWaitingCommand(int waitingTime, int id)
        {
            Queue<Command> _cmdTemp = new Queue<Command>();

            foreach (Command cmd in _cmdLCC[id])
            {
                _cmdTemp.Enqueue(cmd);
            }
            _cmdLCC[id].Clear();
            Command _cmd = new Command(_elapsed, _elapsed + waitingTime, _lcc[id].getCenter(), _lcc[id].getCenter(), _lcc[id].getAngle(), _lcc[id].getAngle(), 0, _cmdTemp.Peek().loadedRS, 3);
            _cmdLCC[id].Enqueue(_cmd);

            _cmd = _cmdTemp.Dequeue();
            _cmd.stAngle = _lcc[id].getAngle();
            _cmd.stPos = _lcc[id].getCenter();
            _cmd.stTime = _elapsed + waitingTime;
            if (_cmd.enTime != int.MaxValue) _cmd.enTime = _cmd.enTime + waitingTime;
            _cmdLCC[id].Enqueue(_cmd);

            while (_cmdTemp.Count > 0)
            {
                _cmd = _cmdTemp.Dequeue();
                _cmd.stTime += waitingTime;
                if (_cmd.enTime != int.MaxValue) _cmd.enTime = _cmd.enTime + waitingTime;
                _cmdLCC[id].Enqueue(_cmd);
            }
        }

        private void processPassingLCCs(int waitingTime)
        {
            float dl = (float)LCCSpeed * SimulationSpeed * timer1.Interval / 3600.0f;
            for (int i = 0; i < LCCCounts; i ++)
            {
                if (_lcc[i].getTelemeter() <= 0 || _cmdLCC[i].Count == 0) continue;
                if (HaDesMath.CalcDistancePoints(_lcc[i].getCenter(), routeAPoints[0]) < 3.0f * dl) continue;
                if (_cmdLCC[i].Peek().type == 3) // waiting...
                {
                    continue;
                }
                for (int j = i + 1; j < LCCCounts; j ++)
                {
                    if (_lcc[j].getTelemeter() <= 0 || _cmdLCC[j].Count == 0) continue;
                    if (HaDesMath.CalcDistancePoints(_lcc[j].getCenter(), routeAPoints[0]) < 3.0f * dl) continue;
                    if (_cmdLCC[j].Peek().type == 3) // waiting...
                    {
                        continue;
                    }
                    if (HaDesMath.CalcDistancePoints(_lcc[i].getCenter(), _lcc[j].getCenter()) < dl * 1.05f)
                    {
                        insertWaitingCommand(waitingTime, i);
                        insertWaitingCommand(waitingTime, j);
                    }
                }
            }
        } */
        private void processLCCSimulation()
        {
            //processPassingLCCs(180);
            for (int i = 0; i < LCCCounts; i ++)
            {
                switch (_lcc[i].getStatus())
                {
                    case 1:  // waiting to check shunt(before carry)
                        if (checkStorage())
                        {
                            int routeNum = curStorage == 0 ? 11 : 21;
                            if (checkRoute(routeNum) && checkRoute(routeNum + 1) && checkRoute(routeNum + 2))
                            {
                                if (curStorage == 0)
                                {
                                    gotoShuntInA(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS());
                                }
                                else
                                {
                                    gotoShuntInB(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS());
                                }
                                waitingRoom.RemoveAt(0);
                                _lcc[i].setStatus(routeNum);
                                stockedRSinStorage++;
                            }
                        }
                        break;
                    case 2:  // wait to load new RS (after carry)
                        waitingRoom.Add(i);
                        PointF curPos = PointF.Empty;
                        float tempAng = -90;
                        if (routeDozerPoints.Count > 0)
                        {
                            curPos = routeDozerPoints[routeDozerPoints.Count - 1];
                        }
                        curPos.Y += 5 * waitingRoom.Count;
                        _lcc[i].setCenter(curPos);
                        _lcc[i].setAngle(tempAng);
                        _lcc[i].clearCommand();
                        _lcc[i].setStatus(0);
                        break;
                    case 12:  // check on shunt on A 
                        if (checkRoute(14))
                        {
                            gotoLocAandBackToShunt(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), false);
                            _lcc[i].setStatus(14);
                        }
                        else
                        {
                            waitShuntInA(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS());
                            _lcc[i].setStatus(11);
                        }
                        break;
                    case 13:  // wait on shunt in A to prev pass
                        if (checkRoute(14))
                        {
                            gotoLocAandBackToShunt(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), true);
                            _lcc[i].setStatus(14);
                        }
                        break;
                    case 15:  // check on shunt on A back
                        if (checkRoute(11))
                        {
                            gotoDozerInA(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), false);
                            _lcc[i].setStatus(11);
                        }
                        else
                        {
                            waitShuntInA(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS());
                            _lcc[i].setStatus(14);
                        }
                        break;
                    case 16:  // check on shunt on A back
                        if (checkRoute(11))
                        {
                            gotoDozerInA(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), true);
                            _lcc[i].setStatus(11);
                        }
                        break;
                    case 22:  // check on shunt on B 
                        if (checkRoute(24))
                        {
                            gotoLocBandBackToShunt(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), false);
                            _lcc[i].setStatus(24);
                        }
                        else
                        {
                            waitShuntInB(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS());
                            _lcc[i].setStatus(21);
                        }
                        break;
                    case 23:  // wait on shunt in B to prev pass
                        if (checkRoute(24))
                        {
                            gotoLocBandBackToShunt(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), true);
                            _lcc[i].setStatus(24);
                        }
                        break;
                    case 25:  // check on shunt on B back
                        if (checkRoute(21))
                        {
                            gotoDozerInB(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), false);
                            _lcc[i].setStatus(21);
                        }
                        else
                        {
                            waitShuntInB(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS());
                            _lcc[i].setStatus(24);
                        }
                        break;
                    case 26:  // check on shunt on B back
                        if (checkRoute(21))
                        {
                            gotoDozerInB(i, _lcc[i].getCenter(), _lcc[i].getAngle(), _elapsed, _lcc[i].getLoadedRS(), true);
                            _lcc[i].setStatus(21);
                        }
                        break;
                    default:
                        break;
                }
                
                int status = _lcc[i].runCommand(_elapsed, SimulationSpeed * timer1.Interval / 1000);
                if (_lcc[i].getLoadedRS() != -1)
                {
                    int id = _lcc[i].getLoadedRS();
                    _rsData[id].setCenter(HaDesMath.movePointByDisAndAngle(_lcc[i].getCenter(), _lcc[i].getAngle(), 2.5f));
                    _rsData[id].setAngle(_lcc[i].getAngle());
                }
                if (status == 1 || status == 2)
                {
                    _lcc[i].setStatus(status);
                }
                else if (status == 4)
                {
                    rsInLocA.Add(_lcc[i].getLoadedRS());
                    lastRSPosInA = HaDesMath.movePointByDisAndAngle(lastRSPosInA, HaDesMath.getAngleByPoints(locAPosEn, locAPosSt), 5.0f);
                }
                else if (status == 5)
                {
                    rsInLocB.Add(_lcc[i].getLoadedRS());
                    lastRSPosInB = HaDesMath.movePointByDisAndAngle(lastRSPosInB, HaDesMath.getAngleByPoints(locBPosEn, locBPosSt), 5.0f);
                }
                else if (status > 10)
                {
                    _lcc[i].setStatus(status);
                }
            }
        }

        private void gotoShuntInA(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(locShuntPos, routeAPoints, out m, out interPos);

            // move in route A: carry
            for (int i = 1; i <= m; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = routeAPoints[i];
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, idRS, 12);
            _lcc[id].addCommand(_cmd);
        }
        private void waitShuntInA(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime;
            PointF curPos = lastPos;
            float curAng = lastAng;
            Command _cmd;
            int type = _lcc[id].getStatus() + 1;
            _cmd = new Command(curTime, curTime + LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng - 90, 8.0f), curAng + 90, curAng + 90, 0, idRS, type);
            _lcc[id].addCommand(_cmd);
        }
        private void gotoLocAandBackToShunt(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(locShuntPos, routeAPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m + 1; i < routeAPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = routeAPoints[i];
                curAng = alpha;
                curTime += step;
            }
            // go to location A load pos
            d = HaDesMath.CalcDistancePoints(curPos, lastRSPosInA);
            alpha = HaDesMath.getAngleByPoints(lastRSPosInA, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, lastRSPosInA, alpha, alpha, 0, idRS);
            _lcc[id].addCommand(_cmd);
            curPos = lastRSPosInA;
            curAng = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + LCCDropTime * 60, curPos, curPos, curAng, curAng + 180, 1, idRS, 4);
            _lcc[id].addCommand(_cmd);
            curAng = curAng + 180;
            curTime += LCCDropTime * 60;
            // move in route A: back
            for (int i = routeAPoints.Count - 1; i > m; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);
                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, -1);
                _lcc[id].addCommand(_cmd);
                curPos = routeAPoints[i];
                curAng = alpha;
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, -1, 15);
            _lcc[id].addCommand(_cmd);
        }
        private void gotoDozerInA(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(locShuntPos, routeAPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, -1, type);
                _lcc[id].addCommand(_cmd);
                curPos = routeAPoints[i];
                curAng = alpha;
                curTime += step;
            }
        }

        private void gotoShuntInB(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(locShuntPos, routeBPoints, out m, out interPos);

            // move in route A: carry
            for (int i = 1; i <= m; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeBPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeBPoints[i], alpha, alpha, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = routeBPoints[i];
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, idRS, 22);
            _lcc[id].addCommand(_cmd);
        }
        private void waitShuntInB(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime;
            PointF curPos = lastPos;
            float curAng = lastAng;
            Command _cmd;
            int type = _lcc[id].getStatus() + 1;
            _cmd = new Command(curTime, curTime + LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng - 90, 8.0f), curAng + 90, curAng + 90, 0, idRS, type);
            _lcc[id].addCommand(_cmd);
        }
        private void gotoLocBandBackToShunt(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(locShuntPos, routeBPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m + 1; i < routeBPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeBPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeBPoints[i], alpha, alpha, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = routeBPoints[i];
                curAng = alpha;
                curTime += step;
            }
            // go to location A load pos
            d = HaDesMath.CalcDistancePoints(curPos, lastRSPosInB);
            alpha = HaDesMath.getAngleByPoints(lastRSPosInB, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, lastRSPosInB, alpha, alpha, 0, idRS);
            _lcc[id].addCommand(_cmd);
            curPos = lastRSPosInB;
            curAng = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + LCCDropTime * 60, curPos, curPos, curAng, curAng + 180, 1, idRS, 5);
            _lcc[id].addCommand(_cmd);
            curAng = curAng + 180;
            curTime += LCCDropTime * 60;
            // move in route A: back
            for (int i = routeBPoints.Count - 1; i > m; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeBPoints[i], curPos);
                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeBPoints[i], alpha, alpha, 0, -1);
                _lcc[id].addCommand(_cmd);
                curPos = routeBPoints[i];
                curAng = alpha;
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, -1, 25);
            _lcc[id].addCommand(_cmd);
        }
        private void gotoDozerInB(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(locShuntPos, routeBPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeBPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeBPoints[i], alpha, alpha, 0, -1, type);
                _lcc[id].addCommand(_cmd);
                curPos = routeBPoints[i];
                curAng = alpha;
                curTime += step;
            }
        }

        private void carryRSToLocA(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;

            // move in route A: carry
            for (int i = 1; i < routeAPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = routeAPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            // go to location A load pos
            d = HaDesMath.CalcDistancePoints(curPos, lastRSPosInA);
            alpha = HaDesMath.getAngleByPoints(lastRSPosInA, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, lastRSPosInA, alpha, alpha, 0, idRS);
            _lcc[id].addCommand(_cmd);
            curPos = lastRSPosInA;
            curAngle = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + LCCDropTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, idRS, 4);
            _lcc[id].addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += LCCDropTime * 60;


            // move in route A: back
            for (int i = routeAPoints.Count - 1; i >= 0 ; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeAPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeAPoints[i], alpha, alpha, 0, -1, type);
                _lcc[id].addCommand(_cmd);
                curPos = routeAPoints[i];
                curAngle = alpha;
                curTime += step;
            }
        }


        private void carryRSToLocB(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;
            

            // move in route B: carry
            for (int i = 1; i < routeBPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeBPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeBPoints[i], alpha, alpha, 0, idRS);
                _lcc[id].addCommand(_cmd);
                curPos = routeBPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            // go to location B load pos
            d = HaDesMath.CalcDistancePoints(curPos, lastRSPosInB);
            alpha = HaDesMath.getAngleByPoints(lastRSPosInB, curPos);

            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, lastRSPosInB, alpha, alpha, 0, idRS);
            _lcc[id].addCommand(_cmd);
            curPos = lastRSPosInB;
            curAngle = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + LCCDropTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, idRS, 5);
            _lcc[id].addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += LCCDropTime * 60;


            // move in route B: back
            for (int i = routeBPoints.Count - 1; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeBPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeBPoints[i], alpha, alpha, 0, -1, type);
                _lcc[id].addCommand(_cmd);
                curPos = routeBPoints[i];
                curAngle = alpha;
                curTime += step;
            }
        }

        // check storages are full or not t: 0, A, t: 1, B
        private bool checkStorage() 
        {
            int maxS;
            if (curStorage == 0)
            {
                maxS = (int)(HaDesMath.CalcDistancePoints(locAPosSt, locAPosEn) / 5.0f);
                if (stockedRSinStorage < maxS) return true;
                if (checkRSinStorage(rsInLocB) == -1)
                {
                    rsInLocB.Clear();
                    lastRSPosInB = HaDesMath.movePointByDisAndAngle(locBPosEn, HaDesMath.getAngleByPoints(locBPosEn, locBPosSt), 5.0f);
                    curStorage = 1;
                    stockedRSinStorage = 0;
                    return true;
                }
                return false;
            }
            maxS = (int)(HaDesMath.CalcDistancePoints(locBPosSt, locBPosEn) / 5.0f);
            if (stockedRSinStorage < maxS) return true;
            if (checkRSinStorage(rsInLocA) == -1)
            {
                rsInLocA.Clear();
                lastRSPosInA = HaDesMath.movePointByDisAndAngle(locAPosEn, HaDesMath.getAngleByPoints(locAPosEn, locAPosSt), 5.0f);
                stockedRSinStorage = 0;
                curStorage = 0;
                return true;
            }
            return false;
        }

        private bool checkRoute(int segNum)
        {
            for (int i = 0; i < LCCCounts; i ++)
            {
                if (_lcc[i].getStatus() == segNum) return false;
            }
            return true;
        }
        private void loadRSToLCC(int idRS, int lastTime)
        {
            Command _cmd;
            int step = (int)(5.0f * 3.6f / LCCSpeed);
            foreach (int i in waitingRoom)
            {
                _cmd = new Command(lastTime, lastTime + step, _lcc[i].getCenter(), new PointF(_lcc[i].getCenter().X, _lcc[i].getCenter().Y - 5), _lcc[i].getAngle(), _lcc[i].getAngle(), 0, -1);
                _lcc[i].addCommand(_cmd);
            }
            for (int i = 0; i < LCCCounts; i ++)
            {
                
            }
            int idx = waitingRoom[0];

            float alpha = HaDesMath.getAngleByPoints(routeDozerPoints[routeDozerPoints.Count - 1], routeDozerPoints[routeDozerPoints.Count - 2]);
            float beta = curStorage == 0 ? HaDesMath.getAngleByPoints(routeAPoints[0], routeAPoints[1]) - 180 : HaDesMath.getAngleByPoints(routeBPoints[0], routeBPoints[1]) - 180;
            int dir;
            if (_lcc[idx].getAngle() < alpha) { dir = 1; }
            else { dir = -1; }
            PointF pos = routeDozerPoints[routeDozerPoints.Count-1];
            _cmd = new Command(lastTime + step, lastTime + step + LCCLoadingTime * 30, pos, pos, _lcc[idx].getAngle(), alpha, dir, -1);
            _lcc[idx].addCommand(_cmd);
            if (alpha < beta) { dir = 1; }
            else { dir = -1; }
            _cmd = new Command(lastTime + step + LCCLoadingTime * 30, lastTime + step + LCCLoadingTime * 60, pos, pos, alpha, beta, dir, idRS, 1);
            _lcc[idx].addCommand(_cmd);
            _dozer.setStatus(0);
        }

        private int checkRSinStorage(List<int> rss)
        {
            for (int i = 0; i < rss.Count; i++)
            {
                if (rss[i] != -1)
                {
                    return i;
                }
            }
            return -1;
        }

        private void carryRSFromLocA(int p, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;

            // move in surface route A
            for (int i = routeSurfaceAPoints.Count - 1; i >= 0; i --)
            {
                PointF pos = i > 0 ? routeSurfaceAPoints[i - 1] : HaDesMath.movePointByDisAndAngle(locAPosEn, HaDesMath.getAngleByPoints(locAPosEn, locAPosSt), 5.0f * p);
                d = HaDesMath.CalcDistancePoints(curPos, pos);
                alpha = HaDesMath.getAngleByPoints(pos, curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
                _surfLcc.addCommand(_cmd);
                curPos = pos;
                curAngle = alpha;
                curTime += step;
            }

            // load RS
            _cmd = new Command(curTime, curTime + LCCLoadingTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, -1);
            _surfLcc.addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += LCCLoadingTime * 60;


            // move in surface route A: back(carrying)
            for (int i = 0; i < routeSurfaceAPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeSurfaceAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeSurfaceAPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeSurfaceAPoints[i], alpha, alpha, 0, rsInLocA[p]);
                _surfLcc.addCommand(_cmd);
                curPos = routeSurfaceAPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            int r = (rsInSurface.Count / 15);
            int c = (rsInSurface.Count % 15);

            // move down.
            d = 5.0f + 5.0f * r;
            alpha = -90;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, rsInLocA[p]);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move left
            d = 5.0f + 2.0f * c;
            alpha = 180;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, rsInLocA[p]);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, rsInLocA[p]);
            _surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += LCCDropTime * 30;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, -1);
            _surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += LCCDropTime * 30;


            // move right
            d = 5.0f + 2.0f * c;
            alpha = 0;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move up.
            d = 5.0f + 5.0f * r;
            alpha = 90;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1, 3);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            rsInSurface.Add(rsInLocA[p]);
            rsInLocA[p] = -1;
            _surfLcc.setStatus(0);
        }

        private void carryRSFromLocB(int p, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;

            // move in surface route B
            for (int i = routeSurfaceBPoints.Count - 1; i >= 0; i--)
            {
                PointF pos = i > 0 ? routeSurfaceBPoints[i - 1] : HaDesMath.movePointByDisAndAngle(locBPosEn, HaDesMath.getAngleByPoints(locBPosEn, locBPosSt), 5.0f * p);
                d = HaDesMath.CalcDistancePoints(curPos, pos);
                alpha = HaDesMath.getAngleByPoints(pos, curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
                _surfLcc.addCommand(_cmd);
                curPos = pos;
                curAngle = alpha;
                curTime += step;
            }

            // load RS
            _cmd = new Command(curTime, curTime + LCCLoadingTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, -1);
            _surfLcc.addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += LCCLoadingTime * 60;


            // move in surface route B: back(carrying)
            for (int i = 0; i < routeSurfaceBPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routeSurfaceBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(routeSurfaceBPoints[i], curPos);

                step = (int)(d * 3.6f / LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routeSurfaceBPoints[i], alpha, alpha, 0, rsInLocB[p]);
                _surfLcc.addCommand(_cmd);
                curPos = routeSurfaceBPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            int r = (rsInSurface.Count / 15);
            int c = (rsInSurface.Count % 15);

            // move down.
            d = 5.0f + 5.0f * r;
            alpha = -90;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, rsInLocB[p]);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move left
            d = 5.0f + 2.0f * c;
            alpha = 180;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, rsInLocB[p]);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, rsInLocB[p]);
            _surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += LCCDropTime * 30;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, -1);
            _surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += LCCDropTime * 30;


            // move right
            d = 5.0f + 2.0f * c;
            alpha = 0;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move up.
            d = 5.0f + 5.0f * r;
            alpha = 90;
            step = (int)(d * 3.6f / LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1, 3);
            _surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            rsInSurface.Add(rsInLocA[p]);
            rsInLocA[p] = -1;
            _surfLcc.setStatus(0);
        }

        private void processSLCCSimulation() {

            if (_surfLcc.getStatus() == 3) // waiting to check there is RS in storage A or B
            {
                if (curStorageInSurface == 0)
                {
                    int cSA = checkRSinStorage(rsInLocA);
                    int cSB = checkRSinStorage(rsInLocB);
                    if ( cSA != -1)
                    {
                        carryRSFromLocA(cSA, _surfLcc.getCenter(), _surfLcc.getAngle(), _elapsed);
                    } else
                    {
                        if (cSB != -1)
                        {
                            curStorageInSurface = 1;
                            carryRSFromLocB(cSB, _surfLcc.getCenter(), _surfLcc.getAngle(), _elapsed);
                        }
                    }
                } else
                {
                    int cSA = checkRSinStorage(rsInLocA);
                    int cSB = checkRSinStorage(rsInLocB);
                    if (cSB != -1)
                    {
                        carryRSFromLocB(cSB, _surfLcc.getCenter(), _surfLcc.getAngle(), _elapsed);
                    }
                    else
                    {
                        if (cSA != -1)
                        {
                            curStorageInSurface = 0;
                            carryRSFromLocA(cSA, _surfLcc.getCenter(), _surfLcc.getAngle(), _elapsed);
                        }
                    }
                }
            }

            int status = _surfLcc.runCommand(_elapsed, SimulationSpeed * timer1.Interval / 1000);
            if (_surfLcc.getLoadedRS() != -1)
            {
                int id = _surfLcc.getLoadedRS();
                _rsData[id].setCenter(HaDesMath.movePointByDisAndAngle(_surfLcc.getCenter(), _surfLcc.getAngle(), 2.5f));
                _rsData[id].setAngle(_surfLcc.getAngle());
            }
            if (status == 3)
            {
                _surfLcc.setStatus(status);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            _elapsed += SimulationSpeed * timer1.Interval / 1000;
            _curTime = _curTime + TimeSpan.FromSeconds(SimulationSpeed * timer1.Interval / 1000);
            SimulationTimeSB.Text = _curTime.ToString();
            ElapsedTimeSB.Text = HaDesMath.ConvertSecToTimeStr(_elapsed);
            processDozerSimulation();
            processLCCSimulation();
            processSLCCSimulation();
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
            SetLocationSurfaceBtn.Enabled = state;
            SetRouteSurfaceABtn.Enabled = state;
            SetRouteSurfaceBBtn.Enabled = state;
        }

        private void DrawArea_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
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
                case 6: // set shunt pos
                    locShuntPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    shuntDistanceA = HaDesMath.calcShuntDistance(locShuntPos, routeAPoints);
                    shuntDistanceB = HaDesMath.calcShuntDistance(locShuntPos, routeBPoints);
                    setUncheckRest(-1);
                    DrawArea.Invalidate();
                    break;
                case 7: // set surface pos
                    locSurfacePos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    setUncheckRest(-1);
                    DrawArea.Invalidate();
                    break;
                case 8: // set route A to surface
                    routeSurfaceAPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    DrawArea.Invalidate();
                    break;
                case 9: // set route B to surface
                    routeSurfaceBPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    DrawArea.Invalidate();
                    break;
                default:
                    break;
            }
        }

        private void DrawArea_MouseDblClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            switch (setPosBtnId)
            {
                case 3: // set route A
                    //routeAPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    routeAcurPos = routeAPoints[routeAPoints.Count - 1];
                    shuntDistanceA = HaDesMath.calcShuntDistance(locShuntPos, routeAPoints);
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 4: // set route B
                    //routeBPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    routeBcurPos = routeBPoints[routeBPoints.Count - 1];
                    shuntDistanceB = HaDesMath.calcShuntDistance(locShuntPos, routeBPoints);
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 5: // set dozer route
                    //routeDozerPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    routeDozercurPos = routeDozerPoints[routeDozerPoints.Count - 1];
                    InitializeLCCs();
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 8: // set route A to surface
                    routeSurfaceAPoints.Add(locSurfacePos);
                    routeSurfaceAcurPos = routeSurfaceAPoints[routeSurfaceAPoints.Count - 1];
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 9: // set route B to surface
                    routeSurfaceBPoints.Add(locSurfacePos);
                    routeSurfaceBcurPos = routeSurfaceBPoints[routeSurfaceBPoints.Count - 1];
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                default:
                    break;
            }
        }

        ///Mouse Event process Start
        private void DrawArea_MouseDown(object sender, MouseEventArgs e)
        {
            Point currentPoint = DrawPanel.PointToClient(DrawArea.PointToScreen(e.Location));
            if (e.Button == MouseButtons.Right)
            {
                moveStartPoint = currentPoint;
                DrawArea.Cursor = Cursors.SizeAll;
            }
            if (e.Button == MouseButtons.Left)
            {
                DrawArea.Cursor = Cursors.Cross;
                moveStartPoint = e.Location;
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
                case 8: // set route A to surface
                    routeSurfaceAcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                case 9: // set route B to surface
                    routeSurfaceBcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
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
                SimulationTime = sfrm.SimulationTime;
                SalvageTime = sfrm.SalvageTime;
                DozerSpeed = sfrm.DozerSpeed;
                LCCSpeed = sfrm.LCCSpeed;
                LCCLoadingTime = sfrm.LCCLoadingTime;
                LCCDropTime = sfrm.LCCDropTime;
                if (LCCCounts != sfrm.LCCCounts)
                {
                    LCCCounts = sfrm.LCCCounts;
                    _lcc = new Carrier[LCCCounts];
                    for (int i = 0; i < LCCCounts; i++)
                    {
                        _lcc[i] = new Carrier();
                    }
                    InitializeLCCs();
                }
                if (RSCounts != sfrm.RSCounts)
                {
                    RSCounts = sfrm.RSCounts;
                    if (RSCounts > 0 && RSCounts < 150)
                    {
                        _available.Clear();
                        _unavailable.Clear();
                        for (int i = 1; i <= RSCounts; i++)
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
                }
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
            if (routeAPoints.Count == 0)
            {
                MessageBox.Show("Please set route A first.");
                return;
            }
            if (SetLocationABtn.Checked)
            {
                setUncheckRest(1);
                initializePointsAndLine(4);
                DrawArea.Invalidate();
            } else
            {
                setUncheckRest(-1);
            }
        }
        private void onSetLocationBBtn(object sender, EventArgs e)
        {
            if (routeBPoints.Count == 0)
            {
                MessageBox.Show("Please set route B first.");
                return;
            }
            if (SetLocationBBtn.Checked)
            {
                setUncheckRest(2);
                initializePointsAndLine(6);
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
            SetLocationSurfaceBtn.Checked = (id == 7);
            SetRouteSurfaceABtn.Checked = (id == 8);
            SetRouteSurfaceBBtn.Checked = (id == 9);

            setPosBtnId = id;
            if (id == -1)
            {
                DrawArea.Cursor = Cursors.Default;
            } else
            {
                DrawArea.Cursor = Cursors.Cross;
            }
        }

        private void initializePointsAndLine(int opt)
        {
            switch (opt)
            {
                case 1:     // Press eframe
                    eFramePoint = PointF.Empty;
                    routeDozerPoints.Clear();
                    routeDozercurPos = PointF.Empty;
                    routeAPoints.Clear();
                    routeAcurPos = PointF.Empty;
                    locAPosSt = locAPosEn = PointF.Empty;
                    routeSurfaceAPoints.Clear();
                    routeSurfaceAcurPos = PointF.Empty;
                    routeBPoints.Clear();
                    routeBcurPos = PointF.Empty;
                    locBPosSt = locBPosEn = PointF.Empty;
                    routeSurfaceBPoints.Clear();
                    routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 2:     // Press dozer route
                    routeDozerPoints.Clear();
                    float alpha = HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos);
                    PointF pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 3.0f);

                    routeDozerPoints.Add(pos);

                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 2.0f * RSCounts + 3.0f);

                    routeDozerPoints.Add(pos);
                    routeDozercurPos = pos;

                    routeDozercurPos = PointF.Empty;

                    routeAPoints.Clear();
                    routeAcurPos = PointF.Empty;
                    locAPosSt = locAPosEn = PointF.Empty;
                    routeSurfaceAPoints.Clear();
                    routeSurfaceAcurPos = PointF.Empty;
                    routeBPoints.Clear();
                    routeBcurPos = PointF.Empty;
                    locBPosSt = locBPosEn = PointF.Empty;
                    routeSurfaceBPoints.Clear();
                    routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 3:     // Press route A
                    routeAPoints.Clear();
                    routeAPoints.Add(routeDozerPoints[routeDozerPoints.Count - 1]);

                    locAPosSt = locAPosEn = PointF.Empty;
                    routeSurfaceAPoints.Clear();
                    routeSurfaceAcurPos = PointF.Empty;
                    break;
                case 4:     // Press location A
                    locAPosSt = routeAPoints[routeAPoints.Count - 1];
                    locAPosEn = PointF.Empty;
                    routeSurfaceAPoints.Clear();
                    routeSurfaceAcurPos = PointF.Empty;
                    break;
                case 5:     // Press route B
                    routeBPoints.Clear();
                    routeBPoints.Add(routeDozerPoints[routeDozerPoints.Count - 1]);

                    locBPosSt = locBPosEn = PointF.Empty;
                    routeSurfaceBPoints.Clear();
                    routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 6:     // Press location B
                    locBPosSt = routeBPoints[routeBPoints.Count - 1];
                    locBPosEn = PointF.Empty;

                    routeSurfaceBPoints.Clear();
                    routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 7:     // Press surface point
                    locSurfacePos = PointF.Empty;
                    routeSurfaceAPoints.Clear();
                    routeSurfaceAcurPos = PointF.Empty;
                    routeSurfaceBPoints.Clear();
                    routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 8:     // Press surface route A
                    routeSurfaceAPoints.Clear();
                    routeSurfaceAPoints.Add(locAPosEn);
                    routeSurfaceAcurPos = routeSurfaceAPoints[0];
                    break;
                case 9:     // Press surface route B
                    routeSurfaceBPoints.Clear();
                    routeSurfaceBPoints.Add(locBPosEn);
                    routeSurfaceBcurPos = routeSurfaceBPoints[0];
                    break;
                default:
                    break;
            }
        }

        private void onSetLocationEFrameBtn(object sender, EventArgs e)
        {
            if (SetLocationEFrameBtn.Checked)
            {
                setUncheckRest(0);
                initializePointsAndLine(1);
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

                initializePointsAndLine(3);
                DrawArea.Invalidate();
                setUncheckRest(3);
            } else
            {
                routeAcurPos = routeAPoints[routeAPoints.Count - 1];
                shuntDistanceA = HaDesMath.calcShuntDistance(locShuntPos, routeAPoints);
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
                initializePointsAndLine(5);
                DrawArea.Invalidate();
                setUncheckRest(4);
            } else
            {
                routeBcurPos = routeBPoints[routeBPoints.Count - 1];
                shuntDistanceB = HaDesMath.calcShuntDistance(locShuntPos, routeBPoints);
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }
        private void onSetRouteDozerBtn(object sender, EventArgs e)
        {
            if (SetRouteDozerBtn.Checked)
            {
                if (eFramePoint == PointF.Empty)
                {
                    MessageBox.Show("Please set e-frame point.");
                    setUncheckRest(-1);
                    return;
                }
                initializePointsAndLine(2);
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

        private bool checkAllParameter()
        {
            if (eFramePoint == Point.Empty)
            {
                MessageBox.Show("Please set e-frame point.");
                return false;
            }
            if (SimulationSpeed <= 0)
            {
                MessageBox.Show("Simulation Speed can't be zero.");
                return false;
            }

            if (routeDozerPoints.Count == 0)
            {
                MessageBox.Show("Please set Dozer route.");
                return false;
            }
            if (routeAPoints.Count == 0)
            {
                MessageBox.Show("Please set route A.");
                return false;
            }
            if (routeBPoints.Count == 0)
            {
                MessageBox.Show("Please set route B.");
                return false;
            }
            if (locSurfacePos == PointF.Empty)
            {
                MessageBox.Show("Please set surface point.");
                return false;
            }
            if (routeSurfaceAPoints.Count == 0)
            {
                MessageBox.Show("Please set surface route A.");
                return false;
            }
            if (routeSurfaceBPoints.Count == 0)
            {
                MessageBox.Show("Please set surface route B.");
                return false;
            }
            if (locShuntPos == PointF.Empty)
            {
                MessageBox.Show("Please set shunt point.");
                return false;
            }
            return true;
        }
        private void onStartSimulationBtn(object sender, EventArgs e)
        {
            if (!checkAllParameter())
            {
                return;
            }
            _elapsed = 0;
            _curTime = SimulationTime;
            StartSimulationBtn.Enabled = false;
            StopSimulationBtn.Enabled = true;
            movedSalvageRS = 0;
            InitializeRSandDozer();
            InitializeLCCs();
            makeEFrameCommand();
            timer1.Interval = 100;
            timer1.Enabled = true;
        }
        private void onStopSimulationBtn(object sender, EventArgs e)
        {
            timer1.Enabled = false;
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

        private void SetLocationSurfaceBtn_Click(object sender, EventArgs e)
        {
            if (SetLocationSurfaceBtn.Checked)
            {
                initializePointsAndLine(7);
                setUncheckRest(7);
            }
            else
            {
                setUncheckRest(-1);
            }
        }

        private void SetRouteSurfaceABtn_Click(object sender, EventArgs e)
        {
            if (SetRouteSurfaceABtn.Checked)
            {
                if (locAPosEn == PointF.Empty || locSurfacePos == PointF.Empty)
                {
                    MessageBox.Show("Please set storage A and Surface point first.");
                    setUncheckRest(-1);
                    return;
                }
                initializePointsAndLine(8);
                DrawArea.Invalidate();
                setUncheckRest(8);
            }
            else
            {
                routeSurfaceAPoints.Add(locSurfacePos);
                routeSurfaceAcurPos = routeSurfaceAPoints[routeSurfaceAPoints.Count - 1];
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }

        private void SetRouteSurfaceBBtn_Click(object sender, EventArgs e)
        {
            if (SetRouteSurfaceBBtn.Checked)
            {
                if (locBPosEn == PointF.Empty || locSurfacePos == PointF.Empty)
                {
                    MessageBox.Show("Please set storage B and Surface point first.");
                    setUncheckRest(-1);
                    return;
                }
                initializePointsAndLine(9);
                DrawArea.Invalidate();
                setUncheckRest(9);
            }
            else
            {
                routeSurfaceBPoints.Add(locSurfacePos);
                routeSurfaceBcurPos = routeSurfaceBPoints[routeSurfaceBPoints.Count - 1];
                DrawArea.Invalidate();
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
