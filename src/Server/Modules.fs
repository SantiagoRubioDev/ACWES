module ServerCode.Modules

open System.IO
open Suave
open Suave.Logging
open Newtonsoft.Json
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open System
open Suave.ServerErrors
open ServerCode.Domain
open Suave.Logging
open Suave.Logging.Message

let logger = Log.create "FableSample"


/// The default initial data 
let defaultModules : ModuleTable =
    [{ ID =  "EE-430"
       Data = { Title = "Digital Electronics"
                Teacher = "Dr.X" } };
     { ID = "EE-350"
       Data = { Title = "Robotics"
                Teacher = "Dr.R" } };
     { ID =  "EE-260"
       Data = { Title = "Software Engineering"
                Teacher = "Dr.E" } } ]

let defaultStudent userName : Student =
    {
        UserName = userName
        ModulesID = [  "EE-430" ;  "EE-260" ]
    }

let defaultAssignments moduleId: AssignmentTable =
    [{ ID =  "CW-1"
       Data = {  ModuleID = moduleId
                 Title = "Coursework 1" 
                 StartDate = "15:00 2/04/2017"
                 EndDate = "20:00 10/04/2017"
                 Grade = ""} };
     { ID = "CW-2"
       Data = {  ModuleID = moduleId
                 Title = "Coursework 2" 
                 StartDate = "15:00 10/02/2017"
                 EndDate = "15:00 10/03/2017"
                 Grade = "B"} }]
       

/// Get the file name used to store the data for a specific user
let getJSONModulesFileName = "./temp/db/modules.json"

let getJSONStudentFileName userName = sprintf "./temp/db/students/%s.json" userName

let getJSONAssignmentFileName moduleId = sprintf "./temp/db/modules/%s/assignments.json" moduleId

let filteredModules (student:Student) (modules:ModuleTable) =
    let filtered:ModuleTable = 
        List.filter 
            (fun x -> 
                List.contains x.ID student.ModulesID )  modules
    filtered

/// Query the database
let getModulesFromDB userName =
    let fi = FileInfo(getJSONModulesFileName)
    if not fi.Exists then
        filteredModules (defaultStudent userName) defaultModules
    else
        File.ReadAllText(fi.FullName)
        |> JsonConvert.DeserializeObject<ModuleTable>

/// Query the database
let getAssignmentsFromDB moduleId =
    let fi = FileInfo(getJSONModulesFileName)
    if not fi.Exists then
        defaultAssignments moduleId
    else
        File.ReadAllText(fi.FullName)
        |> JsonConvert.DeserializeObject<AssignmentTable>

/// Save to the database
let saveModulesToDB (modules:ModuleTable) =
    try
        let fi = FileInfo(getJSONModulesFileName)
        if not fi.Directory.Exists then
            fi.Directory.Create()
        File.WriteAllText(fi.FullName,JsonConvert.SerializeObject modules)
    with exn ->
        logger.error (eventX "Save failed with exception" >> addExn exn)

/// Handle the GET on /api/modules
let getModules (ctx: HttpContext) =
    Auth.useToken ctx (fun token -> async {
        try
            let modules = getModulesFromDB token.UserName
            return! Successful.OK (JsonConvert.SerializeObject modules) ctx
        with exn ->
            logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
            return! SERVICE_UNAVAILABLE "Database not available" ctx
    })


///?ModuleId=
let useModuleId (ctx: HttpContext) =
    match ctx.request.queryParam "ModuleId" with
    | Choice1Of2 id -> id
    | _ -> ""

/// Handle the GET on /api/module
let getModule (ctx: HttpContext) =
    Auth.useToken ctx (fun token -> async {
        try
            let modules = getModulesFromDB token.UserName
            let _module = List.tryFind (fun (x:ModuleRow) -> x.ID = (useModuleId ctx)) modules
            if _module.IsSome then
                return! Successful.OK (JsonConvert.SerializeObject _module.Value) ctx
            else
                return! INTERNAL_ERROR "module id not found in Modules Table" ctx
        with exn ->
            logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
            return! SERVICE_UNAVAILABLE "Database not available" ctx
    })

/// Handle the GET on /api/module/assignments

let getModuleAssignments (ctx: HttpContext) =
    Auth.useToken ctx (fun token -> async {
        try
            let assignments = getAssignmentsFromDB (useModuleId ctx)//token.UserName
            if assignments.IsEmpty then
                return! INTERNAL_ERROR "assignment id not found in assignemtns Table" ctx
            else
                return! Successful.OK (JsonConvert.SerializeObject assignments) ctx
        with exn ->
            logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
            return! SERVICE_UNAVAILABLE "Database not available" ctx
    })

/// Handle the POST on /api/modules
let postModules (ctx: HttpContext) =
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
                saveModulesToDB modules
                return! Successful.OK (JsonConvert.SerializeObject modules) ctx
            else
                return! BAD_REQUEST "Modules is not valid" ctx
        with exn ->
            logger.error (eventX "Database not available" >> addExn exn)
            return! SERVICE_UNAVAILABLE "Database not available" ctx
    })    