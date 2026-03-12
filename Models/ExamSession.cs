using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ExamSession
{
    public int ExamSessionId { get; set; }

    public string SessionName { get; set; } = null!;

    public string SessionCode { get; set; } = null!;

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public virtual ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();
}
