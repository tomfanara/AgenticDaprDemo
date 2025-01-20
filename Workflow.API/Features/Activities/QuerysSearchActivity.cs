// Copyright (c) Alegeus Technologies, LLC. All rights reserved.

namespace Workflow.API.Features.Activities;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Dapr.Workflow;
using System.Reactive;
using System.Threading.Tasks;
using System;
using System.Linq;


public class QuerySearchActivity : WorkflowActivity<string, string[]>
{
    public async override Task<string[]> RunAsync(WorkflowActivityContext context, string input)
    {
        var question = input;
    
        string[] wordsToSearch = { "employees", "inventory", "sales" };

        var foundWords = wordsToSearch.Where(word => question.Contains(word)).ToList();

        string[] subQuestions = null;

            if (foundWords.Any())
            {
                Console.WriteLine("Found words:");
                foreach (var word in foundWords)
                {
                    Console.WriteLine(word);
                }
            }
            else
            {
                Console.WriteLine("No words found.");
            }

        
        return subQuestions;

    }
}