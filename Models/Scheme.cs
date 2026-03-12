using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Scheme
{
    public int SchemeId { get; set; }

    public string SchemeName { get; set; } = null!;

    public int SchemeYear { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();
}
