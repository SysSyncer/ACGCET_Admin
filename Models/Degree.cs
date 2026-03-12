using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Degree
{
    public int DegreeId { get; set; }

    public string DegreeName { get; set; } = null!;

    public string DegreeCode { get; set; } = null!;

    public string? GraduationLevel { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Program> Programs { get; set; } = new List<Program>();
}
