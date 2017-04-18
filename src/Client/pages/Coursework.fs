﻿module Client.Coursework

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
    CourseworkFile : Browser.File option
    TBFile : Browser.File option
    TBForm : Browser.HTMLFormElement option
    User : UserData
    Assignment : Assignment
    Coursework : StudentCoursework
    ActiveTab : CourseworkTab
    ErrorMsg : string }


let getCoursework (model:Model) =
    promise {        
        let url = "/api/coursework/?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<StudentCoursework> url props
    }

let postCoursework (model:Model) =
    promise {        
        let url = "/api/upload/coursework/?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName
        let body = model.CourseworkFile.Value
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

let postTB (model:Model) =
    promise {        
        let url = "/api/upload/testbench/?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName
        let body = model.TBFile.Value
        //let body = model.TBForm.Value
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token)
                //HttpRequestHeaders.ContentType "multipart/form-data" ]
                HttpRequestHeaders.ContentType "application/zip" ]
                //HttpRequestHeaders.ContentType "form-data" ]
              RequestProperties.Body ( unbox body ) ]

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

let postTBCmd model = 
    Cmd.ofPromise postTB model UploadSuccess UploadError




let init (assignment:AssignmentRow) (user:UserData) = 
    let model = { AssignmentID = assignment.ID
                  State = Default
                  CourseworkFile = None
                  TBFile = None
                  TBForm = None
                  User = user
                  Assignment = assignment.Data
                  Coursework = StudentCoursework.New assignment.ID
                  ActiveTab = CourseworkTab.Initial
                  ErrorMsg = "" }
    model , getCourseworkCmd model



let update (msg:CourseworkMsg) model : Model*Cmd<CourseworkMsg> = 
    match msg with
    | CourseworkMsg.SetActiveTab x -> { model with ActiveTab = x }, []
    | CourseworkMsg.SetCourseworkFile f -> 
        { model with CourseworkFile = if f.length > 0.0 then Some f.[0] else None }, Cmd.none//{ model with ActiveTab = x }, []
    | CourseworkMsg.SetTBFile f -> 
        { model with TBFile = if f.length > 0.0 then Some f.[0] else None }, Cmd.none//{ model with ActiveTab = x }, []
    | CourseworkMsg.SetTBForm f -> 
        Browser.console.log(f.length)
        { model with TBForm = Some f }, Cmd.none//{ model with ActiveTab = x }, []
    | CourseworkMsg.ClickUploadCoursework -> 
        if model.CourseworkFile.IsSome then
           { model with State = StartUpload }, postCourseworkCmd(model)
        else
           { model with State = Upload ( Failure "please choose a file before pressing upload" ) } , Cmd.none
    | CourseworkMsg.ClickUploadTB -> 
        if model.TBForm.IsSome then
           { model with State = StartUpload }, postTBCmd(model)
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


let checkType a = Browser.console.log (a.GetType())

let isForm (a:Browser.HTMLFormElement) = 
    Browser.console.log (a)
    Browser.console.log (a.GetType())
    Browser.console.log (a.ToString())
    Browser.console.log (a.getAttribute("files"))
    //Browser.console.log (a.item("files",0))
    
    

let stateView (model:Model) =
    match model.State with
    | Default -> loading "Please wait while we get your files"
    | NoCoursework -> div [] [text ( "Please upload your file")]
    | StartUpload -> loading "uploading"
    | Upload (Success _) -> loading "compiling"
    | Compile (Success _) -> loading "running"
    | Run (Success _) -> div [] [text ( "run successful") ]
    | Upload (Failure msg) -> div [] [text ( "Upload failed: "+msg) ]
    | Compile (Failure msg) -> div [] [text ( "Compile failed: "+msg) ]
    | Run (Failure msg) -> div [] [text ( "Run failed: "+msg) ]

let view (model:Model) (dispatch: AppMsg -> unit) =
    let tabButton text typ show=
        button
            [ ClassName <| "tablinks " + (if typ=model.ActiveTab then "active " else " ") + (if show then "" else "hide")
            ; OnClick (fun _ -> dispatch <| CourseworkMsg (CourseworkMsg.SetActiveTab typ)) ] [ str text ]

    div [] [
        //bcak button
        div [] [ button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (OpenModuleWithID (model.Assignment.ModuleID,ModuleTab.Assignments))) ] [ text "back" ]]
        //Title
        h4 [] [text ( model.Assignment.ModuleID + " - " + model.AssignmentID + " - " + model.Assignment.Title )]
        //tab buttons
        div [ ClassName "tab" ] [
            tabButton "Initial" CourseworkTab.Initial true
            tabButton "Specifications" CourseworkTab.Specifications true
            tabButton "Feedback" CourseworkTab.Feedback true
            tabButton "OriginalCode" CourseworkTab.OriginalCode true
            tabButton "ModifiedCode" CourseworkTab.ModifiedCode (model.User.UserType = "Teacher")
            tabButton "CmdOutput" CourseworkTab.CmdOutput true
            tabButton "TestBench" CourseworkTab.TestBench (model.User.UserType = "Teacher")
            tabButton "Students" CourseworkTab.Students (model.User.UserType = "Teacher")
            tabButton "ModelAnswer" CourseworkTab.ModelAnswer (model.User.UserType = "Teacher") ]
        //tab contents

        //initial tab
        tabcontent (model.ActiveTab = CourseworkTab.Initial) [
                    
            //left side (spec)   
            div [  Style [Float "left"; Padding "10px"; Height "720px"; CSSProp.Width "580px"; Border "1px solid #ccc"; Overflow "auto"]  ] 
                [ div [] [words 25 "Spec"]
                  text "Write a function square x, which returns x^2."]
            
            //right side (upload, feedback, cmdout) 
            div [ Style [Float "right"; Padding "10px";  Height "720px"; CSSProp.Width "580px"; Border "1px solid #ccc"; Overflow "auto"] ] [  //[ ClassName "input-group input-group-lg" ]
                    
                    //upload    
                    div [ Style [ Padding "10px"; Height "130px"; CSSProp.Width "550px"; Border "1px solid #ccc"; Overflow "auto"] ] [
                        div [] [words 25 "Upload coursework"]
                        div [  Style [Margin "5px 0";Float "left"] ] [ 
                            input [
                                HTMLAttr.Type "file"
                                OnChange (fun ev -> dispatch (CourseworkMsg (SetCourseworkFile  !!ev.target?files)))
                                AutoFocus true ] [] ]
                        div [  Style [Margin "0 10px"; Float "left"] ] [ //ClassName "text-center"
                            button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (CourseworkMsg ClickUploadCoursework)) ] [ text "Upload" ] ]
                        div [] [text ( "name: " + (if model.CourseworkFile.IsSome then model.CourseworkFile.Value.name else "empty") ) ]
                        div [] [text ( "type: " + (if model.CourseworkFile.IsSome then model.CourseworkFile.Value.``type`` else "empty") ) ]
                        div [ClassName "text-center"; Style [Margin "5px 0";CSSProp.WhiteSpace "pre-line"] ] [stateView model] ] 
                    
                    //feedback or error
                    div [  Style [ CSSProp.WhiteSpace "pre-line"; Margin "10px 0 0 0"; Padding "10px"; Height "270px"; CSSProp.Width "550px"; Border "1px solid #ccc"; Overflow "auto"]  ] 
                        [ div [] [words 25 "Feedback"]
                          text "As you can see, \n once there's enough text in this box,\n the box will grow scroll bars... that's why we call it a scroll box! You could also place an image into the scroll box."]
                    
                    //cmdout
                    div [  Style [ CSSProp.WhiteSpace "pre-line"; Margin "10px 0 0 0"; Padding "10px"; Height "270px"; CSSProp.Width "550px"; Border "1px solid #ccc"; Overflow "auto"]  ] 
                        [ div [] [words 25 "CmdOut"]
                          text model.Coursework.CmdOut ]
                ]
            ]
        //Testbench tab
        tabcontent (model.ActiveTab = CourseworkTab.TestBench) [
            //upload    
            div [ Style [ Padding "10px"; Height "130px"; CSSProp.Width "550px"; Border "1px solid #ccc"; Overflow "auto"] ] [
                div [] [words 25 "Upload testbench"]
                div [  Style [Margin "5px 0";Float "left"] ] [ 
                    input [
                        HTMLAttr.Type "file"
                        OnChange (fun ev -> dispatch (CourseworkMsg (SetTBFile  !!ev.target?files)))
                        AutoFocus true ] [] ]
                div [  Style [Margin "0 10px"; Float "left"] ] [ //ClassName "text-center"
                    button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (CourseworkMsg ClickUploadTB)) ] [ text "Upload" ] ]
                div [] [text ( "name: " + (if model.TBFile.IsSome then model.TBFile.Value.name else "empty") ) ]
                div [] [text ( "type: " + (if model.TBFile.IsSome then model.TBFile.Value.``type`` else "empty") ) ]
                div [ClassName "text-center"; Style [Margin "5px 0";CSSProp.WhiteSpace "pre-line"] ] [stateView model] ]
            //upload
            //HTMLAttr.Action ("/api/upload/testbench/?ModuleId="+model.Assignment.ModuleID+
            //                        "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName); HTMLAttr.FormEncType "multipart/form-data"
            form [ HTMLAttr.EncType "multipart/form-data";
                                    HTMLAttr.Method "post";
                                    DOMAttr.OnChange (fun ev -> dispatch (CourseworkMsg (SetTBForm  !!ev.target?form))) ] [
                //input [ HTMLAttr.Type "text"; HTMLAttr.Name "submit-name"] []
                //input [ HTMLAttr.Type "file"; HTMLAttr.Name "files"; DOMAttr.OnChange (fun ev -> dispatch (CourseworkMsg (SetTBForm  !!ev.target?form))) ] []
                input [ HTMLAttr.Type "file"; HTMLAttr.Name "files"; DOMAttr.OnChange (fun ev -> isForm !!ev.target?form) ] [] ]
                //input [ HTMLAttr.Type "file"; HTMLAttr.Name "files"; DOMAttr.OnChange (fun ev -> checkType !!ev.target?files) ] [] ]
        ]
    ]
    