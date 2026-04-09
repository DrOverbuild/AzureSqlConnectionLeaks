using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Web.Entities;

namespace MinimumRepro.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _configuration;
    public BasicEntity? Event { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task OnGet()
    {
        var ipHostInfo = await Dns.GetHostEntryAsync("******");
        var ipEndpoint = new IPEndPoint(ipHostInfo.AddressList[0], 5000);

        var sb = new StringBuilder();

        using var client = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await client.ConnectAsync(ipEndpoint);
        var message = "Hi friends! <|EOM|>";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await client.SendAsync(messageBytes, SocketFlags.None);
        _logger.LogInformation("Sent message: {message}", message);


        while (true)
        {
            var buffer = new byte[1024];
            var receivedLength = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, receivedLength);
            sb.Append(response);
            if (response.Contains("<|ACK|>"))
            {
                _logger.LogInformation("Received acknowledgement: {response}", response);
                break;
            }
            else
            {
                _logger.LogInformation("Received response: {response}", response);
            }
        }

        client.Close();

        var address = string.Join("<br/>", ipHostInfo.AddressList.Select(ip => ip.ToString()));
        ViewData["iphost"] = address;
        ViewData["message"] = sb.ToString();
    }
}