using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using UpdateProgram;
//外部引用的命名空间
using DotNet.Utilities;

namespace UpdateProgram
{
    public partial class KillForm : Form
    {
        public KillForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            foreach (Process pro in Program.proc) 
            {
                pro.Kill(); 
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请关闭原程序后，再更新！","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
            Application.Exit();
        }
    }
}
