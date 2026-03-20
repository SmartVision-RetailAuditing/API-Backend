using Microsoft.AspNetCore.SignalR;

namespace YourApp.Hubs
{
    public class ProductHub : Hub
    {
        // React'te sayfa açıldığında bu metod çağrılacak
        public async Task JoinPageGroup(string pageName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, pageName);
        }

        // React'te sayfa kapandığında bu metod çağrılacak
        public async Task LeavePageGroup(string pageName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, pageName);
        }
    }
}