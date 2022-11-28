module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open DataAccess

open DbGen


let todosApi =
    { getTodos = Todos.getTodos
      addTodo = fun todo -> async {
        let todoDto: TableDtos.dbo.todos =
            {
                ``TodoId``= todo.TodoId
                ``Description`` = todo.Description
                ``AssigneeId`` = todo.Assignee |> Option.map(fun x -> x.UserId)
                ``Status`` = todo.Status |> Status.Serialise
                Weekly =
                    if TaskRepeatState.isWeekly todo.RepeatState then
                        todo.RepeatState |> TaskRepeatState.getDay |> Day.toDisplayValue |> Some
                    else
                        None
                Custom =
                    if TaskRepeatState.isCustom todo.RepeatState then
                        todo.RepeatState |> TaskRepeatState.getDateTime |> Some
                    else
                        None
            }
        let! _ =
            DbGen.Scripts.todos_Insert
                .WithConnection("Server=tcp:localhost,1433;Initial Catalog=sql-lab-facil;Persist Security Info=True;User ID=sa;Password=Str0ngPa55w0rd!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;")
                .WithParameters(todoDto)
                .AsyncExecute()
        return todo }
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
