using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InstallerSR.Extensions;

namespace InstallerSR
{
    class Program
    {
        public static string GetLocalIpAddress()
        {
            string localIP = string.Empty;

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;

                if (endPoint != null)
                {
                    localIP = endPoint.Address.ToString();
                }
            }

            return localIP;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var tokenSource = new CancellationTokenSource();

            UTF8Encoding enc = new UTF8Encoding();

            var listener = new UdpClient(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 5003));

            string localIp = GetLocalIpAddress();

            var listenerTask = Task.Run(async () =>
            {
                do
                {
                    try
                    {
                        var result = await listener.ReceiveAsync().WithCancellation(tokenSource.Token);

                        string txt = enc.GetString(result.Buffer, 0, result.Buffer.Length);

                        Console.WriteLine(txt);

                        txt = $"hello client, my ip is {localIp}";

                        var bytes = enc.GetBytes(txt);

                        await listener.SendAsync(bytes, bytes.Length, result.RemoteEndPoint).WithCancellation(tokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                while (!tokenSource.IsCancellationRequested);
            });

            Console.ReadKey();

            tokenSource.Cancel();

            listenerTask.Wait();
        }
    }
}
