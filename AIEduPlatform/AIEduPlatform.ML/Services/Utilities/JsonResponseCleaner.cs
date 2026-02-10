using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Utilities
{
    /// <summary>
    /// Utility class for cleaning LLM JSON responses that may contain markdown code blocks,
    /// explanatory text, or other formatting artifacts.
    /// </summary>
    public static class JsonResponseCleaner
    {
        private static readonly Regex JsonCodeBlockRegex = new(
            @"```(?:json)?\s*(\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\})\s*```",
            RegexOptions.Compiled | RegexOptions.Singleline,
            TimeSpan.FromSeconds(2));

        private static readonly Regex JsonArrayCodeBlockRegex = new(
            @"```(?:json)?\s*(\[(?:[^\[\]]|(?<open>\[)|(?<-open>\]))+(?(open)(?!))\])\s*```",
            RegexOptions.Compiled | RegexOptions.Singleline,
            TimeSpan.FromSeconds(2));

        private static readonly Regex SimpleJsonObjectRegex = new(
            @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}",
            RegexOptions.Compiled | RegexOptions.Singleline,
            TimeSpan.FromSeconds(2));

        private static readonly Regex SimpleJsonArrayRegex = new(
            @"\[(?:[^\[\]]|(?<open>\[)|(?<-open>\]))+(?(open)(?!))\]",
            RegexOptions.Compiled | RegexOptions.Singleline,
            TimeSpan.FromSeconds(2));

        /// <summary>
        /// Cleans an LLM response to extract valid JSON.
        /// Handles markdown code blocks, extra text, and whitespace.
        /// </summary>
        /// <param name="response">Raw LLM response that may contain JSON</param>
        /// <returns>Cleaned JSON string ready for deserialization</returns>
        /// <exception cref="InvalidOperationException">If no valid JSON is found in the response</exception>
        public static string CleanJsonResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                throw new ArgumentException("Response cannot be null or whitespace", nameof(response));

            // Step 1: Try to extract JSON from markdown code blocks (most common case)
            var cleaned = TryExtractFromMarkdownCodeBlock(response);
            if (cleaned != null)
                return cleaned;

            // Step 2: Try to find raw JSON object or array in the response
            cleaned = TryExtractRawJson(response);
            if (cleaned != null)
                return cleaned;

            // Step 3: As last resort, try to clean and use the entire response
            cleaned = response.Trim();
            if (IsLikelyValidJson(cleaned))
                return cleaned;

            throw new InvalidOperationException(
                "Could not extract valid JSON from LLM response. " +
                $"Response starts with: {GetResponsePreview(response)}");
        }

        /// <summary>
        /// Attempts to extract JSON from markdown code blocks like ```json ... ```
        /// </summary>
        private static string? TryExtractFromMarkdownCodeBlock(string response)
        {
            // Try object in code block first
            var match = JsonCodeBlockRegex.Match(response);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value.Trim();
            }

            // Try array in code block
            match = JsonArrayCodeBlockRegex.Match(response);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value.Trim();
            }

            return null;
        }

        /// <summary>
        /// Attempts to extract raw JSON object or array from response
        /// (without markdown code blocks)
        /// </summary>
        private static string? TryExtractRawJson(string response)
        {
            // Try to find JSON object
            var match = SimpleJsonObjectRegex.Match(response);
            if (match.Success)
            {
                var json = match.Value.Trim();
                if (IsLikelyValidJson(json))
                    return json;
            }

            // Try to find JSON array
            match = SimpleJsonArrayRegex.Match(response);
            if (match.Success)
            {
                var json = match.Value.Trim();
                if (IsLikelyValidJson(json))
                    return json;
            }

            return null;
        }

        /// <summary>
        /// Performs basic validation that a string looks like valid JSON
        /// </summary>
        private static bool IsLikelyValidJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            // Must start with { or [
            if (!text.StartsWith("{") && !text.StartsWith("["))
                return false;

            // Must end with } or ]
            if (!text.EndsWith("}") && !text.EndsWith("]"))
                return false;

            // Basic bracket/brace balance check
            int braceCount = 0;
            int bracketCount = 0;
            bool inString = false;
            char prevChar = '\0';

            foreach (char c in text)
            {
                if (c == '"' && prevChar != '\\')
                {
                    inString = !inString;
                }
                else if (!inString)
                {
                    if (c == '{') braceCount++;
                    else if (c == '}') braceCount--;
                    else if (c == '[') bracketCount++;
                    else if (c == ']') bracketCount--;
                }

                prevChar = c;
            }

            return braceCount == 0 && bracketCount == 0;
        }

        /// <summary>
        /// Gets a preview of the response for error messages
        /// </summary>
        private static string GetResponsePreview(string response, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(response))
                return "[empty]";

            var preview = response.Trim();
            if (preview.Length <= maxLength)
                return preview;

            return preview.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Validates and cleans a JSON response with detailed error information
        /// </summary>
        /// <param name="response">Raw LLM response</param>
        /// <param name="contentType">Type of content being parsed (for error messages)</param>
        /// <returns>Cleaned JSON string</returns>
        /// <exception cref="InvalidOperationException">With detailed error message if JSON cannot be extracted</exception>
        public static string CleanAndValidate(string response, string contentType)
        {
            try
            {
                return CleanJsonResponse(response);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to extract valid JSON for {contentType}. " +
                    $"Response preview: {GetResponsePreview(response, 200)}",
                    ex);
            }
        }
    }
}