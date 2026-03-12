using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class RevaluationRequest
{
    public int RevaluationRequestId { get; set; }

    public int? StudentId { get; set; }

    public long? ExamResultId { get; set; }

    public DateTime? RequestDate { get; set; }

    public decimal? Fees { get; set; }

    public int? RevaluationStatusId { get; set; }

    public decimal? OriginalMark { get; set; }

    public decimal? RevaluatedMark { get; set; }

    public decimal? MarkChange { get; set; }

    public DateTime? CompletedDate { get; set; }

    public virtual ExamResult? ExamResult { get; set; }

    public virtual RevaluationStatus? RevaluationStatus { get; set; }

    public virtual Student? Student { get; set; }
}
