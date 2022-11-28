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
    }

    type Msg =
        | SetStreamStatus of StreamStatus
        | SetSubscription of IDisposable
        | SignalRStreamMsg of StreamFrom.Response

    let init (hub: Hub option) =
        let model = {
            Status = StreamStatus.Streaming
            Subscription = None
            ProgressPercentage = 0
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
    let view stream =
        Bulma.box [
            Bulma.label [
                prop.text $"Process progress"
            ]
            Bulma.progress [
                color.isSuccess
                progress.value stream.ProgressPercentage
                progress.max 100
            ]
            Bulma.content [
                prop.text $"{stream.ProgressPercentage}%%"
            ]
        ]

type Model = {
    Hub: Hub option
    Stream: Stream.Model option
}

type Msg =
    | RegisterHub of Hub
    | StartStream
    | StreamMsg of Stream.Msg

let init () =
    let model = {
        Hub = None
        Stream = None
    }
    let cmd =
        Cmd.SignalR.Stream.ServerToClient.connect RegisterHub (fun hub ->
            hub.withUrl(Shared.Endpoints.Root)
                .withAutomaticReconnect()
                .onMessage (fun () -> failwith "onMessage not implemented."))

    model, cmd

let update msg model =
    match msg with
    | RegisterHub hub ->
        { model with Hub = Some hub }, Cmd.none
    | StartStream ->
        // Bookmark 5.
        let stream, streamCmd = Stream.init model.Hub
        let model = { model with Stream = Some stream }
        model, Cmd.map StreamMsg streamCmd
    | StreamMsg msg ->
        match model.Stream with
        | Some stream ->
            let newStream, streamCmd = Stream.update msg stream
            { model with Stream = Some newStream }, Cmd.map StreamMsg streamCmd
        | None ->
            model, Cmd.none

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
                                    let isDisabled =
                                        model.Stream
                                        |> Option.map (fun stream -> stream.Status = Stream.StreamStatus.Streaming)
                                        |> Option.defaultValue false

                                    color.isPrimary
                                    prop.disabled isDisabled
                                    // Bookmark 4.
                                    prop.onClick (fun _ -> if not isDisabled then dispatch StartStream)
                                    prop.text "Start Processing"
                                ]
                            ]
                        ]
                    ]
                ]
                match model.Stream with
                | Some stream -> Stream.view stream
                | None -> ()
            ]
        ]
    ]
