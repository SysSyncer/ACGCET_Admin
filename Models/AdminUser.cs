using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class AdminUser
{
    public int AdminUserId { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Email { get; set; }

    public string? FullName { get; set; }

    public string? Department { get; set; }

    public string? Designation { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsLocked { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<AdminUserRole> AdminUserRoles { get; set; } = new List<AdminUserRole>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<DataCorrectionRequest> DataCorrectionRequestApprovedByNavigations { get; set; } = new List<DataCorrectionRequest>();

    public virtual ICollection<DataCorrectionRequest> DataCorrectionRequestExecutedByNavigations { get; set; } = new List<DataCorrectionRequest>();

    public virtual ICollection<DataCorrectionRequest> DataCorrectionRequestRequestedByNavigations { get; set; } = new List<DataCorrectionRequest>();

    public virtual ICollection<Examination> Examinations { get; set; } = new List<Examination>();

    public virtual ICollection<LockOverrideRequest> LockOverrideRequestApprovedByNavigations { get; set; } = new List<LockOverrideRequest>();

    public virtual ICollection<LockOverrideRequest> LockOverrideRequestRequestedByNavigations { get; set; } = new List<LockOverrideRequest>();

    public virtual ICollection<MalpracticeDetectionLog> MalpracticeDetectionLogInvestigatedByNavigations { get; set; } = new List<MalpracticeDetectionLog>();

    public virtual ICollection<MalpracticeDetectionLog> MalpracticeDetectionLogSuspiciousUsers { get; set; } = new List<MalpracticeDetectionLog>();

    public virtual ICollection<SystemAlert> SystemAlertAcknowledgedByNavigations { get; set; } = new List<SystemAlert>();

    public virtual ICollection<SystemAlert> SystemAlertRelatedUsers { get; set; } = new List<SystemAlert>();

    public virtual ICollection<SystemAlert> SystemAlertResolvedByNavigations { get; set; } = new List<SystemAlert>();

    public virtual ICollection<UserSessionLog> UserSessionLogs { get; set; } = new List<UserSessionLog>();
}
