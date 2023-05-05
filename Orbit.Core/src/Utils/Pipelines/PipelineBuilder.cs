using StudioLE.Core.Patterns;

namespace Orbit.Core.Utils.Pipelines;

public class PipelineBuilder<TOutput> : IBuilder<Pipeline<TOutput>>
{
    private readonly List<PipelineStep> _steps = new();
    private Func<TOutput>? _onSuccess;
    private Func<TOutput>? _onFailure;

    public PipelineBuilder<TOutput> Then(PipelineStep step)
    {
        _steps.Add(step);
        return this;
    }

    public PipelineBuilder<TOutput> OnFailure(Func<TOutput> onFailure)
    {
        _onFailure = onFailure;
        return this;
    }

    public PipelineBuilder<TOutput> OnSuccess(Func<TOutput> onSuccess)
    {
        _onSuccess = onSuccess;
        return this;
    }

    public Pipeline<TOutput> Build()
    {
        if (_onSuccess is null)
            throw new("Failed to build pipeline. No success handler was provided.");
        if (_onFailure is null)
            throw new("Failed to build pipeline. No failure handler was provided.");
        return new(_steps, _onSuccess, _onFailure);
    }
}
