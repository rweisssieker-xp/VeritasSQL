namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-generated narrative story from data
/// </summary>
public class DataStory
{
    public string Title { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty; // 2-3 sentences
    public List<StoryChapter> Chapters { get; set; } = new();
    public string Conclusion { get; set; } = string.Empty;
    public List<string> KeyTakeaways { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string Tone { get; set; } = "professional"; // professional|casual|technical
}

/// <summary>
/// Chapter in data story
/// </summary>
public class StoryChapter
{
    public string ChapterTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Paragraph format
    public List<string> KeyFigures { get; set; } = new(); // "Top performers", "Problem cases"
    public List<string> SupportingData { get; set; } = new(); // Specific numbers
}

/// <summary>
/// Voice transcription result
/// </summary>
public class VoiceTranscription
{
    public string TranscribedText { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // en|de
    public double Confidence { get; set; } // 0.0-1.0
    public double DurationSeconds { get; set; }
}
