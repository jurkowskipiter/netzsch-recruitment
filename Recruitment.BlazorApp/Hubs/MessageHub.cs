using Microsoft.AspNetCore.SignalR;

namespace Recruitment.BlazorApp.Hubs;

public class MessageHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
