using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class RolePermission
{
    public int RolePermissionId { get; set; }

    public int? RoleId { get; set; }

    public int? PermissionId { get; set; }

    public bool? CanCreate { get; set; }

    public bool? CanRead { get; set; }

    public bool? CanUpdate { get; set; }

    public bool? CanDelete { get; set; }

    public virtual Permission? Permission { get; set; }

    public virtual Role? Role { get; set; }
}
