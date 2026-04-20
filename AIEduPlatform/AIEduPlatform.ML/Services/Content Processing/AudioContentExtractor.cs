using AIEduPlatform.Core.DTOs.AI.Simple;
using NAudio.Wave;

namespace AIEduPlatform.ML.MaterialProcessing
{
    /// <summary>
    /// Extracts raw audio data chunks from audio files for transcription.
    /// Handles loading from local paths and URLs, and splitting into time-based chunks.
    /// </summary>
    public class AudioContentExtractor : IDisposable
    {
        private readonly string _audioFilePath;
        private WaveStream? _audioStream;
        private bool _disposed = false;

        public AudioContentExtractor(string audioFilePath)
        {
            if (string.IsNullOrWhiteSpace(audioFilePath))
                throw new ArgumentException("Audio file path cannot be null or empty", nameof(audioFilePath));

            _audioFilePath = audioFilePath;
        }

        /// <summary>
        /// Extracts audio chunks of specified duration
        /// </summary>
        public async Task<List<AudioChunk>> ExtractChunksAsync(
            int chunkDurationSeconds,
            CancellationToken cancellationToken = default)
        {
            if (chunkDurationSeconds <= 0)
                throw new ArgumentException("Chunk duration must be positive", nameof(chunkDurationSeconds));

            var chunks = new List<AudioChunk>();

            // Load audio file
            await LoadAudioFileAsync(cancellationToken);

            if (_audioStream == null)
                throw new InvalidOperationException("Failed to load audio file");

            // Get audio properties
            var format = _audioStream.WaveFormat;
            var totalDuration = _audioStream.TotalTime.TotalSeconds;
            var bytesPerSecond = format.AverageBytesPerSecond;
            var chunkSizeBytes = bytesPerSecond * chunkDurationSeconds;

            // Calculate number of chunks
            var totalChunks = (int)Math.Ceiling(totalDuration / chunkDurationSeconds);

            // Extract chunks
            for (int i = 0; i < totalChunks; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var startTimeSeconds = i * chunkDurationSeconds;
                var chunk = await ExtractChunkAsync(
                    index: i,
                    startTimeSeconds: startTimeSeconds,
                    durationSeconds: chunkDurationSeconds,
                    cancellationToken: cancellationToken);

                if (chunk != null && chunk.AudioData.Length > 0)
                {
                    chunks.Add(chunk);
                }
            }

            return chunks;
        }

        /// <summary>
        /// Loads audio file from URL or local path
        /// </summary>
        private async Task LoadAudioFileAsync(CancellationToken cancellationToken)
        {
            if (_audioStream != null)
                return;

            try
            {
                // Check if it's a URL
                if (_audioFilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    _audioFilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    await LoadFromUrlAsync(cancellationToken);
                }
                else
                {
                    LoadFromLocalPath();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load audio file: {_audioFilePath}", ex);
            }
        }

        /// <summary>
        /// Loads audio from URL
        /// </summary>
        private async Task LoadFromUrlAsync(CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            var response = await httpClient.GetAsync(_audioFilePath, cancellationToken);
            response.EnsureSuccessStatusCode();

            var audioBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var memoryStream = new MemoryStream(audioBytes);

            _audioStream = CreateAudioReader(memoryStream);
        }

        /// <summary>
        /// Loads audio from local file path
        /// </summary>
        private void LoadFromLocalPath()
        {
            if (!File.Exists(_audioFilePath))
                throw new FileNotFoundException($"Audio file not found: {_audioFilePath}");

            var fileStream = File.OpenRead(_audioFilePath);
            _audioStream = CreateAudioReader(fileStream);
        }

        /// <summary>
        /// Creates appropriate audio reader based on file format
        /// </summary>
        private WaveStream CreateAudioReader(Stream stream)
        {
            var extension = Path.GetExtension(_audioFilePath).ToLowerInvariant();

            try
            {
                return extension switch
                {
                    ".wav" => new WaveFileReader(stream),
                    ".mp3" => new Mp3FileReader(stream),
                    _ => throw new NotSupportedException($"Audio format '{extension}' is not supported. Supported formats: .wav, .mp3")
                };
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read audio file with format '{extension}'. The file may be corrupted.", ex);
            }
        }

        /// <summary>
        /// Extracts a single audio chunk
        /// </summary>
        private async Task<AudioChunk?> ExtractChunkAsync(
            int index,
            double startTimeSeconds,
            int durationSeconds,
            CancellationToken cancellationToken)
        {
            if (_audioStream == null)
                return null;

            try
            {
                // Calculate positions
                var format = _audioStream.WaveFormat;
                var startPosition = (long)(startTimeSeconds * format.AverageBytesPerSecond);
                var maxDuration = _audioStream.TotalTime.TotalSeconds - startTimeSeconds;
                var actualDuration = Math.Min(durationSeconds, maxDuration);

                if (actualDuration <= 0)
                    return null;

                var bytesToRead = (int)(actualDuration * format.AverageBytesPerSecond);

                // Align to block
                startPosition -= startPosition % format.BlockAlign;
                bytesToRead -= bytesToRead % format.BlockAlign;

                // Seek to position
                _audioStream.Position = startPosition;

                // Read audio data
                var buffer = new byte[bytesToRead];
                var bytesRead = await Task.Run(() => _audioStream.Read(buffer, 0, bytesToRead), cancellationToken);

                if (bytesRead == 0)
                    return null;

                // Resize buffer if we read less than expected
                if (bytesRead < bytesToRead)
                {
                    Array.Resize(ref buffer, bytesRead);
                }

                // Convert to WAV format (required for transcription service)
                var wavData = ConvertToWav(buffer, format);

                return new AudioChunk
                {
                    Index = index,
                    AudioData = wavData,
                    StartTimeSeconds = startTimeSeconds,
                    DurationSeconds = actualDuration
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract chunk at index {index}", ex);
            }
        }

        /// <summary>
        /// Converts raw audio data to WAV format
        /// </summary>
        private byte[] ConvertToWav(byte[] audioData, WaveFormat format)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new WaveFileWriter(memoryStream, format);

            writer.Write(audioData, 0, audioData.Length);
            writer.Flush();

            return memoryStream.ToArray();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _audioStream?.Dispose();
                _audioStream = null;
            }

            _disposed = true;
        }
    }
}