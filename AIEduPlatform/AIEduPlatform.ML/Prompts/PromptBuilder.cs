using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.ML.Prompts.Grading;
using AIEduPlatform.ML.Prompts.QuestionGeneration;
using AIEduPlatform.ML.Prompts.StudyStudio;
using AIEduPlatform.ML.Prompts.Summarization;
using System.Text;

namespace AIEduPlatform.ML.Prompts
{
    public static class PromptBuilder
    {
        public static string BuildPrompt(string instructions, List<ContextChunk> contextChunks, string userPrompt)
        {
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (string.IsNullOrWhiteSpace(userPrompt))
                throw new ArgumentException("User prompt cannot be null or empty.", nameof(userPrompt));

            var sb = new StringBuilder();

            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            sb.AppendLine("## CONTEXT FROM COURSE MATERIALS");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine();

            sb.AppendLine("## USER REQUEST");
            sb.AppendLine(userPrompt);

            return sb.ToString();
        }

        public static string FormatContextChunks(List<ContextChunk> chunks)
        {
            if (chunks == null || !chunks.Any())
            {
                return "No relevant context available.";
            }

            var sb = new StringBuilder();

            var sortedChunks = chunks.OrderByDescending(c => c.RelevanceScore).ToList();

            for (int i = 0; i < sortedChunks.Count; i++)
            {
                var chunk = sortedChunks[i];
                sb.AppendLine($"## [{i + 1}] {chunk.Metadata.SourceTitle}");
                sb.AppendLine($"**Material Type:** {chunk.Metadata.MaterialType}");

                if (!string.IsNullOrEmpty(chunk.Metadata.PageOrTimestamp))
                {
                    sb.AppendLine($"**Location:** {chunk.Metadata.PageOrTimestamp}");
                }

                if (!string.IsNullOrEmpty(chunk.Metadata.Section))
                {
                    sb.AppendLine($"**Section:** {chunk.Metadata.Section}");
                }

                if (!string.IsNullOrEmpty(chunk.Metadata.LectureName))
                {
                    sb.AppendLine($"**Lecture:** {chunk.Metadata.LectureName}");
                }

                sb.AppendLine($"**Relevance Score:** {chunk.RelevanceScore:P0}");
                sb.AppendLine();
                sb.AppendLine("```");
                sb.AppendLine(chunk.Content);
                sb.AppendLine("```");
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string FormatConversationHistory(List<OllamaMessage> history, int maxMessages = 10)
        {
            if (history == null || !history.Any())
            {
                return "No previous conversation.";
            }

            if (maxMessages <= 0)
                throw new ArgumentException("Max messages must be greater than 0.", nameof(maxMessages));

            var sb = new StringBuilder();

            foreach (var message in history.TakeLast(maxMessages))
            {
                var role = message.Role;
                sb.AppendLine($"**{role}:** {message.Content}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string BuildStudyChatPrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            string userQuestion,
            List<OllamaMessage>? conversationHistory = null)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (string.IsNullOrWhiteSpace(userQuestion))
                throw new ArgumentException("User question cannot be null or empty.", nameof(userQuestion));

            var sb = new StringBuilder();

            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            sb.AppendLine("## RELEVANT COURSE MATERIALS");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine();

            if (conversationHistory != null && conversationHistory.Any())
            {
                sb.AppendLine("## PREVIOUS CONVERSATION");
                sb.AppendLine(FormatConversationHistory(conversationHistory));
                sb.AppendLine();
            }

            sb.AppendLine("## CURRENT QUESTION");
            sb.AppendLine(userQuestion);
            sb.AppendLine();
            sb.AppendLine("Please provide a helpful response based on the course materials. Remember to cite your sources.");

            return sb.ToString();
        }
        public static string BuildStudyChatPrompt(
            List<ContextChunk> contextChunks,
            string userQuestion,
            List<OllamaMessage>? conversationHistory = null)
        {
            return BuildStudyChatPrompt(ChatPrompts.SystemInstructions, contextChunks, userQuestion, conversationHistory);
        }

        public static string BuildSummarizationPrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            int summaryLength,
            bool includeKeyPoints)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (summaryLength <= 0)
                throw new ArgumentException("Summary length must be greater than 0.", nameof(summaryLength));

            var sb = new StringBuilder();

            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            sb.AppendLine("## Content to Summarize:");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("## Summarization Request:");
            sb.AppendLine($"- **Summary Length:** {summaryLength}");
            sb.AppendLine($"- **Include Key Points:** {includeKeyPoints}");
            sb.AppendLine();

            sb.AppendLine("Please summarize the content above according to the specified length.");
            sb.AppendLine();

            sb.AppendLine("## Required JSON Response Format:");
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"summary\": \"The main summary text as clear paragraphs...\",");
            sb.AppendLine("  \"keyPoints\": [");
            sb.AppendLine("    \"Key point 1\",");
            sb.AppendLine("    \"Key point 2\",");
            sb.AppendLine("    \"Key point 3\"");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"keyTerms\": {");
            sb.AppendLine("    \"Term 1\": \"Definition of term 1\",");
            sb.AppendLine("    \"Term 2\": \"Definition of term 2\"");
            sb.AppendLine("  },");
            sb.AppendLine("  \"sourceTitle\": \"Title of the summarized material\",");
            sb.AppendLine("  \"originalLength\": \"Approximate word count of original\",");
            sb.AppendLine("  \"summaryLength\": \"Word count of summary\"");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();

            sb.AppendLine("Generate the summary now:");
            return sb.ToString();
        }
        public static string BuildSummarizationPrompt(
            List<ContextChunk> contextChunks,
            int summaryLength,
            bool includeKeyPoints)
        {
            return BuildSummarizationPrompt(SummarizationPrompts.SystemInstructions, contextChunks, summaryLength, includeKeyPoints);
        }

        public static string BuildFlashCardPrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            string topic,
            int numOfCards)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));
            if (numOfCards <= 0)
                throw new ArgumentException("Number of cards must be greater than 0.", nameof(numOfCards));

            var sb = new StringBuilder();
            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();
            sb.AppendLine("## RELEVANT COURSE MATERIALS");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine();
            sb.AppendLine("## Flashcard Generation Request:");
            sb.AppendLine($"- **Topic:** {topic}");
            sb.AppendLine($"- **Number of Flashcards:** {numOfCards}");
            sb.AppendLine();
            sb.AppendLine($"Generate exactly {numOfCards} flashcards based on the course materials above for the topic: \"{topic}\"");
            sb.AppendLine();
            sb.AppendLine("## Required JSON Response Format:");
            sb.AppendLine("```json");
            sb.AppendLine("[");
            sb.AppendLine("  {");
            sb.AppendLine("    \"front\": \"What is [term/concept]?\",");
            sb.AppendLine("    \"back\": \"[Clear, concise answer/definition]\",");
            sb.AppendLine("    \"difficulty\": \"medium\",");
            sb.AppendLine("    \"sourceTitle\": \"Material Title\",");
            sb.AppendLine("    \"sourceLocation\": \"Page 5\"");
            sb.AppendLine("  }");
            sb.AppendLine("]");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("Difficulty should be one of: \"easy\", \"medium\", \"hard\"");
            sb.AppendLine();
            sb.AppendLine("Generate the flashcards now:");

            return sb.ToString();
        }
        public static string BuildFlashCardPrompt(
            List<ContextChunk> contextChunks,
            string topic,
            int numOfCards)
        {
            return BuildFlashCardPrompt(
                FlashcardPrompts.SystemInstructions,
                contextChunks,
                topic,
                numOfCards
            );
        }

        public static string BuildMindMapPrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            string centralTopic,
            int maxDepth = 3,
            List<OllamaMessage>? conversationHistory = null)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (string.IsNullOrWhiteSpace(centralTopic))
                throw new ArgumentException("Central topic cannot be null or empty.", nameof(centralTopic));
            if (maxDepth <= 0)
                throw new ArgumentException("Max depth must be greater than 0.", nameof(maxDepth));

            var sb = new StringBuilder();

            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            if (conversationHistory != null && conversationHistory.Any())
            {
                sb.AppendLine("## CONVERSATION HISTORY");
                sb.AppendLine(FormatConversationHistory(conversationHistory));
                sb.AppendLine();
            }

            sb.AppendLine("## Source Materials for Mind Map Generation:");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("## Mind Map Generation Request:");
            sb.AppendLine($"- **Central Topic:** {centralTopic}");
            sb.AppendLine($"- **Maximum Depth:** {maxDepth} levels");
            sb.AppendLine();
            sb.AppendLine($"Generate a comprehensive mind map for the topic \"{centralTopic}\" based on the course materials above.");
            sb.AppendLine();

            sb.AppendLine("## Required JSON Response Format:");
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"id\": \"root\",");
            sb.AppendLine("  \"label\": \"Central Topic\",");
            sb.AppendLine("  \"description\": \"Brief description of the central topic\",");
            sb.AppendLine("  \"children\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"id\": \"branch1\",");
            sb.AppendLine("      \"label\": \"Subtopic 1\",");
            sb.AppendLine("      \"description\": \"Description of subtopic\",");
            sb.AppendLine("      \"sourceTitle\": \"Material Title\",");
            sb.AppendLine("      \"sourceLocation\": \"Page 5\",");
            sb.AppendLine("      \"children\": [");
            sb.AppendLine("        {");
            sb.AppendLine("          \"id\": \"branch1-1\",");
            sb.AppendLine("          \"label\": \"Concept\",");
            sb.AppendLine("          \"description\": \"Detailed explanation\",");
            sb.AppendLine("          \"children\": []");
            sb.AppendLine("        }");
            sb.AppendLine("      ]");
            sb.AppendLine("    }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("Generate the mind map now:");

            return sb.ToString();
        }
        public static string BuildMindMapPrompt(
            List<ContextChunk> contextChunks,
            string centralTopic,
            int maxDepth = 3,
            List<OllamaMessage>? conversationHistory = null)
        {
            return BuildMindMapPrompt(
                MindMapPrompts.SystemInstructions,
                contextChunks,
                centralTopic,
                maxDepth,
                conversationHistory
            );
        }

        public static string BuildQuizPrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            string topic,
            int numberOfQuestions,
            string difficulty,
            List<QuestionType> questionTypes,
            List<OllamaMessage>? conversationHistory = null)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));
            if (numberOfQuestions <= 0)
                throw new ArgumentException("Number of questions must be greater than 0.", nameof(numberOfQuestions));
            if (string.IsNullOrWhiteSpace(difficulty))
                throw new ArgumentException("Difficulty cannot be null or empty.", nameof(difficulty));
            if (questionTypes == null || !questionTypes.Any())
                throw new ArgumentException("At least one question type must be specified.", nameof(questionTypes));

            var sb = new StringBuilder();

            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            if (conversationHistory != null && conversationHistory.Any())
            {
                sb.AppendLine("## CONVERSATION HISTORY");
                sb.AppendLine(FormatConversationHistory(conversationHistory));
                sb.AppendLine();
            }

            sb.AppendLine("## Source Materials for Quiz Generation:");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("## Quiz Generation Request:");
            sb.AppendLine($"- **Topic:** {topic}");
            sb.AppendLine($"- **Number of Questions:** {numberOfQuestions}");
            sb.AppendLine($"- **Difficulty Level:** {difficulty}");
            sb.AppendLine($"- **Question Types Requested:** {string.Join(", ", questionTypes)}");
            sb.AppendLine();
            sb.AppendLine($"Generate exactly {numberOfQuestions} practice quiz questions based on the course materials above.");
            sb.AppendLine();

            sb.AppendLine("## Required JSON Response Format:");
            sb.AppendLine("```json");
            sb.AppendLine("[");
            sb.AppendLine("  {");
            sb.AppendLine("    \"questionText\": \"The question text here\",");
            sb.AppendLine("    \"questionType\": \"mcq\",");
            sb.AppendLine("    \"options\": [\"Option A\", \"Option B\", \"Option C\", \"Option D\"],");
            sb.AppendLine("    \"correctAnswer\": \"Option A\",");
            sb.AppendLine("    \"explanation\": \"Explanation of why this is correct\",");
            sb.AppendLine("    \"difficulty\": \"medium\",");
            sb.AppendLine("    \"suggestedPoints\": 2,");
            sb.AppendLine("    \"sourceTitle\": \"Material Title\",");
            sb.AppendLine("    \"sourceLocation\": \"Page 5\"");
            sb.AppendLine("  }");
            sb.AppendLine("]");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("For true_false questions, use options: [\"True\", \"False\"]");
            sb.AppendLine("For short_answer questions, options should be null and correctAnswer should be the expected answer.");
            sb.AppendLine();
            sb.AppendLine("Generate the questions now:");

            return sb.ToString();
        }
        public static string BuildQuizPrompt(
            List<ContextChunk> contextChunks,
            string topic,
            int numberOfQuestions,
            string difficulty,
            List<QuestionType> questionTypes,
            List<OllamaMessage>? conversationHistory = null)
        {
            return BuildQuizPrompt(
                QuizPrompts.SystemInstructions,  
                contextChunks,
                topic,
                numberOfQuestions,
                difficulty,
                questionTypes,
                conversationHistory
            );
        }

        public static string BuildEssayGradingPrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            string questionText,
            int maxPoints,
            string gradingRubric,
            string modelAnswer,
            string studentAnswer)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (string.IsNullOrWhiteSpace(questionText))
                throw new ArgumentException("Question text cannot be null or empty.", nameof(questionText));
            if (maxPoints <= 0)
                throw new ArgumentException("Max points must be greater than 0.", nameof(maxPoints));
            if (string.IsNullOrWhiteSpace(studentAnswer))
                throw new ArgumentException("Student answer cannot be null or empty.", nameof(studentAnswer));

            var sb = new StringBuilder();

            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            sb.AppendLine("## Relevant Course Materials (for fact-checking the answer):");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("## Essay Grading Request:");
            sb.AppendLine();
            sb.AppendLine("### Question Information:");
            sb.AppendLine($"**Question:** {questionText}");
            sb.AppendLine();
            sb.AppendLine($"**Maximum Points:** {maxPoints}");
            sb.AppendLine();

            sb.AppendLine("**Grading Rubric:**");
            if (string.IsNullOrWhiteSpace(gradingRubric))
            {
                sb.AppendLine(EssayGradingPrompts.DefaultRubricTemplate);
            }
            else
            {
                sb.AppendLine(gradingRubric);
            }
            sb.AppendLine();

            sb.AppendLine("**Model Answer (if available):**");
            if (string.IsNullOrWhiteSpace(modelAnswer))
            {
                sb.AppendLine(EssayGradingPrompts.NoModelAnswerNote);
            }
            else
            {
                sb.AppendLine(modelAnswer);
            }
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("### Student's Answer:");
            sb.AppendLine(studentAnswer);
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("## Grading Instructions:");
            sb.AppendLine("Grade the student's answer above based on:");
            sb.AppendLine("1. The question requirements");
            sb.AppendLine("2. The grading rubric (if provided)");
            sb.AppendLine("3. Comparison with the model answer (if provided)");
            sb.AppendLine("4. Accuracy according to the course materials");
            sb.AppendLine();

            sb.AppendLine("## Required JSON Response Format:");
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"score\": 8.5,");
            sb.AppendLine("  \"maxPoints\": 10,");
            sb.AppendLine("  \"percentage\": 85.0,");
            sb.AppendLine("  \"feedback\": \"A comprehensive paragraph of feedback for the student explaining their performance, what they did well, and areas for improvement.\",");
            sb.AppendLine("  \"criteriaBreakdown\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"criterionName\": \"Content Accuracy\",");
            sb.AppendLine("      \"score\": 3.5,");
            sb.AppendLine("      \"maxScore\": 4,");
            sb.AppendLine("      \"feedback\": \"Specific feedback for this criterion\"");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("      \"criterionName\": \"Completeness\",");
            sb.AppendLine("      \"score\": 2.5,");
            sb.AppendLine("      \"maxScore\": 3,");
            sb.AppendLine("      \"feedback\": \"Specific feedback for this criterion\"");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("      \"criterionName\": \"Understanding\",");
            sb.AppendLine("      \"score\": 2.5,");
            sb.AppendLine("      \"maxScore\": 3,");
            sb.AppendLine("      \"feedback\": \"Specific feedback for this criterion\"");
            sb.AppendLine("    }");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"strengths\": [");
            sb.AppendLine("    \"Specific strength 1 demonstrated in the answer\",");
            sb.AppendLine("    \"Specific strength 2 demonstrated in the answer\"");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"areasForImprovement\": [");
            sb.AppendLine("    \"Specific area 1 that could be improved\",");
            sb.AppendLine("    \"Specific area 2 that could be improved\"");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"confidence\": 0.85,");
            sb.AppendLine("  \"requiresTeacherReview\": false,");
            sb.AppendLine("  \"reviewReason\": \"Optional: reason why teacher review is needed\"");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("Grade the essay now:");

            return sb.ToString();
        }
        public static string BuildEssayGradingPrompt(
            List<ContextChunk> contextChunks,
            string questionText,
            int maxPoints,
            string modelAnswer,
            string studentAnswer)
        {
            return BuildEssayGradingPrompt(
                EssayGradingPrompts.SystemInstructions,
                contextChunks,
                questionText,
                maxPoints,
                EssayGradingPrompts.DefaultRubricTemplate,
                modelAnswer,
                studentAnswer
            );
        }

        public static string BuildQuestionGenerationPrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            int numberOfQuestions,
            string difficulty,
            List<string> questionTypes,
            List<string>? focusTopics = null)
        {
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (numberOfQuestions <= 0)
                throw new ArgumentException("Number of questions must be greater than 0.", nameof(numberOfQuestions));
            if (string.IsNullOrWhiteSpace(difficulty))
                throw new ArgumentException("Difficulty cannot be null or empty.", nameof(difficulty));
            if (questionTypes == null || !questionTypes.Any())
                throw new ArgumentException("At least one question type must be specified.", nameof(questionTypes));

            var sb = new StringBuilder();

            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            sb.AppendLine("## Course Materials for Question Generation:");
            sb.AppendLine();
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("## Exam Question Generation Request:");
            sb.AppendLine();
            sb.AppendLine("### Parameters:");
            sb.AppendLine($"- **Number of Questions:** {numberOfQuestions}");
            sb.AppendLine($"- **Difficulty Level:** {difficulty}");
            sb.AppendLine($"- **Question Types:** {string.Join(", ", questionTypes)}");

            if (focusTopics != null && focusTopics.Any())
            {
                sb.AppendLine($"- **Focus Topics:** {string.Join(", ", focusTopics)}");
            }
            else
            {
                sb.AppendLine("- **Focus Topics:** Cover all topics in the materials");
            }
            sb.AppendLine();

            sb.AppendLine("### Instructions for the Teacher:");
            sb.AppendLine($"Generate {numberOfQuestions} high-quality exam questions based on the course materials above.");
            sb.AppendLine();

            if (questionTypes.Contains("mcq", StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine(ExamQuestionPrompts.MCQSpecificInstructions);
                sb.AppendLine();
            }

            if (questionTypes.Contains("essay", StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine(ExamQuestionPrompts.EssaySpecificInstructions);
                sb.AppendLine();
            }

            sb.AppendLine("## Required JSON Response Format:");
            sb.AppendLine("```json");
            sb.AppendLine("[");
            sb.AppendLine("  {");
            sb.AppendLine("    \"questionText\": \"The complete question text\",");
            sb.AppendLine("    \"questionType\": \"mcq\",");
            sb.AppendLine("    \"options\": [\"Option A\", \"Option B\", \"Option C\", \"Option D\"],");
            sb.AppendLine("    \"correctAnswer\": \"Option A\",");
            sb.AppendLine("    \"explanation\": \"Detailed explanation of why this is correct and why others are wrong\",");
            sb.AppendLine("    \"difficulty\": \"medium\",");
            sb.AppendLine("    \"suggestedPoints\": 2,");
            sb.AppendLine("    \"gradingCriteria\": \"For MCQ: Full points for correct answer, 0 for incorrect\",");
            sb.AppendLine("    \"sourceTitle\": \"Material Title\",");
            sb.AppendLine("    \"sourceSection\": \"Section Name\",");
            sb.AppendLine("    \"sourceLocation\": \"Page 5\",");
            sb.AppendLine("    \"learningObjective\": \"What skill/knowledge this question assesses\"");
            sb.AppendLine("  }");
            sb.AppendLine("]");
            sb.AppendLine("```");
            sb.AppendLine();

            if (questionTypes.Contains("essay", StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine("### For Essay Questions, include additional fields:");
                sb.AppendLine("```json");
                sb.AppendLine("{");
                sb.AppendLine("  \"questionType\": \"essay\",");
                sb.AppendLine("  \"modelAnswer\": \"A comprehensive model answer that would receive full marks\",");
                sb.AppendLine("  \"gradingRubric\": [");
                sb.AppendLine("    {\"criterion\": \"Understanding of concept\", \"maxPoints\": 5, \"description\": \"Shows clear understanding...\"},");
                sb.AppendLine("    {\"criterion\": \"Use of examples\", \"maxPoints\": 3, \"description\": \"Provides relevant examples...\"},");
                sb.AppendLine("    {\"criterion\": \"Clarity and organization\", \"maxPoints\": 2, \"description\": \"Well-structured response...\"}");
                sb.AppendLine("  ]");
                sb.AppendLine("}");
                sb.AppendLine("```");
                sb.AppendLine();
            }

            sb.AppendLine("Generate the exam questions now:");

            return sb.ToString();
        }
        public static string BuildQuestionGenerationPrompt(
             List<ContextChunk> contextChunks,
             int numberOfQuestions,
             string difficulty,
             List<string> questionTypes,
             List<string>? focusTopics = null)
        {
            return BuildQuestionGenerationPrompt(
                ExamQuestionPrompts.SystemInstructions,
                contextChunks,
                numberOfQuestions,
                difficulty,
                questionTypes,
                focusTopics
            );
        }

        /// <summary>
        /// Builds a prompt for generating a teacher-student dialogue that explains course content.
        /// The output is designed for audio transcription with distinct speaker voices.
        /// </summary>
        public static string BuildTeacherStudentDialoguePrompt(
            string instructions,
            List<ContextChunk> contextChunks,
            string? topic = null,
            string audienceLevel = "intermediate",
            int numberOfExchanges = 5,
            string dialogueLength = "medium",
            bool includeExamples = true,
            bool includeSummary = true,
            string teachingStyle = "interactive",
            List<string>? focusConcepts = null)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(instructions))
                throw new ArgumentException("Instructions cannot be null or empty.", nameof(instructions));
            if (contextChunks == null)
                throw new ArgumentNullException(nameof(contextChunks));
            if (numberOfExchanges <= 0)
                throw new ArgumentException("Number of exchanges must be greater than 0.", nameof(numberOfExchanges));

            var sb = new StringBuilder();

            // System instructions
            sb.AppendLine("## SYSTEM INSTRUCTIONS");
            sb.AppendLine(instructions);
            sb.AppendLine();

            // Teaching style instructions
            sb.AppendLine(TeacherStudentDialoguePrompts.GetTeachingStyleInstructions(teachingStyle));
            sb.AppendLine();

            // Audience level instructions
            sb.AppendLine(TeacherStudentDialoguePrompts.GetAudienceLevelInstructions(audienceLevel));
            sb.AppendLine();

            // Course materials context
            sb.AppendLine("## COURSE MATERIALS TO EXPLAIN:");
            sb.AppendLine(FormatContextChunks(contextChunks));
            sb.AppendLine("---");
            sb.AppendLine();

            // Dialogue generation request
            sb.AppendLine("## DIALOGUE GENERATION REQUEST:");
            sb.AppendLine();

            // Get length guidelines
            var (minWords, maxWords, approxExchanges) = TeacherStudentDialoguePrompts.GetDialogueLengthGuidelines(dialogueLength);

            sb.AppendLine("### Parameters:");
            if (!string.IsNullOrWhiteSpace(topic))
            {
                sb.AppendLine($"- **Main Topic:** {topic}");
            }
            else
            {
                sb.AppendLine("- **Main Topic:** Cover the key concepts from the provided materials");
            }
            sb.AppendLine($"- **Audience Level:** {audienceLevel}");
            sb.AppendLine($"- **Teaching Style:** {teachingStyle}");
            sb.AppendLine($"- **Target Exchanges:** {numberOfExchanges} dialogue exchanges (teacher explains, student asks, teacher answers)");
            sb.AppendLine($"- **Target Word Count:** {minWords} to {maxWords} words total");
            sb.AppendLine($"- **Dialogue Length:** {dialogueLength} (~{GetApproximateDuration(dialogueLength)})");
            sb.AppendLine($"- **Include Examples:** {(includeExamples ? "Yes" : "No")}");
            sb.AppendLine($"- **Include Summary:** {(includeSummary ? "Yes, include a summary at the end" : "No summary needed")}");
            sb.AppendLine();

            if (focusConcepts != null && focusConcepts.Any())
            {
                sb.AppendLine("### Specific Concepts to Cover:");
                foreach (var concept in focusConcepts)
                {
                    sb.AppendLine($"- {concept}");
                }
                sb.AppendLine();
            }

            // Detailed instructions for the dialogue
            sb.AppendLine("### Dialogue Structure Instructions:");
            sb.AppendLine("1. **Opening**: Teacher warmly introduces the topic");
            sb.AppendLine("2. **Main Content**: Teacher explains concepts, student asks questions");
            sb.AppendLine("3. **Examples**: " + (includeExamples ? "Include concrete examples to illustrate concepts" : "Focus on explanations without extensive examples"));
            sb.AppendLine("4. **Clarifications**: Student should ask for clarification on complex points");
            sb.AppendLine("5. **Closing**: " + (includeSummary ? "End with a brief recap of key points" : "End naturally after covering the content"));
            sb.AppendLine();

            // Critical reminders for audio transcription
            sb.AppendLine("### CRITICAL - Audio Transcription Requirements:");
            sb.AppendLine("- ALWAYS use EXACTLY \"TEACHER\" or \"STUDENT\" as the speaker value");
            sb.AppendLine("- Write content that sounds natural when spoken aloud");
            sb.AppendLine("- Avoid bullet points, numbered lists, or special formatting in content");
            sb.AppendLine("- Use conversational contractions (I'm, you're, let's, etc.)");
            sb.AppendLine("- Include natural speech patterns and transitions");
            sb.AppendLine("- The dialogue will be read by two different voice actors - make it sound like a real conversation");
            sb.AppendLine();

            // JSON format
            sb.AppendLine(TeacherStudentDialoguePrompts.JsonResponseFormat);
            sb.AppendLine();

            sb.AppendLine("Generate the teacher-student dialogue now:");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a prompt for generating a teacher-student dialogue using default instructions.
        /// </summary>
        public static string BuildTeacherStudentDialoguePrompt(
            List<ContextChunk> contextChunks,
            string? topic = null,
            string audienceLevel = "intermediate",
            int numberOfExchanges = 5,
            string dialogueLength = "medium",
            bool includeExamples = true,
            bool includeSummary = true,
            string teachingStyle = "interactive",
            List<string>? focusConcepts = null)
        {
            return BuildTeacherStudentDialoguePrompt(
                TeacherStudentDialoguePrompts.SystemInstructions,
                contextChunks,
                topic,
                audienceLevel,
                numberOfExchanges,
                dialogueLength,
                includeExamples,
                includeSummary,
                teachingStyle,
                focusConcepts
            );
        }

        /// <summary>
        /// Gets approximate duration string for dialogue length
        /// </summary>
        private static string GetApproximateDuration(string length)
        {
            return length.ToLowerInvariant() switch
            {
                "short" => "2-3 minutes",
                "medium" => "5-7 minutes",
                "long" => "10-15 minutes",
                _ => "5-7 minutes"
            };
        }
    }
}