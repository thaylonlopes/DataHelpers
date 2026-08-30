using System.Data;

namespace Dapper.Helpers.Interfaces
{
    public interface IDbContext
    {
        IDbConnection Connection { get; }
    }
}

