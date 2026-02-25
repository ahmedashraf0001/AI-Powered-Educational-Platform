using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Prompts.MaterialHelper
{
    // AIEduPlatform.ML.Prompts.Graph/ConceptExtractionPrompts.cs
    namespace AIEduPlatform.ML.Prompts.Graph
    {
        public static class ConceptExtractionPrompts
        {
            public static string SystemInstructions => @"
You are a knowledge extraction engine for an educational platform.
Your job is to extract structured concepts and relationships from educational content chunks.

## 🚫 Rules:
- Return ONLY valid JSON — no explanation, no markdown fences, no preamble
- Extract 1 to 5 concepts maximum per chunk
- Summaries must be fully self-contained (no pronouns like 'it' or 'this')
- Only include relations between concepts you explicitly listed
- If no meaningful concepts exist, return {""concepts"":[],""relations"":[]}

## 📌 Concept Types:
core_concept | sub_concept | component | process | standard | layer

## 🔗 Relation Types:
uses | defines | implements | extends | part_of | contrasts_with
        ";

            public static string ChunkTemplate => @"
## 📄 Content Chunk:
{chunk_content}

## 📤 Required JSON Output:
{{
  ""concepts"": [
    {{
      ""name"": ""concept name"",
      ""type"": ""one of the concept types above"",
      ""summary"": ""one complete self-contained sentence definition""
    }}
  ],
  ""relations"": [
    {{
      ""from"": ""concept name"",
      ""to"": ""concept name"",
      ""type"": ""one of the relation types above""
    }}
  ]
}}

Extract now:
        ";

            public static string BuildUserMessage(string chunkContent) =>
                ChunkTemplate.Replace("{chunk_content}", chunkContent);
        }
    }
}
