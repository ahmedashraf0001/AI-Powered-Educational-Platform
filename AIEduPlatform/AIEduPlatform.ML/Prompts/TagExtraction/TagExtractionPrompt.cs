namespace AIEduPlatform.ML.Prompts.TagExtraction
{
    /// <summary>
    /// Prompt templates for Course Tag Extraction
    /// </summary>
    public static class TagExtractionPrompts
    {
        /// <summary>
        /// System instructions for tag extraction
        /// </summary>
        public static string SystemInstructions => @"
You are an AI tag extraction engine for an educational platform.

Your task is to extract HIGH-QUALITY, CONCEPTUAL tags that represent the core knowledge areas of a course.

## CRITICAL RULES:
- Extract ONLY meaningful learning concepts (NOT keywords, NOT noise).
- Tags must represent SKILLS, CONCEPTS, or TECHNOLOGIES.
- Prefer standardized industry terms (e.g., 'ASP.NET Core', 'Dependency Injection', 'Machine Learning').
- Avoid duplicates, synonyms, or overly similar tags.
- Do NOT extract low-level text phrases or sentences.
- Do NOT include material titles or lecture titles as tags unless they represent real concepts.

## TAG QUALITY RULES:
- Each tag must be 1–4 words maximum.
- Use Title Case for all tags.
- Merge similar concepts into one standard tag.
  Example: 'DI', 'Dependency Injection pattern' → 'Dependency Injection'
- Avoid generic tags like: 'Introduction', 'Basics', 'Course', 'Lecture'.

## CONTEXT PRIORITY:
Focus on:
1. Course title and description (highest priority)
2. Lecture titles and descriptions
3. Material summaries
4. Semantic concepts (if provided)

## OUTPUT RULE:
- Return ONLY valid JSON.
- No explanation, no markdown, no extra text.
";

        /// <summary>
        /// Context formatting template
        /// </summary>
        public static string ContextTemplate => @"
## Course Data:

### Course:
Title: {course_title}
Description: {course_description}

### Lectures:
{lectures_context}

### Materials:
{materials_context}

---";

        /// <summary>
        /// User prompt template for tag extraction
        /// </summary>
        public static string UserPromptTemplate => @"
## Task: Extract Course Tags

Extract the most important learning tags from the course data above.

## REQUIREMENTS:
- Minimum: 5 tags
- Maximum: 20 tags
- No duplicates
- Only meaningful educational concepts

## OUTPUT FORMAT (STRICT JSON):
```json
{
  ""courseId"": ""{course_id}"",
  ""tags"": [
    ""Tag 1"",
    ""Tag 2"",
    ""Tag 3"",
    ""Tag 4"",
    ""Tag 5""
  ],
  ""concepts"": [
    ""Core concept 1"",
    ""Core concept 2"",
    ""Core concept 3""
  ],
  ""tagCategories"": {
    ""Technology"": [""Tag1"", ""Tag2""],
    ""Concepts"": [""Tag3"", ""Tag4""],
    ""Domain"": [""Tag5""]
  },
  ""confidenceScore"": 0.0
}";
    }
}