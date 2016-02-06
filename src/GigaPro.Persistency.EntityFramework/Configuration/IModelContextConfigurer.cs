using System.Data.Entity;

namespace GigaPro.Persistency.EntityFramework.Configuration
{
    public interface IModelContextConfigurer
    {
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}