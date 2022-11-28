module Fuber.Server

open Saturn
open Microsoft.AspNetCore.Builder

let endpointPipe = pipeline {
    plug head
    plug requestId
}

let configureApp (app:IApplicationBuilder) =
    app.UseDefaultFiles()

let app = application {
    url "http://0.0.0.0:8080/"
    pipe_through endpointPipe
    app_config configureApp
    use_router Routes.router
    memory_cache
    use_static "content"
    use_gzip
}

printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
run app