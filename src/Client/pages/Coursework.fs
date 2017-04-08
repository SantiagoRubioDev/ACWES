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

type UploadState =
| Success of string
| Fail
| Default

type Model = { 
    AssignmentID : ID
    State : UploadState
    File : Browser.File option
    User : UserData
    Coursework : Assignment
    ErrorMsg : string }

let postCoursework (model:Model) =
    promise {        
        let url = "/api/coursework/upload?ModuleId="+model.Coursework.ModuleID+
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
                return (data)
        with
        | _ -> return! failwithf "Could not upload file."
        
    }

let postCourseworkCmd model = 
    Cmd.ofPromise postCoursework model FetchedCoursework FetchCourseworkError

let init (assignment:AssignmentRow) (user:UserData) = 
    { AssignmentID = assignment.ID
      State = Default
      File = None
      User = user
      Coursework = assignment.Data
      ErrorMsg = "" }, Cmd.none//, loadWishListCmd user.Token

let update (msg:CourseworkMsg) model : Model*Cmd<CourseworkMsg> = 
    match msg with
    | CourseworkMsg.SetUploadFile f -> 
        { model with File = if f.length > 0.0 then Some f.[0] else None }, Cmd.none//{ model with ActiveTab = x }, []
    | CourseworkMsg.ClickUpload -> 
        if model.File.IsSome then
            model , postCourseworkCmd(model)
        else
            model , Cmd.none
    | CourseworkMsg.FetchedCoursework response -> 
        { model with State = Success response}, Cmd.none
    | CourseworkMsg.FetchCourseworkError _ -> 
        model, Cmd.none

let succ =
    function
    | Success r -> r
    | _ -> ""

let view (model:Model) (dispatch: AppMsg -> unit) = 
    div [] [
        div [] [ button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (OpenModuleWithID (model.Coursework.ModuleID,Tab.Assignments))) ] [ text "back" ]]
        h4 [] [text ( model.Coursework.ModuleID + " - " + model.Coursework.Title )]
        div [] [  //[ ClassName "input-group input-group-lg" ]
                    input [ 
                        HTMLAttr.Type "file"
                        OnChange (fun ev -> dispatch (CourseworkMsg (SetUploadFile  !!ev.target?files)))
                        AutoFocus true ] [] ]
        div [ ClassName "text-center" ] [
                  button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (CourseworkMsg ClickUpload)) ] [ text "Upload" ]
              ]
        div [] [text ( "name: " + (if model.File.IsSome then model.File.Value.name else "empty") ) ]
        div [] [text ( "type: " + (if model.File.IsSome then model.File.Value.``type`` else "empty") ) ]
        div [] [text ( "success: " + (succ model.State) ) ] ]
    