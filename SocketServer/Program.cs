// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

var ipEndpoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 5000);

using var listener = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
listener.Bind(ipEndpoint);
listener.Listen(100);

while (true)
{
    Console.WriteLine($"Listening on {ipEndpoint.Address}:{ipEndpoint.Port}");
    using var handler = await listener.AcceptAsync();
    Console.WriteLine($"{handler.RemoteEndPoint} connected");
    while (true)
    {
        // Receive message.
        var buffer = new byte[1_024];
        var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
        var response = Encoding.UTF8.GetString(buffer, 0, received);

        var eom = "<|EOM|>";
        if (response.IndexOf(eom) > -1 /* is end of message */)
        {
            Console.WriteLine(
                $"Socket server received message: \"{response.Replace(eom, "")}\"");

            var ackMessage = "<|ACK|>";
            var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
            await handler.SendAsync(Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("s")));
            await handler.SendAsync(echoBytes, 0);
            Console.WriteLine(
                $"Socket server sent acknowledgment: \"{ackMessage}\"");

            break;
        }
    }
    handler.Close();
}