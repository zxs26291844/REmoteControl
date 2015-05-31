using System;  
using System.Collections.Generic;  
using System.Text;  
using System.IO;  
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DotNet.Utilities
{
    public class FTPHelper
    {
        #region 字段
        string ftpURI;
        string ftpUserID;
        string ftpServerIP;
        string ftpPassword;
        string ftpRemotePath;
        #endregion

        #region 文件信息结构
        public struct FileStruct
        {
            public string Flags;
            public string Owner;
            public string Group;
            public bool IsDirectory;
            public DateTime CreateTime;
            public string Name;
        }
        public enum FileListStyle
        {
            UnixStyle,
            WindowsStyle,
            Unknown
        }
        #endregion 

        /// <summary>  
        /// 连接FTP服务器
        /// </summary>  
        /// <param name="FtpServerIP">FTP连接地址</param>  
        /// <param name="FtpUserID">用户名</param>  
        /// <param name="FtpPassword">密码</param>  
        public FTPHelper(string FtpServerIP, string FtpRemotePath, string FtpUserID, string FtpPassword)
        {
            ftpServerIP = FtpServerIP;
            ftpRemotePath = FtpRemotePath;
            ftpUserID = FtpUserID;
            ftpPassword = FtpPassword;
            ftpURI = ftpServerIP + ftpRemotePath + "/";
        }

        /// <summary>  
        /// 上传  
        /// </summary>   
        public void Upload(string FTPPath, string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(FTPPath + fileInf.Name));
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.KeepAlive = false;
            reqFTP.UseBinary = true;
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = fileInf.OpenRead();
            try
            {
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 上传文件夹，就是把foldername文件夹内容（含文件夹）上传到FTPParh下面
        /// </summary>
        /// <param name="FTPParh">上传目录</param>
        /// <param name="foldername">本地目录</param>
        public void Uploadfolder(string FTPParh,string foldername)
        {
            DirectoryInfo folderInf = new DirectoryInfo(foldername);
            //判断FTP目标目录是否存在，若不存在则创建
            if (!FolderExist(folderInf.Name))
            {
                MakeDir(folderInf.Name);
            }
            //上传文件，判断本级文件夹下是否有文件夹，如果没有文件夹则返回"";
            string[] strFolderlist = DirFile.GetFolderNames(foldername);
            foreach (string strfoldername in strFolderlist)
            {
                //即存在文件夹
                if (strfoldername != "")
                {
                    //先上传本级目录下的文件
                    string[] strfilename = DirFile.GetFileNames(foldername);
                    for (int i = 0; i < strfilename.Length; i++)
                    {
                        Upload(FTPParh + folderInf.Name + "/", strfilename[i]);
                    }
                   //再递归下载本级目录下的文件夹
                    string s = FTPParh + folderInf.Name + "/" + Path.GetFileName(strfoldername) + "/";
                    string ss = foldername + "\\" + Path.GetFileName(strfoldername);
                    Uploadfolder(FTPParh + folderInf.Name + "/" + Path.GetFileName(strfoldername) + "/", foldername + "\\" + Path.GetFileName(strfoldername));
                }
                else //不存在文件夹
                {
                    string[] strfilename = DirFile.GetFileNames(foldername);
                    for (int i = 0; i < strfilename.Length; i++)
                    {
                        Upload(FTPParh + folderInf.Name + "/", strfilename[i]);
                    }
                }
            }
        }

        /// <summary>  
        /// 下载  
        /// </summary>   
        public void Download(string ftpPath, string filePath, string fileName)
        {
            try
            {
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + fileName));
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 下载文件夹,把ftpDir下面内容下载到saveDir下面
        /// </summary>
        /// <param name="ftpDir"></param>
        /// <param name="saveDir"></param>
        public void DownLoadFolder(string ftpDir, string saveDir)
        {
            //目标目录是否存在，若不存在则创建
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            //下载文件，判断FTP本级文件夹下是否有文件夹，如果没有文件夹则返回"";
            string[] strFolderlist = GetFolderList(ftpDir);
            foreach (string strfoldername in strFolderlist)
            {
                //即存在文件夹
                if (strfoldername != "")
                {
                    //先下载本级目录下的文件
                    string[] strFilelist = GetFileList(ftpDir);
                    for (int i = 0; i < strFilelist.Length; i++)
                    {
                        Download(ftpDir + "/", saveDir + "\\", strFilelist[i]);
                    }
                    //再递归下载本级目录下的文件夹
                    DownLoadFolder(ftpDir + "/" + strfoldername, saveDir + "\\" + strfoldername);


                }
                else //不存在文件夹
                {
                    string[] strFilelist = GetFileList(ftpDir);
                    for (int i = 0; i < strFilelist.Length; i++)
                    {
                        Download(ftpDir + "/", saveDir + "\\", strFilelist[i]);
                    }
                }
            }
        }

        /// <summary>  
        /// 删除文件  
        /// </summary>  
        public void Delete(string fileName)
        {
            try
            {
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + fileName));
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                reqFTP.KeepAlive = false;
                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>  
        /// 获取当前目录下明细(包含文件和文件夹)  
        /// </summary>  
        public string[] GetFilesDetailList()
        {
            try
            {
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI));
                ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                //line = reader.ReadLine();
                //line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>  
        /// 获取FTP文件列表(包括文件夹)
        /// </summary>   
        private string[] GetAllList(string url)
        {
            List<string> list = new List<string>();
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(new Uri(url));
            req.Credentials = new NetworkCredential(ftpPassword, ftpPassword);
            req.Method = WebRequestMethods.Ftp.ListDirectory;
            req.UseBinary = true;
            req.UsePassive = true;
            try
            {
                using (FtpWebResponse res = (FtpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        string s;
                        while ((s = sr.ReadLine()) != null)
                        {
                            list.Add(s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return list.ToArray();
        }

        /// <summary>  
        /// 获取当前目录下文件列表(不包括文件夹)  
        /// </summary>  
        public string[] GetFileList(string url)
        {
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {

                    if (line.IndexOf("<DIR>") == -1)
                    {
                        result.Append(Regex.Match(line, @"[\S]+ [\S]+", RegexOptions.IgnoreCase).Value.Split(' ')[1]);
                        result.Append("\n");
                    }
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result.ToString().Split('\n');
        }

        /// <summary>
        /// 获取当前目录下文件夹
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string[] GetFolderList(string url)
        {
            try
            {
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                //line = reader.ReadLine();
                //line = reader.ReadLine();
                while (line != null)
                {
                    if (line.IndexOf("<DIR>") != -1)
                    {
                        result.Append(Regex.Match(line, @"[\S]+[\s][\s][\s][\s][\s][\s][\s][\s][\s][\s][\S]+", RegexOptions.IgnoreCase).Value.Split(new string[]{"          "},StringSplitOptions.None )[1]);
                        result.Append("\n");
                    }
                    line = reader.ReadLine();
                }
                if (result.ToString() != "")
                {
                    result.Remove(result.ToString().LastIndexOf("\n"), 1);
                }
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>  
        /// 判断当前目录下指定的文件是否存在  
        /// </summary>  
        /// <param name="RemoteFileName">远程文件名</param>  
        public bool FileExist(string RemoteFileName)
        {
            string[] fileList = GetFileList("*.*");
            foreach (string str in fileList)
            {
                if (str.Trim() == RemoteFileName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>  
        /// 创建文件夹  
        /// </summary>   
        public void MakeDir(string dirName)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + dirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        ///判断FTP上文件夹是否存在 
        /// </summary>
        /// <param name="foldername"></param>
        /// <returns></returns>
        public bool FolderExist(string foldername)
        {
            string[] strfolderlist = GetFolderList(ftpURI);
            foreach (string str in strfolderlist)
            {
                if (str.Trim() == foldername.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>  
        /// 获取指定文件大小  
        /// </summary>  
        public long GetFileSize(string filename)
        {
            FtpWebRequest reqFTP;
            long fileSize = 0;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + filename));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            { }
            return fileSize;
        }

        /// <summary>  
        /// 更改文件名  
        /// </summary> 
        public void ReName(string currentFilename, string newFilename)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + currentFilename));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            { }
        }

        /// <summary>  
        /// 移动文件  
        /// </summary>  
        public void MovieFile(string currentFilename, string newDirectory)
        {
            ReName(currentFilename, newDirectory);
        }

        /// <summary>  
        /// 切换当前目录  
        /// </summary>  
        /// <param name="IsRoot">true:绝对路径 false:相对路径</param>   
        public void GotoDirectory(string DirectoryName, bool IsRoot)
        {
            if (IsRoot)
            {
                ftpRemotePath = DirectoryName;
            }
            else
            {
                ftpRemotePath += DirectoryName + "/";
            }
            ftpURI = "ftp://" + ftpServerIP + "/" + ftpRemotePath + "/";
        }

        #region 从FTP上下载整个文件夹，包括文件夹下的文件和文件夹
        /// <summary>  
        /// 从FTP下载文件到本地服务器,支持断点下载  
        /// </summary>  
        /// <param name="ftpUri">ftp文件路径，如"ftp://localhost/test.txt"</param>  
        /// <param name="saveFile">保存文件的路径，如C:\\test.txt</param>  
        public void BreakPointDownLoadFile(string ftpUri, string saveFile)
        {
            System.IO.FileStream fs = null;
            System.Net.FtpWebResponse ftpRes = null;
            System.IO.Stream resStrm = null;
            try
            {
                //下载文件的URI  
                Uri u = new Uri(ftpUri);
                //设定下载文件的保存路径  
                string downFile = saveFile;

                //FtpWebRequest的作成  
                System.Net.FtpWebRequest ftpReq = (System.Net.FtpWebRequest)
                    System.Net.WebRequest.Create(u);
                //设定用户名和密码  
                ftpReq.Credentials = new System.Net.NetworkCredential(ftpUserID, ftpPassword);
                //MethodにWebRequestMethods.Ftp.DownloadFile("RETR")设定  
                ftpReq.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
                //要求终了后关闭连接  
                ftpReq.KeepAlive = false;
                //使用ASCII方式传送  
                ftpReq.UseBinary = false;
                //设定PASSIVE方式无效  
                ftpReq.UsePassive = false;

                //判断是否继续下载  
                //继续写入下载文件的FileStream  

                if (System.IO.File.Exists(downFile))
                {
                    //继续下载  
                    ftpReq.ContentOffset = (new System.IO.FileInfo(downFile)).Length;
                    fs = new System.IO.FileStream(
                       downFile, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                }
                else
                {
                    //一般下载  
                    fs = new System.IO.FileStream(
                        downFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                }

                //取得FtpWebResponse  
                ftpRes = (System.Net.FtpWebResponse)ftpReq.GetResponse();
                //为了下载文件取得Stream  
                resStrm = ftpRes.GetResponseStream();
                //写入下载的数据  
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int readSize = resStrm.Read(buffer, 0, buffer.Length);
                    if (readSize == 0)
                        break;
                    fs.Write(buffer, 0, readSize);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("从ftp服务器下载文件出错，文件名：" + ftpUri + "异常信息：" + ex.ToString());
            }
            finally
            {
                fs.Close();
                resStrm.Close();
                ftpRes.Close();
            }
        }

        #endregion
    }
}