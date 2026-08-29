namespace DataMapping.Helpers.Common;

/// <summary>
/// Representa o resultado de uma operação contendo sucesso ou erro sem lançar exceções de controle de fluxo.
/// </summary>
/// <typeparam name="T">O tipo do valor de sucesso.</typeparam>
public readonly struct Result<T>
{
    /// <summary>
    /// Indica se a operação foi executada com sucesso.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação resultou em falha.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// O valor retornado em caso de sucesso.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Mensagem ou descrição do erro em caso de falha.
    /// </summary>
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Cria uma instância de sucesso contendo o valor.
    /// </summary>
    /// <param name="value">O valor resultante.</param>
    /// <returns>Um resultado de sucesso.</returns>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Cria uma instância de falha contendo a mensagem de erro.
    /// </summary>
    /// <param name="error">A descrição do erro.</param>
    /// <returns>Um resultado de falha.</returns>
    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Conversão implícita de um valor para <see cref="Result{T}"/>.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}

