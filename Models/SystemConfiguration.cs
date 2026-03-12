using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class SystemConfiguration
{
    public int ConfigurationId { get; set; }

    public string ConfigKey { get; set; } = null!;

    public string? ConfigValue { get; set; }

    public string? DataType { get; set; }

    public string? Description { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
