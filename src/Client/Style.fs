module Client.Style

open System
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Import
open Fable.Core.JsInterop
open Elmish
open Fable.Import.Browser
open Fable.PowerPack
open Elmish.Browser.Navigation
open Elmish.UrlParser
open Messages


let viewLink page description =
  a [ Style [ Padding "0 20px" ]
      Href (toHash page) ]
    [ unbox description]

let centerStyle direction =
    Style [ Display "flex"
            FlexDirection direction
            AlignItems "center"
            unbox("justifyContent", "center")
            Padding "20px 0"
    ]

let words size message =
    span [ Style [ unbox("fontSize", size |> sprintf "%dpx") ] ] [ unbox message ]

let text s = text [] [unbox s ]

let buttonLink cssClass onClick elements = 
    a [ ClassName cssClass
        OnClick (fun _ -> onClick())
        OnTouchStart (fun _ -> onClick())
        Style [ unbox("cursor", "pointer") ] ] elements

let onEnter msg dispatch =
    OnKeyDown (fun (ev:React.KeyboardEvent) ->
        match ev with 
        | _ when ev.keyCode = 13. ->
            ev.preventDefault()
            dispatch msg
        | _ -> ())

//Tabs
let tabcontent active = div [Style (if active then [ Margin "0 auto"; Padding "6px 12px"; CSSProp.Width "1200px"; Border "1px solid #ccc"; Overflow "auto"; Display "block"] else [Display "none"] ) ]
let loading txt = div [ Style [Margin "1em 0 1em 0"] ] [
                    div [ClassName "loader"; Style [Float "left"; Margin "0 1em 0 1em"] ] [] //Src "/img/loading_icon.gif"
                    div [ ] [ text ("..."+txt) ] ]

//form group for new insertions
let form_group name status default_string dispatch_msg errortxt glyphtype =
    div [ClassName ("form-group has-feedback" + status)] [
        yield div [ClassName "input-group"] [
                yield span [ClassName "input-group-addon"] [span [ClassName ("glyphicon glyphicon-"+glyphtype) ] [] ]
                yield input [
                        HTMLAttr.Type "text"
                        Name name
                        DefaultValue (U2.Case1 default_string)
                        ClassName "form-control"
                        Placeholder ("Please insert "+name)
                        Required true
                        OnChange (fun (ev:React.FormEvent) -> dispatch_msg ( unbox ev.target?value )) ] []
                match errortxt with
                | Some e -> yield span [ClassName "glyphicon glyphicon-remove form-control-feedback"] []
                | _ -> ()
        ]
        match errortxt with
        | Some e -> yield p [ClassName "text-danger"][text e]
        | _ -> ()
    ]

let assignment_Grade_ID (assignments:ServerCode.Domain.AssignmentTable) onclickfun=
    td [] [
        for assignment in assignments do
            yield
                button [ ClassName ("btn btn-primary"); OnClick (onclickfun assignment.ID) ] [ text assignment.ID ];
                yield Fable.Helpers.React.text [Style[Margin "1px 12px 1px 1px"; CSSProp.FontSize (U2.Case1 20.0)]] [unbox (if assignment.Data.Grade = "" then "N/A" else assignment.Data.Grade)]
    ]
    