public static class TemplateMapper
{
    public static TemplateMinimalResponse ToMinimalResponse(this TemplateEntity template)
    {
        return new(template.Id, template.Name);
    }
    public static TemplateResponse ToResponse(this TemplateEntity template)
    {
        return new(template.Id, template.Name, template.Exercises.Select(e => e.Id));
    }
    public static TemplateDetailedResponse ToDetailedResponse(this TemplateEntity template)
    {
        return new(template.Id, template.Name, template.Exercises.Select(e => e.ToMinimalResponse()));
    }
}   