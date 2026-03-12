using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Batch
{
    public int BatchId { get; set; }

    public int BatchYear { get; set; }

    public string BatchName { get; set; } = null!;

    public int? CourseId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? ExpectedEndDate { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<PassingCriterion> PassingCriteria { get; set; } = new List<PassingCriterion>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
