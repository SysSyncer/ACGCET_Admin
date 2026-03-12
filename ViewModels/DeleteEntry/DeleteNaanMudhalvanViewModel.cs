using ACGCET_Admin.Models;
namespace ACGCET_Admin.ViewModels.DeleteEntry
{
    public partial class DeleteNaanMudhalvanViewModel : DeleteCourseBaseViewModel
    {
        public DeleteNaanMudhalvanViewModel(AcgcetDbContext dbContext) : base(dbContext, "Naan Mudhalvan") { }
    }
}
