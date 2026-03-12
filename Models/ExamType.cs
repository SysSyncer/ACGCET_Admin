using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ExamType
{
    public int ExamTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string TypeCode { get; set; } = null!;

    public virtual ICollection<Examination> Examinations { get; set; } = new List<Examination>();

    public virtual ICollection<PaperFee> PaperFees { get; set; } = new List<PaperFee>();
}
