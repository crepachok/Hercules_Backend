public record TemplateMinimalResponse
{
    public int TemplateId { get; init; }
    public string Name { get; init; }
    public TemplateMinimalResponse(int templateId, string name)
        => (TemplateId, Name) = (templateId, name);
}