module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model = { Data : obj } // replace with a meaningful model

type Msg = // replace with meaningful messages
    | AnEvent

let init () : Model * Cmd<Msg> =
    let model = { Data = obj() }
    model, Cmd.none

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    model, Cmd.none

open Feliz
open Feliz.Bulma

let view (model: Model) dispatch =
    Bulma.section [
    ]