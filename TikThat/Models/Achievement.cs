namespace TikThat.Models;

public class Achievement
{
    public required string Id { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "trophy_icon.svg";
    public bool IsSecret { get; set; }
    public string Guide { get; set; } = string.Empty;
    public string SampleTaskText { get; set; } = string.Empty;
}