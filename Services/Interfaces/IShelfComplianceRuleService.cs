using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces;

public interface IShelfComplianceService
{
    void EvaluateRules(AiVisionResultDto aiResult);
}