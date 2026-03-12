using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class DeadlineConfiguration
{
    public int DeadlineConfigurationId { get; set; }

    public int? ModuleId { get; set; }

    public int? ExaminationId { get; set; }

    public DateTime DeadlineDateTime { get; set; }

    public bool? ExtensionAllowed { get; set; }

    public int? MaxExtensionHours { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Examination? Examination { get; set; }

    public virtual Module? Module { get; set; }
}
