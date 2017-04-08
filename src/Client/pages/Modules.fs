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
    State : string
    ErrorMsg : string }

/// Get the wish list from the server, used to populate the model
let getModules token =
    promise {        
        let url = "api/modules/"
        let props = 
            [ RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<ModuleTable> url props
    }

let loadModulesCmd token = 
    Cmd.ofPromise getModules token FetchedModules FetchModulesError

let postModules (token,modules) =
    promise {        
        let url = "api/modules/"
        let body = toJson modules
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              RequestProperties.Headers [
                HttpRequestHeaders.Authorization ("Bearer " + token)
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body (unbox body) ]

        return! Fable.PowerPack.Fetch.fetchAs<ModuleTable> url props
    }

let postModulesCmd (token,modules) = 
    Cmd.ofPromise postModules (token,modules) FetchedModules FetchModulesError

let init (user:UserData) = 
    { User = user
      Modules = []
      State = ""
      ErrorMsg = "" }, loadModulesCmd user.Token

let update (msg:ModulesMsg) model : Model*Cmd<ModulesMsg> = 
    match msg with
    | FetchedModules modules ->
        let _modules = modules |> List.sortBy (fun mr -> mr.ID)
        { model with Modules = _modules }, Cmd.none
    | FetchModulesError _ -> 
        model, Cmd.none

let view (model:Model) (dispatch: AppMsg -> unit) = 
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
        div [ClassName (if model.User.UserType = "Teacher" then "" else "hide")][text "You are a teacher"]
    ]
    