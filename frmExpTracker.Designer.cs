namespace EXPTracker
{
    partial class frmEXPTracker
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
            this.cbEnable = new System.Windows.Forms.CheckBox();
            this.cbRankGain = new System.Windows.Forms.CheckBox();
            this.cbGagExp = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtRankGained = new System.Windows.Forms.TextBox();
            this.txtLearned = new System.Windows.Forms.TextBox();
            this.lblRankColor = new System.Windows.Forms.Label();
            this.lblFreshColor = new System.Windows.Forms.Label();
            this.btnRankGained = new System.Windows.Forms.Button();
            this.lblRankGained = new System.Windows.Forms.Label();
            this.lblLearned = new System.Windows.Forms.Label();
            this.btnLearned = new System.Windows.Forms.Button();
            this.lblNormal = new System.Windows.Forms.Label();
            this.btnNormal = new System.Windows.Forms.Button();
            this.lblNormalColor = new System.Windows.Forms.Label();
            this.txtNormal = new System.Windows.Forms.TextBox();
            this.cbLearningRateNumber = new System.Windows.Forms.CheckBox();
            this.cbLearningRate = new System.Windows.Forms.CheckBox();
            this.comboExpSort = new System.Windows.Forms.ComboBox();
            this.lblSort = new System.Windows.Forms.Label();
            this.cbTrackSleep = new System.Windows.Forms.CheckBox();
            this.cbEchoSleep = new System.Windows.Forms.CheckBox();
            this.cbShort = new System.Windows.Forms.CheckBox();
            this.cbPersistent = new System.Windows.Forms.CheckBox();
            this.lblReportSort = new System.Windows.Forms.Label();
            this.comboReportSort = new System.Windows.Forms.ComboBox();
            this.cbCountSkills = new System.Windows.Forms.CheckBox();
            this.updownMinMindstate = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.updownMinMindstate)).BeginInit();
            this.SuspendLayout();
            // 
            // cbEnable
            // 
            this.cbEnable.AutoSize = true;
            this.cbEnable.Location = new System.Drawing.Point(201, 8);
            this.cbEnable.Name = "cbEnable";
            this.cbEnable.Size = new System.Drawing.Size(157, 17);
            this.cbEnable.TabIndex = 1;
            this.cbEnable.Text = "Enable Experience Window";
            this.cbEnable.UseVisualStyleBackColor = true;
            this.cbEnable.CheckedChanged += new System.EventHandler(this.cbEnable_CheckedChanged);
            // 
            // cbRankGain
            // 
            this.cbRankGain.AutoSize = true;
            this.cbRankGain.Location = new System.Drawing.Point(32, 32);
            this.cbRankGain.Name = "cbRankGain";
            this.cbRankGain.Size = new System.Drawing.Size(101, 17);
            this.cbRankGain.TabIndex = 2;
            this.cbRankGain.Text = "Track rank gain";
            this.cbRankGain.UseVisualStyleBackColor = true;
            // 
            // cbGagExp
            // 
            this.cbGagExp.AutoSize = true;
            this.cbGagExp.Location = new System.Drawing.Point(32, 150);
            this.cbGagExp.Name = "cbGagExp";
            this.cbGagExp.Size = new System.Drawing.Size(180, 17);
            this.cbGagExp.TabIndex = 8;
            this.cbGagExp.Text = "Gag experience in game window";
            this.cbGagExp.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(211, 253);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 19;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(301, 253);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtRankGained
            // 
            this.txtRankGained.Location = new System.Drawing.Point(311, 149);
            this.txtRankGained.Name = "txtRankGained";
            this.txtRankGained.Size = new System.Drawing.Size(100, 20);
            this.txtRankGained.TabIndex = 15;
            // 
            // txtLearned
            // 
            this.txtLearned.Location = new System.Drawing.Point(310, 192);
            this.txtLearned.Name = "txtLearned";
            this.txtLearned.Size = new System.Drawing.Size(100, 20);
            this.txtLearned.TabIndex = 17;
            // 
            // lblRankColor
            // 
            this.lblRankColor.Location = new System.Drawing.Point(311, 130);
            this.lblRankColor.Name = "lblRankColor";
            this.lblRankColor.Size = new System.Drawing.Size(255, 16);
            this.lblRankColor.TabIndex = 9;
            this.lblRankColor.Text = "Gained rank color";
            this.lblRankColor.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblFreshColor
            // 
            this.lblFreshColor.Location = new System.Drawing.Point(311, 173);
            this.lblFreshColor.Name = "lblFreshColor";
            this.lblFreshColor.Size = new System.Drawing.Size(255, 19);
            this.lblFreshColor.TabIndex = 10;
            this.lblFreshColor.Text = "Fresh experience color";
            this.lblFreshColor.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnRankGained
            // 
            this.btnRankGained.Location = new System.Drawing.Point(505, 149);
            this.btnRankGained.Name = "btnRankGained";
            this.btnRankGained.Size = new System.Drawing.Size(61, 22);
            this.btnRankGained.TabIndex = 16;
            this.btnRankGained.Text = "Color...";
            this.btnRankGained.UseVisualStyleBackColor = true;
            this.btnRankGained.Click += new System.EventHandler(this.btnRankGained_Click);
            // 
            // lblRankGained
            // 
            this.lblRankGained.BackColor = System.Drawing.Color.Black;
            this.lblRankGained.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRankGained.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblRankGained.Location = new System.Drawing.Point(416, 149);
            this.lblRankGained.Name = "lblRankGained";
            this.lblRankGained.Size = new System.Drawing.Size(83, 20);
            this.lblRankGained.TabIndex = 12;
            this.lblRankGained.Text = "Color";
            this.lblRankGained.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLearned
            // 
            this.lblLearned.BackColor = System.Drawing.Color.Black;
            this.lblLearned.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLearned.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblLearned.Location = new System.Drawing.Point(416, 192);
            this.lblLearned.Name = "lblLearned";
            this.lblLearned.Size = new System.Drawing.Size(83, 20);
            this.lblLearned.TabIndex = 13;
            this.lblLearned.Text = "Color";
            this.lblLearned.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLearned
            // 
            this.btnLearned.Location = new System.Drawing.Point(505, 192);
            this.btnLearned.Name = "btnLearned";
            this.btnLearned.Size = new System.Drawing.Size(61, 22);
            this.btnLearned.TabIndex = 18;
            this.btnLearned.Text = "Color...";
            this.btnLearned.UseVisualStyleBackColor = true;
            this.btnLearned.Click += new System.EventHandler(this.btnLearned_Click);
            // 
            // lblNormal
            // 
            this.lblNormal.BackColor = System.Drawing.Color.Black;
            this.lblNormal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNormal.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblNormal.Location = new System.Drawing.Point(416, 106);
            this.lblNormal.Name = "lblNormal";
            this.lblNormal.Size = new System.Drawing.Size(83, 20);
            this.lblNormal.TabIndex = 18;
            this.lblNormal.Text = "Color";
            this.lblNormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNormal
            // 
            this.btnNormal.Location = new System.Drawing.Point(505, 106);
            this.btnNormal.Name = "btnNormal";
            this.btnNormal.Size = new System.Drawing.Size(61, 22);
            this.btnNormal.TabIndex = 14;
            this.btnNormal.Text = "Color...";
            this.btnNormal.UseVisualStyleBackColor = true;
            this.btnNormal.Click += new System.EventHandler(this.btnNormal_Click);
            // 
            // lblNormalColor
            // 
            this.lblNormalColor.Location = new System.Drawing.Point(308, 90);
            this.lblNormalColor.Name = "lblNormalColor";
            this.lblNormalColor.Size = new System.Drawing.Size(258, 13);
            this.lblNormalColor.TabIndex = 16;
            this.lblNormalColor.Text = "Normal color";
            this.lblNormalColor.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtNormal
            // 
            this.txtNormal.Location = new System.Drawing.Point(311, 106);
            this.txtNormal.Name = "txtNormal";
            this.txtNormal.Size = new System.Drawing.Size(100, 20);
            this.txtNormal.TabIndex = 13;
            // 
            // cbLearningRateNumber
            // 
            this.cbLearningRateNumber.AutoSize = true;
            this.cbLearningRateNumber.Location = new System.Drawing.Point(32, 80);
            this.cbLearningRateNumber.Name = "cbLearningRateNumber";
            this.cbLearningRateNumber.Size = new System.Drawing.Size(216, 17);
            this.cbLearningRateNumber.TabIndex = 4;
            this.cbLearningRateNumber.Text = "Show LearningRate numbers e.g. (4/34)";
            this.cbLearningRateNumber.UseVisualStyleBackColor = true;
            // 
            // cbLearningRate
            // 
            this.cbLearningRate.AutoSize = true;
            this.cbLearningRate.Location = new System.Drawing.Point(32, 56);
            this.cbLearningRate.Name = "cbLearningRate";
            this.cbLearningRate.Size = new System.Drawing.Size(154, 17);
            this.cbLearningRate.TabIndex = 3;
            this.cbLearningRate.Text = "Show LearningRate names";
            this.cbLearningRate.UseVisualStyleBackColor = true;
            // 
            // comboExpSort
            // 
            this.comboExpSort.AutoCompleteCustomSource.AddRange(new string[] {
            "A to Z",
            "Left to Right",
            "Learning Rate",
            "Learning Rate Reverse"});
            this.comboExpSort.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboExpSort.FormattingEnabled = true;
            this.comboExpSort.Items.AddRange(new object[] {
            "A to Z",
            "Left to Right",
            "Learning Rate",
            "Learning Rate Reverse"});
            this.comboExpSort.Location = new System.Drawing.Point(399, 32);
            this.comboExpSort.MaxDropDownItems = 3;
            this.comboExpSort.Name = "comboExpSort";
            this.comboExpSort.Size = new System.Drawing.Size(167, 21);
            this.comboExpSort.TabIndex = 11;
            this.comboExpSort.Text = "A to Z";
            // 
            // lblSort
            // 
            this.lblSort.AutoSize = true;
            this.lblSort.Location = new System.Drawing.Point(311, 32);
            this.lblSort.Name = "lblSort";
            this.lblSort.Size = new System.Drawing.Size(85, 13);
            this.lblSort.TabIndex = 22;
            this.lblSort.Text = "Experience Sort:";
            // 
            // cbTrackSleep
            // 
            this.cbTrackSleep.AutoSize = true;
            this.cbTrackSleep.Location = new System.Drawing.Point(32, 104);
            this.cbTrackSleep.Name = "cbTrackSleep";
            this.cbTrackSleep.Size = new System.Drawing.Size(96, 17);
            this.cbTrackSleep.TabIndex = 5;
            this.cbTrackSleep.Text = "Track sleeping";
            this.cbTrackSleep.UseVisualStyleBackColor = true;
            this.cbTrackSleep.CheckedChanged += new System.EventHandler(this.cbTrackSleep_CheckedChanged);
            // 
            // cbEchoSleep
            // 
            this.cbEchoSleep.AutoSize = true;
            this.cbEchoSleep.Location = new System.Drawing.Point(40, 126);
            this.cbEchoSleep.Name = "cbEchoSleep";
            this.cbEchoSleep.Size = new System.Drawing.Size(161, 17);
            this.cbEchoSleep.TabIndex = 6;
            this.cbEchoSleep.Text = "Echo to Experience Window";
            this.cbEchoSleep.UseVisualStyleBackColor = true;
            // 
            // cbShort
            // 
            this.cbShort.AutoSize = true;
            this.cbShort.Location = new System.Drawing.Point(32, 174);
            this.cbShort.Name = "cbShort";
            this.cbShort.Size = new System.Drawing.Size(218, 17);
            this.cbShort.TabIndex = 9;
            this.cbShort.Text = "Use short names for Experience Window";
            this.cbShort.UseVisualStyleBackColor = true;
            // 
            // cbPersistent
            // 
            this.cbPersistent.AutoSize = true;
            this.cbPersistent.Location = new System.Drawing.Point(32, 198);
            this.cbPersistent.Name = "cbPersistent";
            this.cbPersistent.Size = new System.Drawing.Size(204, 17);
            this.cbPersistent.TabIndex = 10;
            this.cbPersistent.Text = "Make Experience Variables Persistent";
            this.cbPersistent.UseVisualStyleBackColor = true;
            // 
            // lblReportSort
            // 
            this.lblReportSort.AutoSize = true;
            this.lblReportSort.Location = new System.Drawing.Point(311, 61);
            this.lblReportSort.Name = "lblReportSort";
            this.lblReportSort.Size = new System.Drawing.Size(64, 13);
            this.lblReportSort.TabIndex = 26;
            this.lblReportSort.Text = "Report Sort:";
            // 
            // comboReportSort
            // 
            this.comboReportSort.AutoCompleteCustomSource.AddRange(new string[] {
            "A to Z",
            "Left to Right",
            "Learning Rate",
            "Learning Rate Reverse"});
            this.comboReportSort.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboReportSort.FormattingEnabled = true;
            this.comboReportSort.Items.AddRange(new object[] {
            "A to Z",
            "Left to Right",
            "Learning Rate",
            "Learning Rate Reverse"});
            this.comboReportSort.Location = new System.Drawing.Point(399, 61);
            this.comboReportSort.MaxDropDownItems = 3;
            this.comboReportSort.Name = "comboReportSort";
            this.comboReportSort.Size = new System.Drawing.Size(166, 21);
            this.comboReportSort.TabIndex = 12;
            this.comboReportSort.Text = "A to Z";
            // 
            // cbCountSkills
            // 
            this.cbCountSkills.AutoSize = true;
            this.cbCountSkills.Location = new System.Drawing.Point(32, 221);
            this.cbCountSkills.Name = "cbCountSkills";
            this.cbCountSkills.Size = new System.Drawing.Size(216, 17);
            this.cbCountSkills.TabIndex = 27;
            this.cbCountSkills.Text = "Count active skills >                 mindstate";
            this.cbCountSkills.UseVisualStyleBackColor = true;
            this.cbCountSkills.CheckedChanged += new System.EventHandler(this.CbCountSkills_CheckedChanged);
            // 
            // updownMinMindstate
            // 
            this.updownMinMindstate.Location = new System.Drawing.Point(152, 218);
            this.updownMinMindstate.Maximum = new decimal(new int[] {
            33,
            0,
            0,
            0});
            this.updownMinMindstate.Name = "updownMinMindstate";
            this.updownMinMindstate.Size = new System.Drawing.Size(38, 20);
            this.updownMinMindstate.TabIndex = 28;
            this.updownMinMindstate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // frmEXPTracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 306);
            this.Controls.Add(this.updownMinMindstate);
            this.Controls.Add(this.cbCountSkills);
            this.Controls.Add(this.comboReportSort);
            this.Controls.Add(this.lblReportSort);
            this.Controls.Add(this.cbPersistent);
            this.Controls.Add(this.cbShort);
            this.Controls.Add(this.cbEchoSleep);
            this.Controls.Add(this.cbTrackSleep);
            this.Controls.Add(this.lblSort);
            this.Controls.Add(this.comboExpSort);
            this.Controls.Add(this.cbLearningRate);
            this.Controls.Add(this.cbLearningRateNumber);
            this.Controls.Add(this.lblNormal);
            this.Controls.Add(this.btnNormal);
            this.Controls.Add(this.lblNormalColor);
            this.Controls.Add(this.txtNormal);
            this.Controls.Add(this.btnLearned);
            this.Controls.Add(this.lblLearned);
            this.Controls.Add(this.lblRankGained);
            this.Controls.Add(this.btnRankGained);
            this.Controls.Add(this.lblFreshColor);
            this.Controls.Add(this.lblRankColor);
            this.Controls.Add(this.txtLearned);
            this.Controls.Add(this.txtRankGained);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cbGagExp);
            this.Controls.Add(this.cbRankGain);
            this.Controls.Add(this.cbEnable);
            this.Name = "frmEXPTracker";
            this.Text = "Experience Tracker";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.updownMinMindstate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.CheckBox cbEnable;
        public System.Windows.Forms.CheckBox cbRankGain;
        public System.Windows.Forms.CheckBox cbGagExp;
        private System.Windows.Forms.Label lblRankColor;
        private System.Windows.Forms.Button btnRankGained;
        private System.Windows.Forms.Label lblRankGained;
        private System.Windows.Forms.Label lblLearned;
        private System.Windows.Forms.Button btnLearned;
        private System.Windows.Forms.Label lblNormal;
        private System.Windows.Forms.Button btnNormal;
        public System.Windows.Forms.Label lblNormalColor;
        public System.Windows.Forms.TextBox txtNormal;
        public System.Windows.Forms.TextBox txtRankGained;
        protected System.Windows.Forms.Label lblFreshColor;
        public System.Windows.Forms.TextBox txtLearned;
        public System.Windows.Forms.CheckBox cbLearningRateNumber;
        public System.Windows.Forms.CheckBox cbLearningRate;
        private System.Windows.Forms.Label lblSort;
        public System.Windows.Forms.ComboBox comboExpSort;
        public System.Windows.Forms.CheckBox cbTrackSleep;
        public System.Windows.Forms.CheckBox cbEchoSleep;
        public System.Windows.Forms.CheckBox cbShort;
        public System.Windows.Forms.CheckBox cbPersistent;
        private System.Windows.Forms.Label lblReportSort;
        public System.Windows.Forms.ComboBox comboReportSort;
        public System.Windows.Forms.CheckBox cbCountSkills;
        public System.Windows.Forms.NumericUpDown updownMinMindstate;
    }
}