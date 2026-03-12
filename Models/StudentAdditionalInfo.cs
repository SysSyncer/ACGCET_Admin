using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class StudentAdditionalInfo
{
    public int StudentAdditionalInfoId { get; set; }

    public int? StudentId { get; set; }

    public string? FatherName { get; set; }

    public string? MotherName { get; set; }

    public string? GuardianName { get; set; }

    public string? PermanentAddress { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public int? QuotaTypeId { get; set; }

    public virtual QuotaType? QuotaType { get; set; }

    public virtual Student? Student { get; set; }
}
