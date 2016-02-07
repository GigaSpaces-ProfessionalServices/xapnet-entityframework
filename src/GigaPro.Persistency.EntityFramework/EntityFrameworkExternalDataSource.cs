using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Text;
using GigaPro.Persistency.EntityFramework.Collections;
using GigaPro.Persistency.EntityFramework.Configuration;
using GigaPro.Persistency.EntityFramework.Extensions;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework
{
    public class EntityFrameworkExternalDataSource : AbstractExternalDataSource
    {
        private DbContext _dbContext;
        private IEnumerable<ExtendedEntityType> _dbContextTypes;
        private int _loadBatchSize = Constants.Defaults.LoadBatchSize;
        private int _initialLoadThreadPoolSize = Constants.Defaults.InitialLoadThreadPoolSize;
        private Assembly _modelLibrary;

        public EntityFrameworkExternalDataSource()
        {
        }

        public EntityFrameworkExternalDataSource(DbContext dbContext)
        {
            _dbContext = dbContext;
            _modelLibrary = _dbContext.GetType().Assembly;
        }

        public override void Init(Dictionary<string, string> properties)
        {
            base.Init(properties);

            if (_dbContext == null)
            {
                var connectionString = GetProperty(Constants.ConfigurationKeys.ConnectionString);
                var modelContextConfigurer = InstantiateClass(GetProperty(Constants.ConfigurationKeys.ConfigurerClass));

                _loadBatchSize = GetIntProperty(Constants.ConfigurationKeys.LoadBatchSize,
                    Constants.Defaults.LoadBatchSize);
                _initialLoadThreadPoolSize = GetIntProperty(Constants.ConfigurationKeys.InitialLoadThreadPoolSize,
                    Constants.Defaults.InitialLoadThreadPoolSize);

                _modelLibrary = modelContextConfigurer.GetType().Assembly;

                _dbContext = new GigaSpacesDbContext(connectionString, modelContextConfigurer);

                _dbContext.Configuration.LazyLoadingEnabled = false;
                _dbContext.Configuration.ProxyCreationEnabled = false;
                _dbContext.Configuration.AutoDetectChangesEnabled = false;
            }

            IdentifyConfiguredTypes();
        }

        private IModelContextConfigurer InstantiateClass(string configurerFullName)
        {
            var type = Type.GetType(configurerFullName);

            if (type == null) throw new Exception($"Failed to find ConfigurerClass [{configurerFullName}]");

            return (IModelContextConfigurer) Activator.CreateInstance(type);
        }

        private void IdentifyConfiguredTypes()
        {
            var metadataWorkspace = ((IObjectContextAdapter) _dbContext).ObjectContext.MetadataWorkspace;
            var entityMetaData = metadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);

            IList<ExtendedEntityType> entityTypes = new List<ExtendedEntityType>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var entityType in entityMetaData)
            {
                entityTypes.Add(new ExtendedEntityType(_modelLibrary.GetType(entityType.FullName, true, true), entityType));
            }

            _dbContextTypes = entityTypes;
        }

        public override IDataEnumerator InitialLoad()
        {
            IList<IDataEnumerator> enumerators = new List<IDataEnumerator>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var dbContextType in _dbContextTypes)
            {
                enumerators.Add(new EntityFrameworkBatchedEnumerator(_dbContext.Set(dbContextType.Type), dbContextType, _loadBatchSize));
            }
            
            return new ConcurrentMultiDataEnumerator(enumerators, _loadBatchSize, _initialLoadThreadPoolSize);
        }

        public override IDataEnumerator GetEnumerator(Query query)
        {
            return _dbContext.GigaSpaceQuery(_dbContextTypes, query, _loadBatchSize);
        }

        public override void ExecuteBulk(IList<BulkItem> bulk)
        {
            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var bulkItem in bulk)
                    {
                        ProcessItem(bulkItem);
                    }

                    _dbContext.SaveChanges();
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }
        }

        private void ProcessItem(BulkItem bulkItem)
        {
            var item = bulkItem.Item;
            var dbSet = _dbContext.Set(item.GetType());
            switch (bulkItem.Operation)
            {
                case BulkOperation.Remove:
                    dbSet.Remove(item);
                    break;
                case BulkOperation.Update:
                    _dbContext.Entry(item).State = EntityState.Modified;
                    //                            _dbContext.Merge(item);
                    break;
                case BulkOperation.Write:
                    dbSet.Add(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}