using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Prompts.MaterialHelper
{
    public static class GraphMergePrompts
    {
        public static string SystemInstructions => @"
You are a knowledge graph consolidation engine for an educational platform.
You receive concepts and relations extracted from individual content chunks and produce a clean, unified graph.

## 🔧 Your Tasks:
1. **Merge duplicates** — e.g. 'RS-485', 'RS485', 'RS 485' → canonical 'RS-485'
2. **Add implied cross-chunk relations** that are clearly supported by the data
3. **Remove noise** — overly generic concepts like 'System', 'Method', 'Value', 'Data'

## 🚫 Rules:
- Return ONLY valid JSON — no explanation, no markdown fences, no preamble
- All relation endpoints must reference canonical concept names
- Aliases must be listed exhaustively so chunk mappings resolve correctly

## 📌 Concept Types:
core_concept | sub_concept | component | process | standard | layer

## 🔗 Relation Types:
uses | defines | implements | extends | part_of | contrasts_with
        ";

        public static string MergeTemplate => @"
## 📥 Extracted Concepts and Relations (from all chunks):
{extractions_json}

## 📤 Required JSON Output:
{{
  ""concepts"": [
    {{
      ""name"": ""canonical concept name"",
      ""type"": ""one of the concept types above"",
      ""summary"": ""one complete self-contained sentence"",
      ""aliases"": [""any alternative names that map to this concept""]
    }}
  ],
  ""relations"": [
    {{
      ""from"": ""canonical concept name"",
      ""to"": ""canonical concept name"",
      ""type"": ""one of the relation types above""
    }}
  ]
}}

Consolidate and output now:
        ";

        public static string BuildUserMessage(string extractionsJson) =>
            MergeTemplate.Replace("{extractions_json}", extractionsJson);
    }
}
