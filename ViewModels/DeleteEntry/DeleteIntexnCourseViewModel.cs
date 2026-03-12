using ACGCET_Admin.Models;
namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteIntexnCourseViewModel : DeleteCourseBaseViewModel
    {
        public DeleteIntexnCourseViewModel(AcgcetDbContext dbContext) : base(dbContext, "Intexn") { } 
    }
}
