using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Community
{
    public int CommunityId { get; set; }

    public string CommunityName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
