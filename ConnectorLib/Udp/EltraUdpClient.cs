using ConnectorLib.Udp.Response;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorLib.Udp
{
    public class EltraUdpClient : EltraUdpConnector
    {
        #region Events

        public event EventHandler<ReceiveResponse> ResponseReceived;

        #endregion

        #region Events handling

        private void OnResponseReceived(ReceiveResponse e)
        {
            ResponseReceived?.Invoke(this, e);
        }

        #endregion

        #region Methods

        protected override UdpClient CreateUdpClient()
        {
            UdpClient result = new UdpClient();

            result.Connect(Host, Port);

            return result;
        }

        public async Task<int> Send(string text)
        {
            UTF8Encoding enc = new UTF8Encoding();
            var bytes = enc.GetBytes(text);

            int bytesSent = await Send(bytes, bytes.Length);

            if(bytesSent > 0)
            {
                _ = Task.Run(async () =>
                  {
                      var response = await Receive();

                      if(response != null)
                      {
                          OnResponseReceived(response);
                      }
                  });
            }

            return bytesSent;
        }

        #endregion
    }
}
