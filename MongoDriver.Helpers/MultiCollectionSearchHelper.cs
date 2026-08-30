using MongoDB.Bson;
using MongoDB.Driver;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Models;

namespace MongoDriver.Helpers
{
    public class MultiCollectionHelper<T>
    {
        private readonly List<IMongoCollection<T>> _collections;

        public MultiCollectionHelper(IMongoContext context, params string[] collectionNames)
        {
            _collections = new List<IMongoCollection<T>>();
            foreach (var name in collectionNames)
            {
                _collections.Add(context.GetCollection<T>(name));
            }
        }

        public IEnumerable<IMongoCollection<T>> Collections => _collections;

        /// <summary>
        /// Realiza uma busca em várias coleções e combina os resultados.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <returns>Lista combinada dos resultados das coleções.</returns>
        public async Task<List<T>> SearchAsync(FilterDefinition<T> filter)
        {
            var combinedResults = new List<T>();
            foreach (var collection in _collections)
            {
                var results = await collection.Find(filter).ToListAsync();
                combinedResults.AddRange(results);
            }
            return combinedResults;
        }

        /// <summary> 
        /// Realiza uma busca agregada em várias coleções e combina os resultados.
        /// </summary>
        /// <param name="produtoId">O ID do item.</param> 
        /// <param name="pipeline">O pipeline de agregação.</param>
        /// <returns>Um item detalhado </returns>
        public async Task<BsonDocument> SearchAsync(string id, BsonDocument[] pipeline) =>
            await _collections[0]
                .Aggregate<BsonDocument>(pipeline)
                .FirstOrDefaultAsync();
        /// <summary>
        /// Atualiza documentos em várias coleções com base em um filtro e documento atualizado.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <param name="update">Documento atualizado.</param>
        /// <returns>Uma Task que representa a operação assíncrona.</returns>
        public async Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            foreach (var collection in _collections)
            {
                await collection.UpdateManyAsync(filter, update);
            }
        }

        /// <summary>
        /// Deleta documentos em várias coleções com base em um filtro.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <returns>Uma Task que representa a operação assíncrona.</returns>
        public async Task DeleteAsync(FilterDefinition<T> filter)
        {
            foreach (var collection in _collections)
            {
                await collection.DeleteManyAsync(filter);
            }
        }

        /// <summary>
        /// Insere documentos em várias coleções.
        /// </summary>
        /// <param name="documents">Documentos a serem inseridos.</param>
        /// <returns>Uma Task que representa a operação assíncrona.</returns>
        public async Task InsertAsync(IEnumerable<T> documents)
        {
            foreach (var collection in _collections)
            {
                await collection.InsertManyAsync(documents);
            }
        }

        /// <summary>
        /// Conta documentos em várias coleções com base em um filtro.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <returns>O total de documentos encontrados.</returns>
        public async Task<long> CountAsync(FilterDefinition<T> filter)
        {
            long totalCount = 0;
            foreach (var collection in _collections)
            {
                totalCount += await collection.CountDocumentsAsync(filter);
            }
            return totalCount;
        }

        /// <summary>
        /// Realiza uma busca avançada em várias coleções com suporte a ordenação e limite de resultados.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <param name="sort">Definição de ordenação.</param>
        /// <param name="limit">Número máximo de resultados a serem retornados.</param>
        /// <returns>Lista combinada dos resultados das coleções.</returns>
        public async Task<List<T>> AdvancedSearchAsync(FilterDefinition<T> filter, SortDefinition<T> sort, int limit)
        {
            var combinedResults = new List<T>();
            foreach (var collection in _collections)
            {
                var results = await collection.Find(filter)
                                              .Sort(sort)
                                              .Limit(limit)
                                              .ToListAsync();
                combinedResults.AddRange(results);
            }
            return combinedResults;
        }

        /// <summary>
        /// Realiza uma busca em várias coleções filtrando por um intervalo de datas.
        /// </summary>
        /// <param name="startDate">Data inicial do intervalo.</param>
        /// <param name="endDate">Data final do intervalo.</param>
        /// <param name="dateField">Campo de data a ser filtrado.</param>
        /// <returns>Lista combinada dos resultados das coleções.</returns>
        public async Task<List<T>> SearchByDateRangeAsync(DateTime startDate, DateTime endDate, string dateField)
        {
            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.Gte(dateField, startDate),
                Builders<T>.Filter.Lte(dateField, endDate)
            );

            return await SearchAsync(filter);
        }

        /// <summary>
        /// Realiza uma busca em várias coleções com uma projeção personalizada.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <param name="projection">Projeção personalizada.</param>
        /// <returns>Lista combinada dos resultados das coleções com a projeção aplicada.</returns>
        public async Task<List<BsonDocument>> SearchWithProjectionAsync(FilterDefinition<T> filter, ProjectionDefinition<T> projection)
        {
            var combinedResults = new List<BsonDocument>();
            foreach (var collection in _collections)
            {
                var results = await collection.Find(filter)
                                              .Project(projection)
                                              .ToListAsync();
                combinedResults.AddRange(results);
            }
            return combinedResults;
        }

        /// <summary>
        /// Realiza uma busca paginada avançada em várias coleções.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <param name="sort">Definição de ordenação.</param>
        /// <param name="pageNumber">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Uma Task que representa a operação assíncrona. O resultado contém os itens paginados.</returns>
        public async Task<PagedResult<T>> AdvancedPaginatedSearchAsync(FilterDefinition<T> filter, SortDefinition<T> sort, int pageNumber, int pageSize)
        {
            var combinedResults = new List<T>();
            long totalItems = 0;
            foreach (var collection in _collections)
            {
                var results = await collection.Find(filter)
                                              .Sort(sort)
                                              .Skip((pageNumber - 1) * pageSize)
                                              .Limit(pageSize)
                                              .ToListAsync();

                combinedResults.AddRange(results);
                totalItems += await collection.CountDocumentsAsync(filter);
            }

            return new PagedResult<T>(combinedResults, pageNumber, pageSize, totalItems);
        }

        /// <summary>
        /// Verifica se já existe um documento não deletado suavemente em várias coleções.
        /// </summary>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções.</param>
        /// <returns>Verdadeiro se existir um documento não deletado suavemente; caso contrário, falso.</returns>
        public async Task<bool> ExistsAsync(FilterDefinition<T> filter, string softDeleted = "IsDeleted")
        {
            var combinedFilter = Builders<T>.Filter.And(filter, Builders<T>.Filter.Eq(softDeleted, false));
            foreach (var collection in _collections)
            {
                var count = await collection.CountDocumentsAsync(combinedFilter);
                if (count > 0) return true;
            }
            return false;
        }

        /// <summary>
        /// Insere um documento se ele não existir em várias coleções.
        /// </summary>
        /// <param name="documents">Documentos a serem inseridos.</param>
        /// <param name="filter">Filtro a ser aplicado em todas as coleções para verificar duplicidade.</param>
        /// <returns>Uma Task que representa a operação assíncrona.</returns>
        public async Task InsertIfNotExistsAsync(IEnumerable<T> documents, FilterDefinition<T> filter)
        {
            if (!await ExistsAsync(filter))
            {
                await InsertAsync(documents);
            }
        }

    }

}

