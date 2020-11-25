using System;

namespace InstallerSR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var server = new EltraUdpServer();

            server.Start();

            Console.ReadKey();

            server.Stop();
        }
    }
}
