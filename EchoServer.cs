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
        private bool disposed;

        public EchoServer(int port)
        {
            this.port = port;
        }

        public async Task StartAsync()
        {
            if (disposed) return;
            
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
                if (!cts.IsCancellationRequested)
                    throw;
            }
            catch (ObjectDisposedException)
            {
                if (!cts.IsCancellationRequested)
                    throw;
            }
        }

        public void Stop()
        {
            cts.Cancel();
            listener?.Stop();
        }

        public async Task<string> ProcessMessageAsync(string message)
        {
            await Task.Delay(1); // імітація асинхронної роботи
            return message;
        }

        // зробимо internal, щоб тести могли викликати його напряму (опціонально)
        internal async Task HandleClientAsync(TcpClient client)
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

        // Dispose pattern
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                cts.Dispose();
                try
                {
                    listener?.Stop();
                }
                catch
                {
                }
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
