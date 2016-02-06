using System.Data.Entity;
using System.Data.SqlClient;
using GigaPro.Persistency.EntityFramework.Configuration;

namespace GigaPro.Persistency.EntityFramework
{
    public class GigaSpacesDbContext : DbContext
    {
        private readonly IModelContextConfigurer _modelContextConfigurer;

        public GigaSpacesDbContext(string connectionString, IModelContextConfigurer modelContextConfigurer) : base (new SqlConnection(connectionString), true)
        {
            _modelContextConfigurer = modelContextConfigurer;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            _modelContextConfigurer.OnModelCreating(modelBuilder);
        }
    }
}