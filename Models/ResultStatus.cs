using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class ResultStatus
{
    public int ResultStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string StatusCode { get; set; } = null!;

    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
