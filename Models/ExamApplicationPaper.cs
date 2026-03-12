using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ExamApplicationPaper
{
    public int ExamApplicationPaperId { get; set; }

    public int? ExamApplicationId { get; set; }

    public int? PaperId { get; set; }

    public int? Semester { get; set; }

    public decimal? Fees { get; set; }

    public string? Barcode { get; set; }

    public string? DummyNumber { get; set; }

    public virtual ExamApplication? ExamApplication { get; set; }

    public virtual Paper? Paper { get; set; }
}
