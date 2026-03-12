using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int? DegreeId { get; set; }

    public int? ProgramId { get; set; }

    public int? RegulationId { get; set; }

    public int? DurationYears { get; set; }

    public int? TotalSemesters { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual Degree? Degree { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual ICollection<PassingCriterion> PassingCriteria { get; set; } = new List<PassingCriterion>();

    public virtual Program? Program { get; set; }

    public virtual Regulation? Regulation { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
