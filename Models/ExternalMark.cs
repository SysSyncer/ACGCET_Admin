using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ExternalMark
{
    public long ExternalMarkId { get; set; }

    public int? StudentId { get; set; }

    public int? PaperId { get; set; }

    public int? ExaminationId { get; set; }

    public decimal? TheoryMark { get; set; }

    public decimal? LabMark { get; set; }

    public decimal? TotalMark { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? EnteredDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Examination? Examination { get; set; }

    public virtual Paper? Paper { get; set; }

    public virtual Student? Student { get; set; }
}
