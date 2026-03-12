using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ModuleLock
{
    public int ModuleLockId { get; set; }

    public int? ModuleId { get; set; }

    public int? ExaminationId { get; set; }

    public bool? IsLocked { get; set; }

    public DateTime? LockedDateTime { get; set; }

    public string? LockedBy { get; set; }

    public string? LockReason { get; set; }

    public DateTime? UnlockedDateTime { get; set; }

    public string? UnlockedBy { get; set; }

    public string? UnlockReason { get; set; }

    public bool? AutoLocked { get; set; }

    public virtual Examination? Examination { get; set; }

    public virtual ICollection<LockOverrideRequest> LockOverrideRequests { get; set; } = new List<LockOverrideRequest>();

    public virtual Module? Module { get; set; }
}
