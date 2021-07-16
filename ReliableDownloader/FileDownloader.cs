using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ReliableDownloader
{
    public class FileDownloader : IFileDownloader
    {
        private byte[] _remoteFileMd5;
        private long _remoteFileSize;
        //private long _chunkSize = 1000; // maybe do some math to find a sweet spot
        private readonly long _chunkSize;
        private readonly bool _debug = false;

        public FileDownloader(long chunkSize = 100000)
        {
            _chunkSize = chunkSize;
        }

        public async Task<bool> DownloadFile(string contentFileUrl, string localFilePath, Action<FileProgress> onProgressChanged)
        {
            var remoteFileInfo = GetRemoteFileInformation(contentFileUrl);
            _remoteFileMd5 = remoteFileInfo.md5;
            _remoteFileSize = remoteFileInfo.size;

            long startsFrom = 0;

            if (remoteFileInfo.acceptsPartial)
            {
                var existingFile = GetExistingFile(localFilePath);

                if (existingFile.Exists)
                {
                    // if the local file and the remote have the same MD5, the download is complete
                    if (_remoteFileMd5 != null && _remoteFileMd5.SequenceEqual(GetFileMd5(localFilePath)))
                    {
                        return true;
                    }

                    // otherwise we start downloading from where it stopped
                    startsFrom = existingFile.Length;
                }
            }
            await DownloadPartial(contentFileUrl, localFilePath, startsFrom);

            if (_remoteFileMd5.SequenceEqual(GetFileMd5(localFilePath)))
            {
                return true;
            }

            return false;
        }

        public void CancelDownloads()
        {
            throw new NotImplementedException();
        }

        private FileInfo GetExistingFile(string filePath)
        {
            return new FileInfo(filePath);
        }

        private (bool acceptsPartial, byte[] md5, long size) GetRemoteFileInformation(string fileUrl)
        {
            using (var client = new HttpClient())
            {
                var data = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, fileUrl));
                var response = data.Result;

                var acceptsPartial = response.Headers.AcceptRanges.Contains("bytes");
                var md5 = response.Content.Headers.ContentMD5;
                var size = response.Content.Headers.ContentLength ?? 0;

                return (acceptsPartial, md5, size);
            }
        }

        private async Task DownloadPartial(string fileUrl, string fileLocation, long from = 0)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(fileUrl);
            httpWebRequest.Method = "GET";

            var to = Math.Min(from + _chunkSize, _remoteFileSize - 1);

            httpWebRequest.AddRange(from, to);

            try
            {
                using (var httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
                {
                    if (!_debug)
                    {
                        using (var fileStream = new FileStream(fileLocation, FileMode.Append))
                        {
                            httpWebResponse?.GetResponseStream()?.CopyTo(fileStream);
                        }
                    }
                    else
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            httpWebResponse?.GetResponseStream()?.CopyTo(memoryStream);

                            Console.WriteLine($"ms = {string.Join(' ', memoryStream.ToArray())}");
                        }
                    }
                }

            }
            catch (WebException we)
            {
                if ((we.Response as HttpWebResponse).StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
                {
                    // the file found on your system is bigger than the file you're downloading, and isn't the full file, so let's delete and download from 0
                    File.Delete(fileLocation);
                    await DownloadPartial(fileUrl, fileLocation);
                }
            }

            if (to < _remoteFileSize)
            {
                await DownloadPartial(fileUrl, fileLocation, to+1);
            }

        }

        private byte[] GetFileMd5(string localFile)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(localFile))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
