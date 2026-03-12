using ACGCET_Admin.Models;
namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteMandatoryCourseViewModel : DeleteCourseBaseViewModel
    {
        public DeleteMandatoryCourseViewModel(AcgcetDbContext dbContext) : base(dbContext, "Mandatory") { }
    }
}
