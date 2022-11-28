module Index

open Elmish
open Fable.SignalR
open Fable.SignalR.Elmish
open Feliz
open Feliz.Bulma
open System

open Shared.SignalRHub

// Bookmark 3.
type Hub = Elmish.StreamHub.ServerToClient<unit, StreamFrom.Action, unit, StreamFrom.Response>

module Stream =

    [<RequireQualifiedAccess>]
    type StreamStatus =
        | NotStarted
        | Error of exn option
        | Streaming
        | Finished

    type Model = {
        Status: StreamStatus
        Subscription: IDisposable option
        ProgressPercentage: int
        HorseColor: IStyleAttribute
    }

    type Msg =
        | SetStreamStatus of StreamStatus
        | SetSubscription of IDisposable
        | SignalRStreamMsg of StreamFrom.Response

    let rnd = Random ()

    let init (hub: Hub option) =
        let model = {
            Status = StreamStatus.Streaming
            Subscription = None
            ProgressPercentage = 0
            HorseColor =
                match rnd.Next 3 with
                | 0 -> style.color.saddleBrown
                | 1 -> style.color.black
                | _ -> style.color.lightGray
        }
        let cmd =
            let subscriber dispatch = {
                next = SignalRStreamMsg >> dispatch
                complete = fun () -> dispatch (SetStreamStatus StreamStatus.Finished)
                error = fun e -> dispatch (SetStreamStatus (StreamStatus.Error e))
            }
            // Bookmark 6.
            Cmd.SignalR.streamFrom hub StreamFrom.Action.SubscribeToProgressUpdates SetSubscription subscriber
        model, cmd

    let update msg model =
        match msg with
        | SetStreamStatus status ->
            { model with Status = status }, Cmd.none
        | SetSubscription sub ->
            { model with Subscription = Some sub }, Cmd.none
        | SignalRStreamMsg response ->
            match response with
            | StreamFrom.Response.ProgressUpdate percentage ->
                // Bookmark 7.
                { model with ProgressPercentage = percentage }, Cmd.none

    // Bookmark 8.
    let view index stream =
        Bulma.box [
            Bulma.label [
                prop.text $"Horse {index + 1}"
            ]
            Html.div [
                prop.style [ style.marginLeft (length.percent (stream.ProgressPercentage * 95 / 100)) ]
                prop.children [
                    Html.div [
                        Bulma.icon [
                            icon.isLarge
                            prop.children [
                                Html.i [
                                    prop.style [ stream.HorseColor ]
                                    prop.classes [
                                        "fas"
                                        "fa-horse"
                                        "fa-2x"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            Bulma.content [
                match stream.Status with
                | StreamStatus.NotStarted -> "Not started"
                | StreamStatus.Streaming -> $"{(100 - stream.ProgressPercentage) * 10}m from the finish line"
                | StreamStatus.Error _ -> "Error"
                | StreamStatus.Finished -> "Finished!"
                |> prop.text
            ]
        ]

type Model = {
    Hub: Hub option
    Streams: Stream.Model list
}

type Msg =
    | RegisterHub of Hub
    | StartStream
    | StreamMsg of streamIndex : int * Stream.Msg

let init () =
    let model = {
        Hub = None
        Streams = []
    }
    let cmd =
        Cmd.SignalR.Stream.ServerToClient.connect RegisterHub (fun hub ->
            hub.withUrl(Shared.Endpoints.Root)
                .withAutomaticReconnect()
                .onMessage (fun () -> failwith "onMessage not implemented."))

    model, cmd

let update msg model =
    let toCmd ix streamCmd =
        Cmd.map (fun streamMsg -> StreamMsg (ix, streamMsg)) streamCmd
    let replaceStreamAt ix newStream streams =
        streams |> List.mapi (fun i stream -> if i = ix then newStream else stream)

    match msg with
    | RegisterHub hub ->
        { model with Hub = Some hub }, Cmd.none
    | StartStream ->
        // Bookmark 5.
        let newStream, newStreamCmd = Stream.init model.Hub
        let model = { model with Streams = model.Streams @ [ newStream ] }
        let ix = model.Streams.Length - 1
        model, toCmd ix newStreamCmd
    | StreamMsg (ix, msg) ->
        let stream = model.Streams.[ix]
        let newStream, streamCmd = Stream.update msg stream
        let newStreams = replaceStreamAt ix newStream model.Streams
        { model with Streams = newStreams }, toCmd ix streamCmd

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.container [
        Bulma.column [
            column.is6
            column.isOffset3
            prop.children [
                Bulma.box [
                    Bulma.field.div [
                        field.isGrouped
                        prop.children [
                            Bulma.control.p [
                                Bulma.button.a [
                                    color.isPrimary
                                    // Bookmark 4.
                                    prop.onClick (fun _ -> dispatch StartStream)
                                    prop.text "Let a horse out of the traps!"
                                ]
                            ]
                        ]
                    ]
                ]
                for (ix, stream) in List.indexed model.Streams do
                    Stream.view ix stream
            ]
        ]
    ]
