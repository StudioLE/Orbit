namespace Orbit.Core.Utils.Pipelines;

public class Pipeline<TOutput>
{
    private readonly IReadOnlyCollection<PipelineStep> _steps;
    private readonly Func<TOutput> _onSuccess;
    private readonly Func<TOutput> _onFailure;

    internal Pipeline(IReadOnlyCollection<PipelineStep> steps, Func<TOutput> onSuccess, Func<TOutput> onFailure)
    {
        _steps = steps;
        _onSuccess = onSuccess;
        _onFailure = onFailure;
    }

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
