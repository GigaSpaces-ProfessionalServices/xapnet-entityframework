using System;
using System.Collections.Generic;
using GigaPro.Persistency.EntityFramework;
using GigaSpaces.Core.Persistency;
using TestConsole.Models;

namespace TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
//            var type = typeof (TestDataSet);
//
//            return;


            var edfs = new EntityFrameworkExternalDataSource();
            var properties = new Dictionary<string, string>
            {
                {Constants.ConfigurationKeys.ConfigurerClass, "TestConsole.TestConfigurer, TestConsole"},
                {Constants.ConfigurationKeys.ConnectionString, "Data Source=(localdb)\\v11.0;Initial Catalog=TestInitialLoadDataBase;Integrated Security=true"},
//                {Constants.ConfigurationKeys.ConnectionString, "Data Source=(localdb)\\v11.0;Initial Catalog=TestInitialLoadDataBase;Integrated Security=true;MultipleActiveResultSets=true"},
                {Constants.ConfigurationKeys.LoadBatchSize, "2"},
                {Constants.ConfigurationKeys.InitialLoadThreadPoolSize, "10"}
            };

            edfs.Init(properties);

            var dataEnumerator = edfs.GetEnumerator(new Query(null, "FROM TestConsole.Models.TestDataSet "));

            while(dataEnumerator.MoveNext())
            {
                if (dataEnumerator.Current != null)
                {
                    Console.WriteLine("---Not Null: " + dataEnumerator.Current);
                }
                else
                {
                    Console.WriteLine("*** NULL ***");
                }
            } //while (dataEnumerator.MoveNext());

            Console.ReadKey();
        }
    }
}