module Server

open Microsoft.AspNetCore.Http
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Microsoft.Extensions.Logging

module Database =
    let rnd = System.Random()
    let todos = ResizeArray()

    let getTodos () =
        match rnd.Next(1, 6) with
        | 1 -> failwith "Error connecting to database!"
        | _ -> List.ofSeq todos

    let addTodo (todo: Todo) =
        todos.Add todo
        todo

do
    Database.addTodo (Todo.create "Use Result<'T>") |> ignore
    Database.addTodo (Todo.create "Implement the Fable Remoting Error Handler") |> ignore
    Database.addTodo (Todo.create "Use Errors correctly") |> ignore
    Database.addTodo (Todo.create "Handle Exceptions on the client") |> ignore

let todosApi =
    { getTodos = Database.getTodos >> async.Return
      addTodo = Database.addTodo >> async.Return }

let webApp =
    Remoting.createApi ()
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
    }

run app
