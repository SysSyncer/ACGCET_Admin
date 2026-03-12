using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string? AdmissionNumber { get; set; }

    public string? RollNumber { get; set; }

    public string? RegistrationNumber { get; set; }

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? AadharNumber { get; set; }

    public string? MobileNumber { get; set; }

    public string? EmailAddress { get; set; }

    public int? CommunityId { get; set; }

    public int? CourseId { get; set; }

    public int? BatchId { get; set; }

    public int? SectionId { get; set; }

    public int? RegulationId { get; set; }

    public int? JoinYear { get; set; }

    public DateOnly? JoinDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Community? Community { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<ExamApplication> ExamApplications { get; set; } = new List<ExamApplication>();

    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    public virtual ICollection<ExternalMark> ExternalMarks { get; set; } = new List<ExternalMark>();

    public virtual ICollection<InternalMark> InternalMarks { get; set; } = new List<InternalMark>();

    public virtual Regulation? Regulation { get; set; }

    public virtual ICollection<RevaluationRequest> RevaluationRequests { get; set; } = new List<RevaluationRequest>();

    public virtual ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();

    public virtual Section? Section { get; set; }

    public virtual StudentAdditionalInfo? StudentAdditionalInfo { get; set; }
}
