namespace WindowPositionsToggle.Models;


public class SavedWindowPreferences
{
    public string TitlePattern { get; set; } = "";
    public string ClassPattern { get; set; } = "";
    
    public int ExtraLeftOffset { get; set; } = 0;
    public int ExtraTopOffset { get; set; } = 0;
    
    public decimal LeftTopScalingMultiple { get; set; } = 1.0m;
    
    public List<WindowPosition> PreferredPositions { get; set; } = [];
}
