using System.Collections;
using System.Data.Entity;
using System.Linq;
using GigaPro.Persistency.EntityFramework.Extensions;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Collections
{
    public class EntityFrameworkBatchedEnumerator : IDataEnumerator
    {
        private readonly int _batchSize;
        private readonly DbContext _dbContext;
        private readonly ExtendedEntityType _dbContextType;
        private int _offset;
        private IList _pageSet;
        private int _position = -1;

        public EntityFrameworkBatchedEnumerator(DbContext dbContext, ExtendedEntityType dbContextType, int batchSize)
        {
            _dbContext = dbContext;
            _dbContextType = dbContextType;
            _batchSize = batchSize;
        }

        public bool MoveNext()
        {
            var output = false;

            if (_position == -1 || (_position == _batchSize - 1))
            {
                LoadNextBatch();
            }

            if ((_position + 1) < _pageSet.Count)
            {
                _position++;
                _offset++;
                output = true;
            }

            return output;
        }

        public void Reset()
        {
            _position = -1;
            _offset = 0;
            LoadNextBatch();
        }

        public object Current => _pageSet[_position];

        public void Dispose()
        {
        }

        private void LoadNextBatch()
        {
            _position = -1;

            IQueryable dbSet = _dbContext.Set(_dbContextType.Type);
            _pageSet = dbSet.LoadPage(_dbContextType, _offset, _batchSize);
        }
    }
}