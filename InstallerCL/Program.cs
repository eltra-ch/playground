using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectorLib.Extensions;

namespace InstallerCL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var tokenSource = new CancellationTokenSource();

            var clientTask = Task.Run(async () =>
            {
                var udpClient = new UdpClient();

                udpClient.Connect("127.0.0.1", 5003);

                do
                {
                    try
                    {
                        string hello = "hello server";
                        UTF8Encoding enc = new UTF8Encoding();
                        var bytes = enc.GetBytes(hello);

                        var stopwatch = new Stopwatch();

                        stopwatch.Start();

                        var bytesSent = await udpClient.SendAsync(bytes, bytes.Length).WithCancellation(tokenSource.Token);

                        if (bytesSent <= 0)
                        {
                            break;
                        }
                        else
                        {
                            var receiveResult = await udpClient.ReceiveAsync().WithCancellation(tokenSource.Token); 

                            string txt = enc.GetString(receiveResult.Buffer, 0, receiveResult.Buffer.Length);

                            Console.WriteLine($"{txt} - time {stopwatch.ElapsedTicks} ticks");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    await Task.Delay(100);
                }
                while (!tokenSource.IsCancellationRequested);
            });

            Console.ReadKey();

            tokenSource.Cancel();

            clientTask.Wait();
        }
    }
}
