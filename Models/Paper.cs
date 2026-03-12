using System;
using System.Collections.Generic;

namespace ACGCET_Admin.Models;

public partial class Paper
{
    public int PaperId { get; set; }

    public string PaperCode { get; set; } = null!;

    public string PaperName { get; set; } = null!;

    public int? CourseId { get; set; }

    public int Semester { get; set; }

    public decimal? Credits { get; set; }

    public int? PaperTypeId { get; set; }

    public int? SchemeId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<ExamApplicationPaper> ExamApplicationPapers { get; set; } = new List<ExamApplicationPaper>();

    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    public virtual ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();

    public virtual ICollection<ExternalMark> ExternalMarks { get; set; } = new List<ExternalMark>();

    public virtual ICollection<InternalMark> InternalMarks { get; set; } = new List<InternalMark>();

    public virtual ICollection<PaperFee> PaperFees { get; set; } = new List<PaperFee>();

    public virtual PaperMarkDistribution? PaperMarkDistribution { get; set; }

    public virtual PaperType? PaperType { get; set; }

    public virtual Scheme? Scheme { get; set; }
}
