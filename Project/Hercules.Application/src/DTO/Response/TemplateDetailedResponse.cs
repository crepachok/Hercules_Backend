public record TemplateDetailedResponse
{
    public int TemplateId { get; init; }
    public string Name { get; init; }
    public IEnumerable<ExerciseMinimalResponse> Exercises { get; init; }
    public TemplateDetailedResponse(int templateId, string name, IEnumerable<ExerciseMinimalResponse> exercises)
        => (TemplateId, Name, Exercises) = (templateId, name, exercises);
}