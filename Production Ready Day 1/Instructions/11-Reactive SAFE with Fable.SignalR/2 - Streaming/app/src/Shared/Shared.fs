namespace Shared

module SignalRHub =

    // Bookmark 0.
    [<RequireQualifiedAccess>]
    module StreamFrom =

        [<RequireQualifiedAccess>]
        type Action =
            | SubscribeToProgressUpdates

        [<RequireQualifiedAccess>]
        type Response =
            | ProgressUpdate of percentage : int

module Endpoints =
    let [<Literal>] Root = "/socket/SignalR"

