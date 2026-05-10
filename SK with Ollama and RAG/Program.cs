
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.Postgres;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Npgsql;
using Pgvector.Npgsql;
using System.Text;
using UglyToad.PdfPig;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Host=localhost;Database=SKRAG;Username=postgres;Password=admin";

        // ✅ Configure Kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOllamaChatCompletion("llama3:latest", new Uri("http://localhost:11434"));
        builder.AddOllamaTextEmbeddingGeneration("nomic-embed-text:latest", new Uri("http://localhost:11434"));
        //builder.AddOllamaEmbeddingGenerator("nomic-embed-text", new Uri("http://localhost:11434"));
        var kernel = builder.Build();

        // This is to identify and debug the lenght of the embedding model
        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        var testEmbedding = await embeddingService.GenerateEmbeddingAsync("test");
        Console.WriteLine($"Embedding length: {testEmbedding.Length.ToString()}");


        // ✅ Register pgvector type with Npgsql
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector(); // Correct method from Pgvector package
        var dataSource = dataSourceBuilder.Build();

        // ✅ Create PostgresDbClient with schema and vector size
        var dbClient = new PostgresDbClient(dataSource, schema: "public", vectorSize: 768);
        var memoryStore = new PostgresMemoryStore(dbClient);


        //var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        //var testEmbedding = await embeddingService.GenerateEmbeddingAsync("test");
        //Console.WriteLine($"Embedding length: {testEmbedding.Count}");


        // ✅ Suppress experimental warning for SemanticTextMemory
#pragma warning disable SKEXP0010
        var memory = new SemanticTextMemory(memoryStore, kernel.GetRequiredService<ITextEmbeddingGenerationService>());
#pragma warning restore SKEXP0010

        // ✅ Index local files (TXT + PDF)
        var folderPath = @"C:\Users\jyoth\source\repos\SK with Ollama and RAG\SK with Ollama and RAG\files";
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                              .Where(f => f.EndsWith(".txt") || f.EndsWith(".pdf"));

        foreach (var file in files)
        {
            string content = file.EndsWith(".pdf") ? ReadPdf(file) : File.ReadAllText(file);
            var chunks = ChunkText(content, 500);

            foreach (var chunk in chunks)
            {
                await memory.SaveInformationAsync("knowledge_v2", Guid.NewGuid().ToString(), chunk);
            }
        }

        Console.WriteLine("Documents indexed successfully!");

        // ✅ RAG Query
        var query = "How much total money does a player start with in Monopoly? (Answer with number only)";
        //var query = "How many color train cars there in North American train route? Please list them";
        var contextBuilder = new StringBuilder();

        await foreach (var item in memory.SearchAsync("knowledge_v2", query, limit: 5))
        {
            contextBuilder.AppendLine(item.Metadata.Text);
        }

        var prompt = $"Use the following context to answer:\n{contextBuilder}\nQuestion: {query}";
        var ollamaChat = kernel.GetRequiredService<IChatCompletionService>();
        var response = await ollamaChat.GetChatMessageContentAsync(prompt);

        Console.WriteLine("\n=== RAG Response ===");
        Console.WriteLine(response.Content);
    }

    static string ReadPdf(string path)
    {
        var sb = new StringBuilder();
        using (var pdf = PdfDocument.Open(path))
        {
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
        }
        return sb.ToString();
    }

    static List<string> ChunkText(string text, int chunkSize)
    {
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        }
        return chunks;
    }
}

