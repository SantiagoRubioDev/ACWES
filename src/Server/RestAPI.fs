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

                let cmdOut = Processes.runScript (filePath+".cmd")

                let coursework = 
                    { AssignmentID = Query.useAssignmentId ctx
                      State = "Run Success: Done"
                      CmdOut = cmdOut
                      Feedback = "Good Job"
                      Grade = "A" }

                DBFile.Coursework.write coursework ctx
                
                return! Successful.OK ( "upload successful" ) ctx//(dirPath+"attempt1.cmd")  "file uploaded correctly" ctx

            with exn ->
                logger.error (eventX "Database not available" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })    


module Modules =
    /// Handle the GET on /api/modules
    let get (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let modules = DBFile.Modules.readForUser token.UserName
                return! Successful.OK (JsonConvert.SerializeObject modules) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })
   
    /// Handle the POST on /api/modules
    let post (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try

                let newModule:Domain.ModuleRow = 
                    ctx.request.rawForm
                    |> System.Text.Encoding.UTF8.GetString
                    |> JsonConvert.DeserializeObject<Domain.ModuleRow>
            
                //if token.UserName <> modules.UserName then
                //    return! UNAUTHORIZED (sprintf "Modules is not matching user %s" token.UserName) ctx
                //else

                let allModules = DBFile.Modules.readAll() //token.UserName

                let userModules = DBFile.Modules.readForUser token.UserName

                let moduleWithNewModuleID = (ModulesValidation.verifyModules newModule allModules)
                
                if  moduleWithNewModuleID = None then
                    DBFile.Modules.write (newModule::allModules)
                    DBFile.Users.addModuleId [token.UserName] newModule.ID
                    return! Successful.OK (JsonConvert.SerializeObject (newModule::userModules)) ctx
                else
                    return! BAD_REQUEST ( "Module with ID " + moduleWithNewModuleID.Value.ID + " already exists in the database. It is " + moduleWithNewModuleID.Value.Data.Title + " owned by " + moduleWithNewModuleID.Value.Data.Teacher ) ctx
            with exn ->
                logger.error (eventX "Database not available" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })    

module Module =
    /// Handle the GET on /api/module
    let get (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let modules = DBFile.Modules.readForUser token.UserName
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

module Coursework =
    /// Handle the GET on /api/coursework
    let get (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let coursework = DBFile.Coursework.read ctx//token.UserName
                if coursework.AssignmentID = "" then
                    return! INTERNAL_ERROR "Please upload your coursework" ctx
                else
                    return! Successful.OK (JsonConvert.SerializeObject coursework) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })