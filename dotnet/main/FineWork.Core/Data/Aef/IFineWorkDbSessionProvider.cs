using AppBoot.Repos;

namespace FineWork.Data.Aef
{
    public interface IFineWorkDbSessionProvider : ISessionProvider<FineWorkDbContext>
    {
    }
}