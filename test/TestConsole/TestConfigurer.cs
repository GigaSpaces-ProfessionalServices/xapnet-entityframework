using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using GigaPro.Persistency.EntityFramework.Configuration;
using TestConsole.Models;

namespace TestConsole
{
    public class TestConfigurer : IModelContextConfigurer {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var entityTypeConfiguration = modelBuilder.Entity<TestDataSet>();
            entityTypeConfiguration.ToTable("TestDataSets");
            entityTypeConfiguration.HasKey(m => m.Id).Property(m =>m.SomeValue).IsRequired().IsMaxLength();
        }
    }
}