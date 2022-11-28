module Fuber.Routes

open Fuber.Domain
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn
open Saturn.ControllerHelpers.Response

let getProfile next ctx =
    let profile =
        { Username = "anthony_brown"
          GivenName = "Anthony"
          Surname = "Brown" }
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

let addReview rideId ctx = task {
    let! request = Controller.getModel<ReviewRequest> ctx
    match! Database.reviewRide rideId request.ReviewScore with
    | Ok ride ->
        ctx.SetStatusCode 200
        return! Controller.text ctx ("Reviewed " + ride.Id.ToString())
    | Error err ->
        ctx.SetStatusCode 400
        return! Controller.text ctx err }

let reviewController rideIndex = controller {
    create (addReview rideIndex) }

let rideController = controller {
    index listRides
    show getRide
    subController "/review" reviewController }

let getEstimate next (ctx:HttpContext) = task {
    let! request = ctx.BindModelAsync()
    let! price = PricingEngine.getPriceForRoute(request.OLat, request.OLong, request.DLat, request.DLong)
    return! json price next ctx }

let apiRouter = router {
    forward "/ride" rideController
    get "/profile" getProfile
    get "/estimate" getEstimate }

let router = router {
    forward "/api" apiRouter }