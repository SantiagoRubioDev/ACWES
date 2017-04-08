module ServerCode.Processes

open System
open System.IO
open Suave.Logging
open Suave.Logging.Message

let logger = Log.create "FableSample"

let runScript commandPath = 
    logger.debug (eventX "run script start")
    logger.debug (eventX ( "cmd file path: " + commandPath ))
    //let startInfo = new Diagnostics.ProcessStartInfo("cmd.exe", "/c run.cmd")
    let startInfo = new Diagnostics.ProcessStartInfo("cmd.exe", "/c attempt1.cmd")

    startInfo.WorkingDirectory <- startInfo.WorkingDirectory+Path.GetDirectoryName(commandPath)

    logger.debug (eventX ( "working directory after path: " + startInfo.WorkingDirectory ))

    startInfo.UseShellExecute <- false
    startInfo.CreateNoWindow <- true
    startInfo.RedirectStandardError <- true
    startInfo.RedirectStandardOutput <- true

    let _process = Diagnostics.Process.Start(startInfo)

    let out = _process.StandardOutput.ReadToEnd()
    _process.WaitForExit()

    logger.debug (eventX "run script end")

    out