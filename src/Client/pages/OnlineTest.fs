module Client.OnlineTest

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
    State : string
    ErrorMsg : string }

let init (user:UserData) = 
    { State = "";
      ErrorMsg = "" }, Cmd.none//, loadWishListCmd user.Token

let view (model:Model) (dispatch: AppMsg -> unit) = 
    div [] [
        div [ ClassName "text-center" ] [
            text "OnlineTest works"
              ] 
    ]
    