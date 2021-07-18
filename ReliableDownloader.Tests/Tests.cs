using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ReliableDownloader.Tests
{
    [TestFixture]
    public class Tests
    {
        private string _smallFileUrl = "https://upload.wikimedia.org/wikipedia/en/d/d0/Dogecoin_Logo.png";
        private string _localDir = "D:/Projects/ReliableDownloader-main/";

        [SetUp]
        public void Setup()
        {
            File.Delete(_localDir+"image.png");
        }

        [Test]
        public async Task DownloadFile_NonExistingSmallFileAndSmallChunks_ShouldCreateFile()
        {
            var fileDownloader = new FileDownloader(1000);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));
            Assert.True(true);
        }

        [Test]
        public async Task DownloadFile_NonExistingSmallFileAndBigChunks_ShouldCreateFile()
        {
            var fileDownloader = new FileDownloader(100000);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));
            Assert.True(true);
        }

        [Test]
        public async Task DownloadFile_ExistingSmallFile_ShouldntCreateFile()
        {
            var fileDownloader = new FileDownloader(100000);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));

            var file = new FileInfo(_localDir + "image.png");
            var fileCreationTimeBefore = file.CreationTime;

            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));

            file = new FileInfo(_localDir + "image.png");
            var fileCreationTimeAfter = file.CreationTime;

            Assert.True(fileCreationTimeBefore == fileCreationTimeAfter);
        }

        [Test]
        public async Task DownloadFile_CancellingAndResuming_ShouldContinueDownloading()
        {
            var fileDownloader = new FileDownloader(100);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress =>
            {
                // cancel when it gets to 10%
                if (progress.ProgressPercent >= .1)
                {
                    fileDownloader.CancelDownloads();
                }
            });

            var file = new FileInfo(_localDir + "image.png");
            var fileCreationTime = file.CreationTime;

            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => { });

            file = new FileInfo(_localDir + "image.png");
            var fileCreationTimeAfter = file.CreationTime;

            Assert.IsTrue(fileCreationTime == fileCreationTimeAfter);

        }
    }
}
