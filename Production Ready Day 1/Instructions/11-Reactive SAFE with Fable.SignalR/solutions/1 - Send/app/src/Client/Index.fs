module Index

open Elmish
open Fable.SignalR.Elmish

open Shared
open Shared.SignalRHub

type Model =
    { Name: string
      Greetings: string list
      Hub: Elmish.Hub<ClientMsg, ServerMsg> option }

type Msg =
    | RegisterHub of Elmish.Hub<ClientMsg, ServerMsg>
    | SignalRMsg of ServerMsg
    | SetName of string
    | SayHello
    | SendHighFive

let init () : Model * Cmd<Msg> =
    let model =
        { Name = ""
          Greetings = []
          Hub = None }

    let cmd =
        // Bookmark 3.
        Cmd.SignalR.connect RegisterHub (fun hub ->
            hub.withUrl(Endpoints.Root)
                .withAutomaticReconnect()
                .onMessage SignalRMsg)

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    // Bookmark 4.
    | RegisterHub hub ->
        { model with Hub = Some hub }, Cmd.none
    // Bookmark 9.
    | SignalRMsg serverMsg ->
        match serverMsg with
        | ServerMsg.Greeting text -> { model with Greetings = text :: model.Greetings }, Cmd.none
        | ServerMsg.HighFive name -> { model with Greetings = $"{name} sent a high five!" :: model.Greetings }, Cmd.none
    | SetName name ->
        { model with Name = name }, Cmd.none
    // Bookmark 6.
    | SayHello ->
        model, Cmd.SignalR.send model.Hub (ClientMsg.SendGreeting model.Name)
    | SendHighFive ->
        model, Cmd.SignalR.send model.Hub (ClientMsg.SendHighFive model.Name)

open Feliz
open Feliz.Bulma

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.container [
        Bulma.column [
            column.is6
            column.isOffset3
            prop.children [
                Bulma.title [
                    text.hasTextCentered
                    prop.text "Fable.SignalR app"
                ]
                Bulma.box [
                    Bulma.field.div [
                        prop.children [
                            Bulma.label [
                                prop.text "Your name"
                            ]
                            Bulma.control.p [
                                control.isExpanded
                                prop.children [
                                    Bulma.input.text [
                                        prop.value model.Name
                                        prop.onChange (SetName >> dispatch)
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Bulma.buttons [
                        Bulma.button.button [
                            prop.text "Say hello"
                            // Bookmark 5.
                            prop.onClick (fun _ -> dispatch SayHello)
                        ]
                        Bulma.button.button [
                            prop.text "Send a high five!"
                            prop.onClick (fun _ -> dispatch SendHighFive)
                        ]
                    ]
                ]
                Bulma.box [
                    Bulma.field.div [
                        prop.children [
                            Bulma.label [
                                prop.text "Greetings"
                            ]
                            for greeting in model.Greetings do
                                Bulma.block [
                                    // Bookmark 10.
                                    prop.text greeting
                                ]
                        ]
                    ]
                ]
            ]
        ]
    ]
