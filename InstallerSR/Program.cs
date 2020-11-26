using ConnectorLib.Udp;
using System;

namespace InstallerSR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var server = new DummyServer() { Host = "127.0.0.1" };

            server.Start();

            Console.ReadKey();

            server.Stop();
        }
    }
}
