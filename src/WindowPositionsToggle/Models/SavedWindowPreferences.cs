namespace WindowPositionsToggle.Models;


public class SavedWindowPreferences
{
    public string TitlePattern { get; set; } = "";
    public string ClassPattern { get; set; } = "";
    
    public List<WindowPosition> PreferredPositions { get; set; } = [];
}
