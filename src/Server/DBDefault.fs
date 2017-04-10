module ServerCode.DBDefault

open ServerCode.Domain

/// The default initial data 
let modules : ModuleTable =
    [{ ID =  "EE-430"
       Data = { Title = "Digital Electronics"
                Teacher = "Dr.X" } };
     { ID = "EE-350"
       Data = { Title = "Robotics"
                Teacher = "Dr.R" } };
     { ID =  "EE-260"
       Data = { Title = "Software Engineering"
                Teacher = "Dr.E" } } ]

let student userName : Student =
    {
        UserName = userName
        ModulesID = [  "EE-430" ;  "EE-260" ]
    }

let assignments moduleId: AssignmentTable =
    [{ ID =  "CW-1"
       Data = {  ModuleID = moduleId
                 Title = "Coursework 1" 
                 StartDate = "15:00 2/04/2017"
                 EndDate = "20:00 10/04/2017"
                 Grade = ""} };
     { ID = "CW-2"
       Data = {  ModuleID = moduleId
                 Title = "Coursework 2" 
                 StartDate = "15:00 10/02/2017"
                 EndDate = "15:00 10/03/2017"
                 Grade = "B"} }]

let userList:UserTable =
    [ { ID = "1"
        Data = { UserName = "test"
                 Password = "test" 
                 Type = "Student" } }
      { ID = "2"
        Data = { UserName = "test2"
                 Password = "test2" 
                 Type = "Teacher" } }
      { ID = "3"
        Data = { UserName = "Student"
                 Password = "Student" 
                 Type = "Student" } }
      { ID = "4"
        Data = { UserName = "Teacher"
                 Password = "Teacher"
                 Type = "Teacher" } }
      { ID = "5"
        Data = { UserName = "Student2"
                 Password = "Student2" 
                 Type = "Student" } }
      { ID = "6"
        Data = { UserName = "Teacher2"
                 Password = "Teacher2" 
                 Type = "Teacher" } } ] 

let coursework : StudentCoursework =
    { AssignmentID = ""
      State = ""
      CmdOut = ""
      Feedback = ""
      Grade = ""}