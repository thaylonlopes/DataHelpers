using Dapper.Helpers.Interfaces;
using Dapper.Helpers.Models;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Helpers.Repositories
{
    public class DapperCommandRepository<T> : ICommandRepository<T> where T : class
    {
        private readonly IDbContext _context;

        public DapperCommandRepository(IDbContext context)
        {
            _context = context;
        }
        public void Add(T item, CancellationToken cancellationToken = default)
        {
            var properties = GetProperties();
            var query = $"INSERT INTO {typeof(T).Name}s ({string.Join(", ", properties)}) VALUES (@{string.Join(", @", properties)})";
            _context.Connection.Execute(query, item);
        }

        public async Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            var query = $"INSERT INTO {typeof(T).Name}s ({string.Join(", ", GetProperties())}) VALUES (@{string.Join(", @", GetProperties())})";
            await _context.Connection.ExecuteAsync(query, item);
        }

        public void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var query = $"INSERT INTO {typeof(T).Name}s ({string.Join(", ", GetProperties())}) VALUES (@{string.Join(", @", GetProperties())})";
            _context.Connection.Execute(query, items, transaction: null, commandTimeout: null, CommandType.Text);
        }

        public async Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var query = $"INSERT INTO {typeof(T).Name}s ({string.Join(", ", GetProperties())}) VALUES (@{string.Join(", @", GetProperties())})";
            await _context.Connection.ExecuteAsync(query, items, transaction: null, commandTimeout: null, CommandType.Text);
        }

        public void Delete(object key, CancellationToken cancellationToken = default)
        {
            var query = $"DELETE FROM {typeof(T).Name}s WHERE Id = @Id";
            _context.Connection.Execute(query, new { Id = key });
        }

        public void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            var query = $"DELETE FROM {typeof(T).Name}s WHERE {GenerateWhereClause(where)}";
            _context.Connection.Execute(query);
        }

        public async Task DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            var query = $"DELETE FROM {typeof(T).Name}s WHERE Id = @Id";
            await _context.Connection.ExecuteAsync(query, new { Id = key });
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            var query = $"DELETE FROM {typeof(T).Name}s WHERE {GenerateWhereClause(where)}";
            await _context.Connection.ExecuteAsync(query);
        }
        public void Update(T item)
        {
            var query = $"UPDATE {typeof(T).Name}s SET {GenerateUpdateClause(item)} WHERE Id = @Id";
            _context.Connection.Execute(query, item);
        }

        public async Task UpdateAsync(T item, CancellationToken cancellationToken = default)
        {
            var query = $"UPDATE {typeof(T).Name}s SET {GenerateUpdateClause(item)} WHERE Id = @Id";
            await _context.Connection.ExecuteAsync(query, item);
        }

        public void UpdatePartial(object item)
        {
            var updateQuery = GeneratePartialUpdateQuery(item);
            _context.Connection.Execute(updateQuery, item);
        }

        public async Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default)
        {
            var updateQuery = GeneratePartialUpdateQuery(item);
            await _context.Connection.ExecuteAsync(updateQuery, item);
        }

        public void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
            {
                var query = GenerateUpdateQuery(item);
                _context.Connection.Execute(query, item);
            }
        }

        public async Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
            {
                var query = GenerateUpdateQuery(item);
                await _context.Connection.ExecuteAsync(query, item);
            }
        }

        public async Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default)
        {
            var query = GeneratePagedQuery(pageNumber, pageSize, sortField ?? string.Empty, ascending, filter);
            var items = await _context.Connection.QueryAsync<T>(query);
            var totalItems = items.Count();
            return new PagedResult<T>(items.ToList(), pageNumber, pageSize, totalItems);
        }

        public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default)
        {
            var query = GeneratePagedQuery(pageNumber, pageSize, sortField ?? string.Empty, ascending);
            var items = await _context.Connection.QueryAsync<T>(query);
            var totalItems = await _context.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {typeof(T).Name}s");
            return new PagedResult<T>(items.ToList(), pageNumber, pageSize, totalItems);
        }

        public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default)
        {
            var query = GeneratePagedQuery(pageNumber, pageSize, string.Empty, true, sortDefinitions: sortDefinitions);
            var items = await _context.Connection.QueryAsync<T>(query);
            var totalItems = await _context.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {typeof(T).Name}s");
            return new PagedResult<T>(items.ToList(), pageNumber, pageSize, totalItems);
        }

        public async Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default)
        {
            var query = GeneratePagedQuery(pageNumber, pageSize, string.Empty, true, filter, sortDefinitions);
            var items = await _context.Connection.QueryAsync<T>(query);
            var totalItems = await _context.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {typeof(T).Name}s WHERE {GenerateWhereClause(filter)}");
            return new PagedResult<T>(items.ToList(), pageNumber, pageSize, totalItems);
        }
        private string GeneratePartialUpdateQuery(object item)
        {
            var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var keyProperty = properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            if (keyProperty == null)
                throw new InvalidOperationException("The item does not have an Id property.");

            var setClause = new StringBuilder();
            foreach (var property in properties.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            {
                if (setClause.Length > 0)
                    setClause.Append(", ");
                setClause.Append($"{property.Name} = @{property.Name}");
            }

            var tableName = typeof(T).Name;
            return $"UPDATE {tableName}s SET {setClause} WHERE Id = @Id";
        }
        public static string GenerateUpdateQuery(T item)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var keyProperty = properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            if (keyProperty == null)
                throw new InvalidOperationException("The item does not have an Id property.");
            var setClause = new StringBuilder();
            foreach (var property in properties.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            {
                if (setClause.Length > 0) setClause.Append(", ");
                setClause.Append($"{property.Name} = @{property.Name}");
            }
            var tableName = typeof(T).Name;
            return $"UPDATE {tableName}s SET {setClause} WHERE Id = @Id";
        }


        public static string[] GetProperties()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Select(p => p.Name)
                            .ToArray();
        }

        public static string GenerateUpdateClause(T item)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var setClause = new StringBuilder();

            foreach (var property in properties)
            {
                if (setClause.Length > 0)
                    setClause.Append(", ");
                setClause.Append($"{property.Name} = @{property.Name}");
            }

            return setClause.ToString();
        }

        public static string GeneratePagedQuery(int pageNumber, int pageSize, string sortField, bool ascending, Expression<Func<T, bool>>? filter = null, List<SortDefinition>? sortDefinitions = null)
        {
            var whereClause = filter != null ? GenerateWhereClause(filter) : "1=1";
            var orderByClause = GenerateOrderByClause(sortField, ascending, sortDefinitions);

            var skip = (pageNumber - 1) * pageSize;
            return $"SELECT * FROM {typeof(T).Name}s WHERE {whereClause} ORDER BY {orderByClause} OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY";
        }

        private static string ExpressionToString(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            if (expression is ConstantExpression constantExpression)
            {
                return constantExpression.Value?.ToString() ?? string.Empty;
            }
            if (expression is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                return unaryMemberExpression.Member.Name;
            }
            throw new NotSupportedException("Unsupported expression type.");
        }


        protected static string GenerateOrderByClause(string sortField, bool ascending, List<SortDefinition>? sortDefinitions)
        {
            if (sortDefinitions != null && sortDefinitions.Count > 0)
            {
                var orderByParts = sortDefinitions.Select(sd => $"{sd.Field} {(sd.Ascending ? "ASC" : "DESC")}");
                return string.Join(", ", orderByParts);
            }

            return $"{sortField} {(ascending ? "ASC" : "DESC")}";
        }
        protected static string GenerateWhereClause(Expression<Func<T, bool>> where)
        {
            var expression = (BinaryExpression)where.Body;
            var left = ExpressionToString(expression.Left);
            var right = ExpressionToString(expression.Right);
            var operand = GetSqlOperand(expression.NodeType);

            return $"{left} {operand} {right}";
        }

        protected static string GetSqlOperand(ExpressionType expressionType)
        {
            return expressionType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Unsupported expression type: {expressionType}")
            };

        }
    }
}
