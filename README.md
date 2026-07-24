# Learnify — AI-Powered Learning Management System

Learnify is a full-featured Learning Management System built with AI as a core architectural concern rather than a bolted-on feature. Every AI capability in Learnify — chat, quiz generation, grading, recommendations — is grounded entirely in each student's own enrolled course materials, not general web knowledge.

Built as a graduation project by the IT Department, Faculty of Computers & Information, Zagazig University (2025–2026).

---

## Table of Contents

- [Motivation](#motivation)
- [What Learnify Does](#what-learnify-does)
- [AI Study Studio](#ai-study-studio)
- [AI-Assisted Assessment](#ai-assisted-assessment)
- [Recommendation Engine](#recommendation-engine)
- [System Architecture](#system-architecture)
- [Multi-Modal Indexing Pipeline](#multi-modal-indexing-pipeline)
- [Retrieval Pipeline (RAG)](#retrieval-pipeline-rag)
- [Technology Stack](#technology-stack)
- [Engineering Challenges](#engineering-challenges)
- [At a Glance](#at-a-glance)
- [Deployment](#deployment)
- [Team](#team)

---

## Motivation

Most existing LMS platforms treat educational content as static files to be stored and retrieved, not as a knowledge base that can be queried, explored, and understood. This creates four concrete problems:

| Problem | How Learnify fixes it |
|---|---|
| **AI as an afterthought** — chatbots bolted onto an existing system, disconnected from enrollment, progress, and course structure | **AI built into the core** — every part of the system (courses, progress, materials) feeds the same intelligence layer |
| **Same experience for every student** — no adjustment for how a student is actually doing | **An experience that adapts to the student** — AI understands each student's progress and materials individually |
| **AI that doesn't know the course** — students must leave the platform for a general assistant, getting answers that may not match what was taught | **AI that actually knows the course** — every answer is retrieved from the course's own lectures and materials, always traceable |
| **Grading bottleneck** — manual review of essays and written answers delays feedback as enrollment grows | **AI-assisted grading** — automated scoring with confidence flags, routing low-confidence cases to teachers |

## What Learnify Does

Eight feature groups across 129 REST endpoints, 2 SignalR hubs, and 5 background services — with the **AI Study Studio** as the core differentiator.

- Full course platform: authoring, multi-format material upload, enrollment, and payment processing
- Grounded AI Study Studio (6 tools, detailed below)
- AI-assisted assessment with 5 question types and automated essay grading
- Two-phase, vector-similarity course recommendation engine
- Dual clients: React web SPA and Flutter mobile app on a shared backend
- Real-time notifications and progress updates (SignalR)
- Containerized deployment via Docker Compose with GPU-accelerated AI services

## AI Study Studio

Six study tools, all grounded in the actual course material — every tool retrieves real material chunks before responding, so answers are traceable and never hallucinated from general web data.

1. **Grounded chat** — RAG-powered chat that retrieves relevant chunks, then streams the answer over SSE
2. **Quiz generation** — generates quizzes directly from material content, with immediate scoring
3. **Flashcards** — auto-generates spaced-repetition flashcards from any material
4. **Mind maps** — builds a hierarchical mind map of a material's key concepts
5. **Study summaries** — produces a structured, section-by-section summary of a material
6. **Dialogue audio** — generates an AI teacher-student dialogue audio narration of a material (XTTS v2)

## AI-Assisted Assessment

A confidence-gated grading pipeline that separates objective and subjective grading paths:

- Submission is validated (enrollment check, time window, deserialization, single-submission enforcement)
- Question types are detected and routed:
  - **Objective questions** (MCQ, true/false, fill-in-the-blank) → instant rule-based grading
  - **Essay questions** → RAG-grounded LLM grading, producing a score, feedback, and a confidence value
- **Confidence-based routing**: high-confidence essay grades are approved automatically; low-confidence cases are routed to teachers for manual review
- Final grade is recorded and the student is notified

This keeps grading fast and scalable without removing teacher oversight where it matters.

## Recommendation Engine

A two-phase, vector-similarity engine that models each student's evolving interests rather than relying on popularity or category matching.

**Phase 1 — Understanding courses**
- An LLM extracts semantic tags from course content (description, materials, transcripts)
- Each tag is embedded into a 384-dimension vector

**Phase 2 — Modeling student interest**
- An interest profile is built and aggregated from tags of courses the student enrolls in or completes
- **Boost** (+0.2) when a student engages with a tag
- **Decay** (0.05) gradually reduces weights over time
- **Forget** (0.05) removes unused tags to keep the profile current
- Result: a 384-dimension student interest vector

**Phase 3 — Candidate generation & ranking**
- Candidate pool (~100 courses) built from: similar courses (cosine similarity), popular courses, and new courses (for discovery)
- Multi-factor weighted scoring: similarity (40%), quality (20%), popularity (18%), recency (15%)
- Deterministic randomness injection keeps results fresh without full randomness
- Final selection: top 8 by score + 2 from the remaining pool → 10 personalized course recommendations

## System Architecture

A React web SPA and a Flutter mobile app talk through an Nginx reverse proxy to an ASP.NET Core API. The API delegates to five GPU-accelerated Python FastAPI microservices for indexing, retrieval, and generation — all running on a single Docker Compose deployment.

```
CLIENT TIER
 ├── React 19 + Vite 7 (Web SPA)
 └── Flutter + Dart (Mobile App)
        │
        ▼
Nginx Reverse Proxy  (/api → :5000 · /hubs → WS · SPA serving)
        │
        ▼
ASP.NET Core 10 (Kestrel :5000)
 ├── FastEndpoints — 129 REST endpoints
 ├── SignalR Hubs — notifications, AI progress
 └── Background Workers — channel queues + polling
        │
        ▼
APPLICATION + ML LAYER
        │
        ▼
AI SERVICES (Python FastAPI, GPU-accelerated)
 ├── RAG Service       — retrieval-augmented generation, LLM factor routing
 ├── Embedding Service — vector generation
 ├── Vision Service    — image/frame understanding (Qwen2-VL)
 ├── Transcription     — speech-to-text (Whisper)
 └── Video Service     — frame extraction + merge (FFmpeg)
        │
        ▼
DATA TIER
 └── PostgreSQL 16 + pgvector extension

EXTERNAL SERVICES
 └── Stripe (payments) · Gmail SMTP (notifications)
```

Backend follows a modified Clean Architecture, separating domain, application, infrastructure, and API layers.

## Multi-Modal Indexing Pipeline

Four different content formats, four different processing methods, all converging into one searchable knowledge base:

| Media Type | Tooling | Process |
|---|---|---|
| **PDF / Text** | UglyToad.PdfPig | Extract text, then chunk at 800 characters with 150-character overlap |
| **Audio** | Whisper (transcription) + bge-small (embedding) | Speech-to-text transcription, chunked with timing information preserved |
| **Image** | Qwen2-VL-2B | Vision model converts images into descriptive text context |
| **Video** | FFmpeg + Vision + ASR | Frames analyzed via vision model, audio analyzed via ASR, then merged |

All four converge into a single **384-dimension vector store** (PostgreSQL + pgvector), stored as `MaterialChunks` with cosine similarity search. This single index grounds every AI Study Studio tool — chat, quiz generation, and summaries all query the same knowledge base regardless of the original material's format.

## Retrieval Pipeline (RAG)

A course has far more content than fits in any model's context window. Learnify's retrieval pipeline evolved through four versions to solve this:

- **v1 — Vector Search ("dumb" RAG)**: baseline top-K similarity search over chunked embeddings. Fast, but blind to meaning — misses related concepts and returns near-duplicate chunks.
- **v2 — Query Intelligence**: classifies the question's intent (6 categories) and rewrites the query before searching, improving recall over a raw embedded question.
- **v3 — Concept Graph Expansion**: an auto-built knowledge graph expands retrieval outward from seed concepts extracted from initially retrieved chunks (depth-2 BFS), pulling in related material that pure vector similarity misses.
- **v4 — Cross-Encoder Reranking**: candidates from both graph expansion and vector search are re-scored by a cross-encoder, so only the highest-quality chunks fill the limited context window.
- **Final step**: only the top-ranked, highest-quality chunks are sent to the LLM, which answers with grounded, traceable evidence.

## Technology Stack

**Clients**
- React 19 + Vite 7 (Web SPA)
- Flutter + Dart (Mobile App)

**Backend**
- ASP.NET Core 10 (Kestrel), Clean Architecture
- FastEndpoints (REST API)
- Entity Framework Core
- SignalR (real-time notifications & AI progress streaming)

**AI Services** (Python FastAPI, GPU-accelerated)
- RAG orchestration & LLM routing
- Embedding generation
- Vision (Qwen2-VL-2B)
- Transcription (Whisper)
- Video processing (FFmpeg)
- Dialogue audio synthesis (XTTS v2)

**Data**
- PostgreSQL 16 + pgvector extension
- Local filesystem storage for uploaded materials

**External Services**
- Stripe (payment processing)
- Gmail SMTP (email notifications)

**Infrastructure**
- Docker Compose (single-stack deployment)
- Nginx (reverse proxy)

## Engineering Challenges

Two problems shaped most of the system's design:

1. **Context windows & retrieval quality** — a naive RAG setup returns technically-similar but often irrelevant chunks. Solved through the 4-stage retrieval pipeline (query intelligence → concept graph expansion → cross-encoder reranking) described above.
2. **Messy multi-format data** — course material arrives as PDFs, audio, images, and video, each requiring different extraction methods, but all needing to converge into one queryable representation. Solved via format-specific processing pipelines that all resolve to the same 384-d vector space.

## At a Glance

- **129** platform capabilities (REST endpoints)
- **5** AI services built-in
- **33** tracked data points
- **6+** AI Study Studio tools
- **4** media formats indexed
- **2** clients — Web + Mobile

## Deployment

Learnify is deployed as a single, self-contained Docker Compose stack, including:
- Nginx reverse proxy
- ASP.NET Core backend
- 5 GPU-accelerated Python AI microservices
- PostgreSQL + pgvector
- Background workers for async processing

This makes the platform straightforward to self-host, keeping full control over data, infrastructure, and AI cost/performance tradeoffs — the LLM layer can be run fully self-hosted or swapped to a hosted provider at runtime.

## Team

**Developed by:**
- Ahmed Ashraf Moussa Mohamed
- Ziad Hesham Sayed Ahmed
- Ahmed Mahmoud Kamal Mokhtar

**Supervised by:**
- Dr. Eman Selem
- Eng. Sohaila Nasser

Information Technology Department, Faculty of Computers & Information, Zagazig University — 2025–2026
