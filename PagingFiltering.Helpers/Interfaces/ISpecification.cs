using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Interfaces
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
        bool IsSatisfiedBy(T entity);
    }
}
