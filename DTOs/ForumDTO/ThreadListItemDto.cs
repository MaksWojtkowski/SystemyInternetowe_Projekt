namespace ArcadeProject.DTOs.ForumDTO;

public class ThreadListItemDto
{
    public int      Id          { get; init; }
    public string   Title       { get; init; } = string.Empty;
    public string   AuthorName  { get; init; } = string.Empty;
    public int      PostCount   { get; init; }
    public DateTime CreatedAt   { get; init; }
    public DateTime LastPostAt  { get; init; }
    public bool     IsPinned    { get; init; }
    
    public int?     GameId      { get; init; }
    public string?  GameTitle   { get; init; }
    public string?  GameSlug    { get; init; }
}