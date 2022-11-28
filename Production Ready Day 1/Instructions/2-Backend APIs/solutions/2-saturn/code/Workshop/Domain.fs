namespace Fuber.Domain

open System

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

[<CLIMutable>]
type EstimateRequest =
    { OLat : float
      OLong : float
      DLat : float
      DLong  : float }

type Location =
    { Latitude : float
      Longitude : float }
    static member Create(lat, long) = { Latitude = lat; Longitude = long }

type EstimatedPrice =
    { DistanceInKm : float
      DurationInMinutes : float
      EstimatedPrice : float }

module Result =
    let ofOption err = function | None -> Error err | Some x -> Ok x
