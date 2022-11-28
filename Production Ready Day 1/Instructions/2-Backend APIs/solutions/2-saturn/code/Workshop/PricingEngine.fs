module Fuber.PricingEngine

open FSharp.Data
open Fuber.Domain

type private RoutingType = JsonProvider<"""https://route.api.here.com/routing/7.2/calculateroute.json?app_id=1SsfVJTsVa5COwY7sNoE&app_code=q2QfgQchLOdW221Hvve6UQ&waypoint0=geo!52.5,13.4&waypoint1=geo!52.5,13.45&mode=fastest;car;traffic:disabled""">   

let getPriceForRoute (oLat, oLong, dLat, dLong) =
    async {
        let! data =
            let url = sprintf """https://route.api.here.com/routing/7.2/calculateroute.json?app_id=1SsfVJTsVa5COwY7sNoE&app_code=q2QfgQchLOdW221Hvve6UQ&waypoint0=geo!%f,%f&waypoint1=geo!%f,%f&mode=fastest;car;traffic:disabled""" oLat oLong dLat dLong
            RoutingType.AsyncLoad url
        let summary = data.Response.Route.[0].Summary
        let distanceKm = float summary.Distance / 1000.
        let timeMins = float summary.TravelTime / 60.
        let estimatedPrice =
            let distanceCost = distanceKm * 0.40
            let timeCost = timeMins * 0.2
            distanceCost + timeCost
        return
            { EstimatedPrice = estimatedPrice
              DistanceInKm = distanceKm
              DurationInMinutes = timeMins }
    } |> Async.StartAsTask