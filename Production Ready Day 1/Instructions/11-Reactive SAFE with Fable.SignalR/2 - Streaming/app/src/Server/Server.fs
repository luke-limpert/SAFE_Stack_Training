module Server

open Fable.SignalR
open Saturn

module SignalRHub =
    open Shared.SignalRHub

    [<RequireQualifiedAccess>]
    module Stream =
        open System

        open FSharp.Control

        let rnd = Random ()

        let sendToClient msg _hubContext =
            match msg with
            // Bookmark 2.
            | StreamFrom.Action.SubscribeToProgressUpdates -> asyncSeq {
                for i in 5 .. 5 .. 100 do
                    do! Async.Sleep (rnd.Next(200, 2000))
                    yield StreamFrom.Response.ProgressUpdate i
            }
            |> AsyncSeq.toAsyncEnum

let app =
    application {
        use_signalr (
            configure_signalr {
                endpoint Shared.Endpoints.Root
                send (fun _ -> failwith "Not implemented")
                invoke (fun _ -> failwith "Not implemented")
                // Bookmark 1.
                stream_from SignalRHub.Stream.sendToClient
            }
        )
        url "http://0.0.0.0:8085"
        memory_cache
        use_static "public"
        use_gzip
    }

run app
