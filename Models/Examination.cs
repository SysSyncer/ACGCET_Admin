using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Examination
{
    public int ExaminationId { get; set; }

    public string ExamCode { get; set; } = null!;

    public string? ExamMonth { get; set; }

    public int? ExamYear { get; set; }

    public int? ExamTypeId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool? IsResultLocked { get; set; }

    public int? ResultLockedBy { get; set; }

    public DateTime? ResultLockedDateTime { get; set; }

    public virtual ICollection<DeadlineConfiguration> DeadlineConfigurations { get; set; } = new List<DeadlineConfiguration>();

    public virtual ICollection<ExamApplication> ExamApplications { get; set; } = new List<ExamApplication>();

    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    public virtual ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();

    public virtual ExamType? ExamType { get; set; }

    public virtual ICollection<ExternalMark> ExternalMarks { get; set; } = new List<ExternalMark>();

    public virtual ICollection<ModuleLock> ModuleLocks { get; set; } = new List<ModuleLock>();

    public virtual AdminUser? ResultLockedByNavigation { get; set; }
}
