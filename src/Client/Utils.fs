module Client.Utils

open Fable.Import

let load<'T> key =
    Browser.localStorage.getItem(key) |> unbox
    |> Option.map (JS.JSON.parse >> unbox<'T>)

let save key (data: 'T) =
    Browser.localStorage.setItem(key, JS.JSON.stringify data)

let delete key =
    Browser.localStorage.removeItem(key)

(*let getById<'T when 'T :> Browser.HTMLElement> id =
    Browser.document.getElementById(id) :?> 'T

 let getByClass<'T when 'T :> Browser.NodeListOf<Browser.Element>> _class =
    Browser.document.getElementsByClassName(_class) :?> 'T 

let querySelect<'T when 'T :> Browser.HTMLElement> id =
    Browser.document.querySelector(id) :?> 'T*)