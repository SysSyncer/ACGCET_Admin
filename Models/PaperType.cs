using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class PaperType
{
    public int PaperTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string TypeCode { get; set; } = null!;

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();
}
