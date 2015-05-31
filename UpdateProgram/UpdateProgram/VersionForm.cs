using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//外部引用的命名空间
using DotNet.Utilities;

namespace UpdateProgram
{
    public partial class VersionForm : Form
    {
        public VersionForm()
        {
            InitializeComponent();
        }

        private void VersionForm_Load(object sender, EventArgs e)
        {
            string strNewAppName = XMLProcess.Read(@"temp\UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationName");
            string strNewAppVersion = XMLProcess.Read(@"temp\UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationVersion");
            string strNewAppMessages = XMLProcess.Read(@"temp\UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationMessages");
            string[] str = strNewAppMessages.Split(new char[] { '；' });
            lblAppName_Version.Text = strNewAppName + " " + strNewAppVersion;
            for (int i = 0; i < str.Length; i++)
            {
                txtUpdateInfo.Text =txtUpdateInfo.Text+ str[i] + Environment.NewLine;
            }
            txtUpdateInfo.SelectionStart = 0;
            txtUpdateInfo.SelectionLength = 0;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.Hide();
            BackupForm backform = new BackupForm();
            backform.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
