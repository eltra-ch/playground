using System.Net;

namespace ConnectorLib.Udp.Response
{
    public class ReceiveResponse
    {
        public string Text { get; set; }
        public IPEndPoint Endpoint { get; set; }
    }
}
