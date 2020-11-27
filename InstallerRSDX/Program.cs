using System;
using System.Threading.Tasks;
using InstallerRSDX;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var t = Task.Run(async () =>
            {
                var dn = new RsdxDownloader();

                var downloadName = await dn.GetCurrentDownloadNameAsync();

                Console.WriteLine(downloadName);
            });

            t.Wait();

            Console.ReadKey();
        }
    }
}
