#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070


using Microsoft.SemanticKernel;
using Dapr.Workflow;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using Workflow.API.Features.Plugins;


namespace Workflow.API.Features.Activities
{
    public class GroupChatActivity : WorkflowActivity<string, string>
    {

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        public AgentGroupChat chat;
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        public GroupChatActivity()
        {

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var builder = Kernel.CreateBuilder()
                                 .AddOllamaChatCompletion(
                                  modelId: "llama3.1",
                                  endpoint: new Uri("http://localhost:11434"));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            //builder.AddLocalTextEmbeddingGeneration();
            Kernel kernel = builder.Build();

            Kernel toolKernel = kernel.Clone();
            toolKernel.Plugins.AddFromType<ClipboardAccessPlugin>();
            toolKernel.Plugins.AddFromType<InventoryPlugin>();
            toolKernel.Plugins.AddFromType<AccountingPlugin>();

            Console.WriteLine("Defining agents...");

            const string HumanResourcesManagerName = "Khloe";
            const string InventoryManagerName = "Jenny";

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            ChatCompletionAgent agentReviewer =
                new()
                {
                    Name = HumanResourcesManagerName,
                    Instructions =
                        """
                    Your responsiblity is to provide employee content as in current employees.  
                    Return information that has nouns with a synonym of employees.
                    Always reply with current employee content using available tools or functions {{get_employees}}.                  
                    Always copy satisfactory content to the clipboard using available tools and inform user.

                    RULES:
                    - Only identify suggestions that are specific and actionable.
                    - Address the user.
                    """,
                    Kernel = toolKernel,
                    Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                };
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            ChatCompletionAgent agentWriter =
                new()
                {
                    Name = InventoryManagerName,
                    Instructions =
                        """
                    Your responsiblity is to provide inventory content as in current inventory.
                    Return information that has nouns with a synonym of inventory.
                    Always reply with current inventory content using available tools or functions {{get_inventory}}.
                    Always copy satisfactory content to the clipboard using available tools and inform user.

                    RULES:
                    - Only identify suggestions that are specific and actionable.
                    - Address the user.
                    """,
                    Kernel = toolKernel,
                };
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            KernelFunction selectionFunction =
                AgentGroupChat.CreatePromptFunctionForStrategy(
                    $$$"""
                //Examine the provided RESPONSE and choose the next participant.
                State only the name of the chosen participant without explanation.
                //Never choose the participant named in the RESPONSE.

                Choose only from these participants:
                - {{{HumanResourcesManagerName}}}
                - {{{InventoryManagerName}}}

                //Always follow these rules when choosing the next participant:
                - If RESPONSE is user input and input is employee related, the author name is {{{HumanResourcesManagerName}}}'s turn.
                - If RESPONSE is user input and input is inventory related, the author name is {{{InventoryManagerName}}}'s turn.

                RESPONSE:
                {{$lastmessage}}
                """,
                    safeParameterNames: "lastmessage");
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            const string TerminationToken = "yes";

            //If content is satisfactory, respond with a single word without explanation: {{{TerminationToken}}}.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            KernelFunction terminationFunction =
                AgentGroupChat.CreatePromptFunctionForStrategy(
                    $$$"""
                Examine the RESPONSE and determine whether the content has been deemed satisfactory.                
                If specific suggestions are being provided, it is not satisfactory.
                If no correction is suggested, it is satisfactory.

                RESPONSE:
                {{$lastmessage}}
                """,
                    safeParameterNames: "lastmessage");
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            ChatHistoryTruncationReducer historyReducer = new(1);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            chat =
                new(agentReviewer, agentWriter)
                {
                    ExecutionSettings = new AgentGroupChatSettings
                    {
                        SelectionStrategy =
                            new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                            {
                                // Always start with the editor agent.
                                //InitialAgent = agentReviewer,
                                // Save tokens by only including the final response
                                HistoryReducer = historyReducer,
                                // The prompt variable name for the history argument.
                                HistoryVariableName = "lastmessage",
                                // Returns the entire result value as a string.
                                //ResultParser = (result) => result.GetValue<string>() ?? agentReviewer.Name
                            },
                        TerminationStrategy =
                            new KernelFunctionTerminationStrategy(terminationFunction, kernel)
                            {
                                // Only evaluate for editor's response
                                //Agents = [agentReviewer],
                                // Save tokens by only including the final response
                                HistoryReducer = historyReducer,
                                // The prompt variable name for the history argument.
                                HistoryVariableName = "lastmessage",
                                // Limit total number of turns
                                MaximumIterations = 1,
                                // Customer result parser to determine if the response is "yes"
                                ResultParser = (result) => result.GetValue<string>()?.Contains(TerminationToken, StringComparison.OrdinalIgnoreCase) ?? false
                            }
                    }
                };
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            Console.WriteLine("Ready!");


        }

        public override async Task<string> RunAsync(WorkflowActivityContext context, string messages)
        {
            Console.WriteLine("Receiving message...");
            bool isComplete = false;
           
                //Console.WriteLine();
                //Console.Write("> ");
                string input = messages;
            if (string.IsNullOrWhiteSpace(input))
            {
                //return await Task.FromResult("Empty prompt please enter a request");
            }
            input = input.Trim();
            if (input.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                isComplete = true;
                return await Task.FromResult("EXIT");

            }

                if (input.Equals("RESET", StringComparison.OrdinalIgnoreCase))
                {
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    await this.chat.ResetAsync();
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    Console.WriteLine("[Converation has been reset]");
                return await Task.FromResult("[Converation has been reset]");
                
            }

              

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                this.chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            this.chat.IsComplete = false;
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            string fullMessage = string.Empty;
            try
            {
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                await foreach (ChatMessageContent response in this.chat.InvokeAsync())
                {
                    fullMessage = "[" + response.AuthorName.ToUpperInvariant() + "]" + " " + response.Content;
                    Console.WriteLine();
                    Console.WriteLine($"{response.AuthorName.ToUpperInvariant()}:{Environment.NewLine}{response.Content}");
                }
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                 
            }
            catch (HttpOperationException exception)
            {
                Console.WriteLine(exception.Message);
                if (exception.InnerException != null)
                {
                    Console.WriteLine(exception.InnerException.Message);
                    if (exception.InnerException.Data.Count > 0)
                    {
                        Console.WriteLine(JsonSerializer.Serialize(exception.InnerException.Data, new JsonSerializerOptions() { WriteIndented = true }));
                    }
                }
            }
            //} while (!isComplete);          

            //Chat chat = new Chat { Conversation = fullMessage };
            return await Task.FromResult(fullMessage);
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
