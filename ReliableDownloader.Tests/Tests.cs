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
        }

        [Test]
        public async Task DownloadFile_NonExistingSmallFileAndSmallChunks_ShouldCreateFile()
        {
            File.Delete(_localDir+"image.png");

            var fileDownloader = new FileDownloader(1000);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));
            Assert.True(true);
        }

        [Test]
        public async Task DownloadFile_NonExistingSmallFileAndBigChunks_ShouldCreateFile()
        {
            File.Delete(_localDir+"image.png");

            var fileDownloader = new FileDownloader(100000);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));
            Assert.True(true);
        }

        [Test]
        public async Task DownloadFile_ExistingSmallFile_ShouldntCreateFile()
        {
            File.Delete(_localDir+"image.png");

            var fileDownloader = new FileDownloader(100000);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));

            var file = new FileInfo(_localDir + "image.png");
            var fileCreationTimeBefore = file.CreationTime;

            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));

            file = new FileInfo(_localDir + "image.png");
            var fileCreationTimeAfter = file.CreationTime;

            Assert.True(fileCreationTimeBefore == fileCreationTimeAfter);
        }
    }
}
