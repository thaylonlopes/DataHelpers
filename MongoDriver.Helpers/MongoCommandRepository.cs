using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDriver.Helpers.Interface;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Models;

namespace MongoDriver.Helpers;

/// <summary>
/// Repositório de comandos para MongoDB, encapsulando operações CRUD.
/// </summary>
public class MongoCommandRepository<T> : ICommandRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoCommandRepository(IMongoContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _collection = context.GetCollection<T>(typeof(T).Name);
    }

    /// <summary>
    /// Obtém um documento pelo seu identificador.
    /// </summary>
    /// <param name="key">O identificador do documento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O documento encontrado ou nulo.</returns>
    public async Task<T?> GetByIdAsync(object key, CancellationToken cancellationToken = default) =>
        await _collection.Find(Filters.Id<T>(key)).FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Lista todos os documentos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de documentos.</returns>
    public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await _collection.Find(FilterDefinition<T>.Empty).ToListAsync(cancellationToken);

    /// <summary>
    /// Lista documentos que atendem a um filtro específico.
    /// </summary>
    /// <param name="filter">Expressão de filtro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de documentos que atendem ao filtro.</returns>
    public async Task<List<T>> ListAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
        await _collection.Find(filter).ToListAsync(cancellationToken);

    /// <summary>
    /// Adiciona um novo documento.
    /// </summary>
    /// <param name="item">O documento a ser adicionado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public void Add(T item, CancellationToken cancellationToken = default) =>
        _collection.InsertOne(item, null, cancellationToken);

    /// <summary>
    /// Adiciona um novo documento de forma assíncrona.
    /// </summary>
    /// <param name="item">O documento a ser adicionado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task AddAsync(T item, CancellationToken cancellationToken = default) =>
        _collection.InsertOneAsync(item, new InsertOneOptions(), cancellationToken);

    /// <summary>
    /// Adiciona múltiplos documentos.
    /// </summary>
    /// <param name="items">Os documentos a serem adicionados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default) =>
        _collection.InsertMany(items, null, cancellationToken);

    /// <summary>
    /// Adiciona múltiplos documentos de forma assíncrona.
    /// </summary>
    /// <param name="items">Os documentos a serem adicionados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) =>
        _collection.InsertManyAsync(items, null, cancellationToken);

    /// <summary>
    /// Deleta um documento pelo seu identificador.
    /// </summary>
    /// <param name="key">O identificador do documento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public void Delete(object key, CancellationToken cancellationToken = default) =>
        _collection.DeleteOne(Filters.Id<T>(key), cancellationToken);

    /// <summary>
    /// Deleta documentos que atendem a um filtro específico.
    /// </summary>
    /// <param name="where">Expressão de filtro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) =>
        _collection.DeleteMany(where, cancellationToken);

    /// <summary>
    /// Deleta um documento pelo seu identificador de forma assíncrona.
    /// </summary>
    /// <param name="key">O identificador do documento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task DeleteAsync(object key, CancellationToken cancellationToken = default) =>
        _collection.DeleteOneAsync(Filters.Id<T>(key), cancellationToken);

    /// <summary>
    /// Deleta documentos que atendem a um filtro específico de forma assíncrona.
    /// </summary>
    /// <param name="where">Expressão de filtro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) =>
        _collection.DeleteManyAsync(where, cancellationToken);

    /// <summary>
    /// Atualiza um documento.
    /// </summary>
    /// <param name="item">O documento a ser atualizado.</param>
    public void Update(T item)
    {
        var key = GetKey(item);
        if (key != null)
        {
            _collection.ReplaceOne(Filters.Id<T>(key), item);
        }
    }

    /// <summary>
    /// Atualiza um documento de forma assíncrona.
    /// </summary>
    /// <param name="item">O documento a ser atualizado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task UpdateAsync(T item, CancellationToken cancellationToken = default)
    {
        var key = GetKey(item);
        return key != null
            ? _collection.ReplaceOneAsync(Filters.Id<T>(key), item, cancellationToken: cancellationToken)
            : Task.CompletedTask;
    }

    /// <summary>
    /// Atualiza parcialmente um documento.
    /// </summary>
    /// <param name="item">O documento a ser atualizado.</param>
    public void UpdatePartial(object item)
    {
        if (item is T typedItem)
        {
            var key = GetKey(item);
            if (key != null)
            {
                _collection.ReplaceOne(Filters.Id<T>(key), typedItem);
            }
        }
    }

    /// <summary>
    /// Atualiza parcialmente um documento de forma assíncrona.
    /// </summary>
    /// <param name="item">O documento a ser atualizado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default)
    {
        if (item is T typedItem)
        {
            var key = GetKey(item);
            if (key != null)
            {
                return _collection.ReplaceOneAsync(Filters.Id<T>(key), typedItem, cancellationToken: cancellationToken);
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Atualiza múltiplos documentos.
    /// </summary>
    /// <param name="items">Os documentos a serem atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default) =>
        _collection.BulkWrite(CreateUpdates(items), null, cancellationToken);

    /// <summary>
    /// Atualiza múltiplos documentos de forma assíncrona.
    /// </summary>
    /// <param name="items">Os documentos a serem atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) =>
        _collection.BulkWriteAsync(CreateUpdates(items), null, cancellationToken);

    /// <summary>
    /// Retorna uma página de resultados com base no número da página e no tamanho da página.
    /// </summary>
    public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default)
    {
        var totalItems = await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty, cancellationToken: cancellationToken);
        var query = _collection.Find(FilterDefinition<T>.Empty);

        if (!string.IsNullOrEmpty(sortField))
        {
            query = ascending ? query.Sort(Builders<T>.Sort.Ascending(sortField)) : query.Sort(Builders<T>.Sort.Descending(sortField));
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pageNumber, pageSize, totalItems);
    }

    /// <summary>
    /// Retorna uma página de resultados com base no número da página, tamanho da página e filtro aplicado.
    /// </summary>
    public async Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default)
    {
        var totalItems = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var query = _collection.Find(filter);

        if (!string.IsNullOrEmpty(sortField))
        {
            query = ascending ? query.Sort(Builders<T>.Sort.Ascending(sortField)) : query.Sort(Builders<T>.Sort.Descending(sortField));
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pageNumber, pageSize, totalItems);
    }

    /// <summary>
    /// Retorna uma página de resultados com base no número da página, tamanho da página e ordenações opcionais.
    /// </summary>
    public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default)
    {
        var totalItems = await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty, cancellationToken: cancellationToken);
        var query = _collection.Find(FilterDefinition<T>.Empty);

        if (sortDefinitions != null && sortDefinitions.Count > 0)
        {
            var sortBuilder = Builders<T>.Sort;
            var combinedSortpartial = new List<SortDefinition<T>>();
            foreach (var sortDefinition in sortDefinitions)
            {
                var sort = sortDefinition.Ascending ? sortBuilder.Ascending(sortDefinition.Field) : sortBuilder.Descending(sortDefinition.Field);
                combinedSortpartial.Add(sort);
            }
            SortDefinition<T> combinedSort = sortBuilder.Combine(combinedSortpartial);
            query = query.Sort(combinedSort);
        }

        var items = await query.Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pageNumber, pageSize, totalItems);
    }

    /// <summary>
    /// Retorna uma página de resultados com base no número da página, tamanho da página, filtro aplicado e ordenações opcionais.
    /// </summary>
    public async Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default)
    {
        var totalItems = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var query = _collection.Find(filter);

        if (sortDefinitions != null && sortDefinitions.Count > 0)
        {
            var sortBuilder = Builders<T>.Sort;
            var combinedSortpartial = new List<SortDefinition<T>>();
            foreach (var sortDefinition in sortDefinitions)
            {
                var sort = sortDefinition.Ascending ? sortBuilder.Ascending(sortDefinition.Field) : sortBuilder.Descending(sortDefinition.Field);
                combinedSortpartial.Add(sort);
            }
            SortDefinition<T> combinedSort = sortBuilder.Combine(combinedSortpartial);
            query = query.Sort(combinedSort);
        }

        var items = await query.Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pageNumber, pageSize, totalItems);
    }

    private static IEnumerable<WriteModel<T>> CreateUpdates(IEnumerable<T> items) =>
        (from item in items
         let key = GetKey(item)
         where key is not null
         select new ReplaceOneModel<T>(Filters.Id<T>(key), item)).Cast<WriteModel<T>>().ToList();

    private static object? GetKey(object item) => item.GetType().GetProperty("Id")?.GetValue(item, default);
}
