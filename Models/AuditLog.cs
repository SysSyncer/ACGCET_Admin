using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class AuditLog
{
    public long AuditLogId { get; set; }

    public int? AdminUserId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? TableName { get; set; }

    public string? RecordId { get; set; }

    public string? ColumnName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Description { get; set; }

    public string? Ipaddress { get; set; }

    public string? MachineName { get; set; }

    public DateTime? ActionDate { get; set; }

    public virtual AdminUser? AdminUser { get; set; }
}
