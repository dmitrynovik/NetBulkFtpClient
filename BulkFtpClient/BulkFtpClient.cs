using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace BulkFtpClient.Ftp
{
    public class BulkFtpClient
    {
        private readonly int _bufferSize;

        public BulkFtpClient(int bufferSize = 1024 * 1024)
        {
            _bufferSize = bufferSize;
        }

        public void DownloadDirectory(string fromUrl, string intoDirectory)
        {
            if (fromUrl == null) throw new ArgumentNullException(nameof(fromUrl));
            if (intoDirectory == null) throw new ArgumentNullException(nameof(intoDirectory));

            if (!fromUrl.EndsWith("/"))
                fromUrl = fromUrl + "/";

            var files = ListFiles(fromUrl);

            files.Each(f =>
            {
                DownloadSingleFile(fromUrl + f, intoDirectory);
            });
        }

        public void DownloadSingleFile(string fromUrl, string intoDirectory)
        {
            if (fromUrl == null) throw new ArgumentNullException(nameof(fromUrl));
            if (intoDirectory == null) throw new ArgumentNullException(nameof(intoDirectory));

            var fileName = new Uri(fromUrl).Segments.Last();
            if (fileName == null)
                throw new ArgumentException($"{fromUrl} is a directory");

            var intoPath = Path.Combine(intoDirectory, fileName);

            CreateDirIfNeeded(intoDirectory);

            var buf = new char[_bufferSize];

            var request = CreateFtpRequest(fromUrl, WebRequestMethods.Ftp.DownloadFile);
            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    ThrowIfNull(fromUrl, responseStream);

                    using (var reader = new StreamReader(responseStream))
                    {
                        using (var writeStream = File.OpenWrite(intoPath))
                        {
                            using (var writer = new StreamWriter(writeStream, reader.CurrentEncoding))
                            {
                                while (true)
                                {
                                    int readBytes = reader.Read(buf, 0, _bufferSize);
                                    if (readBytes <= 0) break;

                                    writer.Write(buf, 0, readBytes);
                                }
                            }
                        }
                    }
                }
            }
        }

        public ICollection<string> ListFiles(string fromUrl)
        {
            var request = CreateFtpRequest(fromUrl, WebRequestMethods.Ftp.ListDirectory);
            var files = new List<string>();

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    ThrowIfNull(fromUrl, responseStream);

                    using (var reader = new StreamReader(responseStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var filename = reader.ReadLine();
                            files.Add(filename);
                        }
                    }
                }
            }
            return files;
        }

        private static void CreateDirIfNeeded(string intoPath)
        {
            if (!Directory.Exists(intoPath))
                Directory.CreateDirectory(intoPath);
        }

        private static void ThrowIfNull(string fromUrl, Stream responseStream)
        {
            if (responseStream == null)
                throw new IOException($"Could not open FTP stream from {fromUrl}");
        }

        private FtpWebRequest CreateFtpRequest(string fromUrl, string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(fromUrl);
            request.Method = method;
            return request;
        }
    }
}
