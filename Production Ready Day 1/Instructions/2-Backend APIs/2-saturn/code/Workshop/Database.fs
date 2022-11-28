module Fuber.Database

open Fuber.Domain
open System

let private defaultRides =
    [
        1, { Id = 1; Origin = 50.2; Destination = 52.1; Cost = 4.7; Driver = "Tim"; DriverReview = Some 4; PassengerReview = Some 3; Duration = TimeSpan.FromMinutes(22.) }
        2, { Id = 2; Origin = 50.2; Destination = 52.1; Cost = 15.2; Driver = "Steph"; DriverReview = Some 5; PassengerReview = Some 4; Duration = TimeSpan.FromMinutes(45.) }
        3, { Id = 3; Origin = 50.2; Destination = 52.1; Cost = 10.7; Driver = "Jeff"; DriverReview = None; PassengerReview = None; Duration = TimeSpan.FromMinutes 10.}
    ]
    |> Map.ofList

type private DbResponse<'T> = AsyncReplyChannel<Result<'T, string>>

type private MbpMessages =
    | IndexRides of AsyncReplyChannel<Ride list>
    | GetRide of int * DbResponse<Ride>
    | ReviewRide of int * int * DbResponse<Ride>

let private rideStorage = MailboxProcessor.Start(fun mbp ->
    let rec proc state = async {
        let tryFindRide rideId = state |> Map.tryFind rideId |> Result.ofOption "No ride matching the ID was found"
        let! msg = mbp.Receive()
        let state =
            match msg with
            | IndexRides replyChannel -> 
                state |> Map.toList |> List.map snd |> replyChannel.Reply
                state
            | GetRide (rideId, replyChannel) ->
                rideId |> tryFindRide |> replyChannel.Reply
                state
            | ReviewRide (rideId, reviewScore, replyChannel) ->
                let ride =
                    if reviewScore > 5 || reviewScore < 0 then
                        Error "Invalid score, must be between 0 and 5"
                    else
                        rideId
                        |> tryFindRide
                        |> Result.map (fun ride -> { ride with DriverReview = Some reviewScore; PassengerReview = Some 4 })
                
                // Give the updated ride back as response.
                replyChannel.Reply ride

                // If the response was successful, apply the modified ride to the state store.
                match ride with
                | Ok ride -> state |> Map.add rideId ride
                | err -> state
        return! proc state }
    proc defaultRides)

let listRides () =
    rideStorage.PostAndAsyncReply IndexRides |> Async.StartAsTask

let getRide rideId =
    rideStorage.PostAndAsyncReply(fun arc -> GetRide(rideId, arc)) |> Async.StartAsTask

let reviewRide rideId reviewScore =
    rideStorage.PostAndAsyncReply(fun arc -> ReviewRide(rideId, reviewScore, arc)) |> Async.StartAsTask    

