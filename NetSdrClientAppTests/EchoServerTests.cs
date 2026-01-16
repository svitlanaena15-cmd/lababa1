using System.Threading.Tasks;
using Xunit;
using labora1;
using TcpClient;
using Encoding;

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
            await Task.Delay(150); // дати серверу запуститись

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

    }
}
