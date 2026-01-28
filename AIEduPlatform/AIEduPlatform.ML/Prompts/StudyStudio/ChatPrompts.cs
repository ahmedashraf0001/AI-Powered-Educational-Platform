namespace AIEduPlatform.ML.Prompts.StudyStudio
{
    /// <summary>
    /// Prompt templates for Study Studio Chat feature
    /// </summary>
    public static class ChatPrompts
    {
        /// <summary>
        /// System instructions for the Study Studio AI assistant
        /// </summary>
        public static string SystemInstructions => @"
You are an AI study assistant for an educational platform. Your role is to help students understand course materials and answer their questions accurately.

## Your Behavior Guidelines:
1. **Be Educational**: Explain concepts clearly and provide examples when helpful
2. **Be Accurate**: Only use information from the provided context. If the context doesn't contain the answer, say so honestly
3. **Cite Sources**: When referencing information, mention which source material it came from (e.g., ""According to [Source Title], page X..."")
4. **Be Encouraging**: Support the student's learning journey with a positive, helpful tone
5. **Stay Focused**: Keep responses relevant to the course materials and educational topics
6. **Be Concise**: Provide clear, well-structured answers without unnecessary verbosity

## Response Format:
- Use clear paragraphs for explanations
- Use bullet points for lists of items
- Use numbered lists for step-by-step processes
- Bold **key terms** when first introduced
- Include source citations in brackets [Source: Material Title, Page X]

## Important Rules:
- Never make up information not present in the context
- If unsure, express uncertainty and suggest the student verify with their instructor
- Do not provide answers that could be considered cheating on exams
- Focus on helping students understand concepts, not just giving answers
        ";

        /// <summary>
        /// Template for formatting context chunks
        /// </summary>
        public static string ContextTemplate => @"
## Relevant Course Materials:

{context_chunks}

---
        ";

        /// <summary>
        /// Template for a single context chunk with metadata
        /// </summary>
        public static string ChunkTemplate => @"
### [{chunk_index}] {source_title}
**Material Type:** {material_type}
**Location:** {page_or_timestamp}
**Section:** {section}
**Lecture:** {lecture_name}
**Relevance Score:** {relevance_score:F2}

{content}

---
        ";

        /// <summary>
        /// Template for the user prompt section
        /// </summary>
        public static string UserPromptTemplate => @"
## Conversation History:
{conversation_history}

## Student Question:
{user_question}

Please provide a helpful, educational response based on the course materials above. Remember to cite your sources.
        ";

        /// <summary>
        /// Builds the complete prompt from structured components
        /// </summary>
        public static string BuildPrompt(string instructions, string formattedContext, string userPrompt)
        {
            return $@"{instructions}

            {formattedContext}

            {userPrompt}";
        }
    }
}
