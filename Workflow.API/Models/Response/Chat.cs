namespace Workflow.API.Models.Response;

public record Chat()
{
    public string? Conversation
    {
        get; set;
    }
}