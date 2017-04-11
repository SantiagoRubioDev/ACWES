module ServerCode.Query

open Suave

///?ModuleId=
let useModuleId (ctx: HttpContext) =
    match ctx.request.queryParam "ModuleId" with
    | Choice1Of2 id -> id
    | _ -> "fail"

///?AssignmentId=
let useAssignmentId (ctx: HttpContext) =
    match ctx.request.queryParam "AssignmentId" with
    | Choice1Of2 id -> id
    | _ -> "fail"

///?UserName= //this is not needed use token.UserName instead!!!
let useUserName (ctx: HttpContext) =
    match ctx.request.queryParam "UserName" with
    | Choice1Of2 id -> id
    | _ -> "fail"

