namespace Orbit.Core.Utils.Pipelines;

/// <summary>
/// A pipeline of steps that are executed provided the previous step was successful.
/// </summary>
/// <typeparam name="TOutput">The type of the output at the end of the pipeline.</typeparam>
public class Pipeline<TOutput>
{
    private readonly IReadOnlyCollection<PipelineStep> _steps;
    // TODO: Remove onSuccess and onFailure.
    private readonly Func<TOutput> _onSuccess;
    private readonly Func<TOutput> _onFailure;

    /// <summary>
    /// The constructor used by <see cref="PipelineBuilder{TOutput}"/> to construct a <see cref="Pipeline{TOutput}"/>.
    /// </summary>
    internal Pipeline(IReadOnlyCollection<PipelineStep> steps, Func<TOutput> onSuccess, Func<TOutput> onFailure)
    {
        _steps = steps;
        _onSuccess = onSuccess;
        _onFailure = onFailure;
    }

    /// <summary>
    /// Execute the pipeline.
    /// </summary>
    public TOutput Execute()
    {
        bool isSuccess = false;
        if (!_steps.Any())
            throw new("Pipeline has no steps.");
        foreach (PipelineStep step in _steps)
        {
            isSuccess = step.Invoke();
            if (!isSuccess)
                break;
        }
        return isSuccess
            ? _onSuccess.Invoke()
            : _onFailure.Invoke();
    }
}
