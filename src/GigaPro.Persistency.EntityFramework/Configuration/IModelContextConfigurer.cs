using System.Data.Entity;

namespace GigaPro.Persistency.EntityFramework.Configuration
{
    /// <summary>
    /// Configures the EntityFramework database context.
    /// </summary>
    public interface IModelContextConfigurer
    {
        /// <summary>
        /// Builds the EntityFramework object model.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}