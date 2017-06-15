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
  | SetActiveTab of ModuleTab
  | FetchedModule of ModuleRow
  | FetchedAssignments of AssignmentTable
  | NewAssignmentChanged of AssignmentRow
  | AddAssignment
  | FetchedStudents of UserTable
  | NewStudentChanged of string
  | AddStudents
  | FetchModuleError of exn

type InitModule =
    | ID of ID*ModuleTab
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
  | SetActiveTab of CourseworkTab
  //coursework
  | SetCourseworkFile of Fable.Import.Browser.FileList
  | ClickUploadCoursework
  | ReadCourseworkSuccess of string
  | ReadCourseworkError of exn
  | UploadCourseworkSuccess of string
  | UploadCourseworkError of exn
  | FetchedCoursework of StudentCoursework//string//Fable.Import.Browser.File
  | FetchCourseworkError of exn
  //Test Bench
  | SetTBFiles of Fable.Import.Browser.FileList
  | ClickUploadTB
  | ReadTBSuccess of string
  | ReadTBError of exn
  | UploadTBSuccess of string
  | UploadTBError of exn
  | FetchedTB of TeacherCoursework
  | FetchTBError of exn
  //Spec
  | SetSpecFile of Fable.Import.Browser.FileList
  | ClickOpenSpec
  | ClickSaveSpec
  | UpdateSpec of string
  | ReadSpecSuccess of string
  | ReadSpecError of exn
  | UploadSpecSuccess of string
  | UploadSpecError of exn
  //Assignement
  | ClickSaveFeedback
  | UpdateGrade of string
  | UpdateFeedback of string


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
  | OpenModuleWithStudent of ID*User
  | OpenModuleWithRow of ModuleRow
  | OpenModuleWithID of ID*ModuleTab
  | OpenCourseworkWithStudent of ID*ID*User
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
