module Client.App

open Fable.Core
open Fable.Core.JsInterop

open Fable.Core
open Fable.Import
open Elmish
open Elmish.React
open Fable.Import.Browser
open Fable.PowerPack
open Elmish.Browser.Navigation
open Client.Messages
open Elmish.UrlParser

// Model

type SubModel =
  | NoSubModel
  | LoginModel of Login.Model
  | WishListModel of WishList.Model
  | ModulesModel of Modules.Model
  | ModuleModel of Module.Model
  | TutorialModel of Tutorial.Model
  | AssignmentsModel of Assignments.Model
  | CourseworkModel of Coursework.Model
  | OnlineTestModel of OnlineTest.Model

type Model =
  { Page : Page
    Menu : Menu.Model
    SubModel : SubModel }


/// The URL is turned into a Result.
let pageParser : Parser<Page->_,_> =
    oneOf 
        [ format Home (s "home")
          format Page.Login (s "login")
          format WishList (s "wishlist")
          format Modules (s "modules")
          format Module (s "module")
          format Tutorial (s "tutorial")
          format Assignments (s "assignments")
          format Coursework (s "coursework")
          format OnlineTest (s "onlineTest") ]

let hashParser (location:Location) =
    UrlParser.parse id pageParser (location.hash.Substring 1)
    
let urlUpdate (result:Result<Page,string>) model =
    match result with
    | Error e ->
        Browser.console.error("Error parsing url:", e)
        ( model, Navigation.modifyUrl (toHash model.Page) )

    | Ok (Home as page) ->
        { model with Page = page; Menu = { model.Menu with query = "" } }, []

    | Ok (Page.Login as page) ->
        let m,cmd = Login.init model.Menu.User
        { model with Page = page; SubModel = LoginModel m }, Cmd.map LoginMsg cmd

    | Ok (Page.WishList as page) ->
        match model.Menu.User with
        | Some user ->
            let m,cmd = WishList.init user
            { model with Page = page; SubModel = WishListModel m }, Cmd.map WishListMsg cmd
        | None ->
            model, Cmd.ofMsg Logout

    | Ok (Page.Modules as page) ->
        match model.Menu.User with
        | Some user ->
            let m,cmd = Modules.init user
            { model with Page = page; SubModel = ModulesModel m }, Cmd.map ModulesMsg cmd
        | None ->
            model, Cmd.ofMsg Logout

    | Ok (Page.Module as page) ->
        match model.Menu.User with
        | Some user -> //Module should never be accessed by url, redirect to Modules page instead 
            let m,cmd = Modules.init user
            { model with Page = Page.Modules; SubModel = ModulesModel m }, Cmd.map ModulesMsg cmd
        | None ->
            model, Cmd.ofMsg Logout

    | Ok (Page.Tutorial as page) ->
        match model.Menu.User with
        | Some user ->
            let m,cmd = Tutorial.init user
            { model with Page = page; SubModel = TutorialModel m }, Cmd.map TutorialMsg cmd
        | None ->
            model, Cmd.ofMsg Logout

    | Ok (Page.Assignments as page) ->
        match model.Menu.User with
        | Some user ->
            let m,cmd = Assignments.init user
            { model with Page = page; SubModel = AssignmentsModel m }, Cmd.map AssignmentsMsg cmd
        | None ->
            model, Cmd.ofMsg Logout

    | Ok (Page.Coursework as page) ->
        match model.Menu.User with
        | Some user -> //Module should never be accessed by url, redirect to Modules page instead
            let m,cmd = Modules.init user
            { model with Page = Page.Modules; SubModel = ModulesModel m }, Cmd.map ModulesMsg cmd
        | None ->
            model, Cmd.ofMsg Logout

    | Ok (Page.OnlineTest as page) ->
        match model.Menu.User with
        | Some user ->
            let m,cmd = OnlineTest.init user
            { model with Page = page; SubModel = OnlineTestModel m }, Cmd.map OnlineTestMsg cmd
        | None ->
            model, Cmd.ofMsg Logout


let init result =
    let menu,menuCmd = Menu.init()
    let m = 
        { Page = Home
          Menu = menu
          SubModel = NoSubModel }

    let m,cmd = urlUpdate result m
    m,Cmd.batch[cmd; menuCmd]

let update msg model =
    Browser.console.log("update")
    Browser.console.log(msg)
    match msg with
    | AppMsg.OpenLogIn ->
        let m,cmd = Login.init None
        { model with
            Page = Page.Login
            SubModel = LoginModel m }, Cmd.batch [cmd; Navigation.modifyUrl (toHash Page.Login) ]

    | AppMsg.OpenModuleWithRow _module->
        let nextPage = Page.Module
        let m,cmd =
             match model.Menu.User with
                | Some user ->
                    let m,cmd = Module.init (InitModule.Row _module) user
                    { model with Page = nextPage; SubModel = ModuleModel m }, Cmd.map ModuleMsg cmd
                | None ->
                    model, Cmd.ofMsg Logout
        match m.Menu.User with
        | Some user -> //when user.ModuleID <> "" ->
            m, Cmd.batch [cmd; Navigation.modifyUrl (toHash nextPage) ]
        | None ->
            m, Cmd.ofMsg Logout

    | AppMsg.OpenModuleWithID (moduleid,tab) ->
        let nextPage = Page.Module
        let m,cmd =
             match model.Menu.User with
                | Some user ->
                    let m,cmd = Module.init (InitModule.ID (moduleid,tab)) user
                    { model with Page = nextPage; SubModel = ModuleModel m }, Cmd.map ModuleMsg cmd
                | None ->
                    model, Cmd.ofMsg Logout
        match m.Menu.User with
        | Some user -> //when user.ModuleID <> "" ->
            m, Cmd.batch [cmd; Navigation.modifyUrl (toHash nextPage) ]
        | None ->
            m, Cmd.ofMsg Logout

    | AppMsg.OpenCoursework assignment ->
        let nextPage = Page.Coursework
        let m,cmd =
             match model.Menu.User with
                | Some user ->
                    let m,cmd = Coursework.init assignment user
                    { model with Page = nextPage; SubModel = CourseworkModel m }, Cmd.map CourseworkMsg cmd
                | None ->
                    model, Cmd.ofMsg Logout
        match m.Menu.User with
        | Some user ->
            m, Cmd.batch [cmd; Navigation.modifyUrl (toHash nextPage) ]
        | None ->
            m, Cmd.ofMsg Logout

    | StorageFailure e ->
        printfn "Unable to access local storage: %A" e
        model, []

    | LoginMsg msg ->
        match model.SubModel with
        | LoginModel m -> 
            let m,cmd = Login.update msg m
            let cmd = Cmd.map LoginMsg cmd  
            match m.State with
            | Login.LoginState.LoggedIn (token,userType) -> 
                let newUser : UserData = { UserData.New with UserName = m.Login.UserName; Token = token; UserType = userType }
                let cmd =              
                    if model.Menu.User = Some newUser then cmd else
                    Cmd.batch [cmd
                               Cmd.ofFunc (Utils.save "user") newUser (fun _ -> LoggedIn) StorageFailure ]

                { model with 
                    SubModel = LoginModel m
                    Menu = { model.Menu with User = Some newUser }}, cmd
            | _ -> 
                { model with 
                    SubModel = LoginModel m
                    Menu = { model.Menu with User = None } }, cmd
        | _ -> model, Cmd.none

    | WishListMsg msg ->
        match model.SubModel with
        | WishListModel m -> 
            let m,cmd = WishList.update msg m
            let cmd = Cmd.map WishListMsg cmd 
            { model with 
                SubModel = WishListModel m }, cmd
        | _ -> model, Cmd.none

    | ModulesMsg msg ->
        match model.SubModel with
        | ModulesModel m -> 
            let m,cmd = Modules.update msg m
            let cmd = Cmd.map ModulesMsg cmd  
            { model with SubModel = ModulesModel m }, cmd
        | _ -> model, Cmd.none

    | ModuleMsg msg ->
        match model.SubModel with
        | ModuleModel m -> 
            let m,cmd = Module.update msg m
            let cmd = Cmd.map ModuleMsg cmd 
            { model with 
                SubModel = ModuleModel m }, cmd
        | _ -> model, Cmd.none

    | CourseworkMsg msg ->
        match model.SubModel with
        | CourseworkModel m -> 
            let m,cmd = Coursework.update msg m
            let cmd = Cmd.map CourseworkMsg cmd 
            { model with 
                SubModel = CourseworkModel m }, cmd
        | _ -> model, Cmd.none

    | AppMsg.LoggedIn ->
        let nextPage = Page.Modules
        let m,cmd = urlUpdate (Ok nextPage) model
        match m.Menu.User with
        | Some user ->
            m, Cmd.batch [cmd; Navigation.modifyUrl (toHash nextPage) ]
        | None ->
            m, Cmd.ofMsg Logout

    | AppMsg.LoggedOut ->
        { model with
            Page = Page.Home
            SubModel = NoSubModel
            Menu = { model.Menu with User = None } }, 
        Navigation.modifyUrl (toHash Page.Home)

    | AppMsg.Logout ->
        model, Cmd.ofFunc Utils.delete "user" (fun _ -> LoggedOut) StorageFailure

//Santi functions
let deliverablesInTheNextDays days =
    4

// VIEW

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Client.Style

/// Constructs the view for a page given the model and dispatcher.
let viewPage model dispatch =
    match model.Page with
    | Page.Home ->
        [ words 50 "Welcome to"
          words 80 "ACWES"
          //a [ Href "http://fable.io" ] [ words 20 "Learn Fable at fable.io" ]
          words 15 "Automated CourseWork Evaluation Service"
          words 20 "The service provided by Imperial College that helps marking assignements and give students useful feedback instantaneously."
          //a [ Href "http://localhost:8080/#login" ] [ words 15 "Please login using your Imperial College ID" ]
          div [ ClassName "" ][
            //div [ ClassName "" ] [ text (if model.Menu.User.IsSome then (model.Menu.User.Value.UserName+" you have 3 unfinished deliverables due in the following 5 days") else "Please") ; a [Href "http://localhost:8080/#login" ] [text " log in"]; text " using your College ID" ]
            div [ ClassName (if model.Menu.User.IsSome then "" else "hide") ] [ text (if model.Menu.User.IsSome then (model.Menu.User.Value.UserName+" you have "+(string)(deliverablesInTheNextDays 5)+" unfinished deliverables due in the following 5 days") else "") ]
            div [ ClassName (if model.Menu.User.IsSome then "hide" else "") ] [ text "Please" ; a [Href "http://localhost:8080/#login" ] [text " log in"]; text " using your College ID" ]        
            ]
         ]
    | Page.Login -> 
        match model.SubModel with
        | LoginModel m -> 
            [ div [ ] [ Login.view m dispatch ]]
        | _ -> [ ]

    | Page.WishList ->
        match model.SubModel with
        | WishListModel m ->
            [ div [ ] [ lazyView2 WishList.view m dispatch ]]
        | _ -> [ ]

    | Page.Modules ->
        match model.SubModel with
        | ModulesModel m ->
            [ div [ ] [ lazyView2 Modules.view m dispatch ]]
        | _ -> [ ]

    | Page.Module ->
        match model.SubModel with
        | ModuleModel m ->
            [ div [ ] [ lazyView2 Module.view m dispatch ]]
        | _ -> [ ]

    | Page.Tutorial ->
        match model.SubModel with
        | TutorialModel m ->
            [ div [ ] [ lazyView2 Tutorial.view m dispatch ]]
        | _ -> [ ]

    | Page.Assignments ->
        match model.SubModel with
        | AssignmentsModel m ->
            [ div [ ] [ lazyView2 Assignments.view m dispatch ]]
        | _ -> [ ]

    | Page.Coursework ->
        match model.SubModel with
        | CourseworkModel m ->
            [ div [ ] [ lazyView2 Coursework.view m dispatch ]]
        | _ -> [ ]

    | Page.OnlineTest ->
        match model.SubModel with
        | OnlineTestModel m ->
            [ div [ ] [ lazyView2 OnlineTest.view m dispatch ]]
        | _ -> [ ]

/// Constructs the view for the application given the model.
let view model dispatch =
  div []
    [ lazyView2 Menu.view model.Menu dispatch
      hr [] []
      div [ centerStyle "column" ] (viewPage model dispatch)
    ]

open Elmish.React

// App
Program.mkProgram init update view
|> Program.toNavigable hashParser urlUpdate
|> Program.withConsoleTrace
|> Program.withReact "elmish-app"
|> Program.run