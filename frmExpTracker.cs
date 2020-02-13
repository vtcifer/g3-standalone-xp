using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EXPTracker
{
    public partial class frmEXPTracker : Form
    {
        private GeniePlugin.Interfaces.IHost _host;

        public frmEXPTracker(ref GeniePlugin.Interfaces.IHost host)
        {
            InitializeComponent();

            _host = host;
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cbEnable.Checked == true)
                _host.SendText("#var ExpTracker.Window 1");
            else
                _host.SendText("#var ExpTracker.Window 0");

            if (cbRankGain.Checked == true)
                _host.SendText("#var ExpTracker.ShowRankGain 1");
            else
                _host.SendText("#var ExpTracker.ShowRankGain 0");

            if (cbLearningRate.Checked == true)
                _host.SendText("#var ExpTracker.LearningRate 1");
            else
                _host.SendText("#var ExpTracker.LearningRate 0");

            if (cbLearningRateNumber.Checked == true)
                _host.SendText("#var ExpTracker.LearningRateNumber 1");
            else
                _host.SendText("#var ExpTracker.LearningRateNumber 0");

            if (cbTrackSleep.Checked == true)
                _host.SendText("#var ExpTracker.TrackSleep 1");
            else
                _host.SendText("#var ExpTracker.TrackSleep 0");
            if (cbEchoSleep.Checked == true)
                _host.SendText("#var ExpTracker.EchoSleep 1");
            else
                _host.SendText("#var ExpTracker.EchoSleep 0");

            if (cbGagExp.Checked == true)
                _host.SendText("#var ExpTracker.GagExp 1");
            else
                _host.SendText("#var ExpTracker.GagExp 0");

            if (cbShort.Checked == true)
                _host.SendText("#var ExpTracker.ShortNames 1");
            else
                _host.SendText("#var ExpTracker.ShortNames 0");

            if (cbPersistent.Checked == true)
                _host.SendText("#var ExpTracker.Persistent 1");
            else
                _host.SendText("#var ExpTracker.Persistent 0");

            if (cbCountSkills.Checked == true)
                _host.SendText("#var ExpTracker.CountSkills 1");
            else
                _host.SendText("#var ExpTracker.CountSkills 0");
            _host.SendText("#var ExpTracker.CountMinMindstate " + updownMinMindstate.Value.ToString());
            

            if (comboExpSort.Text == "A to Z")
                _host.SendText("#var ExpTracker.SortType 0");
            else if (comboExpSort.Text == "Left to Right")
                _host.SendText("#var ExpTracker.SortType 1");
            else if (comboExpSort.Text == "Learning Rate")
                _host.SendText("#var ExpTracker.SortType 2");
            else
                _host.SendText("#var ExpTracker.SortType 3");

            if (comboReportSort.Text == "A to Z")
                _host.SendText("#var ExpTracker.ReportType 0");
            else if (comboReportSort.Text == "Left to Right")
                _host.SendText("#var ExpTracker.ReportType 1");
            else if (comboReportSort.Text == "Learning Rate")
                _host.SendText("#var ExpTracker.ReportType 2");
            else
                _host.SendText("#var ExpTracker.ReportType 3");

            _host.SendText("#var {ExpTracker.Color.RankGained} {" + txtRankGained.Text + "}");
            _host.SendText("#var {ExpTracker.Color.Learned} {" + txtLearned.Text + "}");
            _host.SendText("#var {ExpTracker.Color.Normal} {" + txtNormal.Text + "}");

            if (cbEchoExp.Checked == true)
                _host.SendText("#var {ExpTracker.EchoExp} {1}");
            else
                _host.SendText("#var {ExpTracker.EchoExp} {0}");

            _host.SendText("#var {ExpTracker.Color.EchoGained} {" + txtEchoGain.Text + "}");
            _host.SendText("#var {ExpTracker.Color.EchoPulsed} {" + txtEchoPulse.Text + "}");

            _host.SendText("#var save");

            this.Close();
        }

        public string ColorToHex(Color oColor)
        {
            return "#" + Hex2Digits(oColor.R) + Hex2Digits(oColor.G) + Hex2Digits(oColor.B);
        }

        public string ColorToString(Color oColor)
        {
            if (oColor.IsNamedColor)
            {
                return oColor.Name;
            }

            return ColorToHex(oColor);
        }

        private static string Hex2Digits(byte color)
        {
            string sColor = color.ToString("X");
            if (sColor.Length == 1)
            {
                sColor = "0" + sColor;
            }
            return sColor;
        }

        private void btnLearned_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                txtLearned.Text = ColorToString(colorDialog.Color);
                lblLearned.ForeColor = colorDialog.Color;
            }
        }

        private void btnRankGained_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                txtRankGained.Text = ColorToString(colorDialog.Color);
                lblRankGained.ForeColor = colorDialog.Color;
            }
        }

        private void btnNormal_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                txtNormal.Text = ColorToString(colorDialog.Color);
                lblNormal.ForeColor = colorDialog.Color;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Color color;
            color = Color.FromName(txtNormal.Text);
            if (color.ToArgb() != 0)
                lblNormal.ForeColor = color;
            else if (txtNormal.Text.StartsWith("#") && txtNormal.Text.Length == 7)
                lblNormal.ForeColor = System.Drawing.ColorTranslator.FromHtml(txtNormal.Text);

            color = Color.FromName(txtRankGained.Text);
            if (color.ToArgb() != 0)
                lblRankGained.ForeColor = color;
            else if (txtRankGained.Text.StartsWith("#") && txtRankGained.Text.Length == 7)
                lblRankGained.ForeColor = System.Drawing.ColorTranslator.FromHtml(txtRankGained.Text);

            color = Color.FromName(txtLearned.Text);
            if (color.ToArgb() != 0)
                lblLearned.ForeColor = color;
            else if (txtLearned.Text.StartsWith("#") && txtLearned.Text.Length == 7)
                lblLearned.ForeColor = System.Drawing.ColorTranslator.FromHtml(txtLearned.Text);
            cbEnable_CheckedChanged(sender,e);

            color = Color.FromName(txtEchoGain.Text);
            if (color.ToArgb() != 0)
                lblColorExpEchoGain.ForeColor = color;
            else if (txtEchoGain.Text.StartsWith("#") && txtEchoGain.Text.Length == 7)
                lblColorExpEchoGain.ForeColor = System.Drawing.ColorTranslator.FromHtml(txtEchoGain.Text);

            color = Color.FromName(txtEchoPulse.Text);
            if (color.ToArgb() != 0)
                lblColorExpEchoPulse.ForeColor = color;
            else if (txtEchoPulse.Text.StartsWith("#") && txtEchoPulse.Text.Length == 7)
                lblColorExpEchoPulse.ForeColor = System.Drawing.ColorTranslator.FromHtml(txtEchoPulse.Text);
            cbEchoExp_CheckedChanged(sender, e);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnable.Checked == true)
            {
                cbRankGain.Enabled = true;
                cbLearningRate.Enabled = true;
                cbLearningRateNumber.Enabled = true;
                cbTrackSleep.Enabled = true;
                if (cbTrackSleep.Checked == true)
                    cbEchoSleep.Enabled = true;
                else
                    cbEchoSleep.Enabled = false;
                cbGagExp.Enabled = true;
                cbShort.Enabled = true;
                cbPersistent.Enabled = true;

                comboExpSort.Enabled = true;
                comboReportSort.Enabled = true;
                txtNormal.Enabled = true;
                
                btnNormal.Enabled = true;
                txtRankGained.Enabled = true;
                btnRankGained.Enabled = true;
                txtLearned.Enabled = true;
                btnLearned.Enabled = true;
            }
            else
            {
                cbRankGain.Enabled = false;
                cbLearningRate.Enabled = false;
                cbLearningRateNumber.Enabled = false;
                cbTrackSleep.Enabled = false;
                cbEchoSleep.Enabled = false;
                cbGagExp.Enabled = false;
                cbShort.Enabled = false;
                cbPersistent.Enabled = false;

                comboExpSort.Enabled = false;
                comboReportSort.Enabled = false;
                txtNormal.Enabled = false;
                btnNormal.Enabled = false;
                txtRankGained.Enabled = false;
                btnRankGained.Enabled = false;
                txtLearned.Enabled = false;
                btnLearned.Enabled = false;
            }
        }

        private void cbTrackSleep_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTrackSleep.Checked == true)
                cbEchoSleep.Enabled = true;
            else
                cbEchoSleep.Enabled = false;
        }

        private void CbCountSkills_CheckedChanged(object sender, EventArgs e)
        {
            if (cbCountSkills.Checked == true)
                updownMinMindstate.Enabled = true;
            else
                updownMinMindstate.Enabled = false;
        }

        private void btnColorExpEchoGain_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                txtEchoGain.Text = ColorToString(colorDialog.Color);
                lblColorExpEchoGain.ForeColor = colorDialog.Color;
            }
        }

        private void btnColorExpEchoPulse_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                txtEchoPulse.Text = ColorToString(colorDialog.Color);
                lblColorExpEchoPulse.ForeColor = colorDialog.Color;
            }
        }

        private void cbEchoExp_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEchoExp.Checked == true)
            {
                txtEchoGain.Enabled = true;
                lblColorExpEchoGain.Enabled = true;
                btnColorExpEchoGain.Enabled = true;

                txtEchoPulse.Enabled = true;
                lblColorExpEchoPulse.Enabled = true;
                btnColorExpEchoPulse.Enabled = true;
            }
            else
            {
                txtEchoGain.Enabled = false;
                lblColorExpEchoGain.Enabled = false;
                btnColorExpEchoGain.Enabled = false;

                txtEchoPulse.Enabled = false;
                lblColorExpEchoPulse.Enabled = false;
                btnColorExpEchoPulse.Enabled = false;

            }
        }
    }
}
