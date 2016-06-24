using System.Data.Entity;

namespace FineWork.Data.Aef
{
    public class FineWorkDbContextInitializer : DropCreateDatabaseAlways<FineWorkDbContext>
    {
        public static int InitializerExecutionCount { get; private set; }

        protected override void Seed(FineWorkDbContext dbContext)
        {
            InitializerExecutionCount++;
        }
    }
}