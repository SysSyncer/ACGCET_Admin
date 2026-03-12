using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ExamResult
{
    public long ExamResultId { get; set; }

    public int? StudentId { get; set; }

    public int? PaperId { get; set; }

    public int? ExaminationId { get; set; }

    public decimal? InternalTotal { get; set; }

    public decimal? ExternalTotal { get; set; }

    public decimal? GrandTotal { get; set; }

    public string? Grade { get; set; }

    public int? ResultStatusId { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Examination? Examination { get; set; }

    public virtual Paper? Paper { get; set; }

    public virtual ResultStatus? ResultStatus { get; set; }

    public virtual ICollection<RevaluationRequest> RevaluationRequests { get; set; } = new List<RevaluationRequest>();

    public virtual Student? Student { get; set; }
}
