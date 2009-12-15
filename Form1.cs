using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Standalone_EXPTracker
{
    public partial class Form1 : Form
    {
        private GeniePlugin.Interfaces.IHost _host;

        public Form1(ref GeniePlugin.Interfaces.IHost host)
        {
            InitializeComponent();

            _host = host;
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cbLearningRate.Checked == true)
                _host.set_Variable("ExpTracker.LearningRate", "1");
            else
                _host.set_Variable("ExpTracker.LearningRate", "0");

            if (cbLearningRateNumber.Checked == true)
                _host.set_Variable("ExpTracker.LearningRateNumber", "1");
            else
                _host.set_Variable("ExpTracker.LearningRateNumber", "0");

            if (cbEnable.Checked == true)
                _host.set_Variable("ExpTracker.Window", "1");
            else
                _host.set_Variable("ExpTracker.Window", "0");

            if (cbRankGain.Checked == true)
                _host.set_Variable("ExpTracker.ShowRankGain", "1");
            else
                _host.set_Variable("ExpTracker.ShowRankGain", "0");

            if (cbGagExp.Checked == true)
                _host.set_Variable("ExpTracker.GagExp", "1");
            else
                _host.set_Variable("ExpTracker.GagExp", "0");

            if (comboSort.Text == "A to Z")
                _host.set_Variable("ExpTracker.SortType", "0");
            else if (comboSort.Text == "Left to Right")
                _host.set_Variable("ExpTracker.SortType", "1");
            else
                _host.set_Variable("ExpTracker.SortType", "2");

            _host.set_Variable("ExpTracker.Color.RankGained", txtRankGained.Text);
            _host.set_Variable("ExpTracker.Color.Learned", txtLearned.Text);
            _host.set_Variable("ExpTracker.Color.Normal", txtNormal.Text);

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
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
