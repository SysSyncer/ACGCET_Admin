using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Block
{
    public int BlockId { get; set; }

    public string BlockName { get; set; } = null!;

    public string? BuildingCode { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
