using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections;

//外部引用的命名空间
using DotNet.Utilities;

namespace UpdateProgram
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        #region 定义全局变量
        public static string FTPPath = "";
        public static string FTPUser = "";
        public static string FTPPassword = "";
        public static string strOldAppName = "";
        public static string strOldAppVersion = "";
        public static string strOldAppUpdateTime = "";
        public static string strOldAppMessages = "";
        public static string strNewAppName = "";
        public static string strNewAppVersion = "";
        public static string strNewAppUpdateTime = "";
        public static string strNewAppMessages = "";
        public static Process[] proc;
        public static FTPHelper ftp;
        #endregion
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            #region 检查是否存在UpdateInfo.xml文件，如果不存在则启动设置窗口，创建UpdateInfo.xml文件，并配置该文件
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory.ToString() + "UpdateInfo.xml"))
            {
                //启动设置窗口
                SettingsForm settingForm = new SettingsForm();
                settingForm.ShowDialog();
            }
            
            #endregion
            #region 连接到FTP，下载最新的XML配置文件到临时文件夹
            //从原程序目录下读取XML文件中的FTP信息,并下载最新的XML文件
            FTPPath = XMLProcess.Read("UpdateInfo.xml", "UpdateInformation", "FTPInformation", "FTPPath");
            FTPUser = XMLProcess.Read("UpdateInfo.xml", "UpdateInformation", "FTPInformation", "FTPUser");
            FTPPassword = XMLProcess.Read("UpdateInfo.xml", "UpdateInformation", "FTPInformation", "FTPPassword");
            //检查temp文件夹是否存在，并创建temp文件夹
            DirFile.CreateDir("temp");
            //将最新的XML文件下载到temp文件夹中
            ftp = new FTPHelper(FTPPath, "", FTPUser, FTPPassword);
            ftp.Download(FTPPath, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\temp", "UpdateInfo.xml");
            #endregion

            #region 获取临时文件夹XML文件中的版本更新日期，以及原程序的版本更新日期
            //获取旧版本版本号及更新时间
            strOldAppName = XMLProcess.Read("UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationName");
            strOldAppVersion = XMLProcess.Read("UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationVersion");
            strOldAppUpdateTime = XMLProcess.Read("UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationUpdateTime");
            strOldAppMessages = XMLProcess.Read("UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationMessages");
            //获取新版本版本号及更新时间
            strNewAppName = XMLProcess.Read(@"temp\UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationName");
            strNewAppVersion = XMLProcess.Read(@"temp\UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationVersion");
            strNewAppUpdateTime = XMLProcess.Read(@"temp\UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationUpdateTime");
            strNewAppMessages = XMLProcess.Read(@"temp\UpdateInfo.xml", "UpdateInformation", "ApplicationInformation", "ApplicationMessages");
            #endregion

            #region 比较两个版本的更新日期，提示用户是否升级，若选择不升级，则退出升级程序
            if (strOldAppVersion != strNewAppVersion || strOldAppUpdateTime != strNewAppUpdateTime)
            {
                VersionForm versionform = new VersionForm();
                versionform.ShowDialog();
            }
            else
            {
                Application.Exit();
            }
            #endregion

            #region 检测原程序是否在运行，若运行则杀死
            //检测原程序是否在运行，若运行则杀死。
            proc = Process.GetProcessesByName(strOldAppName);
            if (proc.Length != 0)
            {
                KillForm killform = new KillForm();
                killform.ShowDialog();
            }
            #endregion

            #region 开始批量下载文件到临时文件夹
            //下载FTP上的文件到temp文件夹
            string[] filelist = ftp.GetFileList(FTPPath);
            for (int i = 0; i < filelist.Length; i++)
            {
                ftp.Download(FTPPath, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\temp", filelist[i]);
            }
            //下载FTP上的文件夹到temp文件夹(包括文件夹下内容),如果没有文件夹,则folderlist.Length=0,不执行下列循环
            string[] folderlist = ftp.GetFolderList(FTPPath);
            for (int i = 0; i < folderlist.Length; i++)
            {
                //根据得到的文件夹列表，在temp下创建相应的文件夹
                DirFile.CreateDir(@"\temp\" + folderlist[i]);
                //获取该文件夹下的文件列表，并下载到文件夹下
                string ftpURI = FTPPath + folderlist[i] + "/";
                string[] InnerFilelist = ftp.GetFileList(ftpURI);
                for (int j = 0; j < InnerFilelist.Length; j++)
                {
                    ftp.Download(ftpURI, AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\temp\" + folderlist[i], InnerFilelist[j]);
                }
            }
            #endregion

            #region 将原程序删除(包括文件和文件夹)，拷贝临时文件夹中的文件到原目录
            //删除原程序中的文件
            string[] deletefilelist = DirFile.GetFileNames(AppDomain.CurrentDomain.BaseDirectory.ToString());
            for (int i = 0; i < deletefilelist.Length; i++)
            {
                if (!deletefilelist[i].Contains("UpdateProgram") & !deletefilelist[i].Contains("CSkin"))
                {
                    DirFile.DeleteFile(deletefilelist[i]);
                }
            }
            //删除原程序中的文件夹
            string[] deletefolderlist = DirFile.GetFolderNames(AppDomain.CurrentDomain.BaseDirectory.ToString());
            for (int i = 0; i < deletefolderlist.Length; i++)
            {
                if (!deletefolderlist[i].Contains("temp") & !deletefolderlist[i].Contains("Backup"))
                {
                    DirFile.DeleteDirectory(deletefolderlist[i]);
                }

            }
            //拷贝temp文件夹下文件到原程序目录
            string[] copyfilelist = DirFile.GetFileNames(AppDomain.CurrentDomain.BaseDirectory.ToString() + "temp");
            string[] strFileName = new string[copyfilelist.Length];
            for (int i = 0; i < copyfilelist.Length; i++)
            {
                strFileName[i] = Path.GetFileName(copyfilelist[i]);
                DirFile.CopyFile(copyfilelist[i], AppDomain.CurrentDomain.BaseDirectory.ToString() + strFileName[i]);
            }
            //拷贝temp文件夹下文件夹到原程序目录
            string[] copyfolderlist = DirFile.GetFolderNames(AppDomain.CurrentDomain.BaseDirectory.ToString() + "temp");
            string[] strFolderName = new string[copyfolderlist.Length];
            for (int i = 0; i < copyfolderlist.Length; i++)
            {
                strFolderName[i] = Path.GetFileName(copyfolderlist[i]);
                DirFile.CopyFolder(copyfolderlist[i], AppDomain.CurrentDomain.BaseDirectory.ToString() + strFolderName[i]);
            }
            #endregion

            #region 拷贝完成后，删除temp文件夹，提示更新完毕，然后启动新的主程序
            DirFile.DeleteDirectory(AppDomain.CurrentDomain.BaseDirectory.ToString() + "temp");
            Process.Start(strNewAppName + ".exe");
            #endregion

            Application.Run(new UpdateForm());
        }
    }
}
