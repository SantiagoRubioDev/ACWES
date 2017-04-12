module Client.Modules

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
    User : UserData
    Modules : ModuleTable
    NewModule: ModuleRow
    TitleErrorText : string option
    IDErrorText : string option
    State : string
    ErrorMsg : string }

/// Get the wish list from the server, used to populate the model
let getModules token =
    promise {        
        let url = "/api/modules/"
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<ModuleTable> url props
    }

let loadModulesCmd token = 
    Cmd.ofPromise getModules token FetchedModules FetchModulesError

let postModules (user,newModule) =
    promise {        
        let url = "/api/modules/"
        let body = toJson newModule
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + user.Token)
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body (unbox body) ]

        return! Fable.PowerPack.Fetch.fetchAs<ModuleTable> url props
    }

let postModuleCmd (user,newModule) = 
    Cmd.ofPromise postModules (user,newModule) FetchedModules AddModuleError

let init (user:UserData) = 
    { User = user
      Modules = []
      NewModule = ModuleRow.New
      TitleErrorText = None
      IDErrorText = None
      State = ""
      ErrorMsg = "" }, loadModulesCmd user.Token

let update (msg:ModulesMsg) model : Model*Cmd<ModulesMsg> = 
    match msg with
    | FetchedModules modules ->
        let _modules = modules |> List.sortBy (fun mr -> mr.ID)
        { model with Modules = _modules }, Cmd.none
    | IDChanged id -> 
        { model with NewModule = { model.NewModule with ID = id }; IDErrorText = ModulesValidation.verifyModuleId id }, Cmd.none
    | TitleChanged title -> 
        { model with NewModule = { model.NewModule with  Data = { Title = title ; Teacher = model.User.UserName } }; TitleErrorText = ModulesValidation.verifyModuleTitle title }, Cmd.none
    | AddModule ->
        if ModulesValidation.verifyModule model.NewModule then
            { model with NewModule = ModuleRow.New }, postModuleCmd(model.User,model.NewModule)
        else
            { model with 
                IDErrorText = ModulesValidation.verifyModuleId model.NewModule.ID 
                TitleErrorText = ModulesValidation.verifyModuleTitle model.NewModule.Data.Title }, Cmd.none
    | FetchModulesError _ -> 
        model, Cmd.none
    | AddModuleError e ->
        { model with ErrorMsg = e.Message }, Cmd.none



let newModuleForm (model:Model) dispatch =
    let buttonActive = if String.IsNullOrEmpty model.NewModule.Data.Title || String.IsNullOrEmpty model.NewModule.ID then "btn-disabled" else "btn-primary"
    
    let idStatus = if String.IsNullOrEmpty model.NewModule.ID then "" else "has-success"
    
    let titleStatus = if String.IsNullOrEmpty model.NewModule.Data.Title then "" else "has-success"

    div [] [
        h4 [] [text "New Module"]

        div [ClassName "container"] [
            div [ClassName "row"] [
                div [ClassName "col-md-8"] [
                    form_group 
                        "ID" 
                        idStatus 
                        model.NewModule.ID 
                        (fun value -> dispatch (ModulesMsg (ModulesMsg.IDChanged (value)))) 
                        model.IDErrorText 
                        "pencil"
                    form_group 
                        "Title" 
                        titleStatus 
                        model.NewModule.Data.Title 
                        (fun value -> dispatch (ModulesMsg (ModulesMsg.TitleChanged (value)))) 
                        model.TitleErrorText 
                        "pencil"
                    button [ ClassName ("btn " + buttonActive); OnClick (fun _ -> dispatch (ModulesMsg ModulesMsg.AddModule))] [
                        i [ClassName "glyphicon glyphicon-plus"; Style [PaddingRight 5]] []
                        text "Add"
                    ]  
                    div [ClassName (if model.ErrorMsg = "" then "hide" else "" )] [text model.ErrorMsg]
                ]                    
            ]        
        ]
    ]

let studentView (model:Model) (dispatch: AppMsg -> unit) =
    div [] [
        h4 [] [text (sprintf "Modules for %s" model.User.UserName) ]
        table [ClassName "table table-striped table-hover"] [
            thead [] [
                    tr [] [
                        th [] [text "ID"]
                        th [] [text "Module"]
                ]
            ]                
            tbody[] [
                if model.Modules.IsEmpty 
                then 
                    yield
                        tr [][td [] [text ""]; td [] [text "You do not have any modules available, please contact your Teacher"]]
                else 
                    for _module in model.Modules do
                        yield 
                            tr [] [
                                td [] [ text _module.ID ]
                                td [] [ button [ ClassName ("btn btn-primary"); OnClick (fun _ -> dispatch (OpenModuleWithRow _module)) ] [ text _module.Data.Title ] ]
                                //td [] [ yield viewLink Page.Module _module.Data.Title ]
                                //td [] [ buttonLink "" (fun _ -> dispatch (WishListMsg (RemoveBook book))) [ text "Remove" ] ]
                            ]
            ]
        ]
        div [] [words 10 "* if one or more of your modules are missing, please contact your relevant teachers so they can make the module available to you"]
        //div [ClassName (if model.User.UserType = "Teacher" then "" else "hide")][text "You are a teacher"]
    ]

let teacherView (model:Model) (dispatch: AppMsg -> unit) = 
    div [] [ 
        studentView model dispatch
        newModuleForm model dispatch ]

let view (model:Model) (dispatch: AppMsg -> unit) = 
    (if model.User.UserType = "Teacher" then teacherView else studentView) model dispatch


    