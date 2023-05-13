using StudioLE.Core.Patterns;

namespace Orbit.Core.Utils.Pipelines;

/// <summary>
/// Build a <see cref="Pipeline{TOutput}"/>.
/// </summary>
/// <typeparam name="TOutput">The type of the output at the end of the pipeline.</typeparam>
public class PipelineBuilder<TOutput> : IBuilder<Pipeline<TOutput>>
{
    private readonly List<PipelineStep> _steps = new();
    private Func<TOutput>? _onSuccess;
    private Func<TOutput>? _onFailure;

    /// <summary>
    /// Add a step to the pipeline.
    /// </summary>
    public PipelineBuilder<TOutput> Then(PipelineStep step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Set the failure handler.
    /// </summary>
    public PipelineBuilder<TOutput> OnFailure(Func<TOutput> onFailure)
    {
        _onFailure = onFailure;
        return this;
    }

    /// <summary>
    /// Set the success handler.
    /// </summary>
    public PipelineBuilder<TOutput> OnSuccess(Func<TOutput> onSuccess)
    {
        _onSuccess = onSuccess;
        return this;
    }

    /// <summary>
    /// Build the pipeline.
    /// </summary>
    public Pipeline<TOutput> Build()
    {
        if (_onSuccess is null)
            throw new("Failed to build pipeline. No success handler was provided.");
        if (_onFailure is null)
            throw new("Failed to build pipeline. No failure handler was provided.");
        return new(_steps, _onSuccess, _onFailure);
    }
}
