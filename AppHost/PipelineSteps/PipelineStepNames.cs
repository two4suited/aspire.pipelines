namespace AppHost.PipelineSteps;

public enum PipelineStepNames
{
    Finisher,
    Driver,
    CreateStep1,
    CreateStep2
}

public static class PipelineStepNamesExtensions
{
    public static string ToStepName(this PipelineStepNames step)
    {
        return step switch
        {
            PipelineStepNames.Finisher => "finisher",
            PipelineStepNames.Driver => "driver",
            PipelineStepNames.CreateStep1 => "create-step1",
            PipelineStepNames.CreateStep2 => "create-step2",
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };
    }
}

