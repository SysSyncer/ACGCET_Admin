using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class PaperMarkDistribution
{
    public int PaperMarkDistributionId { get; set; }

    public int? PaperId { get; set; }

    public decimal? InternalTheoryMax { get; set; }

    public decimal? InternalLabMax { get; set; }

    public decimal? ExternalTheoryMax { get; set; }

    public decimal? ExternalLabMax { get; set; }

    public decimal? InternalTheoryMin { get; set; }

    public decimal? InternalLabMin { get; set; }

    public decimal? ExternalTheoryMin { get; set; }

    public decimal? ExternalLabMin { get; set; }

    public decimal? TotalMax { get; set; }

    public decimal? TotalMin { get; set; }

    public virtual Paper? Paper { get; set; }
}
