using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.Concept;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Prompts.MaterialHelper
{
    public static class QueryIntelligencePrompts
    {
        public static string SystemInstructions => @"
You are a query analysis engine for an educational platform.
You analyze student queries and produce an optimized keyword query for vector database retrieval.

## 🚫 Rules:
- Return ONLY valid JSON — no explanation, no markdown fences, no preamble
- rewritten_query is for vector search ONLY — dense keywords, no sentences, no filler, no file references
- If intent is unclear, default to fact_lookup

## 🎯 Intent Definitions:
- concept_deep_dive — thorough explanation of a concept
- comparison — comparing two or more things
- how_to — how to do or implement something
- fact_lookup — specific fact or value
- troubleshooting — diagnose or fix a problem
- conversational — greeting, acknowledgment, casual message

## 💬 Conversation History:
- Resolve pronouns like 'it', 'that', 'this' using history
- For follow-ups, extract topic keywords from history and include them in rewritten_query

## 📁 Materials & Navigation:
- Set target_material_ids ONLY when the student explicitly asks to explain/summarize specific material(s) by name outside of quotes
- When target_material_ids is set: extract dense keywords from those materials' summaries as rewritten_query
- If the student asks a factual/conceptual question where a material name is the topic: target_material_ids = null
- If the student quotes text from conversation history: target_material_ids = null, treat as conceptual follow-up
- For multi-material requests (compare X and Y): include both ids in target_material_ids

## 📁 Examples:
- ""explain videoplayback.mp4"" → target_material_ids: [""<video id>""], rewritten: ""work energy potential kinetic Mgh gravitational joules pendulum""
- ""compare videoplayback.mp4 and rs485"" → target_material_ids: [""<video id>"", ""<doc id>""], rewritten: ""energy conversion RS-485 electrical interface comparison""
- ""summarize rs485"" → target_material_ids: [""<doc id>""], rewritten: ""RS-485 TIA-485 EIA-485 differential signaling noise immunity industrial automation""
- ""what is the cable length of rs485?"" → target_material_ids: null, rewritten: ""RS-485 maximum cable length meters feet""
- ""[quoted AI response] can you expand?"" → target_material_ids: null, rewritten: keywords from the quoted topic
";

        public static string QueryTemplate => @"
## 🔍 Current Student Query:
""{query}""

## 📤 Required JSON Output:
{{
  ""intent"": ""one of: concept_deep_dive, comparison, how_to, fact_lookup, troubleshooting, conversational"",
  ""rewritten_query"": ""dense space-separated keywords optimized for vector search"",
  ""target_material_ids"": [""material id 1"", ""material id 2""] or null,
  ""target_concepts"": [""main"", ""concepts""]
}}
Analyze and output now:
";

        public static string BuildUserMessage(
            string query,
            List<OllamaMessage>? conversationHistory = null,
            List<MaterialContext>? materials = null)
        {
            var sb = new StringBuilder();

            if (conversationHistory != null && conversationHistory.Any())
            {
                sb.AppendLine("## 💬 Recent Conversation:");
                foreach (var msg in conversationHistory.TakeLast(6))
                {
                    var cleanContent = System.Text.RegularExpressions.Regex
                        .Replace(msg.Content, @"\*\*|__|~~|`", "");
                    sb.AppendLine($"{msg.Role.ToUpper()}: {cleanContent}");
                }
                sb.AppendLine();
            }

            if (materials != null && materials.Any())
            {
                sb.AppendLine("## 📁 Available Course Materials:");
                foreach (var m in materials)
                {
                    sb.AppendLine($"- ID: {m.Id} | Title: {m.Title} | Type: {m.Type}");
                    if (!string.IsNullOrWhiteSpace(m.Summary))
                    {
                        var cleanSummary = System.Text.RegularExpressions.Regex
                            .Replace(m.Summary, @"[#*`]", "").Trim();
                        sb.AppendLine($"  Summary: {cleanSummary}");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine(QueryTemplate.Replace("{query}", query));
            return sb.ToString();
        }
    }
}