using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class RevaluationStatus
{
    public int RevaluationStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<RevaluationRequest> RevaluationRequests { get; set; } = new List<RevaluationRequest>();
}
