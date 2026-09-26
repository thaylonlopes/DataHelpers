namespace MongoDriver.Helpers.Interface;

/// <summary>
/// Contrato unificado agregando comandos e consultas (CQRS) sobre uma coleção de documentos no MongoDB.
/// </summary>
/// <typeparam name="T">O tipo da entidade gerenciada.</typeparam>
public interface IRepositoryBase<T> : ICommandRepository<T>, IQueryRepository<T> where T : class
{
}