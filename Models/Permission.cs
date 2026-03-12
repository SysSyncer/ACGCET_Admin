using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;

    public string PermissionCode { get; set; } = null!;

    public string? ModuleName { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
