# 🎓 AI-Powered Educational Platform - ML Integration Documentation

## Table of Contents
1. [Architecture Overview](#1-architecture-overview)
2. [Core Services You'll Work With](#2-core-services-youll-work-with)
3. [The Big Picture: Data Flow](#3-the-big-picture-data-flow)
4. [Feature-by-Feature Integration Guide](#4-feature-by-feature-integration-guide)
5. [What You Need to Modify/Create](#5-what-you-need-to-modifycreate)
6. [DTOs Quick Reference](#6-dtos-quick-reference)
7. [Quick Start Checklist](#7-quick-start-checklist)

---

## 1. Architecture Overview

Your friend built a **RAG (Retrieval-Augmented Generation)** system. Here's what that means:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           YOUR LAYER (Application)                          │
│   Commands/Queries (MediatR handlers) → IRAGService, IOllamaServiceClient  │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │ (interfaces only - you never touch implementations)
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              ML LAYER (C#)                                  │
│   RAGService (indexing, retrieval) │ OllamaServiceClient (AI generation)   │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │ HTTP calls
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PYTHON ML SERVICES                                │
│   Embedding Service (Port 8000) │ Reranking Service (Port 8001) │ Ollama   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### What is RAG?
RAG = **Retrieval-Augmented Generation**
1. **Retrieval**: Search course materials for relevant content chunks
2. **Augmented**: Attach those chunks as context to the AI prompt
3. **Generation**: AI generates responses grounded in actual course content

This prevents AI hallucination - the AI only answers based on your course materials!

---

## 2. Core Services You'll Work With

### 2.1 `IRAGService` - Content Management & Retrieval
**Location**: `AIEduPlatform.Core\Interfaces\Services\IRAGService.cs`  
**Purpose**: Handles storing and searching course materials (PDFs, etc.)

| Method | What It Does | When to Use |
|--------|-------------|-------------|
| `IndexAsync(RagIndexRequest)` | Processes a course's materials → extracts text → creates embeddings → stores chunks | After materials are uploaded (can be triggered automatically) |
| `RetrieveAsync(RagRetrievalRequest)` | Searches for relevant content chunks based on a query | Before any AI generation (chat, quiz, flashcards, etc.) |
| `DeleteMaterialAsync(Guid)` | Removes indexed chunks for a material | When deleting a material |
| `DeleteLectureAsync(Guid)` | Removes all chunks for a lecture | When deleting a lecture |
| `DeleteCourseAsync(Guid)` | Removes all chunks for a course | When deleting a course |
| `IsMaterialIndexedAsync(Guid)` | Checks if a material has been processed | To show indexing status |
| `GetChunkCountAsync(Guid)` | Gets number of chunks for a material | Statistics/info |
| `GetIndexStatsAsync(Guid)` | Gets full indexing stats for a course | Dashboard/stats |

### 2.2 `IOllamaServiceClient` - AI Generation
**Location**: `AIEduPlatform.Core\Interfaces\Services\IOllamaServiceClient.cs`  
**Purpose**: Generates AI responses using retrieved context

| Method | What It Does | When to Use |
|--------|-------------|-------------|
| `GenerateStudyChatResponseAsync(...)` | Chat with AI about course content | Study chat feature |
| `GenerateFlashcardsAsync(...)` | Creates flashcard Q&A pairs | Flashcard generation |
| `GenerateMindMapAsync(...)` | Creates a mind map structure | Mind map feature |
| `GenerateQuizAsync(...)` | Creates practice quiz questions | Quiz generation |
| `GenerateSummaryAsync(...)` | Summarizes content | Summary feature |
| `GradeEssayAsync(...)` | Grades student essay answers | Teacher grading |
| `GenerateExamQuestionsAsync(...)` | Generates exam questions | Exam creation |

### 2.3 `IStudySessionService` - High-Level Wrapper
**Location**: `AIEduPlatform.Core\Interfaces\Services\IStudySessionService.cs`  
**Implementation**: `AIEduPlatform.Application\Services\StudySessionService.cs`  
**Purpose**: Wraps RAG + Ollama together with session context management

This service currently has `NotImplementedException` - **you'll need to implement it**.

---

## 3. The Big Picture: Data Flow

### 3.1 Material Upload → Indexing Flow
```
1. Teacher uploads PDF to a lecture
   └─► UploadMaterialCommandHandler saves Material entity (Indexed = false)
   
2. When student tries to study (or manually triggered):
   └─► IRAGService.IndexAsync() called for the course
       └─► RAGService extracts PDF text (page by page)
       └─► Chunks text into smaller pieces (~500 tokens each)
       └─► Sends chunks to Python Embedding Service
       └─► Stores MaterialChunk entities with Vector embeddings in DB
       └─► Sets Material.Indexed = true
```

### 3.2 Student Study Flow (Chat, Quiz, Flashcards, etc.)
```
1. Student asks: "Explain neural networks" in a study session
   
2. Your handler:
   └─► Calls IRAGService.RetrieveAsync({
         Query = "Explain neural networks",
         CourseId = ...,
         TopK = 20,      // Get 20 initial matches
         FinalTopK = 5   // Return best 5 after reranking
       })
   
3. RAGService internally:
   └─► Embeds the query using Python Embedding Service
   └─► Searches MaterialChunks using vector similarity (PostgreSQL pgvector)
   └─► Reranks results using Python Reranking Service
   └─► Returns List<ContextChunk> (the relevant content)

4. Your handler:
   └─► Calls IOllamaServiceClient.GenerateStudyChatResponseAsync(
         contextChunks,    // From step 3
         userQuestion,
         conversationHistory
       )
   └─► Returns AI response to student
```

### 3.3 Visual Flow Diagram
```
┌──────────────┐     ┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│   Student    │────►│  Your App   │────►│   IRAGService    │────►│  Database   │
│   Question   │     │   Handler   │     │  RetrieveAsync   │     │  (pgvector) │
└──────────────┘     └──────┬──────┘     └────────┬─────────┘     └─────────────┘
                            │                     │
                            │    ContextChunks    │
                            │◄────────────────────┘
                            │
                            ▼
                   ┌────────────────────┐     ┌─────────────┐
                   │ IOllamaServiceClient│────►│   Ollama    │
                   │ GenerateChatAsync  │     │   (LLM)     │
                   └─────────┬──────────┘     └─────────────┘
                             │
                             ▼
                   ┌──────────────────┐
                   │   AI Response    │
                   │ (grounded in     │
                   │  course content) │
                   └──────────────────┘
```

---

## 4. Feature-by-Feature Integration Guide

### 4.1 📤 Material Upload (MODIFY - Optional)
**Current file**: `UploadMaterialCommandHandler.cs`  
**Current state**: Saves material, sets `Indexed = false`

**What to add**: Nothing immediate! The RAG service auto-indexes when `RetrieveAsync` is called and detects unindexed materials.

**Optional enhancement** - trigger background indexing after upload:
```csharp
// Inject IRAGService
private readonly IRAGService _ragService;

// After saving material, optionally queue indexing:
await _ragService.IndexAsync(new RagIndexRequest 
{ 
    CourseId = course.Id 
}, cancellationToken);
```

---

### 4.2 🗑️ Material Delete (MODIFY - Required)
**Current file**: `DeleteMaterialCommandHandler.cs`

**What's missing**: RAG cleanup!

**Add this** before deleting the material entity:
```csharp
// Inject IRAGService in constructor
private readonly IRAGService _ragService;

// In Handle method, before _unitOfWork.Materials.DeleteAsync():
await _ragService.DeleteMaterialAsync(request.MaterialId, cancellationToken);
```

---

### 4.3 🗑️ Lecture Delete (MODIFY - Required)
**Current file**: `DeleteLectureCommandHandler.cs`

**What's missing**: RAG cleanup!

**Add this**:
```csharp
// Inject IRAGService in constructor
private readonly IRAGService _ragService;

// In Handle method, before _unitOfWork.Lectures.DeleteAsync():
await _ragService.DeleteLectureAsync(request.LectureId, cancellationToken);
```

---

### 4.4 🗑️ Course Delete (MODIFY - Required)
**Current file**: `DeleteCourseCommandHandler.cs`

**What's missing**: RAG cleanup!

**Add this**:
```csharp
// Inject IRAGService in constructor
private readonly IRAGService _ragService;

// In Handle method, before _unitOfWork.Courses.DeleteAsync():
await _ragService.DeleteCourseAsync(request.CourseId, cancellationToken);
```

---

### 4.5 💬 Study Chat (CREATE NEW)
**Create**: New command/handler in Application layer

**Full Pattern**:
```csharp
// Command
public record StudyChatCommand(
    Guid CourseId,
    Guid SessionId,
    string UserQuestion,
    List<OllamaMessage>? ConversationHistory
) : IRequest<ChatResponse>;

// Handler
public class StudyChatCommandHandler : IRequestHandler<StudyChatCommand, ChatResponse>
{
    private readonly IRAGService _ragService;
    private readonly IOllamaServiceClient _ollamaClient;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<ChatResponse> Handle(StudyChatCommand request, CancellationToken ct)
    {
        // 1. Retrieve relevant context
        var retrieval = await _ragService.RetrieveAsync(new RagRetrievalRequest
        {
            Query = request.UserQuestion,
            CourseId = request.CourseId,
            TopK = 20,
            FinalTopK = 5,
            UseReranking = true
        }, ct);
        
        // 2. Generate AI response with context
        var response = await _ollamaClient.GenerateStudyChatResponseAsync(
            retrieval.Chunks,
            request.UserQuestion,
            request.ConversationHistory,
            ct
        );
        
        // 3. Save chat message to database
        var chatMessage = new ChatMessage
        {
            StudySessionId = request.SessionId,
            Role = ChatRole.Assistant,
            Content = response.Answer,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.ChatMessages.AddAsync(chatMessage, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // 4. Return response
        return response;
    }
}
```

---

### 4.6 🎴 Flashcard Generation (CREATE NEW)
**Pattern**:
```csharp
public class GenerateFlashcardsCommandHandler : IRequestHandler<GenerateFlashcardsCommand, List<Flashcard>>
{
    private readonly IRAGService _ragService;
    private readonly IOllamaServiceClient _ollamaClient;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<List<Flashcard>> Handle(GenerateFlashcardsCommand request, CancellationToken ct)
    {
        // 1. Retrieve context about the topic
        var retrieval = await _ragService.RetrieveAsync(new RagRetrievalRequest
        {
            Query = request.Topic,
            CourseId = request.CourseId,
            TopK = 30,
            FinalTopK = 10  // More context for flashcard diversity
        }, ct);
        
        // 2. Generate flashcards
        var flashcards = await _ollamaClient.GenerateFlashcardsAsync(
            retrieval.Chunks,
            request.Topic,
            request.NumberOfCards,
            ct
        );
        
        // 3. Save to Flashcard table
        foreach (var card in flashcards)
        {
            var entity = new Core.Domain.Entities.Flashcard
            {
                StudySessionId = request.SessionId,
                Question = card.Question,
                Answer = card.Answer,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Flashcards.AddAsync(entity, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);
        
        // 4. Return
        return flashcards;
    }
}
```

---

### 4.7 🧠 Mind Map Generation (CREATE NEW)
Same pattern:
1. Retrieve context with `_ragService.RetrieveAsync()`
2. Generate with `_ollamaClient.GenerateMindMapAsync()`
3. Save to `MindMap` table
4. Return result

---

### 4.8 📝 Quiz Generation (CREATE NEW)
Same pattern:
1. Retrieve context with `_ragService.RetrieveAsync()`
2. Generate with `_ollamaClient.GenerateQuizAsync()`
3. Save to `GeneratedQuiz` table
4. Return result

---

### 4.9 📄 Summary Generation (CREATE NEW)
Same pattern:
1. Retrieve context with `_ragService.RetrieveAsync()`
2. Generate with `_ollamaClient.GenerateSummaryAsync()`
3. Optionally save to `Material.Summary` field
4. Return result

---

### 4.10 📊 Course Indexing Stats (CREATE NEW - Optional)
To show teachers how much content is indexed:
```csharp
public class GetIndexStatsQueryHandler : IRequestHandler<GetIndexStatsQuery, RagIndexStats>
{
    private readonly IRAGService _ragService;
    
    public async Task<RagIndexStats> Handle(GetIndexStatsQuery request, CancellationToken ct)
    {
        return await _ragService.GetIndexStatsAsync(request.CourseId, ct);
    }
}

// Returns:
// - TotalLectures
// - TotalMaterials  
// - TotalChunks
// - ByLecture breakdown
// - EstimatedTokenCount
// - LastIndexedAt
```

---

## 5. What You Need to Modify/Create

### 5.1 Modifications to Existing Handlers

| File | What to Add | Priority |
|------|-------------|----------|
| `DeleteMaterialCommandHandler.cs` | Call `_ragService.DeleteMaterialAsync()` | 🔴 Required |
| `DeleteLectureCommandHandler.cs` | Call `_ragService.DeleteLectureAsync()` | 🔴 Required |
| `DeleteCourseCommandHandler.cs` | Call `_ragService.DeleteCourseAsync()` | 🔴 Required |
| `UploadMaterialCommandHandler.cs` | (Optional) Trigger background indexing | 🟡 Optional |

### 5.2 New Features to Create

| Feature | Commands/Queries Needed | Entities Involved | Priority |
|---------|------------------------|-------------------|----------|
| **Study Chat** | `StudyChatCommand` | `StudySession`, `ChatMessage` | 🔴 High |
| **Flashcards** | `GenerateFlashcardsCommand` | `StudySession`, `Flashcard` | 🔴 High |
| **Practice Quiz** | `GenerateQuizCommand` | `StudySession`, `GeneratedQuiz` | 🔴 High |
| **Mind Maps** | `GenerateMindMapCommand` | `StudySession`, `MindMap` | 🟡 Medium |
| **Summaries** | `GenerateSummaryCommand` | `Material` | 🟡 Medium |
| **Manual Indexing** | `IndexCourseCommand` | `Course`, `Material` | 🟢 Low |
| **Index Stats** | `GetIndexStatsQuery` | `Course` | 🟢 Low |

### 5.3 Alternative: Implement `StudySessionService`
Instead of creating individual handlers, you can implement the already-existing `StudySessionService`:

**Location**: `AIEduPlatform.Application\Services\StudySessionService.cs`

Already has `IRAGService` and `IOllamaServiceClient` injected - just needs the method bodies filled in!

```csharp
public async Task<ChatResponse> ChatAsync(StudyChatRequest request, CancellationToken ct = default)
{
    // 1. Retrieve
    var retrieval = await _ragService.RetrieveAsync(new RagRetrievalRequest
    {
        Query = request.UserQuestion,
        CourseId = request.CourseId,
        TopK = 20,
        FinalTopK = 5
    }, ct);
    
    // 2. Generate
    return await _ollamaClient.GenerateStudyChatResponseAsync(
        retrieval.Chunks,
        request.UserQuestion,
        // Convert AiChatMessage to OllamaMessage if needed
        null,
        ct
    );
}
```

---

## 6. DTOs Quick Reference

### 6.1 RAG Requests/Responses

```csharp
// For indexing materials
RagIndexRequest 
{ 
    CourseId,           // Required - which course to index
    Reindex = false,    // If true, re-index already indexed materials
    ChunkingOptions?    // Optional custom chunking settings
}

RagIndexResponse 
{ 
    Success, 
    ChunksIndexed,      // How many chunks were created
    ChunksFailed,       // How many failed
    IndexTimeMs,        // Total time
    EmbeddingTimeMs     // Time spent on embeddings
}
```

```csharp
// For retrieving context (THE MAIN ONE YOU'LL USE)
RagRetrievalRequest 
{ 
    Query,              // The search query (user question, topic, etc.)
    CourseId,           // Required - scope to course
    LectureIds?,        // Optional - filter to specific lectures
    MaterialIds?,       // Optional - filter to specific materials
    MaterialTypes?,     // Optional - filter by type (pdf, video, etc.)
    TopK = 20,          // Initial retrieval count (before reranking)
    FinalTopK = 5,      // After reranking (what you actually get)
    MinScore = 0.3f,    // Minimum relevance score
    UseReranking = true // Whether to use the reranking service
}

RagRetrievalResponse 
{ 
    Success,
    Query,
    Chunks,             // ⭐ List<ContextChunk> - THE MAIN OUTPUT
    TotalFound,         // How many matched before filtering
    RerankingApplied,
    RetrievalTimeMs,
    Metadata            // Detailed timing info
}
```

```csharp
// For deleting indexed content
RagDeleteRequest 
{ 
    MaterialId?,        // Delete chunks for one material
    LectureId?,         // Delete chunks for all materials in lecture
    CourseId?           // Delete chunks for entire course
}

RagDeleteResponse { Success, Error? }
```

### 6.2 Context Chunk (What RAG Returns)

```csharp
ContextChunk
{
    Content,            // The actual text content
    RelevanceScore,     // How relevant (0.0 to 1.0)
    Metadata            // Source information
    {
        SourceTitle,    // "Introduction to ML.pdf"
        MaterialType,   // "pdf"
        PageOrTimestamp,// "Page 5" or "00:15:30"
        Section,        // Chapter/section name
        LectureName,    // "Lecture 1: Basics"
        CourseName,     // "Machine Learning 101"
        MaterialId,
        LectureId,
        CourseId
    }
}
```

### 6.3 AI Request DTOs

All inherit from `RequestBase`:
```csharp
RequestBase
{
    ContextChunks,      // ⭐ You populate this from RagRetrievalResponse.Chunks
    Instructions?,      // Optional custom system prompt
    Temperature = 0.7f,
    MaxTokens = 2048,
    Stream = false,
    Model?
}
```

Specific requests:
```csharp
StudyChatRequest : RequestBase 
{ 
    CourseId, 
    SessionId, 
    UserQuestion, 
    ConversationHistory 
}

FlashcardRequest : RequestBase 
{ 
    SessionId, 
    Topic, 
    NumberOfCards = 10 
}

QuizRequest : RequestBase 
{ 
    SessionId, 
    Topic, 
    NumberOfQuestions = 5, 
    Difficulty = "medium",      // "easy", "medium", "hard"
    QuestionTypes = ["mcq"]     // "mcq", "true_false", "short_answer"
}

MindMapRequest : RequestBase 
{ 
    SessionId, 
    Topic, 
    MaxDepth = 3 
}

SummarizationRequest : RequestBase 
{ 
    MaterialId, 
    SummaryLength = 400,        // Word count target
    IncludeKeyPoints = true 
}
```

### 6.4 Key Entities

```csharp
Material 
{ 
    Id, LectureId, Type, Title, FileUrl,
    Indexed,                    // ⭐ Whether RAG has processed this
    Chunks                      // Navigation to MaterialChunk
}

MaterialChunk 
{ 
    MaterialId, 
    Content,                    // Text content
    Embedding,                  // Vector for similarity search
    Section, 
    PageOrTimestamp 
}

StudySession 
{ 
    StudentId, CourseId, StartedAt, LastActivity,
    ChatMessages,               // Navigation properties
    GeneratedQuizzes, 
    Flashcards, 
    MindMaps 
}
```

---

## 7. Quick Start Checklist

### Step 1: Modify Delete Handlers (Easiest - Do First!)
- [ ] Add `IRAGService` to `DeleteMaterialCommandHandler`
- [ ] Add `IRAGService` to `DeleteLectureCommandHandler`  
- [ ] Add `IRAGService` to `DeleteCourseCommandHandler`
- [ ] Call appropriate delete methods before entity deletion

### Step 2: Create Study Session Features
- [ ] Create `StudyChatCommand` + Handler
- [ ] Create `GenerateFlashcardsCommand` + Handler
- [ ] Create `GenerateQuizCommand` + Handler
- [ ] Create API endpoints for these features

### Step 3: Optional Enhancements
- [ ] Implement `StudySessionService` methods
- [ ] Add mind map generation
- [ ] Add summary generation
- [ ] Add indexing stats query
- [ ] Add manual indexing trigger

---

## 🎯 Golden Rules

1. **Always Retrieve First**: Call `RetrieveAsync` before any AI generation
2. **Pass the Chunks**: `retrieval.Chunks` goes to every Ollama method
3. **Context is King**: The AI only knows what you give it from the course materials
4. **Clean Up on Delete**: Always delete RAG data when deleting entities

---

## 📞 Quick Examples

### Minimal Chat Implementation
```csharp
// Retrieve → Generate → Return
var chunks = (await _ragService.RetrieveAsync(new RagRetrievalRequest
{
    Query = userQuestion,
    CourseId = courseId
}, ct)).Chunks;

return await _ollamaClient.GenerateStudyChatResponseAsync(chunks, userQuestion, history, ct);
```

### Minimal Quiz Implementation
```csharp
var chunks = (await _ragService.RetrieveAsync(new RagRetrievalRequest
{
    Query = topic,
    CourseId = courseId,
    FinalTopK = 10
}, ct)).Chunks;

return await _ollamaClient.GenerateQuizAsync(chunks, topic, 5, "medium", [QuestionType.MCQ], ct);
```

---

**You're ready to go! Start with the delete handler modifications, then build out the study features. Good luck! 🚀**
