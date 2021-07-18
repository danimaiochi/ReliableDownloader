using System;
using System.Threading.Tasks;
using ReliableDownloader;

namespace ReliableDownloader
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // If this url 404's, you can get a live one from https://installerstaging.accurx.com/chain/latest.json.
            var exampleUrl = "https://installerstaging.accurx.com/chain/3.55.11050.0/accuRx.Installer.Local.msi";
            var exampleFilePath = "D:/Projects/ReliableDownloader-main/myfirstdownload.msi";

            var fileDownloader = new FileDownloader();
            Console.WriteLine($"Will start");
            await fileDownloader.DownloadFile(exampleUrl, exampleFilePath, progress => { Console.WriteLine($"Percent progress is {progress.ProgressPercent:P}"); });
            Console.WriteLine($"Finished");
        }
    }
}
