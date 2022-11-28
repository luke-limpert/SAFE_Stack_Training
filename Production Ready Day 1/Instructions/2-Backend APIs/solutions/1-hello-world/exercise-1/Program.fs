open Giraffe
open Saturn

let printNumber number =
    let data = sprintf "Hello from %i!" number
    text data

let routes = router {
    get "/api/foo" (text "Hello from Saturn!")
    getf "/api/bar/%i" printNumber
}

let app = application {
    use_router routes
}

run app