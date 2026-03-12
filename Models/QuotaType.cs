using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class QuotaType
{
    public int QuotaTypeId { get; set; }

    public string QuotaName { get; set; } = null!;

    public string QuotaCode { get; set; } = null!;

    public virtual ICollection<StudentAdditionalInfo> StudentAdditionalInfos { get; set; } = new List<StudentAdditionalInfo>();
}
