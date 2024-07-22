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
        private int _elapsed = 0;
        private int setPosBtnId = -1;

//        private float shuntDistanceA = 50.0f;
//        private float shuntDistanceB = 50.0f;

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
            Global._salvageRSList = new List<int>();
            Global._normalRSList = new List<int>();
            for (int i = 1; i <= Global.RSCounts; i++)
            {
                Global._normalRSList.Add(i);
            }
            SimulationTimeSB.Text = Global.SimulationTime.ToString();
            ElapsedTimeSB.Text = "00:00:00";
            enableSimulation(false);
            for (int i = 0; i < 202; i++)
            {
                Global._rsData[i] = new RoofSupport();
            }
            for (int i = 0; i < Global._lcc.Length; i ++)
            {
                Global._lcc[i] = new Carrier();
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
                if( _haDesDxf.getPrimaryLinePos(ref Global.initRSPos))
                    enableSimulation(true);
                //initialRect = HaDesMath.GetBoundingBox(_dxfDocument, ref Global.initRSPos);
                DrawAreaSize = HaDesMath.UpdateDrawAreaSize(initialRect, zoomFactor);
                DrawArea.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to render DXF file: {ex.Message}");
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
            g.DrawEllipse(new Pen(Color.DarkBlue), new RectangleF(new PointF(Global.locShuntPos.X - 10, Global.locShuntPos.Y - 10), new Size(20, 20)));
            g.FillEllipse(new SolidBrush(Color.Red), new RectangleF(new PointF(Global.locShuntPos.X - 10, Global.locShuntPos.Y - 10), new Size(20, 20)));

            // draw surface point
            g.DrawEllipse(new Pen(Color.DarkBlue), new RectangleF(new PointF(Global.locSurfacePos.X - 2, Global.locSurfacePos.Y - 2), new Size(4, 4)));
            g.FillEllipse(new SolidBrush(Color.Green), new RectangleF(new PointF(Global.locSurfacePos.X - 2, Global.locSurfacePos.Y - 2), new Size(4, 4)));

            _haDesDxf.DrawDxf(g);

            // draw dozer route
            for (int i = 0; i < Global.routeDozerPoints.Count; i ++)
            {
                if (i <  Global.routeDozerPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Yellow, 0.1f), Global.routeDozerPoints[i], Global.routeDozerPoints[i + 1]);
                } else
                {
                    g.DrawLine(new Pen(Color.Yellow, 0.1f), Global.routeDozerPoints[i], Global.routeDozercurPos);
                }
            }

            // draw route A
            for (int i = 0; i < Global.routeAPoints.Count; i++)
            {
                if (i < Global.routeAPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), Global.routeAPoints[i], Global.routeAPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), Global.routeAPoints[i], Global.routeAcurPos);
                }
            }

            // draw storage A
            g.DrawLine(new Pen(Color.Green, 0.2f), Global.locAPosSt, Global.locAPosEn);
            float alpha = HaDesMath.getAngleByPoints(Global.locAPosSt, Global.locAPosEn);
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(Global.locAPosSt, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(Global.locAPosSt, alpha + 90, 1.5f));
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(Global.locAPosEn, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(Global.locAPosEn, alpha + 90, 1.5f));


            // draw route B
            for (int i = 0; i < Global.routeBPoints.Count; i++)
            {
                if (i < Global.routeBPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), Global.routeBPoints[i], Global.routeBPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), Global.routeBPoints[i], Global.routeBcurPos);
                }
            }

            // draw storage B
            g.DrawLine(new Pen(Color.Green, 0.2f), Global.locBPosSt, Global.locBPosEn);
            alpha = HaDesMath.getAngleByPoints(Global.locBPosSt, Global.locBPosEn);
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(Global.locBPosSt, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(Global.locBPosSt, alpha + 90, 1.5f));
            g.DrawLine(new Pen(Color.Green, 0.1f), HaDesMath.movePointByDisAndAngle(Global.locBPosEn, alpha - 90, 1.5f), HaDesMath.movePointByDisAndAngle(Global.locBPosEn, alpha + 90, 1.5f));

            // draw route A to Surface
            for (int i = 0; i < Global.routeSurfaceAPoints.Count; i++)
            {
                if (i < Global.routeSurfaceAPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), Global.routeSurfaceAPoints[i], Global.routeSurfaceAPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Red, 0.1f), Global.routeSurfaceAPoints[i], Global.routeSurfaceAcurPos);
                }
            }

            // draw route B to Surface
            for (int i = 0; i < Global.routeSurfaceBPoints.Count; i++)
            {
                if (i < Global.routeSurfaceBPoints.Count - 1)
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), Global.routeSurfaceBPoints[i], Global.routeSurfaceBPoints[i + 1]);
                }
                else
                {
                    g.DrawLine(new Pen(Color.Blue, 0.1f), Global.routeSurfaceBPoints[i], Global.routeSurfaceBcurPos);
                }
            }

            // draw LCC
            for (int i = 0; i < Global.LCCCounts; i++)
            {
                Global._lcc[i].draw(g);
            }

            Global._surfLcc.draw(g);

            Global._dozer.draw(g);  // draw dozer

            // draw RS
            for (int i = 1; i <= Global.RSCounts; i++)
            {
                Global._rsData[i].draw(g);
            }
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            _elapsed += Global.SimulationSpeed * mainTimer.Interval / 1000;
            _curTime = _curTime + TimeSpan.FromSeconds(Global.SimulationSpeed * mainTimer.Interval / 1000);
            SimulationTimeSB.Text = _curTime.ToString();
            ElapsedTimeSB.Text = HaDesMath.ConvertSecToTimeStr(_elapsed);
            processDozer.run(_elapsed);
            processCarryLCC.run(_elapsed);
            processSurfaceLCC.run(_elapsed);
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
                    Global.eFramePoint = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    setUncheckRest(-1);
                    _elapsed = 0;
                    _curTime = Global.SimulationTime;
                    Global.InitializeRSandDozer();
                    DrawArea.Invalidate();
                    break;
                case 1: // set location A
                    if (Global.locAPosSt == PointF.Empty)
                    {
                        Global.locAPosSt = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    }
                    else
                    {
                        Global.locAPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        DrawArea.Invalidate();
                        setUncheckRest(-1);
                    }
                    break;
                case 2: // set location B
                    if (Global.locBPosSt == PointF.Empty)
                    {
                        Global.locBPosSt = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    }
                    else
                    {
                        Global.locBPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        DrawArea.Invalidate();
                        setUncheckRest(-1);
                    }
                    break;
                case 3: // set route A
                    Global.routeAPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    DrawArea.Invalidate();
                    break;
                case 4: // set route B
                    Global.routeBPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    DrawArea.Invalidate();
                    break;
                case 5: // set dozer route
                    Global.routeDozerPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    DrawArea.Invalidate();
                    break;
                case 6: // set shunt pos
                    Global.locShuntPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    //shuntDistanceA = HaDesMath.calcShuntDistance(Global.locShuntPos, Global.routeAPoints);
                    //shuntDistanceB = HaDesMath.calcShuntDistance(Global.locShuntPos, Global.routeBPoints);
                    setUncheckRest(-1);
                    DrawArea.Invalidate();
                    break;
                case 7: // set surface pos
                    Global.locSurfacePos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    setUncheckRest(-1);
                    DrawArea.Invalidate();
                    break;
                case 8: // set route A to surface
                    Global.routeSurfaceAPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    DrawArea.Invalidate();
                    break;
                case 9: // set route B to surface
                    Global.routeSurfaceBPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
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
                    //Global.routeAPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    Global.routeAcurPos = Global.routeAPoints[Global.routeAPoints.Count - 1];
                    //shuntDistanceA = HaDesMath.calcShuntDistance(Global.locShuntPos, Global.routeAPoints);
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 4: // set route B
                    //Global.routeBPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    Global.routeBcurPos = Global.routeBPoints[Global.routeBPoints.Count - 1];
                    //shuntDistanceB = HaDesMath.calcShuntDistance(Global.locShuntPos, Global.routeBPoints);
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 5: // set dozer route
                    //Global.routeDozerPoints.Add(HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor));
                    Global.routeDozercurPos = Global.routeDozerPoints[Global.routeDozerPoints.Count - 1];
                    Global.InitializeLCCs();
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 8: // set route A to surface
                    Global.routeSurfaceAPoints.Add(Global.locSurfacePos);
                    Global.routeSurfaceAcurPos = Global.routeSurfaceAPoints[Global.routeSurfaceAPoints.Count - 1];
                    DrawArea.Invalidate();
                    setUncheckRest(-1);
                    break;
                case 9: // set route B to surface
                    Global.routeSurfaceBPoints.Add(Global.locSurfacePos);
                    Global.routeSurfaceBcurPos = Global.routeSurfaceBPoints[Global.routeSurfaceBPoints.Count - 1];
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
                    if (Global.locAPosSt != PointF.Empty)
                    {
                        Global.locAPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        DrawArea.Invalidate();
                    }
                    break;
                case 2: // set location B
                    if (Global.locBPosSt != PointF.Empty)
                    {
                        Global.locBPosEn = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                        DrawArea.Invalidate();
                    }
                    break;
                case 3: // set route A
                    Global.routeAcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                case 4: // set route B
                    Global.routeBcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                case 5: // set dozer route
                    Global.routeDozercurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                case 8: // set route A to surface
                    Global.routeSurfaceAcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
                    DrawArea.Invalidate();
                    break;
                case 9: // set route B to surface
                    Global.routeSurfaceBcurPos = HaDesDxf.GetDxfPosF(e.Location, offsetPos, zoomFactor);
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

            if (sfrm.ShowDialog() == DialogResult.OK)
            {
                if (sfrm.needReDraw)
                {
                    DrawArea.Invalidate();
                }
                if (Global.RSCounts > 0 && Global.RSCounts < 150)
                {
                    SetLocationEFrameBtn.Enabled = true;
                    SetLocationEFrameBtn.Checked = false;
                } else
                {
                    SetLocationEFrameBtn.Enabled = false;
                    SetLocationEFrameBtn.Checked = false;
                }
            }

        }
        private void onSalvageSequenceSMenu(object sender, EventArgs e)
        {
            SalvageForm svfrm = new SalvageForm();
            if (svfrm.ShowDialog() == DialogResult.OK)
            {
                Global.InitializeRSandDozer();
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
            if (Global.routeAPoints.Count == 0)
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
            if (Global.routeBPoints.Count == 0)
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
                    Global.eFramePoint = PointF.Empty;
                    Global.routeDozerPoints.Clear();
                    Global.routeDozercurPos = PointF.Empty;
                    Global.routeAPoints.Clear();
                    Global.routeAcurPos = PointF.Empty;
                    Global.locAPosSt = Global.locAPosEn = PointF.Empty;
                    Global.routeSurfaceAPoints.Clear();
                    Global.routeSurfaceAcurPos = PointF.Empty;
                    Global.routeBPoints.Clear();
                    Global.routeBcurPos = PointF.Empty;
                    Global.locBPosSt = Global.locBPosEn = PointF.Empty;
                    Global.routeSurfaceBPoints.Clear();
                    Global.routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 2:     // Press dozer route
                    Global.routeDozerPoints.Clear();
                    float alpha = HaDesMath.getAngleByPoints(Global.initRSPos.StartPos, Global.initRSPos.EndPos);
                    PointF pos = HaDesMath.movePointByDisAndAngle(Global._basePoint, alpha - 90, 3.0f);

                    Global.routeDozerPoints.Add(pos);

                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 2.0f * Global.RSCounts + 3.0f);

                    Global.routeDozerPoints.Add(pos);
                    Global.routeDozercurPos = pos;

                    Global.routeDozercurPos = PointF.Empty;

                    Global.routeAPoints.Clear();
                    Global.routeAcurPos = PointF.Empty;
                    Global.locAPosSt = Global.locAPosEn = PointF.Empty;
                    Global.routeSurfaceAPoints.Clear();
                    Global.routeSurfaceAcurPos = PointF.Empty;
                    Global.routeBPoints.Clear();
                    Global.routeBcurPos = PointF.Empty;
                    Global.locBPosSt = Global.locBPosEn = PointF.Empty;
                    Global.routeSurfaceBPoints.Clear();
                    Global.routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 3:     // Press route A
                    Global.routeAPoints.Clear();
                    Global.routeAPoints.Add(Global.routeDozerPoints[Global.routeDozerPoints.Count - 1]);

                    Global.locAPosSt = Global.locAPosEn = PointF.Empty;
                    Global.routeSurfaceAPoints.Clear();
                    Global.routeSurfaceAcurPos = PointF.Empty;
                    break;
                case 4:     // Press location A
                    Global.locAPosSt = Global.routeAPoints[Global.routeAPoints.Count - 1];
                    Global.locAPosEn = PointF.Empty;
                    Global.routeSurfaceAPoints.Clear();
                    Global.routeSurfaceAcurPos = PointF.Empty;
                    break;
                case 5:     // Press route B
                    Global.routeBPoints.Clear();
                    Global.routeBPoints.Add(Global.routeDozerPoints[Global.routeDozerPoints.Count - 1]);

                    Global.locBPosSt = Global.locBPosEn = PointF.Empty;
                    Global.routeSurfaceBPoints.Clear();
                    Global.routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 6:     // Press location B
                    Global.locBPosSt = Global.routeBPoints[Global.routeBPoints.Count - 1];
                    Global.locBPosEn = PointF.Empty;

                    Global.routeSurfaceBPoints.Clear();
                    Global.routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 7:     // Press surface point
                    Global.locSurfacePos = PointF.Empty;
                    Global.routeSurfaceAPoints.Clear();
                    Global.routeSurfaceAcurPos = PointF.Empty;
                    Global.routeSurfaceBPoints.Clear();
                    Global.routeSurfaceBcurPos = PointF.Empty;
                    break;
                case 8:     // Press surface route A
                    Global.routeSurfaceAPoints.Clear();
                    Global.routeSurfaceAPoints.Add(Global.locAPosEn);
                    Global.routeSurfaceAcurPos = Global.routeSurfaceAPoints[0];
                    break;
                case 9:     // Press surface route B
                    Global.routeSurfaceBPoints.Clear();
                    Global.routeSurfaceBPoints.Add(Global.locBPosEn);
                    Global.routeSurfaceBcurPos = Global.routeSurfaceBPoints[0];
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
                if (Global.routeDozerPoints.Count == 0)
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
                Global.routeAcurPos = Global.routeAPoints[Global.routeAPoints.Count - 1];
                //shuntDistanceA = HaDesMath.calcShuntDistance(Global.locShuntPos, Global.routeAPoints);
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }
        private void onSetRouteBBtn(object sender, EventArgs e)
        {
            if (SetRouteBBtn.Checked)
            {
                if (Global.routeDozerPoints.Count == 0)
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
                Global.routeBcurPos = Global.routeBPoints[Global.routeBPoints.Count - 1];
                //shuntDistanceB = HaDesMath.calcShuntDistance(Global.locShuntPos, Global.routeBPoints);
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }
        private void onSetRouteDozerBtn(object sender, EventArgs e)
        {
            if (SetRouteDozerBtn.Checked)
            {
                if (Global.eFramePoint == PointF.Empty)
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
                Global.routeDozercurPos = Global.routeDozerPoints[Global.routeDozerPoints.Count - 1];
                Global.InitializeLCCs();
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }

        private bool checkAllParameter()
        {
            if (Global.eFramePoint == Point.Empty)
            {
                MessageBox.Show("Please set e-frame point.");
                return false;
            }
            if (Global.SimulationSpeed <= 0)
            {
                MessageBox.Show("Simulation Speed can't be zero.");
                return false;
            }

            if (Global.routeDozerPoints.Count == 0)
            {
                MessageBox.Show("Please set Dozer route.");
                return false;
            }
            if (Global.routeAPoints.Count == 0)
            {
                MessageBox.Show("Please set route A.");
                return false;
            }
            if (Global.routeBPoints.Count == 0)
            {
                MessageBox.Show("Please set route B.");
                return false;
            }
            if (Global.locSurfacePos == PointF.Empty)
            {
                MessageBox.Show("Please set surface point.");
                return false;
            }
            if (Global.routeSurfaceAPoints.Count == 0)
            {
                MessageBox.Show("Please set surface route A.");
                return false;
            }
            if (Global.routeSurfaceBPoints.Count == 0)
            {
                MessageBox.Show("Please set surface route B.");
                return false;
            }
            if (Global.locShuntPos == PointF.Empty)
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
            _curTime = Global.SimulationTime;
            StartSimulationBtn.Enabled = false;
            StopSimulationBtn.Enabled = true;
            Global.movedSalvageRS = 0;
            Global.InitializeRSandDozer();
            Global.InitializeLCCs();
            processDozer.prepareEFrameCommand();
            mainTimer.Interval = 100;
            Global.timerInterval = 100;
            mainTimer.Enabled = true;
        }
        private void onStopSimulationBtn(object sender, EventArgs e)
        {
            mainTimer.Enabled = false;
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
                if (Global.locAPosEn == PointF.Empty || Global.locSurfacePos == PointF.Empty)
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
                Global.routeSurfaceAPoints.Add(Global.locSurfacePos);
                Global.routeSurfaceAcurPos = Global.routeSurfaceAPoints[Global.routeSurfaceAPoints.Count - 1];
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }

        private void SetRouteSurfaceBBtn_Click(object sender, EventArgs e)
        {
            if (SetRouteSurfaceBBtn.Checked)
            {
                if (Global.locBPosEn == PointF.Empty || Global.locSurfacePos == PointF.Empty)
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
                Global.routeSurfaceBPoints.Add(Global.locSurfacePos);
                Global.routeSurfaceBcurPos = Global.routeSurfaceBPoints[Global.routeSurfaceBPoints.Count - 1];
                DrawArea.Invalidate();
                setUncheckRest(-1);
            }
        }

        /// Toolbar Event process End
    }
}
