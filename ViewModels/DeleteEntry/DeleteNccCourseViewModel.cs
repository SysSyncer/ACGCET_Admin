using ACGCET_Admin.Models;
namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteNccCourseViewModel : DeleteCourseBaseViewModel
    {
        public DeleteNccCourseViewModel(AcgcetDbContext dbContext) : base(dbContext, "NCC") { }
    }
}
