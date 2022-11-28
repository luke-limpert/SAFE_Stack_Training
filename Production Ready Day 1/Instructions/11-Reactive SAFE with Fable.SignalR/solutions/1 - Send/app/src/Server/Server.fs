module Server

open Fable.SignalR
open Saturn

module SignalRHub =
    open Shared.SignalRHub

    let send msg (hubContext: FableHub<ClientMsg, ServerMsg>) =
        match msg with
        // Bookmark 8.
        | ClientMsg.SendGreeting name ->
            hubContext.Clients.All.Send (ServerMsg.Greeting $"Hello from {name}!")
        | ClientMsg.SendHighFive name ->
            hubContext.Clients.All.Send (ServerMsg.HighFive name)

let app =
    application {
        use_signalr (
            configure_signalr {
                // Bookmark 2.
                endpoint Shared.Endpoints.Root
                // Bookmark 7.
                send SignalRHub.send
                invoke (fun _ -> failwith "Not implemented")
            }
        )
        url "http://0.0.0.0:8085"
        memory_cache
        use_static "public"
        use_gzip
    }

run app
