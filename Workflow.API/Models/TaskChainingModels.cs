namespace Workflow.API.Models;

public class TaskChainingModels
{
    public record Request(string instanceId, string eventName);

    public record Result(bool successful, string message);

    public record Notification(string message);

    public class Prompts
    {
        public Prompts(int currentIndex, Prompt[] step)
        {
            this.currentIndex = currentIndex;
            this.step = step;
        }

        public int currentIndex
        {
            get; set;
        }

        public Prompt[] step
        {
            get; set;
        }
    }
}

public class Prompt
{
    public int Index
    {
        get; set;
    }

    public string? AgentAPIEndpoint
    {
        get; set;
    }

    public string? AgentName
    {
        get; set;
    }

    public string? Message
    {
        get; set;
    }
}