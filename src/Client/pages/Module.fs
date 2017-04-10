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
    UserName : string
    Module : Module
    Assignments : AssignmentTable
    ActiveTab : Tab
    ErrorMsg : string }

let getAssignments (id,token) =
    Browser.console.log("getAssignments: Id is: " + id)
    promise {        
        let url = "api/assignments?ModuleId="+id
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
        let url = "api/module?ModuleId="+id
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<ModuleRow> url props
    }

let loadModuleCmd id token = 
    Cmd.ofPromise getModule (id,token) FetchedModule FetchModuleError

let init (init:InitModule) (user:UserData) =
    match init with
    | ID (moduleid,tab) ->
        { ID = moduleid
          UserName = user.UserName
          Module = Module.New
          Assignments = [] 
          ActiveTab = tab
          ErrorMsg = "" }, Cmd.batch [loadModuleCmd moduleid user.Token; loadAssignmentsCmd moduleid user.Token] //Cmd.none //, loadWishListCmd user.Token
    | Row _module ->
        { ID = _module.ID
          UserName = user.UserName
          Module = _module.Data //Module.New
          Assignments = [] 
          ActiveTab = ModuleInfo
          ErrorMsg = "" }, loadAssignmentsCmd _module.ID user.Token //Cmd.none //, loadWishListCmd user.Token
    

let update (msg:ModuleMsg) model : Model*Cmd<ModuleMsg> = 
    match msg with
    | ModuleMsg.SetActiveTab x -> { model with ActiveTab = x }, []
    | FetchedModule _module ->
        Browser.console.log("gotModule: " + _module.Data.Title)
        { model with ID = _module.ID; Module = _module.Data }, Cmd.none
    | FetchedAssignments assignments ->
        Browser.console.log("gotAssignment: " + assignments.Head.Data.Title)
        { model with Assignments = assignments }, Cmd.none
    | FetchModuleError _ -> 
        model, Cmd.none
    
let view (model:Model) (dispatch: AppMsg -> unit) =
    let tabButton text typ =
        button
            [ ClassName <| "tablinks " + (if typ=model.ActiveTab then "active" else "")
            ; OnClick (fun _ -> dispatch <| ModuleMsg (SetActiveTab typ)) ] [ str text ]
    div [] [
        //back button
        div [] [ button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch LoggedIn) ] [ text "back" ]]
        //Title
        h4 [] [words 35 model.Module.Title]
        //Tab buttons
        div [ ClassName "tab" ] [
            tabButton "Module Info" ModuleInfo
            tabButton "Assignments" Tab.Assignments
            tabButton "Lecture notes" LectureNotes
            tabButton "Tutorials" Tutorials
            tabButton "Online tests" OnlineTests ]
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
                                    td [] [ text assignment.Data.Grade] ] ] ] ]
        tabcontent (model.ActiveTab = Tab.LectureNotes)
            [ h3 [][text "Lecture notes"]]
        tabcontent (model.ActiveTab = Tab.Tutorials)
            [ h3 [][text "Tutorials"]]
        tabcontent (model.ActiveTab = Tab.OnlineTests)
            [ h3 [][text "Online tests"]]
    ]
