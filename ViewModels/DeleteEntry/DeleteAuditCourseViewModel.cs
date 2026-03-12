using ACGCET_Admin.Models;
namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteAuditCourseViewModel : DeleteCourseBaseViewModel
    {
        public DeleteAuditCourseViewModel(AcgcetDbContext dbContext) : base(dbContext, "Audit") { }
    }
}
