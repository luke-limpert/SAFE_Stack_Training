module Fuber.Routes

open Fuber.Domain
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn
open Saturn.ControllerHelpers.Response

let getProfile next ctx =
    let profile =
        { Username = "luke_limpert"
          GivenName = "Luke"
          Surname = "Limpert" }
    json profile next ctx

let listRides ctx = task {
    let! rides = Database.listRides()
    return! Controller.json ctx rides }

let getRide ctx id = task {
    let! rides = Database.getRide id
    return!
        match rides with
        | Ok ride -> Controller.json ctx ride
        | Error err -> notFound ctx err }

let rideController = controller {
    index listRides
    show getRide }

let getEstimate next (ctx:HttpContext) = task {
    let! estimateRequest = 
        ctx.BindModelAsync<EstimateRequest>()
    let! estimate = 
        estimateRequest
        |> PricingEngine.getPriceForRoute
    return! json estimate next ctx }

let apiRouter = router {
    forward "/ride" rideController
    get "/estimate" getEstimate 
    get "/profile" getProfile }

let router = router {
    forward "/api" apiRouter }