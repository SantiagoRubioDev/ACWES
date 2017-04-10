namespace ServerCode.Domain

open System

type JWT = string

type Date = 
    { Minute: int
      Hour: int
      Day:int
      Month:int
      Year:int}

    static member empty = 
        { Minute = 0
          Hour = 12
          Day= 30
          Month= 3
          Year= 2017 }

type Grade =
    | Provisonal of string
    | Fixed of string
    | NoMarks
    | NotPublished
    | DeferedDecision of string //plagiarism
    | LateSubmission of string

type ID = string
    //Not allowed in database because not compatible with react
    //| ModuleID of string
    //| StudentID of string
    //| AssignmentID of string
    //| TutorialID of string
    //| NoID

///---------------------LOGIN-----------------------///

type Login = 
    { UserName : string
      Password : string }

///---------------------WISHLIST-----------------------///

/// The data for each book in /api/wishlist
type Book = 
    { Title: string
      Authors: string
      Link: string }

    static member empty = 
        { Title = ""
          Authors = ""
          Link = "" }

/// The logical representation of the data for /api/wishlist
type WishList = 
    { UserName : string
      Books : Book list }

    static member New userName = 
        { UserName = userName
          Books = [] }

module WishListValidation =

    let verifyBookTitle title =
        if String.IsNullOrWhiteSpace title then Some "No title was entered" else
        None

    let verifyBookAuthors authors =
        if String.IsNullOrWhiteSpace authors then Some "No author was entered" else
        None

    let verifyBookLink link =
        if String.IsNullOrWhiteSpace link then Some "No link was entered" else
        None

    let verifyBook book =
        verifyBookTitle book.Title = None &&
        verifyBookAuthors book.Authors = None &&
        verifyBookLink book.Link = None

    let verifyWishList wishList =
        wishList.Books |> List.forall verifyBook


///---------------------STUDENT-----------------------///

type Student =
    { UserName: string
      ModulesID: ID list}

    static member New userName = 
        { UserName = userName
          ModulesID = [] }

///---------------------MODULES-----------------------///

// The logical representation of the data for /api/modules

type Module = 
    { Title: string
      Teacher: string }

    static member New = 
        { Title = ""
          Teacher = "" }

module ModulesValidation =

    let verifyModules modules = true

///---------------------MODULE-----------------------///

type Tab = 
    | ModuleInfo
    | Assignments
    | LectureNotes
    | Tutorials
    | OnlineTests

type ModuleRow =
    { ID:ID
      Data: Module}

    static member New = 
        { ID = ""
          Data = Module.New }

type ModuleTable = ModuleRow list

///---------------------Assignment-----------------------///

type Assignment = 
    { ModuleID: ID
      Title: string
      StartDate: string
      EndDate: string
      Grade: string}

    static member New = 
        { ModuleID = ""
          Title = "" 
          StartDate = ""
          EndDate = ""
          Grade = ""}

module AssignmentValidation =

    let verifyModules assignment = true

type AssignmentRow =
    { ID:ID
      Data: Assignment}

    static member New = 
        { ID = ""
          Data = Assignment.New }

type AssignmentTable = AssignmentRow list

//User
type User =
    { UserName : string
      Password : string
      Type : string}

    static member New = 
        { UserName = ""
          Password = ""
          Type = "" }

type UserRow =
    { ID:ID
      Data: User}

    static member New = 
        { ID = ""
          Data = User.New }

type UserTable = UserRow list

/////Coursework///////

type StudentCoursework =
    { AssignmentID: ID
      State: string
      CmdOut: string
      Feedback: string
      Grade: string}

    static member New id= 
        { AssignmentID = id
          State = ""
          CmdOut = ""
          Feedback = ""
          Grade = ""}

(*
type AssignmentHead =
    { Module: ModuleHead
      ID: int
      Title: string
      StartDate: Date
      EndDate: Date
      Grade: Grade }

    static member empty = 
        { Module= ModuleHead.empty
          ID = 0
          Title = ""
          StartDate = Date.empty
          EndDate= Date.empty
          Grade= Grade.NoMarks }

type TutorialHead =
    { ID: int
      Title: string }

    static member empty = 
        { TutorialHead.ID = 0
          TutorialHead.Title = "" }

type LectureHead =
    { ID: int
      Title: string }

    static member empty = 
        { LectureHead.ID = 0
          LectureHead.Title = "" }

type OnlineTestHead =
    { ID: string
      Title: string
      StatDate: Date
      EndDate: Date
      grade: Grade }

    static member empty = 
        { OnlineTestHead.ID= ""
          OnlineTestHead.Title=""
          OnlineTestHead.StatDate=Date.empty
          OnlineTestHead.EndDate= Date.empty
          OnlineTestHead.grade= Grade.NoMarks }

type ModuleInfo =
    { Description: string
      Link: string }

      static member empty = 
        { ModuleInfo.Description= ""
          ModuleInfo.Link="" }

type Module = 
    { UserName : string
      Head : ModuleHead
      Info : ModuleInfo
      Assignments : AssignmentHead list
      Lectures: LectureHead list
      Tutorials : TutorialHead list
      OnlineTests: OnlineTestHead list }

    static member New userName = 
        { UserName = userName
          Head = ModuleHead.empty
          Info = ModuleInfo.empty
          Assignments = []
          Lectures = []
          Tutorials = []
          OnlineTests = [] }

*)