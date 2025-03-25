// using System.Diagnostics;
// using Serilog;
// using WindowPositionsToggle.Models;
//
// namespace WindowPositionsToggle.WindowHelpers;
//
// public class XDoToolWrapper
// {
//     private readonly ILogger _logger;
//     
//     private Process _toolProcess = new();
//     
//     public XDoToolWrapper(ILogger logger)
//     {
//         _logger = logger;
//         
//         _toolProcess.StartInfo.FileName = "xdotool";
//         _toolProcess.StartInfo.UseShellExecute = false;
//         _toolProcess.StartInfo.RedirectStandardOutput = true;
//         _toolProcess.StartInfo.CreateNoWindow = true;
//     }
//     
//     // public List<long> GetWindowIdsByTitle(string windowPartialTitle)
//     // {
//     //     var toolReturnLines = RunXDoTool($"search --name \".*{windowPartialTitle}.*\"");
//     //
//     //     _logger.Debug("[Xdotool RAW COMMAND RETURN START]");
//     //     foreach (var returnLine in toolReturnLines)
//     //     {
//     //         _logger.Debug("{Line}", returnLine);
//     //     }
//     //     _logger.Debug("[Xdotool RAW COMMAND RETURN END]");
//     //     
//     //     if (toolReturnLines.Length < 1)
//     //         return [];
//     //
//     //     var returnIds = new List<long>();
//     //
//     //     foreach (var returnLine in toolReturnLines)
//     //     {
//     //         var id = long.Parse(returnLine);
//     //         
//     //         returnIds.Add(id);
//     //     }
//     //     
//     //     return returnIds;
//     // }   
//
//     public string[] RunXDoTool(string arguments)
//     {
//         // Note to self - Taking try catch out of here to see how fast I can make this, then may want to add back in 
//         //      and see how it slows, if any
//
//         _logger.Debug("[Xdotool] About to run with args: {Arguments}", arguments);
//
//         _toolProcess.StartInfo.Arguments = arguments;        
//
//         _toolProcess.Start();
//             
//         var rawLines = _toolProcess.StandardOutput.ReadToEnd().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
//         
//         _toolProcess.WaitForExit();
//         
//         _logger.Debug("[Xdotool] Ran with args: {Arguments}", arguments);
//         
//         return rawLines;
//     }
// }
