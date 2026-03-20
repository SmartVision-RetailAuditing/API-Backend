using Microsoft.AspNetCore.SignalR;
using YourApp.Hubs;

public interface IEventPublisher
{
    Task PublishProductUpdatedAsync(int productId);
    
    // Audit Issues Conttroller 
    Task PublishAuditIssueCreatedAsync(int auditId, int issueId);
    Task PublishAuditIssueDeletedAsync(int issueId);
    
    // Audit Products Controller
    Task PublishAuditProductCreatedAsync(int auditId, int productId);
    Task PublishAuditProductUpdatedAsync(int productId);
    Task PublishAuditProductDeletedAsync(int productId);
    
    // Audit Controller
    Task PublishAuditCreatedAsync(int auditId);
    Task PublishAuditUpdatedAsync(int auditId);
    Task PublishAuditDeletedAsync(int auditId);
    Task PublishAuditSubmittedAsync(int taskId);
    
    // Stores Controller
    Task PublishStoreCreatedAsync(int storeId);
    Task PublishStoreUpdatedAsync(int storeId);
    Task PublishStoreDeletedAsync(int storeId);
    
    // Task Controller
    Task PublishTaskCreatedAsync(int taskId);
    Task PublishTaskUpdatedAsync(int taskId);
    Task PublishTaskDeletedAsync(int taskId);
    
    // Auth Controller
    Task PublishUserLoggedInAsync(int userId, DateTime loginDate);
}

