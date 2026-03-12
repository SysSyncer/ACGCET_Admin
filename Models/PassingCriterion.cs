using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class PassingCriterion
{
    public int PassingCriteriaId { get; set; }

    public int? CourseId { get; set; }

    public int? BatchId { get; set; }

    public decimal? MinimumCredits { get; set; }

    public int? MinimumPapers { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Course? Course { get; set; }
}
