using System.ComponentModel.DataAnnotations;

namespace FineWork.Colla
{
    public enum ForumSection
    {
        [Display(Description = "使命")] Mission = 1,

        [Display(Description = "愿景")] Vision,

        [Display(Description = "价值观")] Values,

        [Display(Description = "战略")] Strategy,

        [Display(Description = "组织治理机制")] OrgGovernance
    }
}