module Client.Module

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

type Model = { 
    ID : ID
    User : UserData
    StudentName : string option
    Module : Module
    Assignments : AssignmentTable
    NewAssignment : AssignmentRow
    NewAssignmentErrorTxt : string option
    Students : UserTable
    NewStudents : string
    NewStudentsErrorTxt : string option
    ActiveTab : Tab
    ErrorMsg : string }

let getAssignments (id,token) =
    Browser.console.log("getAssignments: Id is: " + id)
    promise {        
        let url = "/api/assignments/?ModuleId="+id
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<AssignmentTable> url props
    }

let loadAssignmentsCmd id token = 
    Cmd.ofPromise getAssignments (id,token) FetchedAssignments FetchModuleError

let getModule (id,token) =
    Browser.console.log("getModule: Id is: " + id)
    promise {        
        let url = "/api/module/?ModuleId="+id
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<ModuleRow> url props
    }

let loadModuleCmd id token = 
    Cmd.ofPromise getModule (id,token) FetchedModule FetchModuleError

let getStudents (id,token) =
    promise {        
        let url = "/api/users/students/?ModuleId="+id
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<UserTable> url props
    }

let loadStudentsCmd id token = 
    Cmd.ofPromise getStudents (id,token) FetchedStudents FetchModuleError

let postNewStudents (model:Model) =
    promise {        
        let url = "/api/user/assignmodule/?ModuleId="+model.ID
        let body = toJson model.NewStudents
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token)
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body (unbox body) ]

        return! Fable.PowerPack.Fetch.fetchAs<UserTable> url props
    }

let postNewStudentsCmd model = 
    Cmd.ofPromise postNewStudents model FetchedStudents FetchModuleError


let postNewAssignment (model:Model) =
    promise {        
        let url = "/api/assignments/"
        let body = toJson model.NewAssignment
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + model.User.Token)
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body (unbox body) ]

        return! Fable.PowerPack.Fetch.fetchAs<AssignmentTable> url props
    }

let postNewAssignmentCmd model = 
    Cmd.ofPromise postNewAssignment model FetchedAssignments FetchModuleError





let init (init:InitModule) (user:UserData) =
    match init with
    | ID (moduleid,tab) ->
        { ID = moduleid
          User = user
          StudentName = None
          Module = Module.New
          Assignments = []
          NewAssignment = { AssignmentRow.New with Data = { AssignmentRow.New.Data with ModuleID = moduleid } }
          NewAssignmentErrorTxt = None
          Students = []
          NewStudents = ""
          NewStudentsErrorTxt = None
          ActiveTab = tab
          ErrorMsg = "" }, Cmd.batch [loadModuleCmd moduleid user.Token; loadAssignmentsCmd moduleid user.Token; loadStudentsCmd moduleid user.Token] //Cmd.none //, loadWishListCmd user.Token
    | Row _module ->
        { ID = _module.ID
          User = user
          StudentName = None
          Module = _module.Data //Module.New
          Assignments = []
          NewAssignment = { AssignmentRow.New with Data = { AssignmentRow.New.Data with ModuleID = _module.ID } }
          NewAssignmentErrorTxt = None 
          Students = []
          NewStudents = ""
          NewStudentsErrorTxt = None
          ActiveTab = ModuleInfo
          ErrorMsg = "" }, Cmd.batch [loadAssignmentsCmd _module.ID user.Token; loadStudentsCmd _module.ID user.Token] //Cmd.none //, loadWishListCmd user.Token
    

let update (msg:ModuleMsg) model : Model*Cmd<ModuleMsg> = 
    match msg with
    | ModuleMsg.SetActiveTab x -> { model with ActiveTab = x }, []
    | FetchedModule _module ->
        Browser.console.log("gotModule: " + _module.Data.Title)
        { model with ID = _module.ID; Module = _module.Data }, Cmd.none
    | FetchedAssignments assignments ->
        Browser.console.log("gotAssignment: " + assignments.Head.Data.Title)
        { model with Assignments = assignments }, Cmd.none
    | AddAssignment ->
        if AssignmentValidation.verifyAssignment model.NewAssignment then
            model , postNewAssignmentCmd(model)
        else
            { model with 
                NewAssignmentErrorTxt = AssignmentValidation.verifyAssignmentID model.NewAssignment.ID }, Cmd.none
    | NewAssignmentChanged assignment ->
        { model with NewAssignment = assignment ; NewAssignmentErrorTxt = AssignmentValidation.verifyAssignmentID model.NewAssignment.ID }, Cmd.none
    | FetchedStudents students ->
        Browser.console.log("gotStudents: " + students.Head.Data.UserName)
        { model with Students = students }, Cmd.none
    | AddStudents ->
        if ModuleValidation.verifyStudents model.NewStudents then
            { model with NewStudents = "" }, postNewStudentsCmd(model)
        else
            { model with 
                NewStudentsErrorTxt = ModuleValidation.verifyStudentsNotEmpty model.NewStudents }, Cmd.none
    | NewStudentChanged students ->
        Browser.console.log("newStudents: " + students)
        { model with NewStudents = students ; NewStudentsErrorTxt = ModuleValidation.verifyStudentsNotEmpty students }, Cmd.none
    | FetchModuleError _ -> 
        model, Cmd.none






let newStudentForm (model:Model) dispatch =
    let buttonActive = if String.IsNullOrEmpty model.NewStudents then "btn-disabled" else "btn-primary"
    
    let studentsStatus = if String.IsNullOrEmpty model.NewStudents then "" else "has-success"

    div [] [
        h4 [] [text "Register Students to module"]

        div [ClassName "container"] [
            div [ClassName "row"] [
                div [ClassName "col-md-8"] [
                    form_group 
                        "Usernames (seperate usernames with commas (i.e spr13,tyn14 ) )" 
                        studentsStatus 
                        model.NewStudents
                        (fun value -> dispatch (ModuleMsg (ModuleMsg.NewStudentChanged (value)))) 
                        model.NewStudentsErrorTxt
                        "pencil"
                    button [ ClassName ("btn " + buttonActive); OnClick (fun _ -> dispatch (ModuleMsg ModuleMsg.AddStudents))] [
                        i [ClassName "glyphicon glyphicon-plus"; Style [PaddingRight 5]] []
                        text "Add"
                    ]  
                    div [ClassName (if model.ErrorMsg = "" then "hide" else "" )] [text model.ErrorMsg]
                ]                    
            ]        
        ]
    ]

let newAssignmentForm (model:Model) dispatch =
    let buttonActive = if String.IsNullOrEmpty model.NewAssignment.ID 
                        || String.IsNullOrEmpty model.NewAssignment.Data.Title
                        || String.IsNullOrEmpty model.NewAssignment.Data.ModuleID
                        || String.IsNullOrEmpty model.NewAssignment.Data.StartDate
                        || String.IsNullOrEmpty model.NewAssignment.Data.EndDate then "btn-disabled" else "btn-primary"
    
    let assignmentIDStatus = if String.IsNullOrEmpty model.NewAssignment.ID then "" else "has-success"

    let assignmentTitleStatus = if String.IsNullOrEmpty model.NewAssignment.Data.Title then "" else "has-success"

    let assignmentStartDateStatus = if String.IsNullOrEmpty model.NewAssignment.Data.StartDate then "" else "has-success"

    let assignmentEndDateStatus = if String.IsNullOrEmpty model.NewAssignment.Data.EndDate then "" else "has-success"

    div [] [
        h4 [] [text "Add New Assignment"]

        div [ClassName "container"] [
            div [ClassName "row"] [
                div [ClassName "col-md-8"] [
                    form_group 
                        "ID" 
                        assignmentIDStatus 
                        model.NewAssignment.ID
                        (fun (value:string) -> dispatch (ModuleMsg (ModuleMsg.NewAssignmentChanged {model.NewAssignment with ID = value } ))) 
                        model.NewAssignmentErrorTxt
                        "pencil"
                    form_group 
                        "Title" 
                        assignmentTitleStatus 
                        model.NewAssignment.Data.Title
                        (fun (value:string) -> dispatch (ModuleMsg (ModuleMsg.NewAssignmentChanged 
                                                            { model.NewAssignment with Data = { model.NewAssignment.Data with Title = value } } ))) 
                        model.NewAssignmentErrorTxt
                        "pencil"
                    form_group 
                        "Start Date" 
                        assignmentStartDateStatus 
                        model.NewAssignment.Data.StartDate
                        (fun (value:string) -> dispatch (ModuleMsg (ModuleMsg.NewAssignmentChanged 
                                                            { model.NewAssignment with Data = { model.NewAssignment.Data with StartDate = value } } ))) 
                        model.NewAssignmentErrorTxt
                        "pencil"
                    form_group 
                        "End Date" 
                        assignmentEndDateStatus 
                        model.NewAssignment.Data.EndDate
                        (fun (value:string) -> dispatch (ModuleMsg (ModuleMsg.NewAssignmentChanged 
                                                            { model.NewAssignment with Data = { model.NewAssignment.Data with EndDate = value } } ))) 
                        model.NewAssignmentErrorTxt
                        "pencil"
                    button [ ClassName ("btn " + buttonActive); OnClick (fun _ -> dispatch (ModuleMsg ModuleMsg.AddAssignment))] [
                        i [ClassName "glyphicon glyphicon-plus"; Style [PaddingRight 5]] []
                        text "Add"
                    ]  
                    div [ClassName (if model.ErrorMsg = "" then "hide" else "" )] [text model.ErrorMsg]
                ]                    
            ]        
        ]
    ]
    
let view (model:Model) (dispatch: AppMsg -> unit) =
    let tabButton text typ show=
        button
            [ ClassName <| "tablinks " + (if typ=model.ActiveTab then "active " else " ") + (if show then "" else "hide")
            ; OnClick (fun _ -> dispatch <| ModuleMsg (SetActiveTab typ)) ] [ str text ]
    div [] [
        //back button
        div [] [ button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch LoggedIn) ] [ text "back" ]]
        //Title
        h4 [] [words 35 model.Module.Title]
        //Tab buttons
        div [ ClassName "tab" ] [
            tabButton "Module Info" ModuleInfo true
            tabButton "Assignments" Tab.Assignments true
            tabButton "Lecture notes" LectureNotes true
            tabButton "Tutorials" Tutorials true
            tabButton "Online tests" OnlineTests true
            tabButton "Students" Students (model.User.UserType = "Teacher") ]
        //Module Info content
        tabcontent (model.ActiveTab = ModuleInfo) 
            [ h3 [] [str "Module Info"]
              div [] [text ("Module ID: " + model.ID)]
              div [] [text ("Module owned by " + model.Module.Teacher)]]
        //Assignments content
        tabcontent (model.ActiveTab = Tab.Assignments) 
            [ h3 [][text "Assignments"]
              table [ClassName "table table-striped table-hover"] [
                thead [] [
                    tr [] [
                        th [] [text "ID"]
                        th [] [text "Assignment"]
                        th [] [text "Start date"]
                        th [] [text "End date"]
                        th [] [text "Grade"] ] ]                
                tbody[] [
                    if model.Assignments.IsEmpty 
                    then 
                        yield
                            tr [][td [] [text ""]; td [] [text "No assignment have been published for this module yet"]]
                    else 
                        for assignment in model.Assignments do
                            yield 
                                tr [] [
                                    td [] [ text (assignment.Data.ModuleID + " " + assignment.ID) ]
                                    td [] [ yield button [ ClassName ("btn btn-primary"); OnClick (fun _ -> dispatch (OpenCoursework assignment)) ] [ text assignment.Data.Title ] ]
                                    td [] [ text assignment.Data.StartDate]
                                    td [] [ text assignment.Data.EndDate]
                                    td [] [ text assignment.Data.Grade] ] ] ]
              div [ClassName ( if model.User.UserType = "Teacher" then "" else "hide" )] [newAssignmentForm model dispatch] ]
        //Lecture Notes
        tabcontent (model.ActiveTab = Tab.LectureNotes)
            [ h3 [][text "Lecture notes"]]
        tabcontent (model.ActiveTab = Tab.Tutorials)
            [ h3 [][text "Tutorials"]]
        tabcontent (model.ActiveTab = Tab.OnlineTests)
            [ h3 [][text "Online tests"]]
        //Students (only for Teacher users)
        tabcontent (model.ActiveTab = Tab.Students) 
            [ h3 [][text "Students"]
              table [ClassName "table table-striped table-hover"] [
                thead [] [
                    tr [] [
                        th [] [text "Student"]
                        th [] [text "Module"] ] ]                
                tbody[] [
                    if model.Students.IsEmpty 
                    then 
                        yield
                            tr [][td [] [text ""]; td [] [text "No Registered to this module"]]
                    else 
                        for student in model.Students do
                            yield 
                                tr [] [
                                    td [] [ text (student.Data.UserName) ]
                                    td [] [ yield button [ ClassName ("btn btn-primary"); OnClick (fun _ -> dispatch (OpenModuleWithStudent (model.ID,student.Data))) ] [ text student.Data.UserName ] ] ] ] ]
              newStudentForm model dispatch ]
    ]
