using ConnectorLib.Helpers;
using ConnectorLib.Udp;
using ConnectorLib.Udp.Response;
using System;
using System.Threading.Tasks;

namespace InstallerSR
{
    class DummyServer : EltraUdpServer
    {
        private int _counter;

        protected override async Task<int> OnMessageReceived(ReceiveResponse receiveResponse)
        {
            Console.WriteLine($"{receiveResponse.Text}, counter = {++_counter}");

            string localIp = IpHelper.GetLocalIpAddress();

            var txt = $"hello client, my ip is {localIp}, counter";

            return await Send(receiveResponse.Endpoint, txt);
        }
    }
}
