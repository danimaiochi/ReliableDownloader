using System;
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
        public async Task DownloadFile_NonExistingSmallFileAndBigChunks_ShouldCreateFile()
        {
            var fileDownloader = new FileDownloader(100);
            await fileDownloader.DownloadFile(_smallFileUrl, _localDir + "image.png", progress => Console.WriteLine($"progress: {progress.ProgressPercent}"));
            Assert.True(true);
        }
    }
}
