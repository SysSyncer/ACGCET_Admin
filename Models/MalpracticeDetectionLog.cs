using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class MalpracticeDetectionLog
{
    public long MalpracticeDetectionLogId { get; set; }

    public string DetectionType { get; set; } = null!;

    public int? SuspiciousUserId { get; set; }

    public string? TargetTable { get; set; }

    public string? TargetRecordId { get; set; }

    public DateTime? DetectionDateTime { get; set; }

    public string? DetectionReason { get; set; }

    public string? SeverityLevel { get; set; }

    public bool? IsInvestigated { get; set; }

    public int? InvestigatedBy { get; set; }

    public string? InvestigationNotes { get; set; }

    public DateTime? InvestigationDateTime { get; set; }

    public string? ActionTaken { get; set; }

    public virtual AdminUser? InvestigatedByNavigation { get; set; }

    public virtual AdminUser? SuspiciousUser { get; set; }
}
