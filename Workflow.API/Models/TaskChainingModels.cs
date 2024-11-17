namespace Workflow.API.Models;

public class TaskChainingModels
{
    public record Request(string instanceId, string eventName);

    public record Result(bool successful, string message);

    public record Notification(string message);

    public class Steps
    {
        public Steps(int currentIndex, Step[] step)
        {
            this.currentIndex = currentIndex;
            this.step = step;
        }

        public int currentIndex
        {
            get; set;
        }

        public Step[] step
        {
            get; set;
        }
    }
    }

public class Step
{
    public int Index
    {
        get; set;
    }

    public string? BusinessLogicEndpoint
    {
        get; set;
    }

    public string? CancelLogicEndpoint
    {
        get; set;
    }

    public string? TaskName
    {
        get; set;
    }
}