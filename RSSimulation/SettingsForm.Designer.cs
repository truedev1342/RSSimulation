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
            this.inputStartTime = new System.Windows.Forms.DateTimePicker();
            this.inputScale = new System.Windows.Forms.NumericUpDown();
            this.inputRSNum = new System.Windows.Forms.NumericUpDown();
            this.inputNecessaryTime = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.inputScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputRSNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputNecessaryTime)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 30);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start time";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(70, 80);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Scale";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 130);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Number of RS";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 180);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "Necessary time";
            // 
            // inputStartTime
            // 
            this.inputStartTime.CustomFormat = "MM/dd/yyyy  hh:mm:ss";
            this.inputStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.inputStartTime.Location = new System.Drawing.Point(129, 24);
            this.inputStartTime.Name = "inputStartTime";
            this.inputStartTime.Size = new System.Drawing.Size(160, 22);
            this.inputStartTime.TabIndex = 1;
            // 
            // inputScale
            // 
            this.inputScale.Location = new System.Drawing.Point(129, 74);
            this.inputScale.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.inputScale.Name = "inputScale";
            this.inputScale.Size = new System.Drawing.Size(160, 22);
            this.inputScale.TabIndex = 2;
            // 
            // inputRSNum
            // 
            this.inputRSNum.Location = new System.Drawing.Point(129, 124);
            this.inputRSNum.Maximum = new decimal(new int[] {
            149,
            0,
            0,
            0});
            this.inputRSNum.Name = "inputRSNum";
            this.inputRSNum.Size = new System.Drawing.Size(160, 22);
            this.inputRSNum.TabIndex = 3;
            // 
            // inputNecessaryTime
            // 
            this.inputNecessaryTime.Location = new System.Drawing.Point(129, 174);
            this.inputNecessaryTime.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.inputNecessaryTime.Name = "inputNecessaryTime";
            this.inputNecessaryTime.Size = new System.Drawing.Size(160, 22);
            this.inputNecessaryTime.TabIndex = 4;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(69, 237);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(179, 237);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 292);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.inputNecessaryTime);
            this.Controls.Add(this.inputRSNum);
            this.Controls.Add(this.inputScale);
            this.Controls.Add(this.inputStartTime);
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
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.inputScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputRSNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputNecessaryTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker inputStartTime;
        private System.Windows.Forms.NumericUpDown inputScale;
        private System.Windows.Forms.NumericUpDown inputRSNum;
        private System.Windows.Forms.NumericUpDown inputNecessaryTime;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}