using Microsoft.Extensions.Logging;

namespace TournamentManager.ModelValidators;

/// <summary>
/// An enumeration of <see cref="FactType"/>s for <see cref="Fact{TFactId}"/>s.
/// </summary>
public enum FactType
{
    Critical,
    Error,
    Warning
}

/// <summary>
/// The interface used for <see cref="Fact{TFactId}"/>s.
/// </summary>
/// <typeparam name="TFactId"></typeparam>
public interface IFact<TFactId>
{
    /// <summary>
    /// The delegate which is invoked to perform the check.
    /// </summary>
    Func<CancellationToken, Task<FactResult>> CheckAsync { get; set; }
    /// <summary>
    /// <see cref="Fact{TFactId}"/>s will be processed, if <see cref="Enabled"/> is true.
    /// </summary>
    bool Enabled { get; set; }
    /// <summary>
    /// The <see cref="Exception"/> which was thrown while <see cref="CheckAsync"/>ing.
    /// </summary>
    Exception? Exception { get; set; }
    /// <summary>
    /// The identifier for the <see cref="Fact{TFactId}"/>
    /// </summary>
    TFactId Id { get; set; }
    /// <summary>
    /// The field names that were checked. Empty list as default.
    /// </summary>
    IEnumerable<string> FieldNames { get; set; }
    /// <summary>
    /// The error message for the case where the <see cref="CheckAsync"/> was not successful.
    /// </summary>
    string Message { get; set; }
    /// <summary>
    /// <c>true</c> indicates, that the <see cref="CheckAsync"/> was run and completed without an <see cref="Exception"/>.
    /// </summary>
    bool IsChecked { get; set; }
    /// <summary>
    /// <c>true</c> indicates, that the <see cref="CheckAsync"/> was successfully completed.
    /// </summary>
    bool Success { get; set; }
    /// <summary>
    /// Resets all <see cref="Fact{TFactId}"/> properties to their default values.
    /// </summary>
    void Reset();
    /// <summary>
    /// The type of the fact.
    /// </summary>
    FactType Type { get; set; }
}

/// <summary>
/// An instance of this class is returned by the Check function of a <see cref="Fact{TFactId}"/> instance.
/// </summary>
public class FactResult
{
    /// <summary>
    /// <c>true</c> if the Check was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The message in case the Check was not successful.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// The <see cref="Fact{TFactId}"/> instance to be checked.
/// </summary>
/// <typeparam name="TFactId"></typeparam>
public class Fact<TFactId> : IFact<TFactId>
{
    /// <summary>
    /// The identifier for this <see cref="Fact{TFactId}"/>.
    /// </summary>
    public TFactId Id { get; set; } = default!;
    /// <summary>
    /// The field name that was checked. Empty string as default.
    /// </summary>
    public IEnumerable<string> FieldNames { get; set; } = new List<string>();
    /// <summary>
    /// The type of the fact.
    /// </summary>
    public FactType Type { get; set; }

    /// <summary>
    /// The error message for the case where the <see cref="CheckAsync"/> was not successful.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// <see cref="Fact{TFactId}"/>s will be processed, if <see cref="Enabled"/> is true.
    /// </summary>
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// <c>true</c> indicates, that the <see cref="CheckAsync"/> was successfully completed.
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// <c>true</c> indicates, that the <see cref="CheckAsync"/> was run and completed without an <see cref="Exception"/>.
    /// </summary>
    public bool IsChecked { get; set; }
    /// <summary>
    /// Resets all <see cref="Fact{TFactId}"/> properties to their default values.
    /// </summary>
    public virtual void Reset()
    {
        Success = IsChecked = false;
        Exception = null;
    }
    /// <summary>
    /// The <see cref="Exception"/> which was thrown while <see cref="CheckAsync"/>ing.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// The delegate which is invoked to perform the check.
    /// </summary>
    public virtual Func<CancellationToken, Task<FactResult>> CheckAsync { get; set; } = token => Task.FromResult(new FactResult { Success = true });
}

/// <summary>
/// The abstract class for model validators.
/// </summary>
/// <typeparam name="TModel">The model to validate.</typeparam>
/// <typeparam name="TData">Data needed for validation, e.g. a Tuple.</typeparam>
/// <typeparam name="TFactId"></typeparam>
public abstract class AbstractValidator<TModel, TData, TFactId>
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="data">The data needed for validation.</param>
    protected AbstractValidator(TModel model, TData data)
    {
        Logger = AppLogging.CreateLogger(GetType().Name);
        Model = model;
        Data = data;
    }

    /// <summary>
    /// Gets the <typeparamref name="TModel"/> of the validator.
    /// </summary>
    public TModel Model { get; }
    /// <summary>
    /// Gets the <typeparamref name="TData"/> needed for validation.
    /// </summary>
    public TData Data { get; protected set; }
    /// <summary>
    /// Gets the <see cref="ILogger"/> for validation.
    /// </summary>
    public ILogger Logger { get; protected set; }
    /// <summary>
    /// Gets the <see cref="HashSet{T}"/> of <see cref="Facts"/> for the validator.
    /// </summary>
    public HashSet<Fact<TFactId>> Facts { get; } = new();

    /// <summary>
    /// Get all facts of a certain <see cref="FactType"/>.
    /// </summary>
    /// <param name="factType">The requested <see cref="FactType"/></param>
    /// <returns>Returns all facts of the requested <see cref="FactType"/>.</returns>
    public IEnumerable<Fact<TFactId>> GetFactsOfType(FactType factType)
    {
        return Facts.Where(f => f.Type == factType);
    }

    /// <summary>
    /// Invokes the Check for a fact with the <typeparamref name="TFactId"/> identifier.
    /// </summary>
    /// <param name="id">The identifier of the <see cref="Fact{TFactId}"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Return an instance of the <see cref="Fact{TFactId}"/> that was checked.</returns>
    public virtual async Task<Fact<TFactId>> CheckAsync(TFactId id, CancellationToken cancellationToken)
    {
        var fact = Facts.First(f => f.Id!.Equals(id));

        if (!fact.Enabled) return fact;

        try
        {
            var factResult = await fact.CheckAsync(cancellationToken);
            fact.Success = factResult.Success;
            fact.Message = factResult.Message;
            fact.IsChecked = true;
            Logger.LogTrace("Fact '{factId}': {factSuccess}", id, fact.Success);
        }
        catch (Exception e)
        {
            fact.Success = false;
            fact.IsChecked = true;
            fact.Message = string.Empty;
            fact.Exception = e;
            Logger.LogCritical(e, "Fact '{factId}': {factSuccess}", id, fact.Success);
        }

        return fact;
    }
    /// <summary>
    /// Invokes the Check for all <see cref="Facts"/> of the validator.
    /// </summary>
    /// <returns>Returns a list of instances of <see cref="Fact{TFactId}"/>s that were checked.</returns>
    public virtual async Task<List<Fact<TFactId>>> CheckAsync(CancellationToken cancellationToken)
    {
        // first process critical facts
        var facts = GetFactsOfType(FactType.Critical).ToList();
        foreach (var fact in facts)
        {
            await CheckAsync(fact.Id, cancellationToken);
        }
        // stop after any not successful critical facts
        if (facts.Any(f => f is { IsChecked: true, Success: false })) return facts;

        // next process error facts
        facts = GetFactsOfType(FactType.Error).ToList();
        foreach (var fact in facts)
        {
            await CheckAsync(fact.Id, cancellationToken);
        }
        // stop after any not successful error facts
        if (facts.Any(f => f.IsChecked && !f.Success)) return facts;

        // Process all other facts
        foreach (var fact in Facts.Where(f => !f.IsChecked))
        {
            await CheckAsync(fact.Id, cancellationToken);
        }

        return Facts.Where(f => f.IsChecked).ToList();
    }

    /// <summary>
    /// Invokes a reset for all <see cref="Facts"/>.
    /// </summary>
    /// <returns>Returns a list of all <see cref="Facts"/> after the reset.</returns>
    public virtual List<Fact<TFactId>> Reset()
    {
        foreach (var fact in Facts)
        {
            fact.Reset();
        }

        return Facts.ToList();
    }

    /// <summary>
    /// Gets a list of enabled facts.
    /// </summary>
    /// <returns>Returns a list of enabled facts.</returns>
    public virtual List<Fact<TFactId>> GetEnabledFacts()
    {
        return Facts.Where(f => f.Enabled).ToList();
    }

    /// <summary>
    /// Gets a list of facts which are enabled, checked and not successfully tested.
    /// </summary>
    /// <returns>Returns a list of facts which are enabled, checked and not successfully tested.</returns>
    public virtual List<Fact<TFactId>> GetFailedFacts()
    {
        return Facts.Where(f => f.IsChecked && (!f.Success || f.Exception != null)).ToList();
    }
}
