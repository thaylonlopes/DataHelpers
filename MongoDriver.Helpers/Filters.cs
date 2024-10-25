using MongoDB.Driver;

namespace MongoDriver.Helpers
{

    public static class Filters
    {
        /// <summary>
        /// Cria um filtro para buscar documentos com o valor do campo Id especificado.
        /// </summary>
        public static FilterDefinition<T> Id<T>(object value) => Builders<T>.Filter.Eq(nameof(Id), value);
        /// <summary>
        /// Cria um filtro para buscar documentos com o valor do campo "_id" especificado.
        /// </summary>
        public static FilterDefinition<T> ById<T>(object value) => Builders<T>.Filter.Eq("_id", value);

        /// <summary>
        /// Cria um filtro para buscar documentos onde um campo especificado é igual ao valor fornecido.
        /// </summary>
        public static FilterDefinition<T> Equals<T>(string field, object value) => Builders<T>.Filter.Eq(field, value);

        /// <summary>
        /// Cria um filtro para buscar documentos onde um campo especificado não é igual ao valor fornecido.
        /// </summary>
        public static FilterDefinition<T> NotEquals<T>(string field, object value) => Builders<T>.Filter.Ne(field, value);

        /// <summary>
        /// Cria um filtro para buscar documentos onde um campo especificado é maior que o valor fornecido.
        /// </summary>
        public static FilterDefinition<T> GreaterThan<T>(string field, object value) => Builders<T>.Filter.Gt(field, value);

        /// <summary>
        /// Cria um filtro para buscar documentos onde um campo especificado é maior ou igual ao valor fornecido.
        /// </summary>
        public static FilterDefinition<T> GreaterThanOrEquals<T>(string field, object value) => Builders<T>.Filter.Gte(field, value);

        /// <summary>
        /// Cria um filtro para buscar documentos onde um campo especificado é menor que o valor fornecido.
        /// </summary>
        public static FilterDefinition<T> LessThan<T>(string field, object value) => Builders<T>.Filter.Lt(field, value);

        /// <summary>
        /// Cria um filtro para buscar documentos onde um campo especificado é menor ou igual ao valor fornecido.
        /// </summary>
        public static FilterDefinition<T> LessThanOrEquals<T>(string field, object value) => Builders<T>.Filter.Lte(field, value);
    }

}
