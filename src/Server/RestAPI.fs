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
    let coursework (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                logger.debug (eventX "santi debug file upload")
                logger.debug (eventX ("headers length "+ctx.request.headers.Length.ToString()))
                logger.debug (eventX ("form length "+ctx.request.form.Length.ToString()))

                let fileText : string = 
                    ctx.request.rawForm
                    |> System.Text.Encoding.UTF8.GetString
                    |> JsonConvert.DeserializeObject<string>

                let dirPath = DBFile.Path.uploadDirStudent ctx

                let filename = "StudentsAnswers"

                let filePath = "./"+dirPath+filename

                DBFile.Upload.coursework fileText ctx

                DBFile.Coursework.createRunBox ctx

                let cmdOut = Processes.runTB ("./"+dirPath+"RunBox/DotNet.fsproj")

                let coursework = 
                    { AssignmentID = Query.useAssignmentId ctx
                      State = "Run Success: Done"
                      CmdOut = cmdOut
                      Feedback = "Good Job"
                      Grade = "A" }

                DBFile.StudentCoursework.write coursework ctx
                
                return! Successful.OK ( "upload successful" ) ctx//(dirPath+"attempt1.cmd")  "file uploaded correctly" ctx

            with exn ->
                logger.error (eventX "Database not available" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })    

    let testbench (ctx: HttpContext) =
        logger.debug (eventX "start testbench")
        Auth.useToken ctx (fun token -> async {
            try
                logger.debug (eventX "santi debug testbench upload")
                logger.debug (eventX ("headers length "+ctx.request.headers.Length.ToString()))
                logger.debug (eventX ("form length "+ctx.request.form.Length.ToString()))
                logger.debug (eventX ("files length "+ctx.request.files.Length.ToString()))

                let fileText : string = 
                    ctx.request.rawForm
                    |> System.Text.Encoding.UTF8.GetString
                    |> JsonConvert.DeserializeObject<string>

                let dirPath = DBFile.Path.uploadDirTB ctx

                let filename = Query.useFileName ctx

                let filePath = "./"+dirPath+filename

                DBFile.Upload.testbench fileText filePath

                return! Successful.OK "Nom nom nom!" ctx

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
                let assignments = DBFile.Assignments.readForModule (Query.useModuleId ctx)//token.UserName
                if assignments.IsEmpty then
                    return! INTERNAL_ERROR "assignment id not found in assignemtns Table" ctx
                else
                    return! Successful.OK (JsonConvert.SerializeObject assignments) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })

    /// Handle the POST on /api/assignments/
    let post (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try

                let newAssignment:Domain.AssignmentRow = 
                    ctx.request.rawForm
                    |> System.Text.Encoding.UTF8.GetString
                    |> JsonConvert.DeserializeObject<Domain.AssignmentRow>

                let moduleId = newAssignment.Data.ModuleID

                let moduleAssignments = DBFile.Assignments.readForModule moduleId
                
                let assignments = newAssignment::moduleAssignments

                DBFile.Assignments.write moduleId assignments
  
                return! Successful.OK (JsonConvert.SerializeObject assignments) ctx

            with exn ->
                logger.error (eventX "Database not available" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })    

module Coursework =
    /// Handle the GET on /api/coursework/student/
    let getStudent (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let coursework = DBFile.StudentCoursework.read ctx//token.UserName
                if coursework.AssignmentID = "" then
                    return! INTERNAL_ERROR "Please upload your coursework" ctx //the string message is not sent to the client only INTERNAL_ERROR 500 is
                else
                    return! Successful.OK (JsonConvert.SerializeObject coursework) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })

    /// Handle the GET on /api/coursework/teacher/
    let getTeacher (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let coursework = DBFile.TeacherCoursework.read ctx//token.UserName
                if coursework.AssignmentID = "" then
                    return! INTERNAL_ERROR "Please upload your coursework" ctx //the string message is not sent to the client only INTERNAL_ERROR 500 is
                else
                    return! Successful.OK (JsonConvert.SerializeObject coursework) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })

module Users =
    /// Handle the GET on /api/coursework
    let getStudents (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let users = DBFile.Users.read()
                let students:UserTable = 
                    List.filter (fun user -> 
                        (List.contains (Query.useModuleId ctx) user.Data.ModulesID) 
                        && (user.Data.UserName <> token.UserName) ) users //
                return! Successful.OK (JsonConvert.SerializeObject students) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })

    let assignModule (ctx: HttpContext) =
        Auth.useToken ctx (fun token -> async {
            try
                let newStudents : string = 
                    ctx.request.rawForm
                    |> System.Text.Encoding.UTF8.GetString
                    |> JsonConvert.DeserializeObject<string>

                let newStudentNamesList = 
                    newStudents.Split [| ' '; '\f'; '\t'; '\r'; '\n'; ',' |]
                    |> Array.toList
                    |> List.filter ((<>) "") // delete empty strings generated by default .Split function

                DBFile.Users.addModuleId newStudentNamesList (Query.useModuleId ctx) 

                let users = DBFile.Users.read()
                let students:UserTable = 
                    List.filter (fun user -> 
                        (List.contains (Query.useModuleId ctx) user.Data.ModulesID) 
                        && (user.Data.UserName <> token.UserName) ) users

                return! Successful.OK (JsonConvert.SerializeObject students) ctx
            with exn ->
                logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
                return! SERVICE_UNAVAILABLE "Database not available" ctx
        })