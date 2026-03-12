using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class InternalMark
{
    public long InternalMarkId { get; set; }

    public int? StudentId { get; set; }

    public int? PaperId { get; set; }

    public int? TestTypeId { get; set; }

    public int? Semester { get; set; }

    public decimal? Mark { get; set; }

    public decimal? MaxMark { get; set; }

    public string? EnteredBy { get; set; }

    public DateTime? EnteredDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Paper? Paper { get; set; }

    public virtual Student? Student { get; set; }

    public virtual TestType? TestType { get; set; }
}
