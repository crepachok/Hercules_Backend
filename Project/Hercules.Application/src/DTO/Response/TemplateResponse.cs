public record TemplateResponse
{
    public int TemplateId { get; init; }
    public string Name { get; init; }
    public IEnumerable<int> ExerciseIds { get; init; } = [];
    public TemplateResponse(int templateId, string name, IEnumerable<int> ids)
        => (TemplateId, Name, ExerciseIds) = (templateId, name, ids.ToArray());
}