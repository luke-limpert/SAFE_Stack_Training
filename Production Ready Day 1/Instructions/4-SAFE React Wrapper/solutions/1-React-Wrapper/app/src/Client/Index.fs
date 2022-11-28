module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Fable.Core
open Fable.Core.JsInterop

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
open Feliz.ReactAwesomeSlider

let view (model: Model) (dispatch: Msg -> unit) =
    AwesomeSlider.create [
        AwesomeSlider.animation CubeAnimation
        AwesomeSlider.selected 2
        AwesomeSlider.children [
            Html.div [
                Html.img [ prop.src "https://images.pexels.com/photos/1323550/pexels-photo-1323550.jpeg?auto=compress&cs=tinysrgb&dpr=2&h=750&w=1260" ]
            ]
            Html.div [
                Html.img [ prop.src "https://images.pexels.com/photos/248771/pexels-photo-248771.jpeg?auto=compress&cs=tinysrgb&dpr=2&h=750&w=1260" ]
            ]
            Html.div [
                Html.img [ prop.src "https://images.pexels.com/photos/36478/amazing-beautiful-beauty-blue.jpg?auto=compress&cs=tinysrgb&dpr=2&h=750&w=1260" ]
            ]
        ]
    ]