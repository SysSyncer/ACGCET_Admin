using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class TestType
{
    public int TestTypeId { get; set; }

    public string TestName { get; set; } = null!;

    public string TestCode { get; set; } = null!;

    public decimal? MaxMark { get; set; }

    public virtual ICollection<InternalMark> InternalMarks { get; set; } = new List<InternalMark>();
}
