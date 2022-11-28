namespace Fuber.Domain

open System

[<CLIMutable>]
type EstimateRequest = 
    {
      OLat : float
      OLong : float
      DLat : float
      DLong : float
    }

type Ride =
    { Id : int
      Origin : float
      Destination : float
      Cost : float
      Driver : string
      DriverReview : int option
      PassengerReview : int option
      Duration : TimeSpan }

type Profile =
    { Username : string
      GivenName : string
      Surname : string }

type ReviewRequest =
    { ReviewScore : int }

type EstimatedPrice =
    { DistanceInKm : float
      DurationInMinutes : float
      EstimatedPrice : float }

module Result =
    let ofOption err = function | None -> Error err | Some x -> Ok x
