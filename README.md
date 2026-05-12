# SK with Ollama and RAG
Semantic Kernel + Ollama + PostgreSQL (pgvector) RAG Pipeline
A clean, production‑ready Retrieval‑Augmented Generation (RAG) pipeline built with:

Semantic Kernel for orchestration

Ollama for local LLM chat + embeddings

PostgreSQL pgvector for vector storage

Automatic PDF/TXT ingestion

Local semantic search + context‑aware answers

This project indexes documents from a folder, stores embeddings in PostgreSQL, retrieves relevant chunks, and uses Llama 3 to answer questions using retrieved context.

🧱 Architecture Overview
Code
Local Files (PDF/TXT)
        ↓
   Text Chunking
        ↓
Embedding Generation (Ollama: nomic-embed-text)
        ↓
PostgreSQL + pgvector (Semantic Memory Store)
        ↓
Semantic Search (Top‑K)
        ↓
RAG Prompt Construction
        ↓
LLM Response (Ollama: llama3)
✨ Features
Local LLM inference using Llama 3

Embedding generation using nomic‑embed‑text

Vector search using PostgreSQL + pgvector

Automatic PDF & text ingestion

Semantic Kernel memory integration

Configurable chunking

Simple, extensible RAG query flow

📦 Project Structure
Code
/src
  Program.cs
/files
  *.txt, *.pdf
README.md
🚀 Prerequisites
1. Install Ollama
Download: https://ollama.com

Pull required models:

bash
ollama pull llama3
ollama pull nomic-embed-text
2. Install PostgreSQL + pgvector
Enable pgvector:

sql
CREATE EXTENSION IF NOT EXISTS vector;
Create database:

sql
CREATE DATABASE "SKRAG";
3. Install .NET 8 SDK
bash
dotnet --version
⚙️ Configuration
PostgreSQL connection string
csharp
var connectionString = "Host=localhost;Database=SKRAG;Username=postgres;Password=admin";
Document ingestion folder
csharp
var folderPath = @"C:\path\to\files";
🧠 How It Works
1. Kernel Setup
Registers Llama 3 for chat

Registers nomic‑embed‑text for embeddings

2. Embedding Length Check
Useful for verifying vector size compatibility with pgvector.

3. PostgreSQL Vector Store
Registers pgvector type

Creates PostgresMemoryStore with vector size 768

4. Document Ingestion
Reads .txt and .pdf files

Extracts text (PDF via PdfPig)

Splits into 500‑character chunks

Saves each chunk to the vector DB

5. RAG Query
Retrieves top‑5 relevant chunks

Builds context

Sends prompt to Llama 3

Prints final answer

📘 Example Query
csharp
var query = "How much total money does a player start with in Monopoly? (Answer with number only)";
🛠️ Running the Project
bash
dotnet run
Expected output:

Embedding length

“Documents indexed successfully!”

RAG answer from Llama 3

📄 Code Overview
Chunking
csharp
static List<string> ChunkText(string text, int chunkSize)
PDF Reading
csharp
static string ReadPdf(string path)
RAG Search
csharp
await foreach (var item in memory.SearchAsync("knowledge_v2", query, limit: 5))
LLM Response
csharp
var response = await ollamaChat.GetChatMessageContentAsync(prompt);
🧩 Future Enhancements
Add metadata (filename, page number)

Add Minimal API layer

Add UI (Blazor / React)

Add streaming responses

Add hybrid search (keyword + vector)
