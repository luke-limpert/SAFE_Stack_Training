open Giraffe
open Saturn

let printNumber number =
    let output = sprintf "Hello from %i!" number
    xml output

let routes = router {
    get "/api/foo" (text "Hello from Saturn!")
    getf "/api/bar/%i" printNumber
}

let app = application {
    use_router routes
}

run app