using ApiBackend.Entities;

namespace ApiBackend.Services.Interfaces
{
    public interface IShelfComplianceRuleService
    {
        (List<AuditIssue> Issues, decimal Score, decimal ShelfShare, string? BrandDistJson) EvaluateRules(List<AuditProduct> products);
    }
}