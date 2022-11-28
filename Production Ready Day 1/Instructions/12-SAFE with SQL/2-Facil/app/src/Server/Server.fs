module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open DataAccess


let todosApi =
    { getTodos = Todos.getTodos
      addTodo = Todos.addTodo
      getUsers = Users.getUsers
      addUser = Users.addUser }

let webApp =
    Remoting.createApi ()
    |> Remoting.withErrorHandler(fun e ri ->
        #if DEBUG
        printfn "\n\nError at '%s'\n\n%O" ri.path e
        #endif
        Propagate e.Message)
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        use_developer_exceptions
    }

run app
