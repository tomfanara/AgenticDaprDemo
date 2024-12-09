#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Sales.API.Features.Microagent.Personas;
using Sales.API.Models.Response;

namespace Sales.API.Features.Microagent.Actors
{
    public class SalesService : ISalesService
    {
        public async Task<Chat> GetSales(string prompt)
        {
            //create a persona Khloe
            Persona persona = new Persona
            {
                Name = "Carlos",
                Tone = "creative",
                Style = "efficient",
                Traits = new List<string> { "empathetic", "helpful", "approachable" }
            };

            var personaSettings = new PersonaSettings();
            ApplyPersona(personaSettings, persona);

            string greeting = personaSettings.GenerateResponse(prompt);

            var question = "Summarize the current iPad sales";
            Console.WriteLine("");
            Console.WriteLine($"This program will answer the following question: {question}");
            Console.WriteLine("1st approach will be to ask the question directly to the Phi-3 model.");
            Console.WriteLine("2nd approach will be to add facts to a semantic memory and ask the question again");
            Console.WriteLine("");

            // Create a chat completion service
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Carlos's prompt:");
            Console.ResetColor();
            Console.WriteLine("");
            Console.WriteLine(greeting);

            var builder = Kernel.CreateBuilder()
                          .AddOllamaChatCompletion(
                           modelId: "llama3.1",
                           endpoint: new Uri("http://localhost:11434"));
            builder.AddLocalTextEmbeddingGeneration();
            Kernel kernel = builder.Build();

            Console.WriteLine(question);

            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = null,
                MaxTokens = 30,
                Temperature = 0,
            };

            var arguments = new KernelArguments(settings)
        {
            { "input", question },
        };

            //var response = kernel.InvokePromptStreamingAsync(question);
            //await foreach (var result in response)
            //{
            //    Console.Write(result);
            //}

            // separator
            Console.WriteLine("");
            Console.WriteLine("==============");
            Console.WriteLine("");

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(question);

            string filePath = "./data/sales.txt";

            // Read the content of the file
            string fileContent = File.ReadAllText(filePath);

            // Split the text into lines
            var lines = new List<string> { fileContent };

            // Define the maximum number of tokens per chunk
            int maxTokensPerChunk = 60;

            // Split the text into chunks
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);

            // separator
            Console.WriteLine("Chunked output from a text document");

            // Output the chunks
            foreach (var chunk in chunks)
            {
                Console.WriteLine(chunk);
            }

            // separator
            Console.WriteLine("");
            Console.WriteLine("==============");
            Console.WriteLine("");

            // get the embeddings generator service
            var embeddingGeneratorChunked = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
            var memoryChunked = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGeneratorChunked);

            const string MemoryCollectionNameChunked = "appleSales";

            int i = 0;
            foreach (var chunk in chunks)
            {
                await memoryChunked.SaveInformationAsync(MemoryCollectionNameChunked, id: "info" + i, text: chunk);
                i++;
            }

            TextMemoryPlugin memoryPluginChunked = new(memoryChunked);

            // Import the text memory plugin into the Kernel.
            kernel.ImportPluginFromObject(memoryPluginChunked);


            var promptChunked = @"
            Question: Summarize the current iPad sales
            Answer the question using the memory content: {{Recall}}";

            var argumentsRAG = new KernelArguments(settings)
            {
                { "input", question },
                { "collection", MemoryCollectionNameChunked }
            };

            Console.WriteLine($"Llama3.1 response (using RAG semantic memory and document chunking).");

            var response = kernel.InvokePromptStreamingAsync(promptChunked, argumentsRAG);
            string fullMessage = "";
            foreach (var result in prompt)
            {
                Console.Write(result);
                fullMessage += result.ToString();
            }

            chatHistory.AddUserMessage(fullMessage);

            Console.WriteLine($" The end!");

            Console.WriteLine($"");
            Chat chat = new Chat { Conversation = fullMessage };
            return chat;
        }

        private static void ApplyPersona(PersonaSettings settings, Persona persona)
        {
            settings.SetTone(persona.Tone);
            settings.SetStyle(persona.Style);
            settings.SetTraits(persona.Traits);
        }

    }


}
