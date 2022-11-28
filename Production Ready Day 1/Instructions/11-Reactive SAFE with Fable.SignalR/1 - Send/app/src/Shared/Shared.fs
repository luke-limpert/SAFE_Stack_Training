namespace Shared

// Bookmark 0.
module SignalRHub =
    [<RequireQualifiedAccess>]
    type ClientMsg =
        | SendGreeting

    [<RequireQualifiedAccess>]
    type ServerMsg =
        | Greeting of string

module Endpoints =
    // Bookmark 1.
    let [<Literal>] Root = "/socket/SignalR"
