module Client.Messages

open System
open ServerCode.Domain


/// The messages processed during login 
type LoginMsg =
  | GetTokenSuccess of string*string
  | SetUserName of string
  | SetPassword of string
  | AuthError of exn
  | ClickLogIn

/// The different messages processed when interacting with the wish list
type WishListMsg =
  | LoadForUser of string
  | FetchedWishList of WishList
  | RemoveBook of Book
  | AddBook
  | TitleChanged of string
  | AuthorsChanged of string
  | LinkChanged of string
  | FetchError of exn

/// The messages processed modules 
type ModulesMsg =
  | FetchedModules of ModuleTable
  | AddModule
  | TitleChanged of string
  | IDChanged of string
  | AddModuleError of exn
  | FetchModulesError of exn

/// The messages processed module 
type ModuleMsg =
  | SetActiveTab of Tab
  | FetchedModule of ModuleRow
  | FetchedAssignments of AssignmentTable
  | FetchModuleError of exn

type InitModule =
    | ID of ID*Tab
    | Row of ModuleRow

/// The messages processed tutorials 
type TutorialsMsg =
  | NO
  | YES

/// The messages processed tutorial 
type TutorialMsg =
  | NO
  | YES

/// The messages processed assignment 
type AssignmentsMsg =
  | NO
  | YES

/// The messages processed coursework 
type CourseworkMsg =
  | ClickUpload
  | UploadSuccess of string
  | SetUploadFile of Fable.Import.Browser.FileList//of string
  | FetchedCoursework of StudentCoursework//string//Fable.Import.Browser.File
  | FetchCourseworkError of exn
  | UploadError of exn

/// The messages processed online test 
type OnlineTestMsg =
  | NO
  | YES

/// The different messages processed by the application
type AppMsg = 
  | LoggedIn
  | LoggedOut
  | StorageFailure of exn
  | OpenLogIn
  | OpenModuleWithRow of ModuleRow
  | OpenModuleWithID of ID*Tab
  | OpenCoursework of AssignmentRow
  | LoginMsg of LoginMsg
  | WishListMsg of WishListMsg
  | ModulesMsg of ModulesMsg
  | ModuleMsg of ModuleMsg
  | TutorialsMsg of TutorialsMsg
  | TutorialMsg of TutorialMsg
  | AssignmentsMsg of AssignmentsMsg
  | CourseworkMsg of CourseworkMsg
  | OnlineTestMsg of OnlineTestMsg
  | Logout

/// The user data sent with every message.
type UserData = 
  { UserName : string
    Token : JWT 
    UserType : string}

  static member New = 
    { UserName = ""
      Token = "" 
      UserType = ""}

/// The different pages of the application. If you add a new page, then add an entry here.
type Page = 
  | Home 
  | Login
  | WishList
  | Modules
  | Module
  | Tutorial
  | Assignments
  | Coursework
  | OnlineTest

let toHash =
  function
  | Home -> "#home"
  | Login -> "#login"
  | WishList -> "#wishlist"
  | Modules -> "#modules"
  | Module -> "#module"
  | Tutorial -> "#tutorial"
  | Assignments -> "#assignments"
  | Coursework -> "#coursework"
  | OnlineTest -> "#onlinetest"
