public class WinFormsMenuItem
{
    public string Text { get; set; } = string.Empty;
    public string? IconCss { get; set; }
    public string? Url { get; set; }
    public List<WinFormsMenuItem>? SubItems { get; set; }
}