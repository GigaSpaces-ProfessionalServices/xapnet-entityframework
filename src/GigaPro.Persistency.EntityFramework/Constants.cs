namespace GigaPro.Persistency.EntityFramework
{
    public static class Constants
    {
        public static class Defaults
        {
            public const int LoadBatchSize = 100;
            public const int InitialLoadThreadPoolSize = 10;
        }

        public static class ConfigurationKeys
        {
            public const string InitialLoadThreadPoolSize = "InitialLoadThreadPoolSize";
            public const string ConnectionString = "connectionString";
            public const string ConfigurerClass = "configurerClass";
            public const string LoadBatchSize = "LoadBatchSize";
        }
    }
}