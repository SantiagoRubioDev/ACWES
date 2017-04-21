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



/////---------------------------------------------------------------------TYPES---------------------------------------------------------------------/////

type StateFS =
    | Success of string
    | Failure of string

type CourseworkUploadState =
    | Default
    | NotFound of string
    | StartUpload
    | Read of StateFS
    | Upload of StateFS
    | Compile of StateFS
    | Run of StateFS

type TBUploadState =
    | Default
    | NotFound of string
    | StartUpload
    | Read of StateFS
    | Upload of StateFS

type FileUploadType =
    { File : Browser.File
      Reader : Browser.FileReader }
    static member addFile file =
        let x = { File = file
                  Reader = Browser.FileReader.Create() }
        //x.Reader.readAsText(x.File) now only do it after pressing upload button
        x

type CourseworkType = 
    { State : CourseworkUploadState
      Files : FileUploadType list option
      Coursework : StudentCoursework }
    static member New assignmentId = 
        { State = CourseworkUploadState.Default
          Files = None 
          Coursework = { AssignmentID = assignmentId
                         State = ""
                         CmdOut = ""
                         Feedback = ""
                         Grade = "" } }
    member this.withNewState newState = 
        { State = newState
          Files = this.Files 
          Coursework = this.Coursework}
    member this.withNewStateandCoursework newState newCoursework = 
        { State = newState
          Files = this.Files 
          Coursework = newCoursework}

type TBType = 
    { State : TBUploadState
      Files : FileUploadType list option
      TB : TeacherCoursework }
    static member New assignmentId = 
        { State = Default
          Files = None 
          TB = { AssignmentID = assignmentId
                 State = ""
                 TBtext = ""
                 ModelAnswertext = ""
                 SampleCodetext = ""  } }
    member this.withNewState newState = 
        { State = newState
          Files = this.Files
          TB = this.TB }
    member this.withNewStateandTB newState newTB = 
        { State = newState
          Files = this.Files 
          TB = newTB}

/////---------------------------------------------------------------------MODEL---------------------------------------------------------------------/////

type Model = { 
    AssignmentID : ID
    Coursework : CourseworkType
    TB : TBType
    User : UserData
    Assignment : Assignment
    ActiveTab : CourseworkTab
    ErrorMsg : string }

/////---------------------------------------------------------------------GET/POST/ASYNC---------------------------------------------------------------------/////


///---------Coursework 


let getCoursework (model:Model) =
    promise {        
        
        let cState = 
            match model.Coursework.State with
            | CourseworkUploadState.Default -> "Default"
            | CourseworkUploadState.NotFound _ -> "NotFound"
            | CourseworkUploadState.StartUpload -> "StartUpload"
            | CourseworkUploadState.Read _ -> "Read"
            | CourseworkUploadState.Upload _ -> "Upload"
            | CourseworkUploadState.Compile _ -> "Compile"
            | CourseworkUploadState.Run _ -> "Run"

        let url = "/api/coursework/student/?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName+
                  "&State="+cState
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<StudentCoursework> url props
    }

let postCoursework (model:Model) =
    promise {        
        let url = "/api/upload/coursework/?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName
        let body = toJson (model.Coursework.Files.Value.Head.Reader.result.ToString())
        let props =
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token)
                //HttpRequestHeaders.ContentType "text/plain" ]
                HttpRequestHeaders.ContentType "application/json" ]
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

let readCoursework (model:Model) =
    async {        
        try
            model.Coursework.Files.Value.Head.Reader.readAsText(model.Coursework.Files.Value.Head.File)
            let mutable count = 0

            while model.Coursework.Files.Value.Head.Reader.readyState <> model.Coursework.Files.Value.Head.Reader.DONE && count < 1000 do
                count <- count + 2
                do! Async.Sleep(2)

            Browser.console.log("counter is: "+count.ToString())
            if model.Coursework.Files.Value.Head.Reader.readyState = model.Coursework.Files.Value.Head.Reader.DONE then
                return "read successfully"
            else   
                return! failwith "Failed read: it took to long your file is too big"

        with
        | e -> return! failwithf "Could not read file: %s" e.Message
        
    }

let readCourseworkCmd model = 
    Cmd.ofAsync readCoursework model ReadCourseworkSuccess ReadCourseworkError

let postCourseworkCmd model = 
    Cmd.ofPromise postCoursework model UploadCourseworkSuccess UploadCourseworkError

let getCourseworkCmd model = 
    Cmd.ofPromise getCoursework model FetchedCoursework FetchCourseworkError



///---------Test Bench

let getTB (model:Model) =
    promise {        
        
        let cState = 
            match model.Coursework.State with
            | CourseworkUploadState.Default -> "Default"
            | CourseworkUploadState.NotFound _ -> "NotFound"
            | CourseworkUploadState.StartUpload -> "StartUpload"
            | CourseworkUploadState.Read _ -> "Read"
            | CourseworkUploadState.Upload _ -> "Upload"
            | CourseworkUploadState.Compile _ -> "Compile"
            | CourseworkUploadState.Run _ -> "Run"

        let url = "/api/coursework/teacher/?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName+
                  "&State="+cState
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<StudentCoursework> url props
    }

let postTB (fiName,(model:Model)) =
    promise {        
        let url = "/api/upload/testbench/?ModuleId="+model.Assignment.ModuleID+
                  "&AssignmentId="+model.AssignmentID+"&UserName="+model.User.UserName+
                  "&FileName="+fiName

        Browser.console.log("reader state: "+(model.TB.Files.Value.[0].Reader.readyState.ToString()))

        let body = //toJson (model.TBUpload.Files.Value.Head.Reader.result.ToString())
            if fiName = model.TB.Files.Value.[0].File.name
            then toJson (model.TB.Files.Value.[0].Reader.result.ToString())
            elif fiName = model.TB.Files.Value.[1].File.name
            then toJson (model.TB.Files.Value.[1].Reader.result.ToString())
            elif fiName = model.TB.Files.Value.[2].File.name
            then toJson (model.TB.Files.Value.[2].Reader.result.ToString())
            else 
                Browser.console.log("ERROR in postTB fiName: "+fiName)
                toJson Browser.File.prototype //should never happen

        let props = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token)
                //HttpRequestHeaders.ContentType "multipart/form-data" ]
                HttpRequestHeaders.ContentType "application/json" ]
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

let readTB (model:Model) =
    async {
        
        Browser.console.log("tbfilse length: "+(model.TB.Files.Value.Length.ToString()))        
        try
            
            model.TB.Files.Value.[0].Reader.readAsText(model.TB.Files.Value.[0].File) //testbench
            model.TB.Files.Value.[1].Reader.readAsText(model.TB.Files.Value.[1].File) //modelanswer
            model.TB.Files.Value.[2].Reader.readAsText(model.TB.Files.Value.[2].File) //sudentanswer

            let mutable count = 0

            while ( model.TB.Files.Value.[0].Reader.readyState <> model.TB.Files.Value.[0].Reader.DONE
                  || model.TB.Files.Value.[1].Reader.readyState <> model.TB.Files.Value.[1].Reader.DONE
                  || model.TB.Files.Value.[2].Reader.readyState <> model.TB.Files.Value.[2].Reader.DONE )
                  && count < 1000 do
                count <- count + 2
                do! Async.Sleep(2)

            Browser.console.log("counter is: "+count.ToString())
            if model.TB.Files.Value.[0].Reader.readyState = model.TB.Files.Value.[0].Reader.DONE
               && model.TB.Files.Value.[1].Reader.readyState = model.TB.Files.Value.[1].Reader.DONE
               && model.TB.Files.Value.[2].Reader.readyState = model.TB.Files.Value.[2].Reader.DONE then
                return "read successfully"
            else   
                return! failwith "Failed read: it took to long your file is too big"

        with
        | e -> return! failwithf "Could not read file: %s" e.Message
        
    }

let readTBCmd model = 
    Cmd.ofAsync readTB model ReadTBSuccess ReadTBError

let postTBCmd fiName model = 
    Cmd.ofPromise postTB (fiName,model) UploadTBSuccess UploadTBError

let getTBCmd model = 
    Cmd.ofPromise getTB model FetchedTB FetchTBError




/////---------------------------------------------------------------------INIT---------------------------------------------------------------------/////

let init (assignment:AssignmentRow) (user:UserData) = 
    let model = { AssignmentID = assignment.ID
                  Coursework = CourseworkType.New assignment.ID
                  TB = TBType.New assignment.ID
                  User = user
                  Assignment = assignment.Data
                  ActiveTab = CourseworkTab.Initial
                  ErrorMsg = "" }
    model , Cmd.batch [getCourseworkCmd model; getTBCmd model]

/////---------------------------------------------------------------------Helpers---------------------------------------------------------------------/////
let checkType a = Browser.console.log (a.GetType())

let isForm (a:Browser.HTMLFormElement) = 
    Browser.console.log (a)
    Browser.console.log (a.GetType())
    Browser.console.log (a.ToString())
    Browser.console.log (a.getAttribute("files"))

let (|Prefix|_|) (p:string) (s:string) =
    if s.StartsWith(p) then
        Some(s.Substring(p.Length))
    else
        None


/////---------------------------------------------------------------------UPDATE---------------------------------------------------------------------/////

let update (msg:CourseworkMsg) model : Model*Cmd<CourseworkMsg> = 
    match msg with
    | CourseworkMsg.SetActiveTab x -> { model with ActiveTab = x }, []

    //-----------Coursework
    | CourseworkMsg.SetCourseworkFile f -> 
        if f.length = 1.0 then
            { model with Coursework = {model.Coursework with Files = Some [FileUploadType.addFile f.[0]] } }, Cmd.none
        else
            { model with Coursework = model.Coursework.withNewState (CourseworkUploadState.Upload ( Failure "please choose only one file" ) ) } , Cmd.none
    | CourseworkMsg.ClickUploadCoursework -> 
        //not up to date (check for filename extension .fs)
        if model.Coursework.Files.IsSome then
            if model.Coursework.Files.Value.Length = 1 then
                { model with Coursework = model.Coursework.withNewState CourseworkUploadState.StartUpload },  readCourseworkCmd model
            else
                { model with Coursework = model.Coursework.withNewState (CourseworkUploadState.Upload ( Failure "please choose only one file" ) ) } , Cmd.none
        else
           { model with Coursework = model.Coursework.withNewState ( CourseworkUploadState.Upload ( Failure "please choose one file before pressing upload" ) ) } , Cmd.none
    | CourseworkMsg.ReadCourseworkSuccess msg ->
        //not up to date (everything)
        { model with Coursework = model.Coursework.withNewState (CourseworkUploadState.Read (Success msg ) ) }, postCourseworkCmd(model)
    | CourseworkMsg.ReadCourseworkError error ->
        { model with Coursework = model.Coursework.withNewState ( CourseworkUploadState.Read ( Failure error.Message ) ) }, Cmd.none
    | CourseworkMsg.UploadCourseworkSuccess res -> 
        { model with Coursework = model.Coursework.withNewState ( CourseworkUploadState.Upload ( Success res ) ) } , getCourseworkCmd model
    | CourseworkMsg.UploadCourseworkError error ->
        { model with Coursework = model.Coursework.withNewState ( CourseworkUploadState.Upload ( Failure error.Message ) ) }, Cmd.none
    //Fetch Coursework
    | CourseworkMsg.FetchedCoursework res ->
        let state, cmdNext = 
            match res.State with
            | Prefix "Upload Success: " msg -> CourseworkUploadState.Upload (Success msg), getCourseworkCmd model   //check for compilation state
            | Prefix "Compile Success: " msg -> Compile (Success msg), getCourseworkCmd model //check for run state
            | Prefix "Run Success: " msg -> Run (Success msg), Cmd.none
            | Prefix "Upload Fail: " msg -> CourseworkUploadState.Upload (Failure msg), Cmd.none
            | Prefix "Compile Fail: " msg -> Compile (Failure msg), Cmd.none
            | Prefix "Run Fail: " msg -> Run (Failure msg), Cmd.none
            | x -> CourseworkUploadState.NotFound ("Unexpected FetchCoursework success response: "+x), Cmd.none
        { model with Coursework = model.Coursework.withNewStateandCoursework state res  }, cmdNext
    | CourseworkMsg.FetchCourseworkError error -> 
        let errorMsg = 
            match error.Message with
            | Prefix "500" _ -> "please upload file" //no coursework has been uploaded yet
            | msg -> msg

        { model with Coursework = model.Coursework.withNewState ( CourseworkUploadState.NotFound errorMsg ) }, Cmd.none

    //--------TestBench
    | CourseworkMsg.SetTBFiles f ->
        //not up to date (check for filenames and extensions .fs) 
        if f.length = 3.0 then
            { model with TB = {model.TB with Files = Some [FileUploadType.addFile f.[0];FileUploadType.addFile f.[1];FileUploadType.addFile f.[2]] } }, Cmd.none
        else
            { model with TB = model.TB.withNewState (Upload ( Failure "please choose exactly three files" ) ) } , Cmd.none
    | CourseworkMsg.ClickUploadTB -> 
        //not up to date (check postTBcmd)
        if model.TB.Files.IsSome then
            if model.TB.Files.Value.Length = 3 then
                { model with TB = model.TB.withNewState StartUpload }, readTBCmd model
            else
                { model with TB = model.TB.withNewState ( Upload ( Failure "please choose exactly three file" ) ) } , Cmd.none
        else
           { model with TB = model.TB.withNewState ( Upload ( Failure "please choose your files before pressing upload" ) ) } , Cmd.none
    | CourseworkMsg.ReadTBSuccess msg->
        //not up to date (everything)
        { model with TB = model.TB.withNewState (Read (Success msg ) ) }, Cmd.batch [postTBCmd model.TB.Files.Value.[0].File.name model ; postTBCmd model.TB.Files.Value.[1].File.name model; postTBCmd model.TB.Files.Value.[2].File.name model]
    | CourseworkMsg.ReadTBError error ->
        { model with TB = model.TB.withNewState ( Read ( Failure error.Message ) ) }, Cmd.none
    | CourseworkMsg.UploadTBSuccess res ->
        //not up to date (add getTBCmd)
        { model with TB = model.TB.withNewState ( Upload ( Success res ) ) } , Cmd.none //getTBCmd model
    | CourseworkMsg.UploadTBError error ->
        { model with TB = model.TB.withNewState ( Upload ( Failure error.Message ) ) }, Cmd.none
    | CourseworkMsg.FetchedTB res ->
        let state, cmdNext = 
            match res.State with
            | Prefix "Upload Success: " msg -> TBUploadState.Upload (Success msg), getCourseworkCmd model   //check for compilation state
            | Prefix "Upload Fail: " msg -> TBUploadState.Upload (Failure msg), Cmd.none
            | x -> TBUploadState.NotFound ("Unexpected FetchCoursework success response: "+x), Cmd.none
        { model with TB = model.TB.withNewStateandTB state res  }, cmdNext
    | CourseworkMsg.FetchTBError error -> 
        let errorMsg = 
            match error.Message with
            | Prefix "500" _ -> "please upload file" //no tb has been uploaded yet
            | msg -> msg
        { model with TB = model.TB.withNewState ( TBUploadState.NotFound errorMsg ) }, Cmd.none
    
    
/////---------------------------------------------------------------------StateView---------------------------------------------------------------------/////

let CourseworkstateView (state:CourseworkUploadState) =
    //not up to date (make two different one for tb one for coursework)
    match state with
    | CourseworkUploadState.Default -> loading "Please wait while we get your files"
    | CourseworkUploadState.NotFound msg -> div [] [text msg]
    | CourseworkUploadState.StartUpload -> loading "reading file"
    | CourseworkUploadState.Read (Success _) -> loading "uploading"
    | CourseworkUploadState.Upload (Success _) -> loading "compiling"
    | CourseworkUploadState.Compile (Success _) -> loading "running"
    | CourseworkUploadState.Run (Success _) -> div [] [text ( "run successful") ]
    | CourseworkUploadState.Read (Failure msg) -> div [] [text ( "Read failed: "+msg) ]
    | CourseworkUploadState.Upload (Failure msg) -> div [] [text ( "Upload failed: "+msg) ]
    | CourseworkUploadState.Compile (Failure msg) -> div [] [text ( "Compile failed: "+msg) ]
    | CourseworkUploadState.Run (Failure msg) -> div [] [text ( "Run failed: "+msg) ]

let TBstateView (state:TBUploadState) =
    //not up to date (make two different one for tb one for coursework)
    match state with
    | TBUploadState.Default -> loading "Please wait while we get your files"
    | TBUploadState.NotFound msg -> div [] [text msg]
    | TBUploadState.StartUpload -> loading "reading files"
    | TBUploadState.Read (Success _) -> loading "uploading"
    | TBUploadState.Upload (Success _) -> div [] [text ( "Upload successful") ]
    | TBUploadState.Read (Failure msg) -> div [] [text ( "Read failed: "+msg) ]
    | TBUploadState.Upload (Failure msg) -> div [] [text ( "Upload failed: "+msg) ]




/////---------------------------------------------------------------------VIEW---------------------------------------------------------------------/////

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
                        div [] [text ( "name: " + (if model.Coursework.Files.IsSome then model.Coursework.Files.Value.Head.File.name else "empty") ) ]
                        div [] [text ( "type: " + (if model.Coursework.Files.IsSome then model.Coursework.Files.Value.Head.File.``type`` else "empty") ) ]
                        div [ClassName "text-center"; Style [Margin "5px 0";CSSProp.WhiteSpace "pre-line"] ] [CourseworkstateView model.Coursework.State] ] 
                    
                    //feedback or error
                    div [  Style [ CSSProp.WhiteSpace "pre-line"; Margin "10px 0 0 0"; Padding "10px"; Height "270px"; CSSProp.Width "550px"; Border "1px solid #ccc"; Overflow "auto"]  ] 
                        [ div [] [words 25 "Feedback"]
                          text "As you can see, \n once there's enough text in this box,\n the box will grow scroll bars... that's why we call it a scroll box! You could also place an image into the scroll box."]
                    
                    //cmdout
                    div [  Style [ CSSProp.WhiteSpace "pre-line"; Margin "10px 0 0 0"; Padding "10px"; Height "270px"; CSSProp.Width "550px"; Border "1px solid #ccc"; Overflow "auto"]  ] 
                        [ div [] [words 25 "CmdOut"]
                          text model.Coursework.Coursework.CmdOut ]
                ]
            ]
        //Testbench tab
        tabcontent (model.ActiveTab = CourseworkTab.TestBench) [
            //upload    
            div [ Style [ Padding "10px"; Height "200px"; CSSProp.Width "550px"; Border "1px solid #ccc"; Overflow "auto"] ] [
                form [ Style [Margin "0 10px"; Float "left"] ] [
                    div [] [words 25 "Upload testbench files"]
                    text "please selct three files with these exact names"
                    text "Testbench.fs, ModelAnswers.fs, StudentsAnswers.fs"
                    input [ HTMLAttr.Type "file"; HTMLAttr.Multiple true; DOMAttr.OnChange (fun ev -> dispatch (CourseworkMsg (SetTBFiles  !!ev.target?files ))) ] [] ]
                div [  Style [Margin "0 10px"; Float "right"] ] [
                    button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch (CourseworkMsg ClickUploadTB)) ] [ text "Upload" ] ]
                div [ClassName "text-center"; Style [Margin "5px 0";CSSProp.WhiteSpace "pre-line"] ] [TBstateView model.TB.State] ]
        ]
    ]
    