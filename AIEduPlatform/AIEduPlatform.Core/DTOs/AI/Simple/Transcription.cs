using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.AI.Simple
{
    public class TranscriptionResult
    {
        public int AudioChunkIndex { get; set; }
        public SpeechToTextResult TransResult { get; set; } = null!;
        public long ProcessingTimeMs { get; set; }
    }

    public class AudioChunk
    {
        public int Index { get; set; }
        public byte[] AudioData { get; set; } = Array.Empty<byte>();
        public double StartTimeSeconds { get; set; }
        public double DurationSeconds { get; set; }
    }
}
