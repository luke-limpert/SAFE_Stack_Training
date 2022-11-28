namespace Shared

// Bookmark 0.
module SignalRHub =
    [<RequireQualifiedAccess>]
    type ClientMsg =
        | SendGreeting of name:string
        | SendHighFive of name:string

    [<RequireQualifiedAccess>]
    type ServerMsg =
        | Greeting of string
        | HighFive of name:string

module Endpoints =
    // Bookmark 1.
    let [<Literal>] Root = "/socket/SignalR"
