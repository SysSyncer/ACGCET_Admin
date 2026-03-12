using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Regulation
{
    public int RegulationId { get; set; }

    public int RegulationYear { get; set; }

    public string RegulationName { get; set; } = null!;

    public DateOnly? EffectiveFrom { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
