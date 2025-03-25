using System.Diagnostics;
using Serilog;
using WindowPositionsToggle.Models;

namespace WindowPositionsToggle.WindowHelpers;

public class ShellCommandWrapper
{
    private readonly ILogger _logger;
    
    private Process _toolProcess = new();
    
    public ShellCommandWrapper(ILogger logger)
    {
        _logger = logger;
        
        _toolProcess.StartInfo.UseShellExecute = false;
        _toolProcess.StartInfo.RedirectStandardOutput = true;
        _toolProcess.StartInfo.CreateNoWindow = true;
    }
    
    public string[] RunInShell(string command, string arguments)
    {
        // Note to self - Taking try catch out of here to see how fast I can make this, then may want to add back in 
        //      and see how it slows, if any

        _toolProcess.StartInfo.FileName = command;
        
        _logger.Debug("[{CommandName}] About to run with args: {Arguments}", command, arguments);

        _toolProcess.StartInfo.Arguments = arguments;        

        _toolProcess.Start();
            
        var rawLines = _toolProcess.StandardOutput.ReadToEnd().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        
        _toolProcess.WaitForExit();
        
        _logger.Debug("[{CommandName}] Ran with args: {Arguments}", command, arguments);
        
        return rawLines;
    }
}
