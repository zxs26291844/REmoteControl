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
        public void Upload(string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + fileInf.Name));
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
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI));
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
            { }
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
        /// 列出FTP服务器上面当前目录的所有文件和目录  
        /// </summary>  
        /// <param name="ftpUri">FTP目录</param>  
        /// <returns></returns>  
        public List<FileStruct> ListFilesAndDirectories()
        {
            WebResponse webresp = null;
            StreamReader ftpFileListReader = null;
            FtpWebRequest ftpRequest = null;
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI));
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                ftpRequest.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                webresp = ftpRequest.GetResponse();
                ftpFileListReader = new StreamReader(webresp.GetResponseStream(), Encoding.Default);
            }
            catch (Exception ex)
            {
                throw new Exception("获取文件列表出错，错误信息如下：" + ex.ToString());
            }
            string Datastring = ftpFileListReader.ReadToEnd();
            return GetList(Datastring);

        }

        /// <summary>  
        /// 列出FTP目录下的所有文件  
        /// </summary>  
        /// <param name="ftpUri">FTP目录</param>  
        /// <returns></returns>  
        public List<FileStruct> ListFiles()
        {
            List<FileStruct> listAll = ListFilesAndDirectories();
            List<FileStruct> listFile = new List<FileStruct>();
            foreach (FileStruct file in listAll)
            {
                if (!file.IsDirectory)
                {
                    listFile.Add(file);
                }
            }
            return listFile;
        }


        /// <summary>  
        /// 列出FTP目录下的所有目录  
        /// </summary>  
        /// <param name="ftpUri">FRTP目录</param>  
        /// <returns>目录列表</returns>  
        public List<FileStruct> ListDirectories()
        {
            List<FileStruct> listAll = ListFilesAndDirectories();
            List<FileStruct> listDirectory = new List<FileStruct>();
            foreach (FileStruct file in listAll)
            {
                if (file.IsDirectory)
                {
                    listDirectory.Add(file);
                }
            }
            return listDirectory;
        }

        /// <summary>  
        /// 获得文件和目录列表  
        /// </summary>  
        /// <param name="datastring">FTP返回的列表字符信息</param>  
        private List<FileStruct> GetList(string datastring)
        {
            List<FileStruct> myListArray = new List<FileStruct>();
            string[] dataRecords = datastring.Split('\n');
            FileListStyle _directoryListStyle = GuessFileListStyle(dataRecords);
            foreach (string s in dataRecords)
            {
                if (_directoryListStyle != FileListStyle.Unknown && s != "")
                {
                    FileStruct f = new FileStruct();
                    f.Name = "..";
                    switch (_directoryListStyle)
                    {
                        case FileListStyle.UnixStyle:
                            f = ParseFileStructFromUnixStyleRecord(s);
                            break;
                        case FileListStyle.WindowsStyle:
                            f = ParseFileStructFromWindowsStyleRecord(s);
                            break;
                    }
                    if (!(f.Name == "." || f.Name == ".."))
                    {
                        myListArray.Add(f);
                    }
                }
            }
            return myListArray;
        }

        /// <summary>  
        /// 从Unix格式中返回文件信息  
        /// </summary>  
        /// <param name="Record">文件信息</param>  
        private FileStruct ParseFileStructFromUnixStyleRecord(string Record)
        {
            FileStruct f = new FileStruct();
            string processstr = Record.Trim();
            f.Flags = processstr.Substring(0, 10);
            f.IsDirectory = (f.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //跳过一部分  
            f.Owner = _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            f.Group = _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //跳过一部分  
            string yearOrTime = processstr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2];
            if (yearOrTime.IndexOf(":") >= 0)  //time  
            {
                processstr = processstr.Replace(yearOrTime, DateTime.Now.Year.ToString());
            }
            f.CreateTime = DateTime.Parse(_cutSubstringFromStringWithTrim(ref processstr, ' ', 8));
            f.Name = processstr;   //最后就是名称  
            return f;
        }

        /// <summary>  
        /// 从Windows格式中返回文件信息  
        /// </summary>  
        /// <param name="Record">文件信息</param>  
        private FileStruct ParseFileStructFromWindowsStyleRecord(string Record)
        {
            FileStruct f = new FileStruct();
            string processstr = Record.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
            DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
            myDTFI.ShortTimePattern = "t";
            f.CreateTime = DateTime.Parse(dateStr + " " + timeStr, myDTFI);
            if (processstr.Substring(0, 5) == "<DIR>")
            {
                f.IsDirectory = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
            }
            else
            {
                string[] strs = processstr.Split(new char[] { ' ' }, 2);// StringSplitOptions.RemoveEmptyEntries);   // true);  
                processstr = strs[1];
                f.IsDirectory = false;
            }
            f.Name = processstr;
            return f;
        }
        /// <summary>  
        /// 按照一定的规则进行字符串截取  
        /// </summary>  
        /// <param name="s">截取的字符串</param>  
        /// <param name="c">查找的字符</param>  
        /// <param name="startIndex">查找的位置</param>  
        private string _cutSubstringFromStringWithTrim(ref string s, char c, int startIndex)
        {
            int pos1 = s.IndexOf(c, startIndex);
            string retString = s.Substring(0, pos1);
            s = (s.Substring(pos1)).Trim();
            return retString;
        }
        /// <summary>  
        /// 判断文件列表的方式Window方式还是Unix方式  
        /// </summary>  
        /// <param name="recordList">文件信息列表</param>  
        private FileListStyle GuessFileListStyle(string[] recordList)
        {
            foreach (string s in recordList)
            {
                if (s.Length > 10
                 && Regex.IsMatch(s.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FileListStyle.UnixStyle;
                }
                else if (s.Length > 8
                 && Regex.IsMatch(s.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FileListStyle.WindowsStyle;
                }
            }
            return FileListStyle.Unknown;
        }

        /// <summary>    
        /// 从FTP下载整个文件夹    
        /// </summary>    
        /// <param name="ftpDir">FTP文件夹路径</param>    
        /// <param name="saveDir">保存的本地文件夹路径</param>    
        public void DownFtpDir(string ftpDir, string saveDir)
        {
            List<FileStruct> files = ListFilesAndDirectories();
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            foreach (FileStruct f in files)
            {
                if (f.IsDirectory) //文件夹，递归查询  
                {
                    DownFtpDir(ftpDir + "/" + f.Name, saveDir + "\\" + f.Name);
                }
                else //文件，直接下载  
                {
                    DownLoadFile(ftpDir + "/" + f.Name, saveDir + "\\" + f.Name);
                }
            }
        }

        /// <summary>  
        /// 从ftp下载文件到本地服务器  
        /// </summary>  
        /// <param name="downloadUrl">要下载的ftp文件路径，如ftp://192.168.1.104/capture-2.avi</param>  
        /// <param name="saveFileUrl">本地保存文件的路径，如(@"d:\capture-22.avi"</param>  
        public void DownLoadFile(string downloadUrl, string saveFileUrl)
        {
            Stream responseStream = null;
            FileStream fileStream = null;
            StreamReader reader = null;

            try
            {
                // string downloadUrl = "ftp://192.168.1.104/capture-2.avi";  

                FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(downloadUrl);
                downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                //string ftpUser = "yoyo";  
                //string ftpPassWord = "123456";  
                downloadRequest.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

                FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse();
                responseStream = downloadResponse.GetResponseStream();

                fileStream = File.Create(saveFileUrl);
                byte[] buffer = new byte[1024];
                int bytesRead;
                while (true)
                {
                    bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    fileStream.Write(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("从ftp服务器下载文件出错，文件名：" + downloadUrl + "异常信息：" + ex.ToString());
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }


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