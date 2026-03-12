using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Section
{
    public int SectionId { get; set; }

    public string SectionName { get; set; } = null!;

    public string SectionCode { get; set; } = null!;

    public int? BatchId { get; set; }

    public int? MaxStudents { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
