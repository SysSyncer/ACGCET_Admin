using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class SeatAllocation
{
    public int SeatAllocationId { get; set; }

    public int? ExamScheduleId { get; set; }

    public int? StudentId { get; set; }

    public int? RoomId { get; set; }

    public int SeatNumber { get; set; }

    public int? RowNumber { get; set; }

    public int? ColumnNumber { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual Room? Room { get; set; }

    public virtual Student? Student { get; set; }
}
