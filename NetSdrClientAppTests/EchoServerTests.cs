using System.Threading.Tasks;
using Xunit;
using labora1;

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
    }
}
