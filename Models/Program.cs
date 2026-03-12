using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Program
{
    public int ProgramId { get; set; }

    public string ProgramName { get; set; } = null!;

    public string ProgramCode { get; set; } = null!;

    public int? DegreeId { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Degree? Degree { get; set; }
}
