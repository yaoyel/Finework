using System;
using FineWork.Security.Repos.Aef;
namespace FineWork.Web.WebApi.Security
{
    public class AccountViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } 

        public string PhoneNumber { get; set; }

        public string SecurityStamp { get; set; }
    }

    public static class AccountViewExtensions
    {
        public static AccountViewModel ToViewModel(this AccountEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AccountViewModel()
            {
                Id = entity.Id,
                Name = entity.Name, 
                PhoneNumber = entity.PhoneNumber,
                SecurityStamp=entity.SecurityStamp
            };
             
            return result;
        }
    }

}