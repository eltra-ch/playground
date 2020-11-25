using ConnectorLib.Helpers;
using EltraCommon.Logger;
using InstallerSR.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstallerSR
{
    class EltraUdpServer
    {
        #region Private fields

        private UdpClient _udpClient;
        private Task _listenerTask;
        private CancellationTokenSource _tokenSource;
        private Encoding _encoding;

        #endregion

        #region Constructors

        public EltraUdpServer()
        {
            _tokenSource = new CancellationTokenSource();

            Host = "0.0.0.0";
            Port = 5100;            
        }

        #endregion

        #region Properties

        public string Host { get; set; }

        public int Port { get; set; }

        protected UdpClient UdpClient => _udpClient ?? (_udpClient = CreateUdpClient());

        protected Encoding Encoding => _encoding ?? (_encoding = new UTF8Encoding());

        #endregion

        #region Methods

        private UdpClient CreateUdpClient()
        {
            var result = new UdpClient(new IPEndPoint(IPAddress.Parse(Host), Port));

            return result;
        }

        public bool Start()
        {
            if (_listenerTask == null || _listenerTask.IsCompleted)
            {
                _listenerTask = Task.Run(async () =>
                {
                    int minWaitTime = 10;

                    do
                    {
                        var receiveResponse = await Receive();

                        if (receiveResponse != null)
                        {
                            Console.WriteLine(receiveResponse.Text);

                            await Send(receiveResponse.Endpoint);
                        }
                        else
                        {
                            await Task.Delay(minWaitTime);
                        }
                    }
                    while (!_tokenSource.IsCancellationRequested);
                });
            }

            return !_listenerTask.IsCompleted;
        }

        public void Stop()
        {
            if (!_listenerTask.IsCompleted)
            {
                _tokenSource.Cancel();

                _listenerTask.Wait();

                _listenerTask = null;
            }
        }

        private async Task<ReceiveResponse> Receive()
        {
            ReceiveResponse result = null;

            try
            {
                var receiveResult = await UdpClient.ReceiveAsync().WithCancellation(_tokenSource.Token);

                var txt = Encoding.GetString(receiveResult.Buffer, 0, receiveResult.Buffer.Length);

                result = new ReceiveResponse() { Endpoint = receiveResult.RemoteEndPoint, Text = txt };
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);
            }

            return result;
        }

        private async Task<bool> Send(IPEndPoint endPoint)
        {
            bool result = false;

            string localIp = IpHelper.GetLocalIpAddress();

            var txt = $"hello client, my ip is {localIp}";

            try
            {
                var bytes = Encoding.GetBytes(txt);

                await UdpClient.SendAsync(bytes, bytes.Length, endPoint).WithCancellation(_tokenSource.Token);

                result = true;
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return result;
        }

        #endregion
    }
}
