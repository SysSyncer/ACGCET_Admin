using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class PaperFee
{
    public int PaperFeeId { get; set; }

    public int? PaperId { get; set; }

    public int? ExamTypeId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly? EffectiveFrom { get; set; }

    public virtual ExamType? ExamType { get; set; }

    public virtual Paper? Paper { get; set; }
}
