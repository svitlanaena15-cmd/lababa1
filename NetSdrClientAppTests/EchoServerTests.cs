using System.Threading.Tasks;
using Xunit;
using labora1;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace NetSdrClientAppTests
{
    public class EchoServerTests
    {
        [Fact]
        public async Task ProcessMessageAsync_ShouldReturnSameMessage()
        {
            var server = new EchoServer(12345);
            string message = "Hello Echo";
            string result = await server.ProcessMessageAsync(message);

            Assert.Equal(message, result);
        }

        [Theory]
        [InlineData("Test")]
        [InlineData("12345")]
        [InlineData("!@#$%^&*()")]
        public async Task ProcessMessageAsync_VariousMessages_ReturnsSame(string msg)
        {
            var server = new EchoServer(0);
            var response = await server.ProcessMessageAsync(msg);

            Assert.Equal(msg, response);
        }

        [Fact]
        public void Stop_ShouldNotThrow()
        {
            var server = new EchoServer(0);

            var exception = Record.Exception(() => server.Stop());

            Assert.Null(exception);
        }

        [Fact]
        public async Task StartAsync_ShouldEchoMessageBack()
        {
            int port = 9105;
            var server = new EchoServer(port);

            var serverTask = Task.Run(() => server.StartAsync());
            await Task.Delay(150);

            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", port);

            using var stream = client.GetStream();
            string message = "Echo Test";

            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Assert.Equal(message, response);

            server.Stop();
            server.Dispose();
        }

        [Fact]
        public async Task StartAsync_Stop_ShouldExitGracefully()
        {
            var server = new EchoServer(0);

            var task = Task.Run(() => server.StartAsync());
            await Task.Delay(50);

            server.Stop();
            await Task.Delay(50);

            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task HandleClientAsync_DirectInvocation_EchoesMessage()
        {
            // Arrange
            var server = new EchoServer(0); // порт тут не використовуємо
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            var client = new TcpClient();
            var clientConnectTask = client.ConnectAsync("127.0.0.1", port);

            var acceptedTask = listener.AcceptTcpClientAsync();
            await clientConnectTask;
            using var accepted = await acceptedTask; // цей TcpClient — "серверний" бік

            using var clientStream = client.GetStream();

            // Act: клієнт пише повідомлення
            string message = "Direct Handle Test";
            byte[] outBytes = Encoding.UTF8.GetBytes(message);
            await clientStream.WriteAsync(outBytes, 0, outBytes.Length);

            // Викликаємо сервісний метод без запуску StartAsync
            await server.HandleClientAsync(accepted);

            // Тепер клієнт читає відповідь
            byte[] buffer = new byte[1024];
            int bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Assert
            Assert.Equal(message, response);

            // Cleanup
            client.Close();
            listener.Stop();
            server.Dispose();
        }

        [Fact]
        public async Task StartAsync_Dispose_ShouldComplete()
        {
            var server = new EchoServer(0);
            var t = Task.Run(() => server.StartAsync());
            await Task.Delay(50);

            server.Dispose();
            await Task.Delay(50);

            Assert.True(t.IsCompleted || t.IsCanceled || t.IsFaulted);
        }

    }
}
