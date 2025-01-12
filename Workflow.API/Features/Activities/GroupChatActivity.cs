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
using static Workflow.API.Models.TaskChainingModels;
using Workflow.API.Models.Response;
using Dapr.Client;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;


namespace Workflow.API.Features.Activities
{
    public class GroupChatActivity : WorkflowActivity<string, string>
    {
        private readonly DaprClient daprClient;

        public GroupChatActivity(DaprClient daprClient)
        {
            this.daprClient = daprClient;
        }

        public override async Task<string> RunAsync(WorkflowActivityContext context, string messages)
        {
            var builder = Kernel.CreateBuilder()
                             .AddOllamaChatCompletion(
                              modelId: "llama3",
                              endpoint: new Uri("http://localhost:11434"));
            builder.AddLocalTextEmbeddingGeneration();
            Kernel kernel = builder.Build();

            Kernel toolKernel = kernel.Clone();
            toolKernel.Plugins.AddFromType<ClipboardAccess>();

            string fullMessage = "";

            Console.WriteLine("Defining agents...");

            const string ReviewerName = "Reviewer";
            const string WriterName = "Writer";

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            ChatCompletionAgent agentReviewer =
                new()
                {
                    Name = ReviewerName,
                    Instructions =
                        """
                            Your responsiblity is to review and identify how to improve user provided content.
                            If the user has providing input or direction for content already provided, specify how to address this input.
                            Never directly perform the correction or provide example.
                            Once the content has been updated in a subsequent response, you will review the content again until satisfactory.
                            Always copy satisfactory content to the clipboard using available tools and inform user.

                            RULES:
                            - Only identify suggestions that are specific and actionable.
                            - Verify previous suggestions have been addressed.
                            - Never repeat previous suggestions.
                            """,
                    Kernel = toolKernel,
                    Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                };
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            ChatCompletionAgent agentWriter =
                new()
                {
                    Name = WriterName,
                    Instructions =
                        """
                            Your sole responsiblity is to rewrite content according to review suggestions.

                            - Always apply all review direction.
                            - Always revise the content in its entirety without explanation.
                            - Never address the user.
                            """,
                    Kernel = kernel,
                };
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            KernelFunction selectionFunction =
                AgentGroupChat.CreatePromptFunctionForStrategy(
                    $$$"""
                        Examine the provided RESPONSE and choose the next participant.
                        State only the name of the chosen participant without explanation.
                        Never choose the participant named in the RESPONSE.

                        Choose only from these participants:
                        - {{{ReviewerName}}}
                        - {{{WriterName}}}

                        Always follow these rules when choosing the next participant:
                        - If RESPONSE is user input, it is {{{ReviewerName}}}'s turn.
                        - If RESPONSE is by {{{ReviewerName}}}, it is {{{WriterName}}}'s turn.
                        - If RESPONSE is by {{{WriterName}}}, it is {{{ReviewerName}}}'s turn.

                        RESPONSE:
                        {{$lastmessage}}
                        """,
                    safeParameterNames: "lastmessage");
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            const string TerminationToken = "yes";

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            KernelFunction terminationFunction =
                AgentGroupChat.CreatePromptFunctionForStrategy(
                    $$$"""
                        Examine the RESPONSE and determine whether the content has been deemed satisfactory.
                        If content is satisfactory, respond with a single word without explanation: {{{TerminationToken}}}.
                        If specific suggestions are being provided, it is not satisfactory.
                        If no correction is suggested, it is satisfactory.

                        RESPONSE:
                        {{$lastmessage}}
                        """,
                    safeParameterNames: "lastmessage");
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // Removed unnecessary assignment to historyReducer
            // ChatHistoryTruncationReducer historyReducer = new(1);

            Chat chat = new Chat { Conversation = "hello from group chat!" };
            return await Task.FromResult(chat.Conversation);
        }

        private sealed class ClipboardAccess
        {
            [KernelFunction]
            [Description("Copies the provided content to the clipboard.")]
            public static void SetClipboard(string content)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return;
                }

                using Process clipProcess = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "clip",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                    });

                clipProcess.StandardInput.Write(content);
                clipProcess.StandardInput.Close();
            }
        }
    }
}
