using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Materials
{
    public class MaterialSearchResult
    {
        public Material Material { get; set; } = default!;
        public List<MaterialChunk> TopChunks { get; set; } = new();
    }
}
