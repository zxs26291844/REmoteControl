using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;

//外部引用的命名空间
using DotNet.Utilities;

namespace UpdateProgram
{
    public partial class BackupForm : Form
    {
        public BackupForm()
        {
            InitializeComponent();
            #region 若选择升级，则获取更新日志，提示用户是否保存原版本

            #endregion
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            try 
            {
                DirFile.CreateDir("Backup");
                //备份文件
                string[] strFileName = DirFile.GetFileNames(AppDomain.CurrentDomain.BaseDirectory.ToString());
                for (int i = 0; i < strFileName.Length; i++)
                {
                    strFileName[i] = Path.GetFileName(strFileName[i].ToString());
                    string strTempPath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Backup" + @"\" + strFileName[i];
                    DirFile.CopyFile(strFileName[i].ToString(), strTempPath);
                }
                //备份文件夹
                string[] strFolderlist = DirFile.GetFolderNames(AppDomain.CurrentDomain.BaseDirectory.ToString());
                string[] strFolderName = new string[strFolderlist.Length];
                for (int i = 0; i < strFolderlist.Length; i++)
                {
                    
                    strFolderName[i] = Path.GetFileName(strFolderlist[i]);
                    if (strFolderName[i] != "Backup" & strFolderName[i] != "temp")
                    {
                        DirFile.CopyFolder(strFolderlist[i], AppDomain.CurrentDomain.BaseDirectory.ToString() + "Backup" + @"\" + strFolderName[i]);
                    }
                    
                }
                MessageBox.Show("备份完毕！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
