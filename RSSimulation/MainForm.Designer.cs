namespace RSSimulation
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.NewSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.CloseSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomInSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomOutSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomFitSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.MoveLeftSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveRightsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveUpSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveDownSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.SimulationPropertiesSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.SalvageSequenceSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewHelpSMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusBar = new System.Windows.Forms.StatusStrip();
            this.ScaleSBLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ScaleSB = new System.Windows.Forms.ToolStripStatusLabel();
            this.SplitSBLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.TimeSBLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SimulationTimeSB = new System.Windows.Forms.ToolStripStatusLabel();
            this.SplitSBLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ElapsedTimeSBLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ElapsedTimeSB = new System.Windows.Forms.ToolStripStatusLabel();
            this.SplitSBLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.TimeSpeedSBLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.TimeSpeedSB = new System.Windows.Forms.ToolStripStatusLabel();
            this.MousePositionSB = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.OpenBtn = new System.Windows.Forms.ToolStripButton();
            this.NewBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.SimulationPropertiesBtn = new System.Windows.Forms.ToolStripButton();
            this.SalvageSequenceBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.SetLocationEFrameBtn = new System.Windows.Forms.ToolStripButton();
            this.SetLocationABtn = new System.Windows.Forms.ToolStripButton();
            this.SetLocationBBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.SetRouteABtn = new System.Windows.Forms.ToolStripButton();
            this.SetRouteBBtn = new System.Windows.Forms.ToolStripButton();
            this.SetRouteDozerBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.StartSimulationBtn = new System.Windows.Forms.ToolStripButton();
            this.StopSimulationBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.DrawPanel = new System.Windows.Forms.Panel();
            this.DrawArea = new System.Windows.Forms.PictureBox();
            this.MainMenu.SuspendLayout();
            this.StatusBar.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.DrawPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DrawArea)).BeginInit();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.EditMenu,
            this.ToolMenu,
            this.HelpMenu});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(928, 24);
            this.MainMenu.TabIndex = 0;
            this.MainMenu.Text = "menuStrip1";
            // 
            // FileMenu
            // 
            this.FileMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewSMenu,
            this.OpenSMenu,
            this.toolStripSeparator1,
            this.CloseSMenu});
            this.FileMenu.Name = "FileMenu";
            this.FileMenu.Size = new System.Drawing.Size(37, 20);
            this.FileMenu.Text = "&File";
            // 
            // NewSMenu
            // 
            this.NewSMenu.Name = "NewSMenu";
            this.NewSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.NewSMenu.Size = new System.Drawing.Size(146, 22);
            this.NewSMenu.Text = "New";
            this.NewSMenu.Click += new System.EventHandler(this.onNewSMenu);
            // 
            // OpenSMenu
            // 
            this.OpenSMenu.Name = "OpenSMenu";
            this.OpenSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.OpenSMenu.Size = new System.Drawing.Size(146, 22);
            this.OpenSMenu.Text = "Open";
            this.OpenSMenu.Click += new System.EventHandler(this.onOpenSMenu);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(143, 6);
            // 
            // CloseSMenu
            // 
            this.CloseSMenu.Name = "CloseSMenu";
            this.CloseSMenu.Size = new System.Drawing.Size(146, 22);
            this.CloseSMenu.Text = "Close";
            this.CloseSMenu.Click += new System.EventHandler(this.onCloseSMenu);
            // 
            // EditMenu
            // 
            this.EditMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ZoomInSMenu,
            this.ZoomOutSMenu,
            this.ZoomFitSMenu,
            this.toolStripSeparator6,
            this.MoveLeftSMenu,
            this.MoveRightsMenu,
            this.MoveUpSMenu,
            this.MoveDownSMenu});
            this.EditMenu.Name = "EditMenu";
            this.EditMenu.Size = new System.Drawing.Size(39, 20);
            this.EditMenu.Text = "&Edit";
            // 
            // ZoomInSMenu
            // 
            this.ZoomInSMenu.Name = "ZoomInSMenu";
            this.ZoomInSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.PageUp)));
            this.ZoomInSMenu.Size = new System.Drawing.Size(203, 22);
            this.ZoomInSMenu.Text = "Zoom In";
            this.ZoomInSMenu.Click += new System.EventHandler(this.onZoomInSMenu);
            // 
            // ZoomOutSMenu
            // 
            this.ZoomOutSMenu.Name = "ZoomOutSMenu";
            this.ZoomOutSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Next)));
            this.ZoomOutSMenu.Size = new System.Drawing.Size(203, 22);
            this.ZoomOutSMenu.Text = "Zoom Out";
            this.ZoomOutSMenu.Click += new System.EventHandler(this.onZoomOutSMenu);
            // 
            // ZoomFitSMenu
            // 
            this.ZoomFitSMenu.Name = "ZoomFitSMenu";
            this.ZoomFitSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
            this.ZoomFitSMenu.Size = new System.Drawing.Size(203, 22);
            this.ZoomFitSMenu.Text = "Zoom Fit";
            this.ZoomFitSMenu.Click += new System.EventHandler(this.onZoomFitSMenu);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(200, 6);
            // 
            // MoveLeftSMenu
            // 
            this.MoveLeftSMenu.Name = "MoveLeftSMenu";
            this.MoveLeftSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
            this.MoveLeftSMenu.Size = new System.Drawing.Size(203, 22);
            this.MoveLeftSMenu.Text = "Move Left";
            this.MoveLeftSMenu.Click += new System.EventHandler(this.onMoveLeftSMenu);
            // 
            // MoveRightsMenu
            // 
            this.MoveRightsMenu.Name = "MoveRightsMenu";
            this.MoveRightsMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
            this.MoveRightsMenu.Size = new System.Drawing.Size(203, 22);
            this.MoveRightsMenu.Text = "Move Right";
            this.MoveRightsMenu.Click += new System.EventHandler(this.onMoveRightsMenu);
            // 
            // MoveUpSMenu
            // 
            this.MoveUpSMenu.Name = "MoveUpSMenu";
            this.MoveUpSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.MoveUpSMenu.Size = new System.Drawing.Size(203, 22);
            this.MoveUpSMenu.Text = "Move Up";
            this.MoveUpSMenu.Click += new System.EventHandler(this.onMoveUpSMenu);
            // 
            // MoveDownSMenu
            // 
            this.MoveDownSMenu.Name = "MoveDownSMenu";
            this.MoveDownSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.MoveDownSMenu.Size = new System.Drawing.Size(203, 22);
            this.MoveDownSMenu.Text = "Move Down";
            this.MoveDownSMenu.Click += new System.EventHandler(this.onMoveDownSMenu);
            // 
            // ToolMenu
            // 
            this.ToolMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SimulationPropertiesSMenu,
            this.SalvageSequenceSMenu});
            this.ToolMenu.Name = "ToolMenu";
            this.ToolMenu.Size = new System.Drawing.Size(46, 20);
            this.ToolMenu.Text = "&Tools";
            // 
            // SimulationPropertiesSMenu
            // 
            this.SimulationPropertiesSMenu.Name = "SimulationPropertiesSMenu";
            this.SimulationPropertiesSMenu.Size = new System.Drawing.Size(188, 22);
            this.SimulationPropertiesSMenu.Text = "Simutation Porperties";
            this.SimulationPropertiesSMenu.Click += new System.EventHandler(this.onSimulationProperiesSMenu);
            // 
            // SalvageSequenceSMenu
            // 
            this.SalvageSequenceSMenu.Name = "SalvageSequenceSMenu";
            this.SalvageSequenceSMenu.Size = new System.Drawing.Size(188, 22);
            this.SalvageSequenceSMenu.Text = "Salvage Sequence ";
            this.SalvageSequenceSMenu.Click += new System.EventHandler(this.onSalvageSequenceSMenu);
            // 
            // HelpMenu
            // 
            this.HelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewHelpSMenu});
            this.HelpMenu.Name = "HelpMenu";
            this.HelpMenu.Size = new System.Drawing.Size(44, 20);
            this.HelpMenu.Text = "&Help";
            // 
            // ViewHelpSMenu
            // 
            this.ViewHelpSMenu.Name = "ViewHelpSMenu";
            this.ViewHelpSMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.ViewHelpSMenu.Size = new System.Drawing.Size(173, 22);
            this.ViewHelpSMenu.Text = "View Help";
            this.ViewHelpSMenu.Click += new System.EventHandler(this.onViewHelpSMenu);
            // 
            // StatusBar
            // 
            this.StatusBar.BackColor = System.Drawing.SystemColors.GrayText;
            this.StatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ScaleSBLabel,
            this.ScaleSB,
            this.SplitSBLabel1,
            this.TimeSBLabel,
            this.SimulationTimeSB,
            this.SplitSBLabel2,
            this.ElapsedTimeSBLabel,
            this.ElapsedTimeSB,
            this.SplitSBLabel3,
            this.TimeSpeedSBLabel,
            this.TimeSpeedSB,
            this.MousePositionSB});
            this.StatusBar.Location = new System.Drawing.Point(0, 529);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Size = new System.Drawing.Size(928, 22);
            this.StatusBar.TabIndex = 3;
            this.StatusBar.Text = "StatusBar";
            // 
            // ScaleSBLabel
            // 
            this.ScaleSBLabel.Name = "ScaleSBLabel";
            this.ScaleSBLabel.Size = new System.Drawing.Size(40, 17);
            this.ScaleSBLabel.Text = "Scale :";
            // 
            // ScaleSB
            // 
            this.ScaleSB.Name = "ScaleSB";
            this.ScaleSB.Size = new System.Drawing.Size(22, 17);
            this.ScaleSB.Text = "1.0";
            // 
            // SplitSBLabel1
            // 
            this.SplitSBLabel1.Name = "SplitSBLabel1";
            this.SplitSBLabel1.Size = new System.Drawing.Size(10, 17);
            this.SplitSBLabel1.Text = ",";
            // 
            // TimeSBLabel
            // 
            this.TimeSBLabel.Name = "TimeSBLabel";
            this.TimeSBLabel.Size = new System.Drawing.Size(99, 17);
            this.TimeSBLabel.Text = "Simulation Time :";
            // 
            // SimulationTimeSB
            // 
            this.SimulationTimeSB.Name = "SimulationTimeSB";
            this.SimulationTimeSB.Size = new System.Drawing.Size(105, 17);
            this.SimulationTimeSB.Text = "00/00/00 00:00 AM";
            // 
            // SplitSBLabel2
            // 
            this.SplitSBLabel2.Name = "SplitSBLabel2";
            this.SplitSBLabel2.Size = new System.Drawing.Size(10, 17);
            this.SplitSBLabel2.Text = ",";
            // 
            // ElapsedTimeSBLabel
            // 
            this.ElapsedTimeSBLabel.Name = "ElapsedTimeSBLabel";
            this.ElapsedTimeSBLabel.Size = new System.Drawing.Size(82, 17);
            this.ElapsedTimeSBLabel.Text = "Elapsed Time :";
            // 
            // ElapsedTimeSB
            // 
            this.ElapsedTimeSB.Name = "ElapsedTimeSB";
            this.ElapsedTimeSB.Size = new System.Drawing.Size(49, 17);
            this.ElapsedTimeSB.Text = "00:00:00";
            // 
            // SplitSBLabel3
            // 
            this.SplitSBLabel3.Name = "SplitSBLabel3";
            this.SplitSBLabel3.Size = new System.Drawing.Size(10, 17);
            this.SplitSBLabel3.Text = ",";
            // 
            // TimeSpeedSBLabel
            // 
            this.TimeSpeedSBLabel.Name = "TimeSpeedSBLabel";
            this.TimeSpeedSBLabel.Size = new System.Drawing.Size(74, 17);
            this.TimeSpeedSBLabel.Text = "Time Speed :";
            // 
            // TimeSpeedSB
            // 
            this.TimeSpeedSB.Name = "TimeSpeedSB";
            this.TimeSpeedSB.Size = new System.Drawing.Size(13, 17);
            this.TimeSpeedSB.Text = "1";
            // 
            // MousePositionSB
            // 
            this.MousePositionSB.Name = "MousePositionSB";
            this.MousePositionSB.Size = new System.Drawing.Size(10, 17);
            this.MousePositionSB.Text = ".";
            // 
            // toolBar
            // 
            this.toolBar.Dock = System.Windows.Forms.DockStyle.None;
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenBtn,
            this.NewBtn,
            this.toolStripSeparator3,
            this.SimulationPropertiesBtn,
            this.SalvageSequenceBtn,
            this.toolStripSeparator7,
            this.SetLocationEFrameBtn,
            this.SetLocationABtn,
            this.SetLocationBBtn,
            this.toolStripSeparator4,
            this.SetRouteABtn,
            this.SetRouteBBtn,
            this.SetRouteDozerBtn,
            this.toolStripSeparator5,
            this.StartSimulationBtn,
            this.StopSimulationBtn,
            this.toolStripSeparator2});
            this.toolBar.Location = new System.Drawing.Point(3, 0);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(349, 25);
            this.toolBar.TabIndex = 5;
            this.toolBar.Text = "toolStrip1";
            // 
            // OpenBtn
            // 
            this.OpenBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenBtn.Image = global::RSSimulation.Properties.Resources.btn_open;
            this.OpenBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenBtn.Name = "OpenBtn";
            this.OpenBtn.Size = new System.Drawing.Size(23, 22);
            this.OpenBtn.Text = "Open Dxf File";
            this.OpenBtn.ToolTipText = "Open Dxf File";
            this.OpenBtn.Click += new System.EventHandler(this.onOpenSMenu);
            // 
            // NewBtn
            // 
            this.NewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewBtn.Image = global::RSSimulation.Properties.Resources.btn_new;
            this.NewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewBtn.Name = "NewBtn";
            this.NewBtn.Size = new System.Drawing.Size(23, 22);
            this.NewBtn.Text = "New Simulation";
            this.NewBtn.ToolTipText = "New Simulation";
            this.NewBtn.Click += new System.EventHandler(this.onNewSMenu);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // SimulationPropertiesBtn
            // 
            this.SimulationPropertiesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SimulationPropertiesBtn.Image = ((System.Drawing.Image)(resources.GetObject("SimulationPropertiesBtn.Image")));
            this.SimulationPropertiesBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SimulationPropertiesBtn.Name = "SimulationPropertiesBtn";
            this.SimulationPropertiesBtn.Size = new System.Drawing.Size(23, 22);
            this.SimulationPropertiesBtn.Text = "Simulation Properties Setting";
            this.SimulationPropertiesBtn.ToolTipText = "Set Simulation Properties";
            this.SimulationPropertiesBtn.Click += new System.EventHandler(this.onSimulationProperiesSMenu);
            // 
            // SalvageSequenceBtn
            // 
            this.SalvageSequenceBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SalvageSequenceBtn.Image = ((System.Drawing.Image)(resources.GetObject("SalvageSequenceBtn.Image")));
            this.SalvageSequenceBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SalvageSequenceBtn.Name = "SalvageSequenceBtn";
            this.SalvageSequenceBtn.Size = new System.Drawing.Size(23, 22);
            this.SalvageSequenceBtn.Text = "Salvage Sequence Setting";
            this.SalvageSequenceBtn.ToolTipText = "Set Salvage Sequence";
            this.SalvageSequenceBtn.Click += new System.EventHandler(this.onSalvageSequenceSMenu);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // SetLocationEFrameBtn
            // 
            this.SetLocationEFrameBtn.CheckOnClick = true;
            this.SetLocationEFrameBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetLocationEFrameBtn.Image = global::RSSimulation.Properties.Resources.btn_location;
            this.SetLocationEFrameBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetLocationEFrameBtn.Name = "SetLocationEFrameBtn";
            this.SetLocationEFrameBtn.Size = new System.Drawing.Size(23, 22);
            this.SetLocationEFrameBtn.Text = "Set E-Frame Position";
            this.SetLocationEFrameBtn.ToolTipText = "Set E-Frame Position";
            this.SetLocationEFrameBtn.Click += new System.EventHandler(this.onSetLocationEFrameBtn);
            // 
            // SetLocationABtn
            // 
            this.SetLocationABtn.CheckOnClick = true;
            this.SetLocationABtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetLocationABtn.Image = global::RSSimulation.Properties.Resources.btn_storageA;
            this.SetLocationABtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetLocationABtn.Name = "SetLocationABtn";
            this.SetLocationABtn.Size = new System.Drawing.Size(23, 22);
            this.SetLocationABtn.Text = "Set A Storage Location";
            this.SetLocationABtn.ToolTipText = "Set A Storage Location";
            this.SetLocationABtn.Click += new System.EventHandler(this.onSetLocationABtn);
            // 
            // SetLocationBBtn
            // 
            this.SetLocationBBtn.CheckOnClick = true;
            this.SetLocationBBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetLocationBBtn.Image = global::RSSimulation.Properties.Resources.btn_storageB;
            this.SetLocationBBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetLocationBBtn.Name = "SetLocationBBtn";
            this.SetLocationBBtn.Size = new System.Drawing.Size(23, 22);
            this.SetLocationBBtn.Text = "Set B Storage Location";
            this.SetLocationBBtn.ToolTipText = "Set B Storage Location";
            this.SetLocationBBtn.Click += new System.EventHandler(this.onSetLocationEFrameBtn);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // SetRouteABtn
            // 
            this.SetRouteABtn.CheckOnClick = true;
            this.SetRouteABtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetRouteABtn.Image = global::RSSimulation.Properties.Resources.btn_routeA;
            this.SetRouteABtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetRouteABtn.Name = "SetRouteABtn";
            this.SetRouteABtn.Size = new System.Drawing.Size(23, 22);
            this.SetRouteABtn.Text = "Set Route to A Storage";
            this.SetRouteABtn.ToolTipText = "Set Route to A Storage";
            this.SetRouteABtn.Click += new System.EventHandler(this.onSetRouteABtn);
            // 
            // SetRouteBBtn
            // 
            this.SetRouteBBtn.CheckOnClick = true;
            this.SetRouteBBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetRouteBBtn.Image = global::RSSimulation.Properties.Resources.btn_routeB;
            this.SetRouteBBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetRouteBBtn.Name = "SetRouteBBtn";
            this.SetRouteBBtn.Size = new System.Drawing.Size(23, 22);
            this.SetRouteBBtn.Text = "Set Route to B Storage";
            this.SetRouteBBtn.ToolTipText = "Set Route to B Storage";
            this.SetRouteBBtn.Click += new System.EventHandler(this.onSetRouteBBtn);
            // 
            // SetRouteDozerBtn
            // 
            this.SetRouteDozerBtn.CheckOnClick = true;
            this.SetRouteDozerBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetRouteDozerBtn.Image = global::RSSimulation.Properties.Resources.btn_routeD;
            this.SetRouteDozerBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetRouteDozerBtn.Name = "SetRouteDozerBtn";
            this.SetRouteDozerBtn.Size = new System.Drawing.Size(23, 22);
            this.SetRouteDozerBtn.Text = "Set Route of Dozer";
            this.SetRouteDozerBtn.ToolTipText = "Set Route of Dozer";
            this.SetRouteDozerBtn.Click += new System.EventHandler(this.onSetRouteDozerBtn);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // StartSimulationBtn
            // 
            this.StartSimulationBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StartSimulationBtn.Image = global::RSSimulation.Properties.Resources.btn_start;
            this.StartSimulationBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StartSimulationBtn.Name = "StartSimulationBtn";
            this.StartSimulationBtn.Size = new System.Drawing.Size(23, 22);
            this.StartSimulationBtn.Text = "Start Simulation";
            this.StartSimulationBtn.ToolTipText = "Start Simulation";
            this.StartSimulationBtn.Click += new System.EventHandler(this.onStartSimulationBtn);
            // 
            // StopSimulationBtn
            // 
            this.StopSimulationBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StopSimulationBtn.Image = global::RSSimulation.Properties.Resources.btn_stop;
            this.StopSimulationBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StopSimulationBtn.Name = "StopSimulationBtn";
            this.StopSimulationBtn.Size = new System.Drawing.Size(23, 22);
            this.StopSimulationBtn.Text = "Stop Simulation";
            this.StopSimulationBtn.ToolTipText = "Stop Simulation";
            this.StopSimulationBtn.Click += new System.EventHandler(this.onStopSimulationBtn);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.DrawPanel);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(928, 480);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(928, 505);
            this.toolStripContainer1.TabIndex = 6;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolBar);
            // 
            // DrawPanel
            // 
            this.DrawPanel.AutoScroll = true;
            this.DrawPanel.BackColor = System.Drawing.SystemColors.InfoText;
            this.DrawPanel.Controls.Add(this.DrawArea);
            this.DrawPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawPanel.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.DrawPanel.Location = new System.Drawing.Point(0, 0);
            this.DrawPanel.Name = "DrawPanel";
            this.DrawPanel.Size = new System.Drawing.Size(928, 480);
            this.DrawPanel.TabIndex = 4;
            // 
            // DrawArea
            // 
            this.DrawArea.BackColor = System.Drawing.SystemColors.MenuText;
            this.DrawArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawArea.Location = new System.Drawing.Point(0, 0);
            this.DrawArea.Name = "DrawArea";
            this.DrawArea.Size = new System.Drawing.Size(928, 480);
            this.DrawArea.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.DrawArea.TabIndex = 1;
            this.DrawArea.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.ClientSize = new System.Drawing.Size(928, 551);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.StatusBar);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MainMenu;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Real Time Roof Support Simulation";
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.StatusBar.ResumeLayout(false);
            this.StatusBar.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.DrawPanel.ResumeLayout(false);
            this.DrawPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DrawArea)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem NewSMenu;
        private System.Windows.Forms.ToolStripMenuItem OpenSMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem CloseSMenu;
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem ToolMenu;
        private System.Windows.Forms.ToolStripMenuItem HelpMenu;
        private System.Windows.Forms.ToolStripMenuItem ViewHelpSMenu;
        private System.Windows.Forms.StatusStrip StatusBar;
        private System.Windows.Forms.ToolStripMenuItem SimulationPropertiesSMenu;
        private System.Windows.Forms.ToolStrip toolBar;
        private System.Windows.Forms.ToolStripButton StartSimulationBtn;
        private System.Windows.Forms.ToolStripButton SetLocationEFrameBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton StopSimulationBtn;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.PictureBox DrawArea;
        private System.Windows.Forms.Panel DrawPanel;
        private System.Windows.Forms.ToolStripStatusLabel ScaleSBLabel;
        private System.Windows.Forms.ToolStripStatusLabel ScaleSB;
        private System.Windows.Forms.ToolStripStatusLabel SplitSBLabel1;
        private System.Windows.Forms.ToolStripStatusLabel TimeSBLabel;
        private System.Windows.Forms.ToolStripStatusLabel SimulationTimeSB;
        private System.Windows.Forms.ToolStripStatusLabel SplitSBLabel2;
        private System.Windows.Forms.ToolStripStatusLabel ElapsedTimeSBLabel;
        private System.Windows.Forms.ToolStripStatusLabel ElapsedTimeSB;
        private System.Windows.Forms.ToolStripStatusLabel SplitSBLabel3;
        private System.Windows.Forms.ToolStripStatusLabel TimeSpeedSBLabel;
        private System.Windows.Forms.ToolStripStatusLabel TimeSpeedSB;
        private System.Windows.Forms.ToolStripStatusLabel MousePositionSB;
        private System.Windows.Forms.ToolStripMenuItem ZoomInSMenu;
        private System.Windows.Forms.ToolStripMenuItem ZoomOutSMenu;
        private System.Windows.Forms.ToolStripMenuItem ZoomFitSMenu;
        private System.Windows.Forms.ToolStripButton NewBtn;
        private System.Windows.Forms.ToolStripButton OpenBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton SetLocationABtn;
        private System.Windows.Forms.ToolStripButton SetLocationBBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton SetRouteABtn;
        private System.Windows.Forms.ToolStripButton SetRouteBBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem SalvageSequenceSMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem MoveLeftSMenu;
        private System.Windows.Forms.ToolStripMenuItem MoveRightsMenu;
        private System.Windows.Forms.ToolStripMenuItem MoveUpSMenu;
        private System.Windows.Forms.ToolStripMenuItem MoveDownSMenu;
        private System.Windows.Forms.ToolStripButton SetRouteDozerBtn;
        private System.Windows.Forms.ToolStripButton SimulationPropertiesBtn;
        private System.Windows.Forms.ToolStripButton SalvageSequenceBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    }
}

