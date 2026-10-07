using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace AcxiomCRM.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string action,
            string entityName,
            string? recordId = null,
            object? oldValue = null,
            object? newValue = null,
            string? userId = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var auditLog = new AuditLog
            {
                UserId = userId ??
                         httpContext?.User?.FindFirst(
                             ClaimTypes.NameIdentifier)?.Value,

                Action = action,

                EntityName = entityName,

                RecordId = recordId,

                OldValue = oldValue == null
                    ? null
                    : JsonSerializer.Serialize(oldValue),

                NewValue = newValue == null
                    ? null
                    : JsonSerializer.Serialize(newValue),

                CreatedDate = DateTime.UtcNow,

                IpAddress = httpContext?.Connection?.RemoteIpAddress?
                    .ToString()
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }
    }
}