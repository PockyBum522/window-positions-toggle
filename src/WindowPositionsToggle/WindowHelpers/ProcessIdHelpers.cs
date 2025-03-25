namespace WindowPositionsToggle.WindowHelpers;

public class ProcessIdHelpers
{
    public static string LongIdToHexLeadingZero(long processId)
    {
        var toHex = "0x";
        
        toHex += processId.ToString("x8");
        
        return toHex;
    }
    
    public static long HexIdToLong(string hexProcessId)
    {
        var trimmedHex = hexProcessId.Trim();
        
        trimmedHex = hexProcessId.Trim('\'');
        
        return Convert.ToInt32(trimmedHex, 16);
    }
}