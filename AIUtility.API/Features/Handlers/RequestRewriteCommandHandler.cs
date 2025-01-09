
namespace AIUtility.API.Features.Microagent.Handlers;

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
using System.Threading.Tasks;
using AIUtility.API.Models.Response;
using MediatR;

    
    public class RequestRewriteCommandHandler() : IRequestHandler<RequestRewriteHandlerRequest, Chat>
    {
        public async Task<Chat> Handle(RequestRewriteHandlerRequest request, CancellationToken cancellationToken)
        {
            var question = request.Messages;// "I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales.";

            var builder = Kernel.CreateBuilder()
                              .AddOllamaChatCompletion(
                               modelId: "llama3",
                               endpoint: new Uri("http://localhost:11434"));
            builder.AddLocalTextEmbeddingGeneration();
            Kernel kernel = builder.Build();

            string fullMessage = "";

            // Read the content of the file
            string fileContent = "I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales.";
            //string fileContent = input;
            // Split the text into lines
            var lines = new List<string> { question };

            // Define the maximum number of tokens per chunk
            int maxTokensPerChunk = 300;

            // Split the text into chunks
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);

       
            // get the embeddings generator service
            var embeddingGeneratorChunked = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
            var memoryChunked = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGeneratorChunked);

            const string MemoryCollectionNameChunked = "originalPrompt";

            int i = 0;
            foreach (var chunk in chunks)
            {
                await memoryChunked.SaveInformationAsync(MemoryCollectionNameChunked, id: "info" + i, text: chunk);
                i++;
            }

            TextMemoryPlugin memoryPluginChunked = new(memoryChunked);

            // Import the text memory plugin into the Kernel.
            kernel.ImportPluginFromObject(memoryPluginChunked);

            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = null,
                Temperature = 0,
                TopP = 0
            };

            var promptChunked = @"You are a helpful assistant that generates search queries "
                    + "based on a single input query. "
                    + "Perform query decomposition and break it down into distinct sub questions that you need to "
                    + "answer in order to answer the original question "
                    + "If there are acronyms and words you are not familiar with, do not try to rephrase them. "
                    + "Return sub questions in CSV content. {{Recall}}";

            var arguments = new KernelArguments(settings)
            {
                { "input", question },
                { "collection", MemoryCollectionNameChunked }
            };
            
            //Console.WriteLine($"Phi-3 response (using semantic memory and document chunking).");

            var response = kernel.InvokePromptStreamingAsync(promptChunked, arguments);
            await foreach (var result in response)
            {
                //Console.Write(result);
                fullMessage += result.ToString();
            }

            int index = fullMessage.IndexOf("CSV format:");

            string subQuestions = fullMessage.Substring(index + 17, 82);

            string[] questions = subQuestions.Split(new char[] { ',' });

            Console.Write(subQuestions);

            //Console.Write(questions[4]);

            //return questions;

            Chat chat = new Chat{ Conversation = questions };
            return chat;
        }
    }
