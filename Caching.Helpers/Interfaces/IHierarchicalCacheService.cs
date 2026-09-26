namespace Caching.Helpers.Interfaces;

/// <summary>
/// Contrato para serviços de caching hierárquico multinível combinando L1 (memória local rápida) e L2 (armazenamento distribuído).
/// </summary>
public interface IHierarchicalCacheService : ICacheService
{
}
