using AIEduPlatform.Core.DTOs.AI.Simple;

namespace AIEduPlatform.Core.DTOs.AI.Responses.Dialogue
{
    /// <summary>
    /// Response wrapper for Teacher-Student Dialogue Generation
    /// </summary>
    public class TeacherStudentDialogueResponse : ResponseBase
    {
        /// <summary>
        /// The generated dialogue
        /// </summary>
        public TeacherStudentDialogue? Dialogue { get; set; }

        /// <summary>
        /// Whether the dialogue is ready for audio transcription
        /// </summary>
        public bool ReadyForTranscription => 
            Dialogue != null && 
            Dialogue.Turns.Any() && 
            Dialogue.Turns.All(t => 
                !string.IsNullOrWhiteSpace(t.Speaker) && 
                !string.IsNullOrWhiteSpace(t.Content));
    }
}
