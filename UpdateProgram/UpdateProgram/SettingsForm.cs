using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DotNet.Utilities;

namespace UpdateProgram
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openfiledialog = new FolderBrowserDialog();
            openfiledialog.Description = "请选择文件路径";
            if (openfiledialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = openfiledialog.SelectedPath;
                txtFolderPath.Text = foldPath;
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            string[] strfilename = DirFile.GetFileNames(txtFolderPath.Text);
            string[] strfoldername = DirFile.GetFolderNames(txtFolderPath.Text);
            FTPHelper ftp = new FTPHelper(txtFTPPath.Text, "", txtFTPUser.Text, txtFTPPassword.Text);
            ftp.Uploadfolder(txtFTPPath.Text, txtFolderPath.Text);
            for (int i = 0; i < strfilename.Length; i++)
            {
                ftp.Upload(txtFTPPath.Text, strfilename[i]);
            }
        }

        private void btnCreateXML_Click(object sender, EventArgs e)
        {

        }
    }
}
