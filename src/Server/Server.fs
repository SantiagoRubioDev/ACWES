module ServerCode.Server

open System.IO
open Suave
open Suave.Logging
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors

let startServer clientPath =
    if not (Directory.Exists clientPath) then
        failwithf "Client-HomePath '%s' doesn't exist." clientPath

    let outPath = Path.Combine(clientPath,"public")
    if not (Directory.Exists outPath) then
        failwithf "Out-HomePath '%s' doesn't exist." outPath

    if Directory.EnumerateFiles outPath |> Seq.isEmpty then
        failwithf "Out-HomePath '%s' is empty." outPath

    let logger = Logging.Targets.create Logging.Verbose [| "Suave" |] //changed Logging.Info to Verbose for debug

    let serverConfig =
        { defaultConfig with
            logger = Targets.create LogLevel.Verbose [||] //changed LogLevel.Debug to Verbose for debug
            homeFolder = Some clientPath
            bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") 8085us] }

    let app =
        choose [
            GET >=> choose [
                path "/" >=> Files.browseFileHome "index.html"
                pathRegex @"/(public|js|css|Images)/(.*)\.(css|png|gif|jpg|js|map)" >=> Files.browseHome

                path "/api/wishlist/" >=> WishList.getWishList
                
                path "/api/modules/" >=> RestAPI.Modules.get

                path "/api/module" >=> RestAPI.Module.get
                
                path "/api/assignments" >=> RestAPI.Assignments.get 
                
                path "/api/coursework" >=> RestAPI.Coursework.get ]

            POST >=> choose [
                path "/api/users/login" >=> Auth.login

                path "/api/wishlist/" >=> WishList.postWishList

                path "/api/modules/" >=> RestAPI.Modules.post

                path "/api/upload" >=> RestAPI.Upload.post  // Successful.OK "is valid"//
            ]                
            
            NOT_FOUND "Page not found."

        ] >=> logWithLevelStructured Logging.Verbose logger logFormatStructured //changed Logging.Info to Verbose for debug

    startWebServer serverConfig app