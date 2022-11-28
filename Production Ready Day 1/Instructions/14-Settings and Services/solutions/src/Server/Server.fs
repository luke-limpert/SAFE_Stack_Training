module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks

type Storage() =
    let todos = ResizeArray<_>()

    member __.GetTodos() = List.ofSeq todos

    member __.AddTodo(logger : ILogger, todo: Todo) =
        match todo.Description.Length with
        | l when l < 10 ->
            logger.LogInformation "Small todo added"
        | l when l > 10 && l < 20 ->
            logger.LogWarning "Large todo added"
        | _ ->
            logger.LogCritical "Massive todo added"

        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let storage = Storage()

let todosApi (ctx:HttpContext) =
    let todoLogger = ctx.GetService<ILogger<Todo>>()
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
          fun todo ->
              async {
                  match storage.AddTodo (todoLogger, todo) with
                  | Ok () -> return todo
                  | Error e -> return failwith e
              } }

let webApp next ctx = task {
    let handler =
        Remoting.createApi ()
        |> Remoting.withErrorHandler(fun e ri ->
            #if DEBUG
            printfn "\n\nError at '%s'\n\n%O" ri.path e
            #endif
            ErrorResult.Propagate e.Message)
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue (todosApi ctx)
        |> Remoting.buildHttpHandler

    return! handler next ctx
}

let configureHost (hostBuilder : IHostBuilder) =
    hostBuilder
        .ConfigureLogging (fun logging ->
            logging
                .ClearProviders()
                .AddConsole()
            |> ignore)

// This is another way to hook up the same thing
//let configureLogging (logging : ILoggingBuilder) =
//    logging
//        .ClearProviders()
//        .AddConsole()
//    |> ignore

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        host_config configureHost
        //logging configureLogging
        memory_cache
        use_static "public"
        use_gzip
    }


run app
