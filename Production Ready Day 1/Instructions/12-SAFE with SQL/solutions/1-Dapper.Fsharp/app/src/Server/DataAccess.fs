module DataAccess

open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Microsoft.Data.SqlClient
open Shared
open System


// This is just to explicitly show the connection string, please do not show this use app settings
let getConnection () = new SqlConnection """Server=tcp:localhost,1433;Initial Catalog=sql-lab-dapper;Persist Security Info=True;User ID=sa;Password=Str0ngPa55w0rd!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"""

type TodoDTO =
    { TodoId: Guid
      Description: string
      AssigneeId: Guid option
      Status: string
      Weekly: string option
      Custom: DateTime option }

let userTable = table'<User> "users"
let todoTable = table'<TodoDTO> "todos"

module Users =
    let addUser user =
        async {
            use conn = getConnection ()
            do! insert {
                into userTable
                value user
                }
                |> conn.InsertAsync
                |> Async.AwaitTask
                |> Async.Ignore
            return user
        }

    let getUsers () =
        async {
            use conn = getConnection ()
            let! users =
                select {
                    for user in userTable do
                    selectAll
                }
                |> conn.SelectAsync<User>
                |> Async.AwaitTask
            return
                users |> Seq.toList
        }

module Todos =

    let addTodo (todo: Todo) =
        async {
            use conn = getConnection ()
            let status = Status.Serialise todo.Status
            let todoDto: TodoDTO =
                { TodoId = todo.TodoId
                  Description = todo.Description
                  Status = status
                  AssigneeId = todo.Assignee |> Option.map (fun user -> user.UserId)
                  Weekly =
                    if TaskRepeatState.isWeekly todo.RepeatState then
                        todo.RepeatState |> TaskRepeatState.getDay |> Day.toDisplayValue |> Some
                    else
                        None
                  Custom =
                    if TaskRepeatState.isCustom todo.RepeatState then
                        todo.RepeatState |> TaskRepeatState.getDateTime |> Some
                    else
                        None }
            do!
                insert {
                    into todoTable
                    value todoDto
                }
                |> conn.InsertAsync
                |> Async.AwaitTask
                |> Async.Ignore
            return todo
            }

    let getTodos () =
        async {
            OptionTypes.register()
            use conn = getConnection ()
            let! todoDtos =
                select {
                    for todo in todoTable do
                    leftJoin user in userTable on (todo.AssigneeId = (Some user.UserId))
                    selectAll
                }
                |> conn.SelectAsync<TodoDTO, User>
                |> Async.AwaitTask
            return
                todoDtos
                |> Seq.map (fun (todoDto, user) ->
                    let status = Status.Deserialise todoDto.Status
                    let repeatState =
                        match todoDto.Weekly, todoDto.Custom with
                        | Some day,  _ -> day |> Day.fromDisplayValue |> Weekly
                        | _, Some date -> Custom date
                        | None, None -> NoRepeat
                    { TodoId = todoDto.TodoId
                      Description = todoDto.Description
                      Status = status
                      RepeatState = repeatState
                      Assignee = Some user })
                |> Seq.toList }