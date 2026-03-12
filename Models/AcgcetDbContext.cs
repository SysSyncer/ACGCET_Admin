using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ACGCET_Admin.Models;

public partial class AcgcetDbContext : DbContext
{
    public AcgcetDbContext()
    {
    }

    public AcgcetDbContext(DbContextOptions<AcgcetDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminUser> AdminUsers { get; set; }

    public virtual DbSet<AdminUserRole> AdminUserRoles { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<Community> Communities { get; set; }

    public virtual DbSet<CorrectionRequestType> CorrectionRequestTypes { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<DataCorrectionRequest> DataCorrectionRequests { get; set; }

    public virtual DbSet<DeadlineConfiguration> DeadlineConfigurations { get; set; }

    public virtual DbSet<Degree> Degrees { get; set; }

    public virtual DbSet<ExamApplication> ExamApplications { get; set; }

    public virtual DbSet<ExamApplicationPaper> ExamApplicationPapers { get; set; }

    public virtual DbSet<ExamResult> ExamResults { get; set; }

    public virtual DbSet<ExamSchedule> ExamSchedules { get; set; }

    public virtual DbSet<ExamSession> ExamSessions { get; set; }

    public virtual DbSet<ExamType> ExamTypes { get; set; }

    public virtual DbSet<Examination> Examinations { get; set; }

    public virtual DbSet<ExternalMark> ExternalMarks { get; set; }

    public virtual DbSet<InternalMark> InternalMarks { get; set; }

    public virtual DbSet<LockOverrideRequest> LockOverrideRequests { get; set; }

    public virtual DbSet<MalpracticeDetectionLog> MalpracticeDetectionLogs { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<ModuleLock> ModuleLocks { get; set; }

    public virtual DbSet<Paper> Papers { get; set; }

    public virtual DbSet<PaperFee> PaperFees { get; set; }

    public virtual DbSet<PaperMarkDistribution> PaperMarkDistributions { get; set; }

    public virtual DbSet<PaperType> PaperTypes { get; set; }

    public virtual DbSet<PassingCriterion> PassingCriteria { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<QuotaType> QuotaTypes { get; set; }

    public virtual DbSet<Regulation> Regulations { get; set; }

    public virtual DbSet<ResultStatus> ResultStatuses { get; set; }

    public virtual DbSet<RevaluationRequest> RevaluationRequests { get; set; }

    public virtual DbSet<RevaluationStatus> RevaluationStatuses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Scheme> Schemes { get; set; }

    public virtual DbSet<SeatAllocation> SeatAllocations { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAdditionalInfo> StudentAdditionalInfos { get; set; }

    public virtual DbSet<SystemAlert> SystemAlerts { get; set; }

    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    public virtual DbSet<TestType> TestTypes { get; set; }

    public virtual DbSet<UserSessionLog> UserSessionLogs { get; set; }

    public virtual DbSet<VwActiveUserSession> VwActiveUserSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Only configure if not already configured (e.g. by DI)
            // Ideally we'd remove this entirely, but this preserves behavior for parameterless ctor scenarios if needed
            // The warning is suppressed because the connection string should come from appsettings.json when using DI
             optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS01;Initial Catalog=ACGCET_MASTER;Integrated Security=True;Trust Server Certificate=True");
    
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.AdminUserId).HasName("PK__AdminUse__02DDFE7B975EE92C");

            entity.HasIndex(e => e.Email, "UQ__AdminUse__A9D10534CE858AA6").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__AdminUse__C9F284566E5EC0C4").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<AdminUserRole>(entity =>
        {
            entity.HasKey(e => e.AdminUserRoleId).HasName("PK__AdminUse__52B797062038BD10");

            entity.HasIndex(e => new { e.AdminUserId, e.RoleId }, "UK_AdminUserRole").IsUnique();

            entity.Property(e => e.AssignedBy).HasMaxLength(100);
            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AdminUser).WithMany(p => p.AdminUserRoles)
                .HasForeignKey(d => d.AdminUserId)
                .HasConstraintName("FK__AdminUser__Admin__04AFB25B");

            entity.HasOne(d => d.Role).WithMany(p => p.AdminUserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__AdminUser__RoleI__05A3D694");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBDDFFCB8EC");

            entity.ToTable("AuditLog");

            entity.HasIndex(e => e.ActionDate, "IX_AuditLog_DateTime");

            entity.HasIndex(e => e.TableName, "IX_AuditLog_Table");

            entity.HasIndex(e => e.AdminUserId, "IX_AuditLog_User");

            entity.Property(e => e.ActionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.ColumnName).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.MachineName).HasMaxLength(100);
            entity.Property(e => e.RecordId).HasMaxLength(50);
            entity.Property(e => e.TableName).HasMaxLength(100);

            entity.HasOne(d => d.AdminUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.AdminUserId)
                .HasConstraintName("FK_AuditLog_AdminUser");
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PK__Batches__5D55CE5830C10030");

            entity.HasIndex(e => new { e.BatchYear, e.CourseId }, "UK_Batch").IsUnique();

            entity.Property(e => e.BatchName).HasMaxLength(100);

            entity.HasOne(d => d.Course).WithMany(p => p.Batches)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Batches__CourseI__571DF1D5");
        });

        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.BlockId).HasName("PK__Blocks__144215F1305D24D9");

            entity.HasIndex(e => e.BlockName, "UQ__Blocks__9AD2ADCA9B4629F6").IsUnique();

            entity.Property(e => e.BlockName).HasMaxLength(100);
            entity.Property(e => e.BuildingCode).HasMaxLength(50);
        });

        modelBuilder.Entity<Community>(entity =>
        {
            entity.HasKey(e => e.CommunityId).HasName("PK__Communit__CCAA5B694DD56333");

            entity.HasIndex(e => e.CommunityName, "UQ__Communit__A9B7C1973552F738").IsUnique();

            entity.Property(e => e.CommunityName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(250);
        });

        modelBuilder.Entity<CorrectionRequestType>(entity =>
        {
            entity.HasKey(e => e.CorrectionRequestTypeId).HasName("PK__Correcti__FE832F9B5CB4308A");

            entity.HasIndex(e => e.TypeCode, "UQ__Correcti__3E1CDC7C6D37FED7").IsUnique();

            entity.HasIndex(e => e.TypeName, "UQ__Correcti__D4E7DFA8E14C307A").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RequiresCoeapproval)
                .HasDefaultValue(true)
                .HasColumnName("RequiresCOEApproval");
            entity.Property(e => e.TypeCode).HasMaxLength(50);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A73CE252C6");

            entity.HasIndex(e => e.CourseCode, "UQ__Courses__FC00E0006743D224").IsUnique();

            entity.Property(e => e.CourseCode).HasMaxLength(100);
            entity.Property(e => e.CourseName).HasMaxLength(200);

            entity.HasOne(d => d.Degree).WithMany(p => p.Courses)
                .HasForeignKey(d => d.DegreeId)
                .HasConstraintName("FK__Courses__DegreeI__5165187F");

            entity.HasOne(d => d.Program).WithMany(p => p.Courses)
                .HasForeignKey(d => d.ProgramId)
                .HasConstraintName("FK__Courses__Program__52593CB8");

            entity.HasOne(d => d.Regulation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.RegulationId)
                .HasConstraintName("FK__Courses__Regulat__534D60F1");
        });

        modelBuilder.Entity<DataCorrectionRequest>(entity =>
        {
            entity.HasKey(e => e.DataCorrectionRequestId).HasName("PK__DataCorr__ADFB12E9154C398C");

            entity.HasIndex(e => e.ApprovalStatus, "IX_DataCorrectionRequests_Status");

            entity.Property(e => e.ApprovalComments).HasMaxLength(500);
            entity.Property(e => e.ApprovalDateTime).HasColumnType("datetime");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ExecutedDateTime).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RequestedDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TargetRecordId).HasMaxLength(50);
            entity.Property(e => e.TargetTable).HasMaxLength(100);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.DataCorrectionRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_DataCorrectionReq_ApprovedBy");

            entity.HasOne(d => d.CorrectionRequestType).WithMany(p => p.DataCorrectionRequests)
                .HasForeignKey(d => d.CorrectionRequestTypeId)
                .HasConstraintName("FK__DataCorre__Corre__6442E2C9");

            entity.HasOne(d => d.ExecutedByNavigation).WithMany(p => p.DataCorrectionRequestExecutedByNavigations)
                .HasForeignKey(d => d.ExecutedBy)
                .HasConstraintName("FK_DataCorrectionReq_ExecutedBy");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.DataCorrectionRequestRequestedByNavigations)
                .HasForeignKey(d => d.RequestedBy)
                .HasConstraintName("FK_DataCorrectionReq_RequestedBy");
        });

        modelBuilder.Entity<DeadlineConfiguration>(entity =>
        {
            entity.HasKey(e => e.DeadlineConfigurationId).HasName("PK__Deadline__345F1551F1EC2469");

            entity.ToTable("DeadlineConfiguration");

            entity.HasIndex(e => new { e.ModuleId, e.ExaminationId }, "UK_DeadlineConfig").IsUnique();

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeadlineDateTime).HasColumnType("datetime");
            entity.Property(e => e.ExtensionAllowed).HasDefaultValue(false);
            entity.Property(e => e.MaxExtensionHours).HasDefaultValue(0);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Examination).WithMany(p => p.DeadlineConfigurations)
                .HasForeignKey(d => d.ExaminationId)
                .HasConstraintName("FK__DeadlineC__Exami__4C6B5938");

            entity.HasOne(d => d.Module).WithMany(p => p.DeadlineConfigurations)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__DeadlineC__Modul__4B7734FF");
        });

        modelBuilder.Entity<Degree>(entity =>
        {
            entity.HasKey(e => e.DegreeId).HasName("PK__Degrees__4D94AD2E9A9C4E05");

            entity.HasIndex(e => e.DegreeCode, "UQ__Degrees__DB036BBCE4BBD40B").IsUnique();

            entity.Property(e => e.DegreeCode).HasMaxLength(50);
            entity.Property(e => e.DegreeName).HasMaxLength(100);
            entity.Property(e => e.GraduationLevel).HasMaxLength(20);
        });

        modelBuilder.Entity<ExamApplication>(entity =>
        {
            entity.HasKey(e => e.ExamApplicationId).HasName("PK__ExamAppl__C10AF609F8CDBA29");

            entity.ToTable(tb => tb.HasTrigger("TR_ExamApplications_Audit"));

            entity.HasIndex(e => new { e.StudentId, e.ExaminationId }, "UK_ExamApplication").IsUnique();

            entity.Property(e => e.ApplicationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.IsPaid).HasDefaultValue(false);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.TotalFees).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Examination).WithMany(p => p.ExamApplications)
                .HasForeignKey(d => d.ExaminationId)
                .HasConstraintName("FK__ExamAppli__Exami__0F624AF8");

            entity.HasOne(d => d.Student).WithMany(p => p.ExamApplications)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__ExamAppli__Stude__0E6E26BF");
        });

        modelBuilder.Entity<ExamApplicationPaper>(entity =>
        {
            entity.HasKey(e => e.ExamApplicationPaperId).HasName("PK__ExamAppl__3F0A49D9729FC5BF");

            entity.HasIndex(e => new { e.ExamApplicationId, e.PaperId }, "UK_ExamAppPaper").IsUnique();

            entity.Property(e => e.Fees).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ExamApplication).WithMany(p => p.ExamApplicationPapers)
                .HasForeignKey(d => d.ExamApplicationId)
                .HasConstraintName("FK__ExamAppli__ExamA__160F4887");

            entity.HasOne(d => d.Paper).WithMany(p => p.ExamApplicationPapers)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__ExamAppli__Paper__17036CC0");
        });

        modelBuilder.Entity<ExamResult>(entity =>
        {
            entity.HasKey(e => e.ExamResultId).HasName("PK__ExamResu__3DBFDE2619D8F3AB");

            entity.ToTable(tb => tb.HasTrigger("TR_ExamResults_Audit"));

            entity.HasIndex(e => new { e.StudentId, e.PaperId, e.ExaminationId }, "UK_ExamResult").IsUnique();

            entity.Property(e => e.ExternalTotal).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Grade).HasMaxLength(5);
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.InternalTotal).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ProcessedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Examination).WithMany(p => p.ExamResults)
                .HasForeignKey(d => d.ExaminationId)
                .HasConstraintName("FK__ExamResul__Exami__3A4CA8FD");

            entity.HasOne(d => d.Paper).WithMany(p => p.ExamResults)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__ExamResul__Paper__395884C4");

            entity.HasOne(d => d.ResultStatus).WithMany(p => p.ExamResults)
                .HasForeignKey(d => d.ResultStatusId)
                .HasConstraintName("FK__ExamResul__Resul__3B40CD36");

            entity.HasOne(d => d.Student).WithMany(p => p.ExamResults)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__ExamResul__Stude__3864608B");
        });

        modelBuilder.Entity<ExamSchedule>(entity =>
        {
            entity.HasKey(e => e.ExamScheduleId).HasName("PK__ExamSche__D03AF2C247257BEE");

            entity.ToTable("ExamSchedule");

            entity.HasIndex(e => new { e.ExaminationId, e.PaperId }, "UK_ExamSchedule").IsUnique();

            entity.HasOne(d => d.ExamSession).WithMany(p => p.ExamSchedules)
                .HasForeignKey(d => d.ExamSessionId)
                .HasConstraintName("FK__ExamSched__ExamS__1CBC4616");

            entity.HasOne(d => d.Examination).WithMany(p => p.ExamSchedules)
                .HasForeignKey(d => d.ExaminationId)
                .HasConstraintName("FK__ExamSched__Exami__1AD3FDA4");

            entity.HasOne(d => d.Paper).WithMany(p => p.ExamSchedules)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__ExamSched__Paper__1BC821DD");
        });

        modelBuilder.Entity<ExamSession>(entity =>
        {
            entity.HasKey(e => e.ExamSessionId).HasName("PK__ExamSess__85F7FB90C986C819");

            entity.HasIndex(e => e.SessionCode, "UQ__ExamSess__30AEBB84CB2C3103").IsUnique();

            entity.HasIndex(e => e.SessionName, "UQ__ExamSess__919C70DEF34BF5BD").IsUnique();

            entity.Property(e => e.SessionCode).HasMaxLength(20);
            entity.Property(e => e.SessionName).HasMaxLength(50);
        });

        modelBuilder.Entity<ExamType>(entity =>
        {
            entity.HasKey(e => e.ExamTypeId).HasName("PK__ExamType__087D50F06D043F3F");

            entity.HasIndex(e => e.TypeCode, "UQ__ExamType__3E1CDC7CDDAF90F5").IsUnique();

            entity.HasIndex(e => e.TypeName, "UQ__ExamType__D4E7DFA87D069A42").IsUnique();

            entity.Property(e => e.TypeCode).HasMaxLength(50);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<Examination>(entity =>
        {
            entity.HasKey(e => e.ExaminationId).HasName("PK__Examinat__C4A924209A42BD12");

            entity.HasIndex(e => e.ExamCode, "UQ__Examinat__FFB9F6CF8070A476").IsUnique();

            entity.Property(e => e.ExamCode).HasMaxLength(100);
            entity.Property(e => e.ExamMonth).HasMaxLength(50);
            entity.Property(e => e.IsResultLocked).HasDefaultValue(false);
            entity.Property(e => e.ResultLockedDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.ExamType).WithMany(p => p.Examinations)
                .HasForeignKey(d => d.ExamTypeId)
                .HasConstraintName("FK__Examinati__ExamT__09A971A2");

            entity.HasOne(d => d.ResultLockedByNavigation).WithMany(p => p.Examinations)
                .HasForeignKey(d => d.ResultLockedBy)
                .HasConstraintName("FK_Examinations_ResultLockedBy");
        });

        modelBuilder.Entity<ExternalMark>(entity =>
        {
            entity.HasKey(e => e.ExternalMarkId).HasName("PK__External__7D33B1F24C48DA5E");

            entity.ToTable(tb => tb.HasTrigger("TR_ExternalMarks_Audit"));

            entity.HasIndex(e => e.ExaminationId, "IX_ExternalMarks_Exam");

            entity.HasIndex(e => e.StudentId, "IX_ExternalMarks_Student");

            entity.HasIndex(e => new { e.StudentId, e.PaperId, e.ExaminationId }, "UK_ExternalMark").IsUnique();

            entity.Property(e => e.EnteredBy).HasMaxLength(100);
            entity.Property(e => e.EnteredDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LabMark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.TheoryMark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalMark).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Examination).WithMany(p => p.ExternalMarks)
                .HasForeignKey(d => d.ExaminationId)
                .HasConstraintName("FK__ExternalM__Exami__339FAB6E");

            entity.HasOne(d => d.Paper).WithMany(p => p.ExternalMarks)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__ExternalM__Paper__32AB8735");

            entity.HasOne(d => d.Student).WithMany(p => p.ExternalMarks)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__ExternalM__Stude__31B762FC");
        });

        modelBuilder.Entity<InternalMark>(entity =>
        {
            entity.HasKey(e => e.InternalMarkId).HasName("PK__Internal__64B117EE02BD1C83");

            entity.ToTable(tb => tb.HasTrigger("TR_InternalMarks_Audit"));

            entity.HasIndex(e => e.PaperId, "IX_InternalMarks_Paper");

            entity.HasIndex(e => e.StudentId, "IX_InternalMarks_Student");

            entity.HasIndex(e => new { e.StudentId, e.PaperId, e.TestTypeId, e.Semester }, "UK_InternalMark").IsUnique();

            entity.Property(e => e.EnteredBy).HasMaxLength(100);
            entity.Property(e => e.EnteredDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MaxMark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Paper).WithMany(p => p.InternalMarks)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__InternalM__Paper__2BFE89A6");

            entity.HasOne(d => d.Student).WithMany(p => p.InternalMarks)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__InternalM__Stude__2B0A656D");

            entity.HasOne(d => d.TestType).WithMany(p => p.InternalMarks)
                .HasForeignKey(d => d.TestTypeId)
                .HasConstraintName("FK__InternalM__TestT__2CF2ADDF");
        });

        modelBuilder.Entity<LockOverrideRequest>(entity =>
        {
            entity.HasKey(e => e.LockOverrideRequestId).HasName("PK__LockOver__1F5B7C722B1E0AF3");

            entity.Property(e => e.ApprovalComments).HasMaxLength(500);
            entity.Property(e => e.ApprovalDateTime).HasColumnType("datetime");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RequestReason).HasMaxLength(500);
            entity.Property(e => e.RequestedDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TemporaryUnlockExpiry).HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LockOverrideRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_LockOverrideReq_ApprovedBy");

            entity.HasOne(d => d.ModuleLock).WithMany(p => p.LockOverrideRequests)
                .HasForeignKey(d => d.ModuleLockId)
                .HasConstraintName("FK__LockOverr__Modul__58D1301D");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.LockOverrideRequestRequestedByNavigations)
                .HasForeignKey(d => d.RequestedBy)
                .HasConstraintName("FK_LockOverrideReq_RequestedBy");
        });

        modelBuilder.Entity<MalpracticeDetectionLog>(entity =>
        {
            entity.HasKey(e => e.MalpracticeDetectionLogId).HasName("PK__Malpract__5DFCDBBE1F9A0D60");

            entity.ToTable("MalpracticeDetectionLog");

            entity.HasIndex(e => e.SeverityLevel, "IX_MalpracticeLog_Severity");

            entity.Property(e => e.ActionTaken).HasMaxLength(500);
            entity.Property(e => e.DetectionDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DetectionType).HasMaxLength(100);
            entity.Property(e => e.InvestigationDateTime).HasColumnType("datetime");
            entity.Property(e => e.IsInvestigated).HasDefaultValue(false);
            entity.Property(e => e.SeverityLevel).HasMaxLength(20);
            entity.Property(e => e.TargetRecordId).HasMaxLength(50);
            entity.Property(e => e.TargetTable).HasMaxLength(100);

            entity.HasOne(d => d.InvestigatedByNavigation).WithMany(p => p.MalpracticeDetectionLogInvestigatedByNavigations)
                .HasForeignKey(d => d.InvestigatedBy)
                .HasConstraintName("FK_MalpracticeLog_InvestigatedBy");

            entity.HasOne(d => d.SuspiciousUser).WithMany(p => p.MalpracticeDetectionLogSuspiciousUsers)
                .HasForeignKey(d => d.SuspiciousUserId)
                .HasConstraintName("FK_MalpracticeLog_SuspiciousUser");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("PK__Modules__2B7477A797B2AD43");

            entity.HasIndex(e => e.ModuleName, "UQ__Modules__EAC9AEC3F70D9CA4").IsUnique();

            entity.HasIndex(e => e.ModuleCode, "UQ__Modules__EB27D43353209192").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsLockable).HasDefaultValue(true);
            entity.Property(e => e.ModuleCode).HasMaxLength(50);
            entity.Property(e => e.ModuleName).HasMaxLength(100);
        });

        modelBuilder.Entity<ModuleLock>(entity =>
        {
            entity.HasKey(e => e.ModuleLockId).HasName("PK__ModuleLo__3641AAEF1D056560");

            entity.HasIndex(e => e.IsLocked, "IX_ModuleLocks_IsLocked");

            entity.HasIndex(e => new { e.ModuleId, e.ExaminationId }, "UK_ModuleLock").IsUnique();

            entity.Property(e => e.AutoLocked).HasDefaultValue(false);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.LockReason).HasMaxLength(250);
            entity.Property(e => e.LockedBy).HasMaxLength(100);
            entity.Property(e => e.LockedDateTime).HasColumnType("datetime");
            entity.Property(e => e.UnlockReason).HasMaxLength(250);
            entity.Property(e => e.UnlockedBy).HasMaxLength(100);
            entity.Property(e => e.UnlockedDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.Examination).WithMany(p => p.ModuleLocks)
                .HasForeignKey(d => d.ExaminationId)
                .HasConstraintName("FK__ModuleLoc__Exami__540C7B00");

            entity.HasOne(d => d.Module).WithMany(p => p.ModuleLocks)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__ModuleLoc__Modul__531856C7");
        });

        modelBuilder.Entity<Paper>(entity =>
        {
            entity.HasKey(e => e.PaperId).HasName("PK__Papers__AB86120BE03DB23E");

            entity.HasIndex(e => new { e.PaperCode, e.CourseId, e.Semester }, "UK_Paper").IsUnique();

            entity.Property(e => e.Credits).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.PaperCode).HasMaxLength(50);
            entity.Property(e => e.PaperName).HasMaxLength(250);

            entity.HasOne(d => d.Course).WithMany(p => p.Papers)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Papers__CourseId__5EBF139D");

            entity.HasOne(d => d.PaperType).WithMany(p => p.Papers)
                .HasForeignKey(d => d.PaperTypeId)
                .HasConstraintName("FK__Papers__PaperTyp__5FB337D6");

            entity.HasOne(d => d.Scheme).WithMany(p => p.Papers)
                .HasForeignKey(d => d.SchemeId)
                .HasConstraintName("FK__Papers__SchemeId__60A75C0F");
        });

        modelBuilder.Entity<PaperFee>(entity =>
        {
            entity.HasKey(e => e.PaperFeeId).HasName("PK__PaperFee__889528F47278EE0D");

            entity.HasIndex(e => new { e.PaperId, e.ExamTypeId, e.EffectiveFrom }, "UK_PaperFee").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ExamType).WithMany(p => p.PaperFees)
                .HasForeignKey(d => d.ExamTypeId)
                .HasConstraintName("FK__PaperFees__ExamT__72C60C4A");

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperFees)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__PaperFees__Paper__71D1E811");
        });

        modelBuilder.Entity<PaperMarkDistribution>(entity =>
        {
            entity.HasKey(e => e.PaperMarkDistributionId).HasName("PK__PaperMar__946E9D15FFFB3AFD");

            entity.ToTable("PaperMarkDistribution");

            entity.HasIndex(e => e.PaperId, "UQ__PaperMar__AB86120AA0109AB8").IsUnique();

            entity.Property(e => e.ExternalLabMax)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ExternalLabMin)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ExternalTheoryMax)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ExternalTheoryMin)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.InternalLabMax)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.InternalLabMin)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.InternalTheoryMax)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.InternalTheoryMin)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalMax)
                .HasDefaultValue(100m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalMin)
                .HasDefaultValue(50m)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Paper).WithOne(p => p.PaperMarkDistribution)
                .HasForeignKey<PaperMarkDistribution>(d => d.PaperId)
                .HasConstraintName("FK__PaperMark__Paper__6477ECF3");
        });

        modelBuilder.Entity<PaperType>(entity =>
        {
            entity.HasKey(e => e.PaperTypeId).HasName("PK__PaperTyp__F95F66DA4B750FAE");

            entity.HasIndex(e => e.TypeCode, "UQ__PaperTyp__3E1CDC7C7984030D").IsUnique();

            entity.HasIndex(e => e.TypeName, "UQ__PaperTyp__D4E7DFA8B2A257F6").IsUnique();

            entity.Property(e => e.TypeCode).HasMaxLength(50);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<PassingCriterion>(entity =>
        {
            entity.HasKey(e => e.PassingCriteriaId).HasName("PK__PassingC__F9578D724FCFA5A5");

            entity.HasIndex(e => new { e.CourseId, e.BatchId }, "UK_PassingCriteria").IsUnique();

            entity.Property(e => e.MinimumCredits).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Batch).WithMany(p => p.PassingCriteria)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK__PassingCr__Batch__778AC167");

            entity.HasOne(d => d.Course).WithMany(p => p.PassingCriteria)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__PassingCr__Cours__76969D2E");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2F6CA5FEAB");

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__0FFDA357C4CE42D2").IsUnique();

            entity.HasIndex(e => e.PermissionCode, "UQ__Permissi__91FE5750D531AB57").IsUnique();

            entity.Property(e => e.ModuleName).HasMaxLength(100);
            entity.Property(e => e.PermissionCode).HasMaxLength(50);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.ProgramId).HasName("PK__Programs__752560586D454DF2");

            entity.HasIndex(e => new { e.ProgramCode, e.DegreeId }, "UK_Program").IsUnique();

            entity.Property(e => e.ProgramCode).HasMaxLength(50);
            entity.Property(e => e.ProgramName).HasMaxLength(150);

            entity.HasOne(d => d.Degree).WithMany(p => p.Programs)
                .HasForeignKey(d => d.DegreeId)
                .HasConstraintName("FK__Programs__Degree__2F10007B");
        });

        modelBuilder.Entity<QuotaType>(entity =>
        {
            entity.HasKey(e => e.QuotaTypeId).HasName("PK__QuotaTyp__DE48CFC9C648F031");

            entity.HasIndex(e => e.QuotaCode, "UQ__QuotaTyp__8115D40420241D7D").IsUnique();

            entity.HasIndex(e => e.QuotaName, "UQ__QuotaTyp__99C6D954BBF32B06").IsUnique();

            entity.Property(e => e.QuotaCode).HasMaxLength(50);
            entity.Property(e => e.QuotaName).HasMaxLength(100);
        });

        modelBuilder.Entity<Regulation>(entity =>
        {
            entity.HasKey(e => e.RegulationId).HasName("PK__Regulati__A192C7E94848EF26");

            entity.HasIndex(e => e.RegulationYear, "UQ__Regulati__E3EC935E788BDC1F").IsUnique();

            entity.Property(e => e.RegulationName).HasMaxLength(100);
        });

        modelBuilder.Entity<ResultStatus>(entity =>
        {
            entity.HasKey(e => e.ResultStatusId).HasName("PK__ResultSt__B5E90BA5CA115890");

            entity.HasIndex(e => e.StatusName, "UQ__ResultSt__05E7698A903D7926").IsUnique();

            entity.HasIndex(e => e.StatusCode, "UQ__ResultSt__6A7B44FC265B96BE").IsUnique();

            entity.Property(e => e.StatusCode).HasMaxLength(20);
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<RevaluationRequest>(entity =>
        {
            entity.HasKey(e => e.RevaluationRequestId).HasName("PK__Revaluat__66D2D31AE032E698");

            entity.Property(e => e.CompletedDate).HasColumnType("datetime");
            entity.Property(e => e.Fees).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MarkChange).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.OriginalMark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RevaluatedMark).HasColumnType("decimal(5, 2)");

            /*entity.HasOne(d => d.ExamResult).WithMany(p => p.RevaluationRequests)
                .HasForeignKey(d => d.ExamResultId)
                .HasConstraintName("FK__Revaluati__ExamR__3F115E1A");*/

            entity.HasOne(d => d.RevaluationStatus).WithMany(p => p.RevaluationRequests)
                .HasForeignKey(d => d.RevaluationStatusId)
                .HasConstraintName("FK__Revaluati__Reval__40F9A68C");

            entity.HasOne(d => d.Student).WithMany(p => p.RevaluationRequests)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Revaluati__Stude__3E1D39E1");
        });

        modelBuilder.Entity<RevaluationStatus>(entity =>
        {
            entity.HasKey(e => e.RevaluationStatusId).HasName("PK__Revaluat__8FEB1765FBC7F374");

            entity.HasIndex(e => e.StatusName, "UQ__Revaluat__05E7698A21F5978C").IsUnique();

            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A3C341506");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616060513EEE").IsUnique();

            entity.Property(e => e.RoleDescription).HasMaxLength(250);
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("PK__RolePerm__120F46BAF1C09EB7");

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UK_RolePermission").IsUnique();

            entity.Property(e => e.CanCreate).HasDefaultValue(false);
            entity.Property(e => e.CanDelete).HasDefaultValue(false);
            entity.Property(e => e.CanRead).HasDefaultValue(false);
            entity.Property(e => e.CanUpdate).HasDefaultValue(false);

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("FK__RolePermi__Permi__0B5CAFEA");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__RolePermi__RoleI__0A688BB1");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__3286393920D0A070");

            entity.HasIndex(e => new { e.RoomNumber, e.BlockId }, "UK_Room").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoomNumber).HasMaxLength(100);

            entity.HasOne(d => d.Block).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.BlockId)
                .HasConstraintName("FK__Rooms__BlockId__208CD6FA");
        });

        modelBuilder.Entity<Scheme>(entity =>
        {
            entity.HasKey(e => e.SchemeId).HasName("PK__Schemes__DB7E1A62CE2E8852");

            entity.HasIndex(e => new { e.SchemeName, e.SchemeYear }, "UK_Scheme").IsUnique();

            entity.Property(e => e.SchemeName).HasMaxLength(150);
        });

        modelBuilder.Entity<SeatAllocation>(entity =>
        {
            entity.HasKey(e => e.SeatAllocationId).HasName("PK__SeatAllo__A30A31171D75DEDA");

            entity.HasIndex(e => new { e.ExamScheduleId, e.RoomId, e.SeatNumber }, "UK_SeatAllocation").IsUnique();

            entity.HasOne(d => d.ExamSchedule).WithMany(p => p.SeatAllocations)
                .HasForeignKey(d => d.ExamScheduleId)
                .HasConstraintName("FK__SeatAlloc__ExamS__25518C17");

            entity.HasOne(d => d.Room).WithMany(p => p.SeatAllocations)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__SeatAlloc__RoomI__2739D489");

            entity.HasOne(d => d.Student).WithMany(p => p.SeatAllocations)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__SeatAlloc__Stude__2645B050");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PK__Sections__80EF08728A21BFA3");

            entity.HasIndex(e => new { e.SectionCode, e.BatchId }, "UK_Section").IsUnique();

            entity.Property(e => e.SectionCode).HasMaxLength(20);
            entity.Property(e => e.SectionName).HasMaxLength(50);

            entity.HasOne(d => d.Batch).WithMany(p => p.Sections)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK__Sections__BatchI__5AEE82B9");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B996AC8BD53");

            entity.ToTable(tb => tb.HasTrigger("TR_Students_Audit"));

            entity.HasIndex(e => e.BatchId, "IX_Students_Batch");

            entity.HasIndex(e => e.CourseId, "IX_Students_Course");

            entity.HasIndex(e => e.RegistrationNumber, "IX_Students_RegistrationNumber");

            entity.HasIndex(e => e.AdmissionNumber, "UQ__Students__B468CC97AEDBB1E8").IsUnique();

            entity.HasIndex(e => e.RegistrationNumber, "UQ__Students__E88646023A526AAA").IsUnique();

            entity.Property(e => e.AadharNumber).HasMaxLength(20);
            entity.Property(e => e.AdmissionNumber).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.RollNumber).HasMaxLength(50);

            entity.HasOne(d => d.Batch).WithMany(p => p.Students)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK__Students__BatchI__7E37BEF6");

            entity.HasOne(d => d.Community).WithMany(p => p.Students)
                .HasForeignKey(d => d.CommunityId)
                .HasConstraintName("FK__Students__Commun__7C4F7684");

            entity.HasOne(d => d.Course).WithMany(p => p.Students)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Students__Course__7D439ABD");

            entity.HasOne(d => d.Regulation).WithMany(p => p.Students)
                .HasForeignKey(d => d.RegulationId)
                .HasConstraintName("FK__Students__Regula__00200768");

            entity.HasOne(d => d.Section).WithMany(p => p.Students)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK__Students__Sectio__7F2BE32F");
        });

        modelBuilder.Entity<StudentAdditionalInfo>(entity =>
        {
            entity.HasKey(e => e.StudentAdditionalInfoId).HasName("PK__StudentA__D54D0F8A3904EFE5");

            entity.ToTable("StudentAdditionalInfo");

            entity.HasIndex(e => e.StudentId, "UQ__StudentA__32C52B9843AA9FE5").IsUnique();

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.FatherName).HasMaxLength(200);
            entity.Property(e => e.GuardianName).HasMaxLength(200);
            entity.Property(e => e.MotherName).HasMaxLength(200);
            entity.Property(e => e.Pincode).HasMaxLength(10);
            entity.Property(e => e.State).HasMaxLength(100);

            entity.HasOne(d => d.QuotaType).WithMany(p => p.StudentAdditionalInfos)
                .HasForeignKey(d => d.QuotaTypeId)
                .HasConstraintName("FK__StudentAd__Quota__05D8E0BE");

            entity.HasOne(d => d.Student).WithOne(p => p.StudentAdditionalInfo)
                .HasForeignKey<StudentAdditionalInfo>(d => d.StudentId)
                .HasConstraintName("FK__StudentAd__Stude__04E4BC85");
        });

        modelBuilder.Entity<SystemAlert>(entity =>
        {
            entity.HasKey(e => e.SystemAlertId).HasName("PK__SystemAl__1281706B2056C500");

            entity.HasIndex(e => e.AlertSeverity, "IX_SystemAlerts_Severity");

            entity.Property(e => e.AcknowledgedDateTime).HasColumnType("datetime");
            entity.Property(e => e.AlertDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.AlertMessage).HasMaxLength(500);
            entity.Property(e => e.AlertSeverity).HasMaxLength(20);
            entity.Property(e => e.AlertType).HasMaxLength(50);
            entity.Property(e => e.IsAcknowledged).HasDefaultValue(false);
            entity.Property(e => e.IsResolved).HasDefaultValue(false);
            entity.Property(e => e.RelatedModule).HasMaxLength(100);
            entity.Property(e => e.RelatedRecordId).HasMaxLength(50);
            entity.Property(e => e.RelatedTable).HasMaxLength(100);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(500);
            entity.Property(e => e.ResolvedDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.AcknowledgedByNavigation).WithMany(p => p.SystemAlertAcknowledgedByNavigations)
                .HasForeignKey(d => d.AcknowledgedBy)
                .HasConstraintName("FK_SystemAlerts_AcknowledgedBy");

            entity.HasOne(d => d.RelatedUser).WithMany(p => p.SystemAlertRelatedUsers)
                .HasForeignKey(d => d.RelatedUserId)
                .HasConstraintName("FK_SystemAlerts_RelatedUser");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.SystemAlertResolvedByNavigations)
                .HasForeignKey(d => d.ResolvedBy)
                .HasConstraintName("FK_SystemAlerts_ResolvedBy");
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.ConfigurationId).HasName("PK__SystemCo__95AA53BB9AE16D6F");

            entity.ToTable("SystemConfiguration");

            entity.HasIndex(e => e.ConfigKey, "UQ__SystemCo__4A306784A5BEAC00").IsUnique();

            entity.Property(e => e.ConfigKey).HasMaxLength(100);
            entity.Property(e => e.DataType).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.HasKey(e => e.TestTypeId).HasName("PK__TestType__9BB876A6DCEF34AC");

            entity.HasIndex(e => e.TestCode, "UQ__TestType__0B0C35F797B5238F").IsUnique();

            entity.HasIndex(e => e.TestName, "UQ__TestType__2AF07A7D2CA4133C").IsUnique();

            entity.Property(e => e.MaxMark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TestCode).HasMaxLength(50);
            entity.Property(e => e.TestName).HasMaxLength(100);
        });

        modelBuilder.Entity<UserSessionLog>(entity =>
        {
            entity.HasKey(e => e.UserSessionLogId).HasName("PK__UserSess__37099E9307DCC28D");

            entity.ToTable("UserSessionLog", tb => tb.HasTrigger("TR_UserSessionLog_Monitor"));

            entity.HasIndex(e => e.LoginDateTime, "IX_UserSessionLog_LoginDateTime");

            entity.Property(e => e.FailureReason).HasMaxLength(250);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LoginDateTime).HasColumnType("datetime");
            entity.Property(e => e.LoginStatus).HasMaxLength(50);
            entity.Property(e => e.LogoutDateTime).HasColumnType("datetime");
            entity.Property(e => e.MachineName).HasMaxLength(100);
            entity.Property(e => e.SessionDuration).HasComputedColumnSql("(datediff(minute,[LoginDateTime],[LogoutDateTime]))", true);

            entity.HasOne(d => d.AdminUser).WithMany(p => p.UserSessionLogs)
                .HasForeignKey(d => d.AdminUserId)
                .HasConstraintName("FK_UserSessionLog_AdminUser");
        });

        modelBuilder.Entity<VwActiveUserSession>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_ActiveUserSessions");

            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LoginDateTime).HasColumnType("datetime");
            entity.Property(e => e.MachineName).HasMaxLength(100);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
