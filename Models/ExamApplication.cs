using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ExamApplication
{
    public int ExamApplicationId { get; set; }

    public int? StudentId { get; set; }

    public int? ExaminationId { get; set; }

    public DateTime? ApplicationDate { get; set; }

    public decimal? TotalFees { get; set; }

    public bool? IsPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual ICollection<ExamApplicationPaper> ExamApplicationPapers { get; set; } = new List<ExamApplicationPaper>();

    public virtual Examination? Examination { get; set; }

    public virtual Student? Student { get; set; }
}
