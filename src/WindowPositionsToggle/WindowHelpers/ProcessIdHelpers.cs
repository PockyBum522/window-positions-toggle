namespace WindowPositionsToggle.WindowHelpers;

public static class ProcessIdHelpers
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
        
        trimmedHex = trimmedHex.Trim('\'');
        
        return Convert.ToInt32(trimmedHex, 16);
    }
}