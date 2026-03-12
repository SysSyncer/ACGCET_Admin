using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Module
{
    public int ModuleId { get; set; }

    public string ModuleName { get; set; } = null!;

    public string ModuleCode { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsLockable { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<DeadlineConfiguration> DeadlineConfigurations { get; set; } = new List<DeadlineConfiguration>();

    public virtual ICollection<ModuleLock> ModuleLocks { get; set; } = new List<ModuleLock>();
}
