module Client.Coursework

open Fable.Core
open Fable.Import
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open ServerCode.Domain
open Style
open Messages
open System
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types

type StateFS =
    | Success of string
    | Failure of string

type CourseworkState =
    | Default
    | NoCoursework
    | StartUpload
    | Upload of StateFS
    | Compile of StateFS
    | Run of StateFS

type Model = { 
    AssignmentID : ID
    State : CourseworkState
    File : Browser.File option
    User : UserData
    Assignment : Assignment
    Coursework : StudentCoursework
    ErrorMsg : string }


let getCoursework (model:Model) =
    promise {        
        let url = "/api/coursework?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<StudentCoursework> url props
    }

let postCoursework (model:Model) =
    promise {        
        let url = "/api/upload?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName
        let body = model.File.Value
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token)
                HttpRequestHeaders.ContentType "text/plain" ]
                //HttpRequestHeaders.ContentType "multipart/form-data" ]
              RequestProperties.Body (unbox body) ]

        try
            let! response = Fetch.fetch url props

            if not response.Ok then
                return! failwithf "Error: %d" response.Status
            else    
                let! data = response.text() 
                return data
        with
        | _ -> return! failwithf "Could not upload file."
        
    }

let postCourseworkCmd model = 
    Cmd.ofPromise postCoursework model UploadSuccess UploadError

let getCourseworkCmd model = 
    Cmd.ofPromise getCoursework model FetchedCoursework FetchCourseworkError

let init (assignment:AssignmentRow) (user:UserData) = 
    let model = { AssignmentID = assignment.ID
                  State = Default
                  File = None
                  User = user
                  Assignment = assignment.Data
                  Coursework = StudentCoursework.New assignment.ID
                  ErrorMsg = "" }
    model , getCourseworkCmd model

let update (msg:CourseworkMsg) model : Model*Cmd<CourseworkMsg> = 
    match msg with
    | CourseworkMsg.SetUploadFile f -> 
        { model with File = if f.length > 0.0 then Some f.[0] else None }, Cmd.none//{ model with ActiveTab = x }, []
    | CourseworkMsg.ClickUpload -> 
        if model.File.IsSome then
           { model with State = StartUpload }, postCourseworkCmd(model)
        else
           { model with State = Upload ( Failure "please choose a file before pressing upload" ) } , Cmd.none
    | CourseworkMsg.UploadSuccess res -> 
        { model with State = Upload ( Success res ) } , getCourseworkCmd model
    | CourseworkMsg.UploadError error ->
        { model with State = Upload ( Failure error.Message ) }, Cmd.none
    | CourseworkMsg.FetchedCoursework res ->
        let (|Prefix|_|) (p:string) (s:string) =
            if s.StartsWith(p) then
                Some(s.Substring(p.Length))
            else
                None
        let state, cmdNext = 
            match res.State with
            | Prefix "Upload Success: " msg -> Upload (Success msg), Cmd.none
            | Prefix "Compile Success: " msg -> Compile (Success msg), Cmd.none
            | Prefix "Run Success: " msg -> Run (Success msg), Cmd.none
            | Prefix "Upload Fail: " msg -> Upload (Failure msg), Cmd.none
            | Prefix "Compile Fail: " msg -> Compile (Failure msg), Cmd.none
            | Prefix "Run Fail: " msg -> Run (Failure msg), Cmd.none
            | _ -> NoCoursework, Cmd.none
        { model with Coursework = res ; State = state }, cmdNext
    | CourseworkMsg.FetchCourseworkError error -> 
        { model with State = NoCoursework }, Cmd.none

let stateView (model:Model) =
    match model.State with
    | Default -> loading "Please wait while we get your files"
    | NoCoursework -> div [] [text ( "Please upload your file")]
    | StartUpload -> loading "uploading"
    | Upload (Success _) -> loading "compiling"
    | Compile (Success _) -> loading "running"
    | Run (Success _) -> div [] [text ( "Command Line Output: " + ( model.Coursework.CmdOut ) ) ]
    | Upload (Failure msg) -> div [] [text ( "Upload failed: "+msg) ]
    | Compile (Failure msg) -> div [] [text ( "Compile failed: "+msg) ]
    | Run (Failure msg) -> div [] [text ( "Run failed: "+msg) ]

let view (model:Model) (dispatch: AppMsg -> unit) = 
    div [] [
        div [] [ button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (OpenModuleWithID (model.Assignment.ModuleID,Tab.Assignments))) ] [ text "back" ]]
        h4 [] [text ( model.Assignment.ModuleID + " - " + model.Assignment.Title )]
        div [] [  //[ ClassName "input-group input-group-lg" ]
                input [ 
                    HTMLAttr.Type "file"
                    OnChange (fun ev -> dispatch (CourseworkMsg (SetUploadFile  !!ev.target?files)))
                    AutoFocus true ] [] ]
        div [ ClassName "text-center" ] [
                  button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (CourseworkMsg ClickUpload)) ] [ text "Upload" ] ]
        div [] [text ( "name: " + (if model.File.IsSome then model.File.Value.name else "empty") ) ]
        div [] [text ( "type: " + (if model.File.IsSome then model.File.Value.``type`` else "empty") ) ]

        stateView model ]
    