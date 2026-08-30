using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDriver.Helpers.Interface;
using MongoDriver.Helpers.Interface.Context;
using System.Linq.Expressions;

namespace MongoDriver.Helpers
{
    public class MongoQueryRepository<T> : IQueryRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoQueryRepository(IMongoContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context)); _collection = context.GetCollection<T>(typeof(T).Name);
        }
        /// <summary>
        /// Obtém a coleção como IQueryable.
        /// </summary>
        public IQueryable<T> Queryable => _collection.AsQueryable();

        /// <summary>
        /// Obtém a coleção como IQueryable.
        /// </summary>
        /// <returns>A coleção como IQueryable.</returns>

        public IQueryable<T> AsQueryable() => _collection.AsQueryable();

        /// <summary>
        /// Verifica se há algum documento na coleção.
        /// </summary>
        /// <returns>Verdadeiro se houver algum documento; caso contrário, falso.</returns>
        public bool Any() => Queryable.Any();

        /// <summary>
        /// Verifica se há algum documento na coleção que atenda ao filtro.
        /// </summary>
        /// <param name="where">Expressão de filtro.</param>
        /// <returns>Verdadeiro se houver algum documento que atenda ao filtro; caso contrário, falso.</returns>

        public bool Any(Expression<Func<T, bool>> where) => Queryable.Where(where).Any();
        /// <summary>
        /// Verifica de forma assíncrona se há algum documento na coleção.
        /// </summary>
        /// <returns>Uma Task que representa a operação assíncrona. O resultado contém verdadeiro se houver algum documento; caso contrário, falso.</returns>

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default(CancellationToken)) => await Queryable.AnyAsync(cancellationToken);

        /// <summary>
        /// Verifica de forma assíncrona se há algum documento na coleção que atenda ao filtro.
        /// </summary>
        /// <param name="where">Expressão de filtro.</param>
        /// <returns>Uma Task que representa a operação assíncrona. O resultado contém verdadeiro se houver algum documento que atenda ao filtro; caso contrário, falso.</returns>

        public Task<bool> AnyAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default(CancellationToken)) => Queryable.Where(where).AnyAsync(cancellationToken);

        /// <summary>
        /// Conta o número de documentos na coleção.
        /// </summary>
        /// <returns>O número de documentos na coleção.</returns>

        public long Count() => Queryable.LongCount();

        /// <summary>
        /// Conta o número de documentos na coleção que atendam ao filtro.
        /// </summary>
        /// <param name="where">Expressão de filtro.</param>
        /// <returns>O número de documentos que atendam ao filtro.</returns>

        public long Count(Expression<Func<T, bool>> where) => Queryable.Where(where).LongCount();

        public Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken)) => Queryable.LongCountAsync(cancellationToken);
        /// <summary>
        /// Conta de forma assíncrona o número de documentos na coleção.
        /// </summary>
        /// <returns>Uma Task que representa a operação assíncrona. O resultado contém o número de documentos na coleção.</returns>

        public Task<long> CountAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default(CancellationToken)) => Queryable.Where(where).LongCountAsync(cancellationToken);

        /// <summary>
        /// Conta de forma assíncrona o número de documentos na coleção que atendam ao filtro.
        /// </summary>
        /// <param name="where">Expressão de filtro.</param>
        /// <returns>Uma Task que representa a operação assíncrona. O resultado contém o número de documentos que atendam ao filtro.</returns>

        public T Get(object key, CancellationToken cancellationToken = default(CancellationToken)) => _collection.Find(Filters.Id<T>(key)).SingleOrDefault(cancellationToken);

        /// <summary>
        /// Obtém de forma assíncrona um documento pelo seu identificador.
        /// </summary>
        /// <param name="key">O identificador do documento.</param>
        /// <returns>Uma Task que representa a operação assíncrona. O resultado contém o documento encontrado.</returns>

        public Task<T> GetByIdAsync(object key, CancellationToken cancellationToken = default(CancellationToken)) => _collection.Find(Filters.Id<T>(key)).SingleOrDefaultAsync(cancellationToken);

        /// <summary>
        /// Lista todos os documentos.
        /// </summary>
        /// <returns>Lista de documentos.</returns>

        public IEnumerable<T> List() => Queryable.ToList();

        /// <summary>
        /// Obtém de forma assíncrona um documento pelo seu identificador.
        /// </summary>
        /// <param name="key">O identificador do documento.</param>
        /// <returns>Uma Task que representa a operação assíncrona. O resultado contém o documento encontrado.</returns>
        public async Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default(CancellationToken)) => await Queryable.ToListAsync(cancellationToken);

    }
}
