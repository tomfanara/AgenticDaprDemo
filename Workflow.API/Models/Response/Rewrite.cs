﻿namespace Workflow.API.Models.Response;

public record Rewrite()
{
    public string? Conversation
    {
        get; set;
    }
}