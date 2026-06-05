namespace ArcadeProject.DTOs.ForumDTO;

public class PostDto
{
    public int      Id          { get; init; }
    public string   AuthorName  { get; init; } = string.Empty;
    public string   AuthorId    { get; init; } = string.Empty;
    public string   Body        { get; init; } = string.Empty;
    public DateTime CreatedAt   { get; init; }
    public DateTime? UpdatedAt  { get; init; }
    public bool     IsEdited    => UpdatedAt.HasValue;
}