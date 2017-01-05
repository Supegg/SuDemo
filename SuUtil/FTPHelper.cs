using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.Net.Ftp
{
    /// <summary>
    /// FTP处理操作类
    /// 功能：
    /// 下载文件
    /// 上传文件
    /// 上传文件的进度信息
    /// 下载文件的进度信息
    /// 删除文件
    /// 列出文件
    /// 列出目录
    /// 进入子目录
    /// 退出当前目录返回上一层目录
    /// 判断远程文件是否存在
    /// 删除远程文件    
    /// 建立目录
    /// 删除目录
    /// 文件（目录）改名

    /// </summary>
    /// <remarks>
    /// 创建人：南疯
    /// 创建时间：2007年4月28日
    /// 
    /// 更新 by supegg at 2016-01-13
    /// 删除析构函数，使用IDisposable接口
    /// 覆盖上传下载时的同名文件
    /// 
    /// http://www.cnblogs.com/zhangjun1130/archive/2010/03/24/1693932.html
    /// </remarks>
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

    public class FTPHelper:IDisposable
    {
        /// <summary>
        /// 是否需要删除临时文件
        /// </summary>
        private bool isDeleteTempFile = false;
        /// <summary>
        /// 异步上传所临时生成的文件
        /// </summary>
        private string uploadTempFile = "";
        private bool disposed = false;
        private object disposingLock = new object();

        #region 属性信息
        /// <summary>
        /// FTP请求对象
        /// </summary>
        FtpWebRequest Request = null;
        /// <summary>
        /// FTP响应对象
        /// </summary>
        FtpWebResponse Response = null;
        /// <summary>
        /// FTP服务器地址
        /// </summary>
        private Uri uri;
        /// <summary>
        /// FTP服务器地址
        /// </summary>
        public Uri Uri
        {
            get
            {
                if (directoryPath == "/")
                {
                    return uri;
                }
                else
                {
                    string strUri = uri.ToString();
                    if (strUri.EndsWith("/"))
                    {
                        strUri = strUri.Substring(0, strUri.Length - 1);
                    }
                    return new Uri(strUri + this.DirectoryPath);
                }
            }
            set
            {
                if (value.Scheme != Uri.UriSchemeFtp)
                {
                    throw new Exception("Ftp 地址格式错误!");
                }
                uri = new Uri(value.GetLeftPart(UriPartial.Authority));
                directoryPath = value.AbsolutePath;
                if (!directoryPath.EndsWith("/"))
                {
                    directoryPath += "/";
                }
            }
        }

        /// <summary>
        /// 当前工作目录
        /// </summary>
        private string directoryPath;

        /// <summary>
        /// 当前工作目录
        /// </summary>
        public string DirectoryPath
        {
            get { return directoryPath; }
            set { directoryPath = value; }
        }

        /// <summary>
        /// FTP登录用户
        /// </summary>
        private string userName;
        /// <summary>
        /// FTP登录用户
        /// </summary>
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        private string errorMsg;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg
        {
            get { return errorMsg; }
            set { errorMsg = value; }
        }

        /// <summary>
        /// FTP登录密码
        /// </summary>
        private string password;
        /// <summary>
        /// FTP登录密码
        /// </summary>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// 连接FTP服务器的代理服务
        /// </summary>
        private WebProxy proxy = null;
        /// <summary>
        /// 连接FTP服务器的代理服务
        /// </summary>
        public WebProxy Proxy
        {
            get
            {
                return proxy;
            }
            set
            {
                proxy = value;
            }
        }
        #endregion

        #region 事件
        public delegate void AsynDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e);
        public delegate void AsynDownloadDataCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
        public delegate void AsynUploadProgressChanged(object sender, UploadProgressChangedEventArgs e);
        public delegate void AsynUploadFileCompleted(object sender, UploadFileCompletedEventArgs e);

        /// <summary>
        /// 异步下载进度发生改变触发的事件
        /// </summary>
        public event AsynDownloadProgressChanged DownloadProgressChanged;
        /// <summary>
        /// 异步下载文件完成之后触发的事件
        /// </summary>
        public event AsynDownloadDataCompleted DownloadDataCompleted;
        /// <summary>
        /// 异步上传进度发生改变触发的事件
        /// </summary>
        public event AsynUploadProgressChanged UploadProgressChanged;
        /// <summary>
        /// 异步上传文件完成之后触发的事件
        /// </summary>
        public event AsynUploadFileCompleted UploadFileCompleted;
        #endregion

        #region 构造函数
        /// <summary>
        /// 匿名登录
        /// </summary>
        public FTPHelper()
        {
            this.userName = "anonymous";  //匿名用户
            this.password = "@anonymous";
            this.uri = null;
            this.proxy = null;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="FtpUri">FTP地址</param>
        /// <param name="userName">登录用户名</param>
        /// <param name="password">登录密码</param>
        public FTPHelper(Uri FtpUri, string userName, string password)
        {
            this.uri = new Uri(FtpUri.GetLeftPart(UriPartial.Authority));
            directoryPath = FtpUri.AbsolutePath;
            if (!directoryPath.EndsWith("/"))
            {
                directoryPath += "/";
            }
            this.userName = userName;
            this.password = password;
            this.proxy = null;
        }

        /// <summary>
        /// 代理登陆
        /// </summary>
        /// <param name="FtpUri">FTP地址</param>
        /// <param name="userName">登录用户名</param>
        /// <param name="password">登录密码</param>
        /// <param name="proxy">连接代理</param>
        public FTPHelper(Uri FtpUri, string userName, string password, WebProxy proxy)
        {
            this.uri = new Uri(FtpUri.GetLeftPart(UriPartial.Authority));
            directoryPath = FtpUri.AbsolutePath;
            if (!directoryPath.EndsWith("/"))
            {
                directoryPath += "/";
            }
            this.userName = userName;
            this.password = password;
            this.proxy = proxy;
        }

        /// <summary>
        /// 析构函数
        /// 不能在析构函数中释放托管资源，因为析构函数是有垃圾回收器调用的
        /// 可能在析构函数调用之前，类包含的托管资源已经被回收了，从而导致无法预知的结果
        /// </summary>
        //~FTPHelper()
        //{
        //    if (Response != null)
        //    {
        //        Response.Close();
        //        Response = null;
        //    }
        //    if (Request != null)
        //    {
        //        Request.Abort();
        //        Request = null;
        //    }
        //}

        public void Dispose()
        {
            dispose(true);
        }

        protected void dispose(bool disposing)
        {
            lock (disposingLock)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        // Dispose managed resources.
                    }

                    if (Response != null)
                    {
                        Response.Close();
                        Response = null;
                    }
                    if (Request != null)
                    {
                        Request.Abort();
                        Request = null;
                    }

                    // Dispose unmanaged resources.
                    disposed = true;
                }
            }
        }
        #endregion

        #region 建立连接
        /// <summary>
        /// 建立FTP链接,返回响应对象
        /// </summary>
        /// <param name="uri">FTP地址</param>
        /// <param name="ftpMethod">操作命令</param>
        private FtpWebResponse Open(Uri uri, string ftpMethod)
        {
            int retry = 1;
            try
            {
                Request = (FtpWebRequest)WebRequest.Create(uri);
                Request.Method = ftpMethod;
                //Request.UseBinary = true;
                Request.UsePassive = true;
                //Request.KeepAlive = false;
                Request.ReadWriteTimeout = 1000;
                Request.Timeout = 1000;
                Request.Credentials = new NetworkCredential(this.UserName, this.Password);
                if (this.Proxy != null)
                {
                    Request.Proxy = this.Proxy;
                }
                return (FtpWebResponse)Request.GetResponse();
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                if (retry-- > 0)
                {
                    return Open(uri, ftpMethod);
                }
                else
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 建立FTP链接,返回请求对象
        /// </summary>
        /// <param name="uri">FTP地址</param>
        /// <param name="FtpMethod">操作命令</param>
        private FtpWebRequest OpenRequest(Uri uri, string FtpMethod)
        {
            try
            {
                Request = (FtpWebRequest)WebRequest.Create(uri);
                Request.Method = FtpMethod;
                Request.UseBinary = true;
                Request.ReadWriteTimeout = 3000;
                Request.Timeout = 3000;
                Request.Credentials = new NetworkCredential(this.UserName, this.Password);
                if (this.Proxy != null)
                {
                    Request.Proxy = this.Proxy;
                }
                return Request;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        #endregion

        #region 下载文件

        /// <summary>
        /// 从FTP服务器下载文件，使用与远程文件同名的文件名来保存文件
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        /// <param name="LocalPath">本地路径</param>

        public bool DownloadFile(string RemoteFileName, string LocalPath)
        {
            return DownloadFile(RemoteFileName, LocalPath, RemoteFileName);
        }

        /// <summary>
        /// 从FTP服务器下载文件，指定本地路径和本地文件名
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        /// <param name="LocalPath">本地路径</param>
        /// <param name="LocalFilePath">保存文件的本地路径,后面带有"\"</param>
        /// <param name="LocalFileName">保存本地的文件名</param>
        public bool DownloadFile(string RemoteFileName, string LocalPath, string LocalFileName)
        {
            byte[] bt = null;
            try
            {
                if (!IsValidFileChars(RemoteFileName) || !IsValidFileChars(LocalFileName) || !IsValidPathChars(LocalPath))
                {
                    throw new Exception("非法文件名或目录名");
                }
                if (!Directory.Exists(LocalPath))
                {
                    Directory.CreateDirectory(LocalPath);
                    //throw new Exception("本地文件路径不存在");
                }

                string LocalFullPath = Path.Combine(LocalPath, LocalFileName);
                if (File.Exists(LocalFullPath))
                {
                    File.Delete(LocalFullPath);
                    //throw new Exception("当前路径下已经存在同名文件！");
                }
                bt = DownloadFile(RemoteFileName);
                if (bt != null)
                {
                    FileStream stream = new FileStream(LocalFullPath, FileMode.Create);
                    stream.Write(bt, 0, bt.Length);
                    stream.Flush();
                    stream.Close();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 从FTP服务器下载文件，返回文件二进制数据
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        public byte[] DownloadFile(string RemoteFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName))
                {
                    throw new Exception("非法文件名或目录名");
                }
                Response = Open(new Uri(this.Uri.ToString() + RemoteFileName), WebRequestMethods.Ftp.DownloadFile);
                Stream Reader = Response.GetResponseStream();

                MemoryStream mem = new MemoryStream(1024 * 500);
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                int TotalByteRead = 0;
                while (true)
                {
                    bytesRead = Reader.Read(buffer, 0, buffer.Length);
                    TotalByteRead += bytesRead;
                    if (bytesRead == 0)
                        break;
                    mem.Write(buffer, 0, bytesRead);
                }
                if (mem.Length > 0)
                {
                    return mem.ToArray();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        #endregion

        #region 异步下载文件
        /// <summary>
        /// 从FTP服务器异步下载文件，指定本地路径和本地文件名
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>        
        /// <param name="LocalPath">保存文件的本地路径,后面带有"\"</param>
        /// <param name="LocalFileName">保存本地的文件名</param>
        public void DownloadFileAsync(string RemoteFileName, string LocalPath, string LocalFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName) || !IsValidFileChars(LocalFileName) || !IsValidPathChars(LocalPath))
                {
                    throw new Exception("非法文件名或目录名");
                }
                if (!Directory.Exists(LocalPath))
                {
                    Directory.CreateDirectory(LocalPath);
                    //throw new Exception("本地文件路径不存在");
                }

                string LocalFullPath = Path.Combine(LocalPath, LocalFileName);
                if (File.Exists(LocalFullPath))
                {
                    File.Delete(LocalFullPath);
                    //throw new Exception("当前路径下已经存在同名文件！");
                }
                DownloadFileAsync(RemoteFileName, LocalFullPath);

            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 从FTP服务器异步下载文件，指定本地完整路径文件名
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        /// <param name="LocalFullPath">本地完整路径文件名</param>
        public void DownloadFileAsync(string RemoteFileName, string LocalFullPath)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName))
                {
                    throw new Exception("非法文件名或目录名");
                }
                if (File.Exists(LocalFullPath))
                {
                    File.Delete(LocalFullPath);
                    //throw new Exception("当前路径下已经存在同名文件！");
                }
                MyWebClient client = new MyWebClient();

                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.Credentials = new NetworkCredential(this.UserName, this.Password);
                if (this.Proxy != null)
                {
                    client.Proxy = this.Proxy;
                }
                client.DownloadFileAsync(new Uri(this.Uri.ToString() + RemoteFileName), LocalFullPath);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 异步下载文件完成之后触发的事件
        /// </summary>
        /// <param name="sender">下载对象</param>
        /// <param name="e">数据信息对象</param>
        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (DownloadDataCompleted != null)
            {
                DownloadDataCompleted(sender, e);
            }
        }

        /// <summary>
        /// 异步下载进度发生改变触发的事件
        /// </summary>
        /// <param name="sender">下载对象</param>
        /// <param name="e">进度信息对象</param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(sender, e);
            }
        }
        #endregion

        #region 上传文件
        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="LocalFullPath">本地带有完整路径的文件</param>
        /// <param name="OverWriteRemoteFile">是否覆盖远程服务器上面同名的文件</param>
        public bool UploadFile(string LocalFullPath)
        {
            return UploadFile(LocalFullPath, Path.GetFileName(LocalFullPath));
        }

        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="LocalFullPath">本地带有完整路径的文件名</param>
        /// <param name="RemoteFileName">要在FTP服务器上面保存文件名</param>
        /// <param name="OverWriteRemoteFile">是否覆盖远程服务器上面同名的文件</param>
        public bool UploadFile(string LocalFullPath, string RemoteFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName) || !IsValidFileChars(Path.GetFileName(LocalFullPath)) || !IsValidPathChars(Path.GetDirectoryName(LocalFullPath)))
                {
                    throw new Exception("非法文件名或目录名");
                }
                if (File.Exists(LocalFullPath))
                {
                    //FileStream Stream = new FileStream(LocalFullPath, FileMode.Open, FileAccess.Read);
                    //byte[] buf = new byte[Stream.Length];
                    //Stream.Read(buf, 0, (Int32)Stream.Length);   //注意，因为Int32的最大限制，最大上传文件只能是大约2G多一点
                    //Stream.Close();
                    byte[] buf = File.ReadAllBytes(LocalFullPath);
                    return UploadFile(buf, RemoteFileName);
                }
                else
                {
                    throw new Exception("本地文件不存在");
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="FileBytes">上传的二进制数据</param>
        /// <param name="RemoteFileName">要在FTP服务器上面保存文件名</param>
        public bool UploadFile(byte[] FileBytes, string RemoteFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName))
                {
                    throw new Exception("非法文件名");
                }

                Response = Open(new Uri(this.Uri.ToString() + RemoteFileName), WebRequestMethods.Ftp.UploadFile);
                Stream requestStream = Request.GetRequestStream();
                MemoryStream mem = new MemoryStream(FileBytes);

                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                int TotalRead = 0;
                while (true)
                {
                    bytesRead = mem.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    TotalRead += bytesRead;
                    requestStream.Write(buffer, 0, bytesRead);
                }
                requestStream.Close();
                Response = (FtpWebResponse)Request.GetResponse();
                mem.Close();
                mem.Dispose();
                FileBytes = null;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        #endregion

        #region 异步上传文件
        /// <summary>
        /// 异步上传文件到FTP服务器
        /// </summary>
        /// <param name="LocalFullPath">本地带有完整路径的文件名</param>
        public void UploadFileAsync(string LocalFullPath)
        {
            UploadFileAsync(LocalFullPath, Path.GetFileName(LocalFullPath));
        }

        /// <summary>
        /// 异步上传文件到FTP服务器
        /// </summary>
        /// <param name="LocalFullPath">本地带有完整路径的文件名</param>
        /// <param name="RemoteFileName">要在FTP服务器上面保存文件名</param>
        public void UploadFileAsync(string LocalFullPath, string RemoteFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName) || !IsValidFileChars(Path.GetFileName(LocalFullPath)) || !IsValidPathChars(Path.GetDirectoryName(LocalFullPath)))
                {
                    throw new Exception("非法文件名或目录名");
                }

                if (File.Exists(LocalFullPath))
                {
                    MyWebClient client = new MyWebClient();

                    client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged);
                    client.UploadFileCompleted += new UploadFileCompletedEventHandler(client_UploadFileCompleted);
                    client.Credentials = new NetworkCredential(this.UserName, this.Password);
                    if (this.Proxy != null)
                    {
                        client.Proxy = this.Proxy;
                    }
                    client.UploadFileAsync(new Uri(this.Uri.ToString() + RemoteFileName), LocalFullPath);
                }
                else
                {
                    throw new Exception("本地文件不存在");
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 异步上传文件到FTP服务器
        /// </summary>
        /// <param name="FileBytes">上传的二进制数据</param>
        /// <param name="RemoteFileName">要在FTP服务器上面保存文件名</param>
        public void UploadFileAsync(byte[] FileBytes, string RemoteFileName)
        {
            if (!IsValidFileChars(RemoteFileName))
            {
                throw new Exception("非法文件名或目录名!");
            }

            try
            {

                if (!IsValidFileChars(RemoteFileName))
                {
                    throw new Exception("非法文件名！");
                }
                string TempPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Templates);
                if (!TempPath.EndsWith("\\"))
                {
                    TempPath += "\\";
                }
                string TempFile = TempPath + Path.GetRandomFileName();
                TempFile = Path.ChangeExtension(TempFile, Path.GetExtension(RemoteFileName));
                FileStream Stream = new FileStream(TempFile, FileMode.CreateNew, FileAccess.Write);
                Stream.Write(FileBytes, 0, FileBytes.Length);   //注意，因为Int32的最大限制，最大上传文件只能是大约2G多一点
                Stream.Flush();
                Stream.Close();
                Stream.Dispose();
                isDeleteTempFile = true;
                uploadTempFile = TempFile;
                FileBytes = null;
                UploadFileAsync(TempFile, RemoteFileName);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 异步上传文件完成之后触发的事件
        /// </summary>
        /// <param name="sender">下载对象</param>
        /// <param name="e">数据信息对象</param>
        void client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            if (isDeleteTempFile)
            {
                if (File.Exists(uploadTempFile))
                {
                    File.SetAttributes(uploadTempFile, FileAttributes.Normal);
                    File.Delete(uploadTempFile);
                }
                isDeleteTempFile = false;
            }
            if (UploadFileCompleted != null)
            {
                UploadFileCompleted(sender, e);
            }
        }

        /// <summary>
        /// 异步上传进度发生改变触发的事件
        /// </summary>
        /// <param name="sender">下载对象</param>
        /// <param name="e">进度信息对象</param>
        void client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (UploadProgressChanged != null)
            {
                UploadProgressChanged(sender, e);
            }
        }
        #endregion

        #region 列出目录文件信息
        /// <summary>
        /// 列出FTP服务器上面当前目录的所有文件和目录
        /// </summary>
        public FileStruct[] ListFilesAndDirectories()
        {
            Response = Open(this.Uri, WebRequestMethods.Ftp.ListDirectoryDetails);
            StreamReader stream = new StreamReader(Response.GetResponseStream(), Encoding.Default);
            string Datastring = stream.ReadToEnd();
            FileStruct[] list = GetList(Datastring);
            return list;
        }
        /// <summary>
        /// 列出FTP服务器上面当前目录的所有文件
        /// </summary>
        public FileStruct[] ListFiles()
        {
            FileStruct[] listAll = ListFilesAndDirectories();
            List<FileStruct> listFile = new List<FileStruct>();
            foreach (FileStruct file in listAll)
            {
                if (!file.IsDirectory)
                {
                    listFile.Add(file);
                }
            }
            return listFile.ToArray();
        }

        /// <summary>
        /// 列出FTP服务器上面当前目录的所有的目录
        /// </summary>
        public FileStruct[] ListDirectories()
        {
            FileStruct[] listAll = ListFilesAndDirectories();
            List<FileStruct> listDirectory = new List<FileStruct>();
            foreach (FileStruct file in listAll)
            {
                if (file.IsDirectory)
                {
                    listDirectory.Add(file);
                }
            }
            return listDirectory.ToArray();
        }
        /// <summary>
        /// 获得文件和目录列表
        /// </summary>
        /// <param name="datastring">FTP返回的列表字符信息</param>
        private FileStruct[] GetList(string datastring)
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
                            if(Regex.IsMatch(s, "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                            {
                                f = ParseFileStructFromUnixStyleRecord(s);
                            }
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
            return myListArray.ToArray();
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
                string[] strs = processstr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);   // true);
                processstr = strs[1];
                f.IsDirectory = false;
            }
            f.Name = processstr;
            return f;
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
        #endregion

        #region 目录或文件存在的判断
        /// <summary>
        /// 判断当前目录下指定的子目录是否存在
        /// </summary>
        /// <param name="RemoteDirectoryName">指定的目录名</param>
        public bool DirectoryExist(string RemoteDirectoryName)
        {
            try
            {
                if (!IsValidPathChars(RemoteDirectoryName))
                {
                    throw new Exception("目录名非法");
                }

                FileStruct[] listDir = ListDirectories();
                foreach (FileStruct dir in listDir)
                {
                    if (dir.Name == RemoteDirectoryName)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 判断一个远程文件是否存在服务器当前目录下面
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        public bool FileExist(string RemoteFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName))
                {
                    throw new Exception("文件名非法");
                }
                FileStruct[] listFile = ListFiles();
                foreach (FileStruct file in listFile)
                {
                    if (file.Name == RemoteFileName)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        #endregion

        #region 删除文件
        /// <summary>
        /// 从FTP服务器上面删除一个文件
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        public void DeleteFile(string RemoteFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName))
                {
                    throw new Exception("文件名非法");
                }
                Response = Open(new Uri(this.Uri.ToString() + RemoteFileName), WebRequestMethods.Ftp.DeleteFile);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        #endregion

        #region 重命名文件
        /// <summary>
        /// 更改一个文件的名称或一个目录的名称
        /// </summary>
        /// <param name="RemoteFileName">原始文件或目录名称</param>
        /// <param name="NewFileName">新的文件或目录的名称</param>
        public bool ReName(string RemoteFileName, string NewFileName)
        {
            try
            {
                if (!IsValidFileChars(RemoteFileName) || !IsValidFileChars(NewFileName))
                {
                    throw new Exception("文件名非法");
                }
                if (RemoteFileName == NewFileName)
                {
                    return true;
                }
                if (FileExist(RemoteFileName))
                {
                    Request = OpenRequest(new Uri(this.Uri.ToString() + RemoteFileName), WebRequestMethods.Ftp.Rename);
                    Request.RenameTo = NewFileName;
                    Response = (FtpWebResponse)Request.GetResponse();

                }
                else
                {
                    throw new Exception("文件在服务器上不存在");
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        #endregion

        #region 拷贝、移动文件
        /// <summary>
        /// 把当前目录下面的一个文件拷贝到服务器上面另外的目录中，注意，拷贝文件之后，当前工作目录还是文件原来所在的目录
        /// </summary>
        /// <param name="RemoteFile">当前目录下的文件名</param>
        /// <param name="DirectoryName">新目录名称。
        /// 说明：如果新目录是当前目录的子目录，则直接指定子目录。如: SubDirectory1/SubDirectory2 ；
        /// 如果新目录不是当前目录的子目录，则必须从根目录一级一级的指定。如： ./NewDirectory/SubDirectory1/SubDirectory2
        /// </param>
        /// <returns></returns>
        public bool CopyFileToAnotherDirectory(string RemoteFile, string DirectoryName)
        {
            string CurrentWorkDir = this.DirectoryPath;
            try
            {
                byte[] bt = DownloadFile(RemoteFile);
                GotoDirectory(DirectoryName);
                bool Success = UploadFile(bt, RemoteFile);
                this.DirectoryPath = CurrentWorkDir;
                return Success;
            }
            catch (Exception ex)
            {
                this.DirectoryPath = CurrentWorkDir;
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 把当前目录下面的一个文件移动到服务器上面另外的目录中，注意，移动文件之后，当前工作目录还是文件原来所在的目录
        /// </summary>
        /// <param name="RemoteFile">当前目录下的文件名</param>
        /// <param name="DirectoryName">新目录名称。
        /// 说明：如果新目录是当前目录的子目录，则直接指定子目录。如: SubDirectory1/SubDirectory2 ；
        /// 如果新目录不是当前目录的子目录，则必须从根目录一级一级的指定。如： ./NewDirectory/SubDirectory1/SubDirectory2
        /// </param>
        /// <returns></returns>
        public bool MoveFileToAnotherDirectory(string RemoteFile, string DirectoryName)
        {
            string CurrentWorkDir = this.DirectoryPath;
            try
            {
                if (DirectoryName == "")
                    return false;
                if (!DirectoryName.StartsWith("/"))
                    DirectoryName = "/" + DirectoryName;
                if (!DirectoryName.EndsWith("/"))
                    DirectoryName += "/";
                bool Success = ReName(RemoteFile, DirectoryName + RemoteFile);
                this.DirectoryPath = CurrentWorkDir;
                return Success;
            }
            catch (Exception ex)
            {
                this.DirectoryPath = CurrentWorkDir;
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        #endregion

        #region 建立、删除子目录
        /// <summary>
        /// 在FTP服务器上当前工作目录建立一个子目录
        /// </summary>
        /// <param name="DirectoryName">子目录名称</param>
        public bool MakeDirectory(string DirectoryName)
        {
            try
            {
                if (!IsValidPathChars(DirectoryName))
                {
                    throw new Exception("目录名非法");
                }
                if (DirectoryExist(DirectoryName))
                {
                    return true;
                }

                Response = Open(new Uri(this.Uri.ToString() + DirectoryName), WebRequestMethods.Ftp.MakeDirectory);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }

        /// <summary>
        /// 从当前工作目录中删除一个子目录
        /// </summary>
        /// <param name="DirectoryName">子目录名称</param>
        public bool RemoveDirectory(string DirectoryName)
        {
            try
            {
                if (!IsValidPathChars(DirectoryName))
                {
                    throw new Exception("目录名非法");
                }
                if (!DirectoryExist(DirectoryName))
                {
                    throw new Exception("服务器上面不存在指定的文件名或目录名");
                }
                Response = Open(new Uri(this.Uri.ToString() + DirectoryName), WebRequestMethods.Ftp.RemoveDirectory);
                return true;
            }
            catch (Exception ep)
            {
                ErrorMsg = ep.ToString();
                throw ep;
            }
        }
        #endregion

        #region 文件、目录名称有效性判断
        /// <summary>
        /// 判断目录名中字符是否合法
        /// </summary>
        /// <param name="DirectoryName">目录名称</param>
        public bool IsValidPathChars(string DirectoryName)
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            char[] DirChar = DirectoryName.ToCharArray();
            foreach (char C in DirChar)
            {
                if (Array.BinarySearch(invalidPathChars, C) >= 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断文件名中字符是否合法
        /// </summary>
        /// <param name="FileName">文件名称</param>
        public bool IsValidFileChars(string FileName)
        {
            char[] invalidFileChars = Path.GetInvalidFileNameChars();
            char[] NameChar = FileName.ToCharArray();
            foreach (char C in NameChar)
            {
                if (Array.BinarySearch(invalidFileChars, C) >= 0)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 目录切换操作
        /// <summary>
        /// 进入一个目录
        /// </summary>
        /// <param name="DirectoryName">
        /// 新目录的名字
        /// 说明：如果新目录是当前目录的子目录，则直接指定子目录。如: SubDirectory1/SubDirectory2 
        /// 如果新目录不是当前目录的子目录，则必须从根目录一级一级的指定。如： ./NewDirectory/SubDirectory1/SubDirectory2
        /// </param>
        public bool GotoDirectory(string DirectoryName)
        {
            string CurrentWorkPath = this.DirectoryPath;
            try
            {
                DirectoryName = DirectoryName.Replace("\\", "/");
                string[] DirectoryNames = DirectoryName.Split(new char[] { '/' });
                if (DirectoryNames[0] == ".")
                {
                    this.DirectoryPath = "/";
                    if (DirectoryNames.Length == 1)
                    {
                        return true;
                    }
                    Array.Clear(DirectoryNames, 0, 1);
                }
                bool Success = false;
                foreach (string dir in DirectoryNames)
                {
                    if (dir != null)
                    {
                        Success = EnterOneSubDirectory(dir);
                        if (!Success)
                        {
                            this.DirectoryPath = CurrentWorkPath;
                            return false;
                        }
                    }
                }
                return Success;

            }
            catch (Exception ex)
            {
                this.DirectoryPath = CurrentWorkPath;
                ErrorMsg = ex.ToString();
                throw ex;
            }
        }
        /// <summary>
        /// 从当前工作目录进入一个子目录
        /// </summary>
        /// <param name="DirectoryName">子目录名称</param>
        private bool EnterOneSubDirectory(string DirectoryName)
        {
            try
            {
                if (DirectoryName.IndexOf("/") >= 0 || !IsValidPathChars(DirectoryName))
                {
                    throw new Exception("目录名非法!");
                }
                if (DirectoryName.Length > 0 && DirectoryExist(DirectoryName))
                {
                    if (!DirectoryName.EndsWith("/"))
                    {
                        DirectoryName += "/";
                    }
                    directoryPath += DirectoryName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ep)
            {
                ErrorMsg = ep.ToString();
                throw ep;
            }
        }
        /// <summary>
        /// 从当前工作目录往上一级目录
        /// </summary>
        public bool ComeoutDirectory()
        {
            if (directoryPath == "/")
            {
                ErrorMsg = "当前目录已经是根目录！";
                throw new Exception("当前目录已经是根目录！");
            }
            char[] sp = new char[1] { '/' };

            string[] strDir = directoryPath.Split(sp, StringSplitOptions.RemoveEmptyEntries);
            if (strDir.Length == 1)
            {
                directoryPath = "/";
            }
            else
            {
                directoryPath = String.Join("/", strDir, 0, strDir.Length - 1);
            }
            return true;

        }
        #endregion

        #region 重载WebClient，支持FTP进度
        internal class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                FtpWebRequest req = (FtpWebRequest)base.GetWebRequest(address);
                req.UsePassive = false;
                return req;
            }
        }
        #endregion
    }
}