module ServerCode.Auth

open System.IO
open Suave
open Suave.Logging
open Newtonsoft.Json
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open System
open Suave.ServerErrors
open ServerCode.Domain
open Suave.Logging
open Suave.Logging.Message

let unauthorized s = Suave.Response.response HTTP_401 s

let UNAUTHORIZED s = unauthorized (UTF8.bytes s)

let login (ctx: HttpContext) = async {
    let login:Domain.Login = 
        ctx.request.rawForm 
        |> System.Text.Encoding.UTF8.GetString
        |> JsonConvert.DeserializeObject<Domain.Login>

    try
        let user = List.tryFind (fun user -> user.Data.UserName = login.UserName) DBFile.Users.read
        if user.IsNone then
            return! failwithf "Could not authenticate %s" login.UserName
        elif user.IsSome then
            if user.Value.Data.Password <> login.Password then
                return! failwithf "Could not authenticate %s" login.UserName
                
        let userright : ServerTypes.UserRights = { UserName = login.UserName }
        let token = TokenUtils.encode userright

        return! Successful.OK (token+" "+user.Value.Data.Type) ctx
    with
    | _ -> return! UNAUTHORIZED (sprintf "User '%s' can't be logged in." login.UserName) ctx
}

let useToken ctx f = async {
    match ctx.request.header "Authorization" with
    | Choice1Of2 accesstoken when accesstoken.StartsWith "Bearer " -> 
        let jwt = accesstoken.Replace("Bearer ","")
        match TokenUtils.isValid jwt with
        | None -> return! FORBIDDEN "Accessing this API is not allowed" ctx
        | Some token -> return! f token
    | _ -> return! BAD_REQUEST "Request doesn't contain a JSON Web Token" ctx
}
