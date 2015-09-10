using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FtpClient
{
    // http://www.cnblogs.com/swtseaman/archive/2011/03/29/1998611.html
    // http://blog.sina.com.cn/s/blog_66eff145010133ea.html
    public class FtpLib
    {
        const int BUFFER_LENGTH = 10240;

        private string ftpHost;
        private string ftpUsername;
        private string ftpPassword;
        private int ftpPort = 21;

        public enum FtpFileType
        {
            ALL,            // 所有文件
            ONLY_FILE,      // 仅文件
            ONLY_DIR        // 仅目录
        }

        /// <param name="host">FTP连接地址</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public FtpLib(string host, string username, string password)
        {
            this.ftpHost = host;
            this.ftpUsername = username;
            this.ftpPassword = password;
        }

        /// <param name="host">FTP连接地址</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="port">端口（默认21）</param>
        public FtpLib(string host, string username, string password, int port)
            : this(host, username, password)
        {
            this.ftpPort = port;
        }

        /// <summary>
        /// 拼装URI
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Uri pieceUri(string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ftp://").Append(ftpHost);
            if (ftpPort > 0 && ftpPort <= 65535)
            {
                sb.Append(":").Append(ftpPort);
            }
            if (!path.StartsWith("/"))
            {
                sb.Append("//");
            }
            else
            {
                sb.Append("/");
            }
            sb.Append(path);
            Uri uri = new Uri(sb.ToString());
            return uri;
        }

        /// <summary>
        /// 创建FTP请求
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private FtpWebRequest initRequest(Uri uri, string method)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(uri);
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.KeepAlive = false;
            request.UseBinary = true;
            request.UsePassive = true;
            request.EnableSsl = false;
            request.Proxy = null;
            request.Method = method;
            return request;
        }

        /// <summary>
        /// 创建FTP请求
        /// </summary>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private FtpWebRequest initRequest(string path, string method)
        {
            return initRequest(pieceUri(path), method);
        }

        private string commGetResponseResult(FtpWebRequest request)
        {
            string result;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                using (StreamReader resultReader = new StreamReader(response.GetResponseStream()))
                {
                    result = resultReader.ReadToEnd();
                }
            }
            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="localFile">本地文件</param>
        /// <param name="remotePath">远程保存路径</param>
        /// <param name="remoteFilename">远程文件名 null表示使用当前文件名</param>
        /// <returns></returns>
        public string UploadFile(FileInfo localFile, string remotePath, string remoteFilename)
        {
            string result = String.Empty;

            if (remoteFilename == null)
            {
                remoteFilename = localFile.Name;
            }
            string uploadFilePath = remotePath.TrimEnd('/') + "/" + remoteFilename;
            FtpWebRequest request = initRequest(uploadFilePath, WebRequestMethods.Ftp.UploadFile);
            request.ContentLength = localFile.Length;

            byte[] buffer = new byte[BUFFER_LENGTH];
            int flag;
            using (FileStream fs = localFile.OpenRead())
            {
                using (Stream uploadStream = request.GetRequestStream())
                {
                    do
                    {
                        flag = fs.Read(buffer, 0, BUFFER_LENGTH);
                        uploadStream.Write(buffer, 0, flag);
                    }
                    while (flag > 0);
                }
            }

            return commGetResponseResult(request);
        }

        public string UploadFile(FileInfo localFile, string remotePath)
        {
            return UploadFile(localFile, remotePath, null);
        }

        /// <summary>
        /// 下载文件并保存到本地磁盘
        /// </summary>
        /// <param name="remoteFile">需要下载的文件</param>
        /// <param name="savePath">保存的路径</param>
        /// <param name="saveName">另存为的文件名 null表示使用原有文件</param>
        public void DownloadFileAsSave(string remoteFile, string savePath, string saveName)
        {
            if (Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            FtpWebRequest request = initRequest(remoteFile, WebRequestMethods.Ftp.DownloadFile);
            if (saveName == null)
            {
                string[] seg = request.RequestUri.Segments;
                saveName = seg[seg.Length - 1];
            }
            string saveFile = savePath.TrimEnd('\\') + "\\" + saveName;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                using (Stream downloadStream = response.GetResponseStream())
                {
                    using (FileStream fs = new FileStream(saveFile, FileMode.Create))
                    {
                        byte[] buffer = new byte[BUFFER_LENGTH];
                        int flag;
                        do
                        {
                            flag = downloadStream.Read(buffer, 0, BUFFER_LENGTH);
                            fs.Write(buffer, 0, flag);
                        }
                        while (flag > 0);
                    }
                }
            }

        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <returns></returns>
        public string DeleteFile(string remoteFile)
        {
            FtpWebRequest request = initRequest(remoteFile, WebRequestMethods.Ftp.DeleteFile);
            return commGetResponseResult(request);
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="remoteDirectory"></param>
        /// <returns></returns>
        public string RemoveDirectory(string remoteDirectory)
        {
            FtpWebRequest request = initRequest(remoteDirectory, WebRequestMethods.Ftp.RemoveDirectory);
            return commGetResponseResult(request);
        }

        /// <summary>
        /// 获取当前目录下明细(包含文件和文件夹)
        /// </summary>
        /// <param name="remotePath">远程路径</param>
        /// <param name="type">查找文件的类别</param>
        /// <param name="mask">查找文件的正则</param>
        /// <returns></returns>
        public List<FtpFileInfo> GetFilesDetailList(string remotePath, FtpFileType type, Regex mask)
        {
            List<FtpFileInfo> result = new List<FtpFileInfo>();

            FtpWebRequest request = initRequest(remotePath, WebRequestMethods.Ftp.ListDirectoryDetails);
            using (WebResponse resp = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        FtpFileInfo fileInfo = new FtpFileInfo(line);
                        if ((type == FtpFileType.ALL) || (type == FtpFileType.ONLY_FILE && !fileInfo.IsFolder) && (type == FtpFileType.ONLY_DIR && fileInfo.IsFolder))
                        {
                            if (mask == null || mask.Match(fileInfo.Name).Success)
                            {
                                result.Add(fileInfo);
                            }
                        }
                    };
                };
            }
            return result;
        }

        public List<FtpFileInfo> GetFilesDetailList(string remotePath, Regex mask)
        {
            return GetFilesDetailList(remotePath, FtpFileType.ALL, mask);
        }

        public List<FtpFileInfo> GetFilesDetailList(string remotePath, FtpFileType type)
        {
            return GetFilesDetailList(remotePath, type, null);
        }

        public List<FtpFileInfo> GetFilesDetailList(string remotePath)
        {
            return GetFilesDetailList(remotePath, FtpFileType.ALL, null);
        }


        /// <summary>
        /// 文件或目录是否存在
        /// </summary>
        /// <param name="remotePath"></param>
        public bool FileExist(string remotePath)
        {
            bool result = false;
            List<FtpFileInfo> list = GetFilesDetailList(remotePath);
            Uri uri = pieceUri(remotePath);
            string[] seg = uri.Segments;
            string fileName = seg[seg.Length - 1];
            foreach (FtpFileInfo file in list)
            {
                if (file.Name == fileName)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="newDirectory"></param>
        public string MakeDirectory(string newDirectory)
        {
            FtpWebRequest request = initRequest(newDirectory, WebRequestMethods.Ftp.MakeDirectory);
            return commGetResponseResult(request);
        }


        /// <summary>
        /// 改名
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public string ReName(string remotePath, string newName)
        {
            FtpWebRequest request = initRequest(remotePath, WebRequestMethods.Ftp.Rename);
            request.RenameTo = newName;
            return commGetResponseResult(request);
        }

    }

}
