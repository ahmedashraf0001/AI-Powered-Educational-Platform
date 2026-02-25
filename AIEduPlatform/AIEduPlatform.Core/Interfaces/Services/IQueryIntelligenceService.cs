using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.Concept;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IQueryIntelligenceService
    {
        Task<QueryIntelligenceResult> AnalyzeAsync(
                    string query,
                    List<OllamaMessage>? conversationHistory = null,
                    List<MaterialContext>? materials = null,
                    CancellationToken ct = default); 
    }

}
