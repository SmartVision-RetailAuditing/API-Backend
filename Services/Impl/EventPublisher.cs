using Microsoft.AspNetCore.SignalR;
using YourApp.Hubs;

namespace ApiBackend.Services.Impl;

public class EventPublisher : IEventPublisher
{
    private readonly IHubContext<ProductHub> _hubContext;

    public EventPublisher(IHubContext<ProductHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishProductUpdatedAsync(int productId)
    {
        // Sadece "ProductPage" grubunda olanlara gönderiyoruz!
        await _hubContext.Clients.Group("ProductPage").SendAsync("ProductUpdated", productId);
    }
     
    
    public async Task PublishAuditIssueCreatedAsync(int auditId, int issueId)
    {
        await _hubContext.Clients.Group("AuditPage").SendAsync("AuditIssueCreated", new { AuditId = auditId, IssueId = issueId });
    }

    public async Task PublishAuditIssueDeletedAsync(int issueId)
    {
        await _hubContext.Clients.Group("AuditPage").SendAsync("AuditIssueDeleted", issueId);
    }
    
    public async Task PublishAuditProductCreatedAsync(int auditId, int productId)
    {
        // Audit sayfasına yeni ürün eklendiğini bildir
        await _hubContext.Clients.Group("AuditPage").SendAsync("AuditProductCreated", new { AuditId = auditId, ProductId = productId });
    }

    public async Task PublishAuditProductUpdatedAsync(int productId)
    {
        // Ürünün güncellendiğini bildir
        await _hubContext.Clients.Group("AuditPage").SendAsync("AuditProductUpdated", productId);
    }

    public async Task PublishAuditProductDeletedAsync(int productId)
    {
        // Ürünün silindiğini bildir
        await _hubContext.Clients.Group("AuditPage").SendAsync("AuditProductDeleted", productId);
    }
    public async Task PublishAuditCreatedAsync(int auditId)
    {
        // Ana liste sayfasında olanlara "Yeni denetim eklendi" haberi ver
        await _hubContext.Clients.Group("AuditsList").SendAsync("AuditCreated", auditId);
    }

    public async Task PublishAuditUpdatedAsync(int auditId)
    {
        // Hem listeye hem de o denetimin detay sayfasında olanlara haber verebilirsin
        await _hubContext.Clients.Group("AuditsList").SendAsync("AuditUpdated", auditId);
        await _hubContext.Clients.Group("AuditPage").SendAsync("AuditUpdated", auditId);
    }

    public async Task PublishAuditDeletedAsync(int auditId)
    {
        await _hubContext.Clients.Group("AuditsList").SendAsync("AuditDeleted", auditId);
    }

    public async Task PublishAuditSubmittedAsync(int taskId)
    {
        // Saha görevlisi fotoğrafı yükleyip submit ettiğinde yöneticilere haber ver
        await _hubContext.Clients.Group("AuditsList").SendAsync("AuditTaskSubmitted", taskId);
    }
    
    public async Task PublishStoreCreatedAsync(int storeId)
    {
        await _hubContext.Clients.Group("StoresList").SendAsync("StoreCreated", storeId);
    }

    public async Task PublishStoreUpdatedAsync(int storeId)
    {
        await _hubContext.Clients.Group("StoresList").SendAsync("StoreUpdated", storeId);
    }

    public async Task PublishStoreDeletedAsync(int storeId)
    {
        await _hubContext.Clients.Group("StoresList").SendAsync("StoreDeleted", storeId);
    }
    
    public async Task PublishTaskCreatedAsync(int taskId)
    {
        // Görevler listesi sayfasında olanlara haber ver
        await _hubContext.Clients.Group("TasksList").SendAsync("TaskCreated", taskId);
    }

    public async Task PublishTaskUpdatedAsync(int taskId)
    {
        // Hem listeye hem de varsa o görevin detay sayfasına haber verebilirsin
        await _hubContext.Clients.Group("TasksList").SendAsync("TaskUpdated", taskId);
    }

    public async Task PublishTaskDeletedAsync(int taskId)
    {
        await _hubContext.Clients.Group("TasksList").SendAsync("TaskDeleted", taskId);
    }
    
    public async Task PublishUserLoggedInAsync(int userId, DateTime loginDate)
    {
        // Sadece "AdminDashboard" grubunda olan (yani o sayfayı açık tutan) yöneticilere gönder
        await _hubContext.Clients.Group("AdminDashboard").SendAsync("UserLoginUpdated", new 
        { 
            UserId = userId, 
            LastLoginDate = loginDate 
        });
    }

}