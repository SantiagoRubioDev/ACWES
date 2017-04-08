module ServerCode.RestAPI

open Newtonsoft.Json
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.ServerErrors
open Suave.Logging
open Suave.Logging.Message
open ServerCode.Domain

let logger = Log.create "FableSample"



/// Handle the POST on /api/upload
module Upload =
    let post (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                logger.debug (eventX "santi debug file upload")
                logger.debug (eventX ("headers length "+ctx.request.headers.Length.ToString()))
                logger.debug (eventX ("form length "+ctx.request.form.Length.ToString()))

                let fileText =
                    match ctx.request.form.Head with
                    | a,_ -> a//logger.debug (eventX ("form head type "+a+" ;;;;; "+( if b.IsSome then b.Value else "None")))

                let dirPath = DBFile.Path.uploadDir ctx

                let filename = "attempt1"

                let filePath = "./"+dirPath+filename

                DBFile.Upload.write fileText ctx
                
                return! Successful.OK ( Processes.runScript (filePath+".cmd") ) ctx//(dirPath+"attempt1.cmd")  "file uploaded correctly" ctx

            with exn ->
                logger.error (eventX "Database not available" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })    


module Modules =
    /// Handle the GET on /api/modules
    let get (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let modules = DBFile.Modules.read token.UserName
                return! Successful.OK (JsonConvert.SerializeObject modules) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })
   
    /// Handle the POST on /api/modules
    let post (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let modules:Domain.ModuleTable = 
                    ctx.request.rawForm
                    |> System.Text.Encoding.UTF8.GetString
                    |> JsonConvert.DeserializeObject<Domain.ModuleTable>
            
                //if token.UserName <> modules.UserName then
                //    return! UNAUTHORIZED (sprintf "Modules is not matching user %s" token.UserName) ctx
                //else
                
                if ModulesValidation.verifyModules modules then
                    DBFile.Modules.write modules
                    return! Successful.OK (JsonConvert.SerializeObject modules) ctx
                else
                    return! BAD_REQUEST "Modules is not valid" ctx
            with exn ->
                logger.error (eventX "Database not available" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })    

module Module =
    /// Handle the GET on /api/module
    let get (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let modules = DBFile.Modules.read token.UserName
                let _module = List.tryFind (fun (x:ModuleRow) -> x.ID = (Query.useModuleId ctx)) modules
                if _module.IsSome then
                    return! Successful.OK (JsonConvert.SerializeObject _module.Value) ctx
                else
                    return! INTERNAL_ERROR "module id not found in Modules Table" ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })

module Assignments =
    /// Handle the GET on /api/assignments
    let get (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let assignments = DBFile.Assignments.read (Query.useModuleId ctx)//token.UserName
                if assignments.IsEmpty then
                    return! INTERNAL_ERROR "assignment id not found in assignemtns Table" ctx
                else
                    return! Successful.OK (JsonConvert.SerializeObject assignments) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })