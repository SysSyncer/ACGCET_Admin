using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ExamSchedule
{
    public int ExamScheduleId { get; set; }

    public int? ExaminationId { get; set; }

    public int? PaperId { get; set; }

    public DateOnly ExamDate { get; set; }

    public int? ExamSessionId { get; set; }

    public virtual ExamSession? ExamSession { get; set; }

    public virtual Examination? Examination { get; set; }

    public virtual Paper? Paper { get; set; }

    public virtual ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
