module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Fable.Core.JsInterop
open Feliz.ReactAwesomeSlider


type Model = { Input: string }

type Msg =
    | Nothing

let init () : Model * Cmd<Msg> =
    let model = { Input = "" }

    model, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | Nothing -> model, Cmd.none

open Feliz
open Feliz.Bulma


let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.children [
            Html.h1 "React Wrapper Module"
        ]
    ]
