module ServerCode.DBFile

open System.IO
open Newtonsoft.Json
open ServerCode.Domain
open Suave.Logging
open Suave.Logging.Message

let logger = Log.create "FableSample"      


//Do I keep this here??
let filteredModules (student:Student) (modules:ModuleTable) =
    let filtered:ModuleTable = 
        List.filter 
            (fun x -> 
                List.contains x.ID student.ModulesID )  modules
    filtered


//get path for upload download and others
module Path =
    let uploadDir ctx = "temp/db/modules/"+(Query.useModuleId ctx)+"/assignments/"+
                                     (Query.useAssignmentId ctx)+"/students/"+(Query.useUserName ctx)+"/"
//get file name path
module JSONFileName =

    let modules = "./temp/db/modules.json"

    let student userName = sprintf "./temp/db/students/%s.json" userName

    let assignment moduleId = sprintf "./temp/db/modules/%s/assignments.json" moduleId

    let users = "./temp/db/users.json"

/// Query the database for Modules
module Modules =

    let read userName =
        let fi = FileInfo(JSONFileName.modules)
        if not fi.Exists then
            filteredModules (DBDefault.student userName) DBDefault.modules
        else
            File.ReadAllText(fi.FullName)
            |> JsonConvert.DeserializeObject<ModuleTable>

    let write (modules:ModuleTable) =
        try
            let fi = FileInfo(JSONFileName.modules)
            if not fi.Directory.Exists then
                fi.Directory.Create()
            File.WriteAllText(fi.FullName,JsonConvert.SerializeObject modules)
        with exn ->
            logger.error (eventX "Save failed with exception" >> addExn exn)

/// Query the database
module Assignments =
    let read moduleId =
        let fi = FileInfo(JSONFileName.assignment moduleId)
        if not fi.Exists then
            DBDefault.assignments moduleId
        else
            File.ReadAllText(fi.FullName)
            |> JsonConvert.DeserializeObject<AssignmentTable>

module Users =
    let read =
        let fi = FileInfo(JSONFileName.users)
        if not fi.Exists then
            DBDefault.userList
        else
            File.ReadAllText(fi.FullName)
            |> JsonConvert.DeserializeObject<UserTable>


module Upload =
    let write fileText ctx=
        try
            
            let filename = "attempt1"

            let filePath = "./"+(Path.uploadDir ctx)+filename

            let fi = FileInfo(filePath)

            if not fi.Directory.Exists then
                fi.Directory.Create()
            File.WriteAllText(filePath+".fs",fileText)

            File.WriteAllText(filePath+".cmd","@echo off \n"+
                "cls \n \n"+
                @"..\..\..\..\..\..\..\..\..\..\packages\FSharp.Compiler.Tools\Tools\fsc.exe "+
                filename+".fs"+" --standalone -o "+filename+".exe \n"+
                filename)

        with exn ->
            logger.error (eventX "Save failed with exception" >> addExn exn)

