namespace Bowling_Runway
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panel1 = new Bowling_Runway.BufferedPanel();
            this.direction = new System.Windows.Forms.Label();
            this.power = new System.Windows.Forms.Label();
            this.shotbutton = new System.Windows.Forms.Button();
            this.directionbar = new System.Windows.Forms.HScrollBar();
            this.powerbar = new System.Windows.Forms.HScrollBar();
            this.alleyLight = new System.Windows.Forms.CheckBox();
            this.pinsLight = new System.Windows.Forms.CheckBox();
            this.Lightswitch = new System.Windows.Forms.CheckBox();
            this.trackBarZ = new System.Windows.Forms.TrackBar();
            this.trackBarY = new System.Windows.Forms.TrackBar();
            this.trackBarX = new System.Windows.Forms.TrackBar();
            this.restartbutton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.restartbutton);
            this.panel1.Controls.Add(this.direction);
            this.panel1.Controls.Add(this.power);
            this.panel1.Controls.Add(this.shotbutton);
            this.panel1.Controls.Add(this.directionbar);
            this.panel1.Controls.Add(this.powerbar);
            this.panel1.Controls.Add(this.alleyLight);
            this.panel1.Controls.Add(this.pinsLight);
            this.panel1.Controls.Add(this.Lightswitch);
            this.panel1.Controls.Add(this.trackBarZ);
            this.panel1.Controls.Add(this.trackBarY);
            this.panel1.Controls.Add(this.trackBarX);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1200, 800);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel1_Paint);
            // 
            // direction
            // 
            this.direction.AutoSize = true;
            this.direction.Location = new System.Drawing.Point(1026, 258);
            this.direction.Name = "direction";
            this.direction.Size = new System.Drawing.Size(49, 13);
            this.direction.TabIndex = 13;
            this.direction.Text = "Direction";
            // 
            // power
            // 
            this.power.AutoSize = true;
            this.power.Location = new System.Drawing.Point(1026, 214);
            this.power.Name = "power";
            this.power.Size = new System.Drawing.Size(37, 13);
            this.power.TabIndex = 12;
            this.power.Text = "Power";
            // 
            // shotbutton
            // 
            this.shotbutton.Location = new System.Drawing.Point(1012, 302);
            this.shotbutton.Name = "shotbutton";
            this.shotbutton.Size = new System.Drawing.Size(75, 23);
            this.shotbutton.TabIndex = 11;
            this.shotbutton.Text = "Shot !";
            this.shotbutton.UseVisualStyleBackColor = true;
            this.shotbutton.Click += new System.EventHandler(this.ShotButton_Click);

            // 
            // directionbar
            // 
            this.directionbar.Location = new System.Drawing.Point(994, 275);
            this.directionbar.Name = "directionbar";
            this.directionbar.Size = new System.Drawing.Size(98, 21);
            this.directionbar.Minimum = -15;
            this.directionbar.Maximum = 15;
            this.directionbar.Value = 0;
            this.directionbar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.DirectionBar_Scroll);

            // 
            // powerbar
            // 
            this.powerbar.Location = new System.Drawing.Point(994, 234);
            this.powerbar.Name = "powerbar";
            this.powerbar.Size = new System.Drawing.Size(98, 21);
            this.powerbar.Minimum = 10;
            this.powerbar.Maximum = 150;
            this.powerbar.Value = 50;
            this.powerbar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PowerBar_Scroll);
            // 
            // alleyLight
            // 
            this.alleyLight.AutoSize = true;
            this.alleyLight.Checked = true;
            this.alleyLight.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alleyLight.Location = new System.Drawing.Point(1010, 193);
            this.alleyLight.Margin = new System.Windows.Forms.Padding(2);
            this.alleyLight.Name = "alleyLight";
            this.alleyLight.Size = new System.Drawing.Size(79, 17);
            this.alleyLight.TabIndex = 8;
            this.alleyLight.Text = "Alley Lights";
            this.alleyLight.UseVisualStyleBackColor = true;
            this.alleyLight.CheckedChanged += new System.EventHandler(this.AlleyLight_CheckedChanged);
            // 
            // pinsLight
            // 
            this.pinsLight.AutoSize = true;
            this.pinsLight.Checked = true;
            this.pinsLight.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pinsLight.Location = new System.Drawing.Point(1010, 161);
            this.pinsLight.Margin = new System.Windows.Forms.Padding(2);
            this.pinsLight.Name = "pinsLight";
            this.pinsLight.Size = new System.Drawing.Size(77, 17);
            this.pinsLight.TabIndex = 7;
            this.pinsLight.Text = "Pins Lights";
            this.pinsLight.UseVisualStyleBackColor = true;
            this.pinsLight.CheckedChanged += new System.EventHandler(this.PinsLight_CheckedChanged);
            // 
            // Lightswitch
            // 
            this.Lightswitch.AutoSize = true;
            this.Lightswitch.Checked = true;
            this.Lightswitch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Lightswitch.Location = new System.Drawing.Point(1010, 128);
            this.Lightswitch.Margin = new System.Windows.Forms.Padding(2);
            this.Lightswitch.Name = "Lightswitch";
            this.Lightswitch.Size = new System.Drawing.Size(82, 17);
            this.Lightswitch.TabIndex = 6;
            this.Lightswitch.Text = "Light switch";
            this.Lightswitch.UseVisualStyleBackColor = true;
            this.Lightswitch.CheckedChanged += new System.EventHandler(this.LightSwitch_CheckedChanged);
            // 
            // trackBarZ
            // 
            this.trackBarZ.Location = new System.Drawing.Point(994, 76);
            this.trackBarZ.Name = "trackBarZ";
            this.trackBarZ.Size = new System.Drawing.Size(104, 45);
            this.trackBarZ.TabIndex = 5;
            this.trackBarZ.TabStop = false;
            this.trackBarZ.Scroll += new System.EventHandler(this.TrackBarZ_Scroll);
            this.trackBarZ.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TrackBars_PreviewKeyDown);
            // 
            // trackBarY
            // 
            this.trackBarY.Location = new System.Drawing.Point(994, 44);
            this.trackBarY.Name = "trackBarY";
            this.trackBarY.Size = new System.Drawing.Size(104, 45);
            this.trackBarY.TabIndex = 3;
            this.trackBarY.TabStop = false;
            this.trackBarY.Scroll += new System.EventHandler(this.TrackBarY_Scroll);
            this.trackBarY.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TrackBars_PreviewKeyDown);
            // 
            // trackBarX
            // 
            this.trackBarX.Location = new System.Drawing.Point(994, 12);
            this.trackBarX.Name = "trackBarX";
            this.trackBarX.Size = new System.Drawing.Size(104, 45);
            this.trackBarX.TabIndex = 1;
            this.trackBarX.TabStop = false;
            this.trackBarX.Scroll += new System.EventHandler(this.TrackBarX_Scroll);
            this.trackBarX.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TrackBars_PreviewKeyDown);
            // 
            // restartbutton
            // 
            this.restartbutton.Location = new System.Drawing.Point(1012, 403);
            this.restartbutton.Name = "restartbutton";
            this.restartbutton.Size = new System.Drawing.Size(75, 23);
            this.restartbutton.TabIndex = 14;
            this.restartbutton.Text = "Restart";
            this.restartbutton.UseVisualStyleBackColor = true;
            this.restartbutton.Click += new System.EventHandler(this.RestartGame);


            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1117, 450);
            this.Controls.Add(this.panel1);
            this.KeyPreview = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Panel BufferedPanel;
        private System.Windows.Forms.TrackBar trackBarX;
        private System.Windows.Forms.TrackBar trackBarY;
        private System.Windows.Forms.TrackBar trackBarZ;
        private System.Windows.Forms.CheckBox Lightswitch;
        private System.Windows.Forms.CheckBox alleyLight;
        private System.Windows.Forms.CheckBox pinsLight;
        private System.Windows.Forms.Button shotbutton;
        private System.Windows.Forms.HScrollBar directionbar;
        private System.Windows.Forms.HScrollBar powerbar;
        private System.Windows.Forms.Label power;
        private System.Windows.Forms.Label direction;
        private System.Windows.Forms.Button restartbutton;
    }
}
