using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace labora1
{
    public class EchoServer
    {
        private readonly int port;
        private TcpListener? listener;

        public EchoServer(int port)
        {
            this.port = port;
        }

        public async Task StartAsync()
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client); 
            }
        }

        public void Stop()
        {
            listener?.Stop();
        }

        public async Task<string> ProcessMessageAsync(string message)
        {
            await Task.Delay(1); // імітує асинхронну роботу :)
            return message;
        }

        // обробка ТСР внутрішньо
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
    }
}
