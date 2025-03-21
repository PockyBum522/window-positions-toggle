using System.Diagnostics;
using Serilog;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class WmCtrlWrapper
{
    private readonly ILogger _logger;
    
    private Process _toolProcess = new();
    
    public WmCtrlWrapper(ILogger logger)
    {
        _logger = logger;
        
        _toolProcess.StartInfo.FileName = "wmctrl";
        _toolProcess.StartInfo.UseShellExecute = false;
        _toolProcess.StartInfo.RedirectStandardOutput = true;
        _toolProcess.StartInfo.CreateNoWindow = true;
    }
    
    public string[] RunWmCtrl(string arguments)
    {
        // Note to self - Taking try catch out of here to see how fast I can make this, then may want to add back in 
        //      and see how it slows, if any

        _logger.Debug("[Wmctrl] About to run with args: {Arguments}", arguments);

        _toolProcess.StartInfo.Arguments = arguments;        

        _toolProcess.Start();
            
        var rawLines = _toolProcess.StandardOutput.ReadToEnd().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        
        _toolProcess.WaitForExit();
        
        _logger.Debug("[Wmctrl] Ran with args: {Arguments}", arguments);
        
        return rawLines;
    }
}
