using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace labora1
{
    public class EchoServer : IDisposable
    {
        private readonly int port;
        private TcpListener? listener;
        private readonly CancellationTokenSource cts = new();

        public EchoServer(int port)
        {
            this.port = port;
        }

        public async Task StartAsync()
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (SocketException)
            {

            }
        }

        public void Stop()
        {
            cts.Cancel();
            listener?.Stop();
        }

        public async Task<string> ProcessMessageAsync(string message)
        {
            await Task.Delay(1); //імітація асинхронності :))
            return message;
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using var stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string response = await ProcessMessageAsync(received);

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

            client.Close();
        }

        public void Dispose()
        {
            cts.Dispose();
            listener?.Stop();
        }
    }
}
//тест