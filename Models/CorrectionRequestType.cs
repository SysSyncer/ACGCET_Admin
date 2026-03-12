using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class CorrectionRequestType
{
    public int CorrectionRequestTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string TypeCode { get; set; } = null!;

    public bool? RequiresCoeapproval { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<DataCorrectionRequest> DataCorrectionRequests { get; set; } = new List<DataCorrectionRequest>();
}
