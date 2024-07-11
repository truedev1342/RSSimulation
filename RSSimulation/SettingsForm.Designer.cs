namespace RSSimulation
{
    partial class SettingsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SimulationTimeTxt = new System.Windows.Forms.DateTimePicker();
            this.SimulationSpeedTxt = new System.Windows.Forms.NumericUpDown();
            this.RSCountsTxt = new System.Windows.Forms.NumericUpDown();
            this.EFrameTimeTxt = new System.Windows.Forms.NumericUpDown();
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.SalvageTimeTxt = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.DozerSpeedTxt = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.LCCLoadingTimeTxt = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.LCCSpeedTxt = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.LCCDropTimeTxt = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.LCCCountsTxt = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.SimulationSpeedTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RSCountsTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EFrameTimeTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SalvageTimeTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DozerSpeedTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCLoadingTimeTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCSpeedTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCDropTimeTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCCountsTxt)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Simulation Time :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 60);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Simulation Speed :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 93);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Roof Support Counts :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 126);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "E-Frame Built Time :";
            // 
            // SimulationTimeTxt
            // 
            this.SimulationTimeTxt.CustomFormat = "MM/dd/yyyy  hh:mm:ss";
            this.SimulationTimeTxt.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.SimulationTimeTxt.Location = new System.Drawing.Point(167, 24);
            this.SimulationTimeTxt.Name = "SimulationTimeTxt";
            this.SimulationTimeTxt.Size = new System.Drawing.Size(160, 22);
            this.SimulationTimeTxt.TabIndex = 10;
            // 
            // SimulationSpeedTxt
            // 
            this.SimulationSpeedTxt.Location = new System.Drawing.Point(186, 57);
            this.SimulationSpeedTxt.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.SimulationSpeedTxt.Name = "SimulationSpeedTxt";
            this.SimulationSpeedTxt.Size = new System.Drawing.Size(141, 22);
            this.SimulationSpeedTxt.TabIndex = 1;
            // 
            // RSCountsTxt
            // 
            this.RSCountsTxt.Location = new System.Drawing.Point(186, 90);
            this.RSCountsTxt.Maximum = new decimal(new int[] {
            149,
            0,
            0,
            0});
            this.RSCountsTxt.Name = "RSCountsTxt";
            this.RSCountsTxt.Size = new System.Drawing.Size(141, 22);
            this.RSCountsTxt.TabIndex = 2;
            // 
            // EFrameTimeTxt
            // 
            this.EFrameTimeTxt.Location = new System.Drawing.Point(186, 123);
            this.EFrameTimeTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.EFrameTimeTxt.Name = "EFrameTimeTxt";
            this.EFrameTimeTxt.Size = new System.Drawing.Size(141, 22);
            this.EFrameTimeTxt.TabIndex = 3;
            // 
            // OKBtn
            // 
            this.OKBtn.Location = new System.Drawing.Point(150, 359);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 11;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.onOKBtn);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(252, 359);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 12;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.onCancelBtn);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 159);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 16);
            this.label5.TabIndex = 0;
            this.label5.Text = "Salvage Time :";
            // 
            // SalvageTimeTxt
            // 
            this.SalvageTimeTxt.Location = new System.Drawing.Point(186, 156);
            this.SalvageTimeTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.SalvageTimeTxt.Name = "SalvageTimeTxt";
            this.SalvageTimeTxt.Size = new System.Drawing.Size(141, 22);
            this.SalvageTimeTxt.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 192);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(140, 16);
            this.label6.TabIndex = 0;
            this.label6.Text = "Dozer Moving Speed :";
            // 
            // DozerSpeedTxt
            // 
            this.DozerSpeedTxt.DecimalPlaces = 5;
            this.DozerSpeedTxt.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.DozerSpeedTxt.Location = new System.Drawing.Point(186, 189);
            this.DozerSpeedTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.DozerSpeedTxt.Name = "DozerSpeedTxt";
            this.DozerSpeedTxt.Size = new System.Drawing.Size(141, 22);
            this.DozerSpeedTxt.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(24, 258);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(124, 16);
            this.label7.TabIndex = 0;
            this.label7.Text = "LCC Loading Time :";
            // 
            // LCCLoadingTimeTxt
            // 
            this.LCCLoadingTimeTxt.Location = new System.Drawing.Point(186, 255);
            this.LCCLoadingTimeTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.LCCLoadingTimeTxt.Name = "LCCLoadingTimeTxt";
            this.LCCLoadingTimeTxt.Size = new System.Drawing.Size(141, 22);
            this.LCCLoadingTimeTxt.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 225);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(129, 16);
            this.label8.TabIndex = 0;
            this.label8.Text = "LCC Moving Speed :";
            // 
            // LCCSpeedTxt
            // 
            this.LCCSpeedTxt.DecimalPlaces = 5;
            this.LCCSpeedTxt.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.LCCSpeedTxt.Location = new System.Drawing.Point(186, 222);
            this.LCCSpeedTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.LCCSpeedTxt.Name = "LCCSpeedTxt";
            this.LCCSpeedTxt.Size = new System.Drawing.Size(141, 22);
            this.LCCSpeedTxt.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(24, 291);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(105, 16);
            this.label9.TabIndex = 0;
            this.label9.Text = "LCC Drop Time :";
            // 
            // LCCDropTimeTxt
            // 
            this.LCCDropTimeTxt.Location = new System.Drawing.Point(186, 288);
            this.LCCDropTimeTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.LCCDropTimeTxt.Name = "LCCDropTimeTxt";
            this.LCCDropTimeTxt.Size = new System.Drawing.Size(141, 22);
            this.LCCDropTimeTxt.TabIndex = 8;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(24, 323);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 16);
            this.label10.TabIndex = 0;
            this.label10.Text = "LCC Counts :";
            // 
            // LCCCountsTxt
            // 
            this.LCCCountsTxt.Location = new System.Drawing.Point(186, 320);
            this.LCCCountsTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.LCCCountsTxt.Name = "LCCCountsTxt";
            this.LCCCountsTxt.Size = new System.Drawing.Size(141, 22);
            this.LCCCountsTxt.TabIndex = 9;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(327, 126);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 16);
            this.label11.TabIndex = 0;
            this.label11.Text = "hours";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(327, 159);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 16);
            this.label12.TabIndex = 0;
            this.label12.Text = "mins";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(327, 257);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(35, 16);
            this.label13.TabIndex = 0;
            this.label13.Text = "mins";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(327, 291);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(35, 16);
            this.label14.TabIndex = 0;
            this.label14.Text = "mins";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(327, 191);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(36, 16);
            this.label15.TabIndex = 0;
            this.label15.Text = "km/h";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(327, 225);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(36, 16);
            this.label16.TabIndex = 0;
            this.label16.Text = "km/h";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 401);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.LCCSpeedTxt);
            this.Controls.Add(this.LCCCountsTxt);
            this.Controls.Add(this.LCCDropTimeTxt);
            this.Controls.Add(this.LCCLoadingTimeTxt);
            this.Controls.Add(this.DozerSpeedTxt);
            this.Controls.Add(this.SalvageTimeTxt);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.EFrameTimeTxt);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.RSCountsTxt);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.SimulationSpeedTxt);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.SimulationTimeTxt);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Setting Simulation Properties";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SimulationSpeedTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RSCountsTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EFrameTimeTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SalvageTimeTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DozerSpeedTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCLoadingTimeTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCSpeedTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCDropTimeTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LCCCountsTxt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker SimulationTimeTxt;
        private System.Windows.Forms.NumericUpDown SimulationSpeedTxt;
        private System.Windows.Forms.NumericUpDown RSCountsTxt;
        private System.Windows.Forms.NumericUpDown EFrameTimeTxt;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown SalvageTimeTxt;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown DozerSpeedTxt;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown LCCLoadingTimeTxt;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown LCCSpeedTxt;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown LCCDropTimeTxt;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown LCCCountsTxt;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
    }
}