using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? RoleDescription { get; set; }

    public virtual ICollection<AdminUserRole> AdminUserRoles { get; set; } = new List<AdminUserRole>();

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
