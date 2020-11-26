using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectorLib.Udp;
using ConnectorLib.Udp.Response;

namespace InstallerCL
{
    class Program
    {
        private static Stopwatch _stopwatch;

        private static int _counter;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var udpClient = new EltraUdpClient();
            
            udpClient.ResponseReceived += OnResponseReceived;
            udpClient.ErrorRaised += OnErrorRaised;

            var clientTask = Task.Run(async () =>
            {
                do 
                {
                    try
                    {
                        string hello = "hello server";
                        
                        _stopwatch = new Stopwatch();

                        _stopwatch.Start();

                        var bytesSent = await udpClient.Send(hello);

                        if (bytesSent <= 0)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    await Task.Delay(100);
                }
                while (!udpClient.IsCanceled);
            });

            Console.ReadKey();

            udpClient.Cancel();

            clientTask.Wait();
        }

        private static void OnErrorRaised(object sender, SocketError e)
        {
            Console.WriteLine($"ERROR: error code = {e}");
        }

        private static void OnResponseReceived(object sender, ReceiveResponse e)
        {
            Console.WriteLine($"{e.Text} - time {_stopwatch.ElapsedMilliseconds} ms, counter {++_counter}");
        }
    }
}
