module DataAccess

open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Microsoft.Data.SqlClient
open Dapper
open Shared
open System
open System.Data

// This is just to explicitly show the connection string, please do not show this use appSettings.json in a real application
let getConnection () = new SqlConnection """Server=tcp:localhost,1433;Initial Catalog=sql-lab-dapper;Persist Security Info=True;User ID=sa;Password=Str0ngPa55w0rd!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"""

module Users =
    let addUser (user: User) =
        async {
            use conn = getConnection ()
            let sqlQuery = "INSERT INTO users (UserId, Name) VALUES (@UserId, @Name)"

            use cmd = new SqlCommand (sqlQuery, conn)
            conn.Open ()

            cmd.Parameters.AddWithValue ("@UserId", user.UserId) |> ignore
            cmd.Parameters.AddWithValue ("@Name", user.Name) |> ignore

            do!
                cmd.ExecuteNonQueryAsync ()
                |> Async.AwaitTask
                |> Async.Ignore
            return user
        }

    let getUsers () =
        async {
            use conn = getConnection ()
            let sqlQuery = "SELECT * FROM users"

            use cmd = new SqlCommand (sqlQuery, conn)
            conn.Open ()

            let! reader =
                cmd.ExecuteReaderAsync ()
                |> Async.AwaitTask

            let users =
                seq {
                    while reader.Read() do
                        let id =
                            "UserId"
                            |> reader.GetOrdinal
                            |> reader.GetGuid

                        let name =
                            "Name"
                            |> reader.GetOrdinal
                            |> reader.GetString
                        { UserId = id; Name = name }
                }
            return users |> Seq.toList
        }

module Todos =


    let addTodo (todo: Todo) =
        async {
            use conn = getConnection ()
            let sqlQuery = "INSERT INTO todos (TodoId, Description, AssigneeId, Weekly, Custom, Status) VALUES (@TodoId, @Description, @AssigneeId, @Weekly, @Custom, @Status)"
            use cmd = new SqlCommand (sqlQuery, conn)
            conn.Open ()

            let assigneeId =
                match todo.Assignee with
                | None -> box DBNull.Value
                | Some user -> box (user.UserId)

            let weekly =
                if TaskRepeatState.isWeekly todo.RepeatState then
                    todo.RepeatState
                    |> TaskRepeatState.getDay
                    |> Day.Serialise
                    |> box
                else
                    box DBNull.Value

            let custom =
                if TaskRepeatState.isCustom todo.RepeatState then
                    todo.RepeatState
                    |> TaskRepeatState.getDateTime
                    |> box
                else
                    box DBNull.Value


            cmd.Parameters.AddWithValue ("@TodoId", todo.TodoId) |> ignore
            cmd.Parameters.AddWithValue ("@Description", todo.Description) |> ignore
            cmd.Parameters.AddWithValue ("@AssigneeId", assigneeId) |> ignore
            cmd.Parameters.AddWithValue ("@Weekly", weekly) |> ignore
            cmd.Parameters.AddWithValue ("@Custom", custom) |> ignore
            cmd.Parameters.AddWithValue ("@Status", todo.Status |> Status.Serialise) |> ignore

            do!
                cmd.ExecuteNonQueryAsync ()
                |> Async.AwaitTask
                |> Async.Ignore

            return todo
        }

    let getTodos () =
        async {
            use conn = getConnection ()
            let sqlQuery =
                "SELECT * FROM todos
                LEFT JOIN users ON users.UserId = todos.AssigneeId"

            use cmd = new SqlCommand (sqlQuery, conn)
            conn.Open ()

            let! reader =
                cmd.ExecuteReaderAsync ()
                |> Async.AwaitTask

            let todos =
                seq {
                    while reader.Read() do
                        let id =
                            "TodoId"
                            |> reader.GetOrdinal
                            |> reader.GetGuid

                        let description =
                            "Description"
                            |> reader.GetOrdinal
                            |> reader.GetString

                        let status =
                            "Status"
                            |> reader.GetOrdinal
                            |> reader.GetString
                            |> Status.Deserialise

                        let assignee =
                            let isAssigned =
                                "AssigneeId"
                                |> reader.GetOrdinal
                                |> reader.IsDBNull
                                |> not
                            if isAssigned then
                                let id =
                                    "UserId"
                                    |> reader.GetOrdinal
                                    |> reader.GetGuid

                                let name =
                                    "Name"
                                    |> reader.GetOrdinal
                                    |> reader.GetString

                                Some { UserId = id; Name = name }
                            else
                                None

                        let weekly =
                            let isWeekly =
                                "Weekly"
                                |> reader.GetOrdinal
                                |> reader.IsDBNull
                                |> not

                            if isWeekly then
                                "Weekly"
                                |> reader.GetOrdinal
                                |> reader.GetString
                                |> Day.Deserialise
                                |> Weekly
                                |> Some
                            else
                                None

                        let custom =
                            let isCustom =
                                "Custom"
                                |> reader.GetOrdinal
                                |> reader.IsDBNull
                                |> not

                            if isCustom then
                                "Custom"
                                |> reader.GetOrdinal
                                |> reader.GetDateTime
                                |> Custom
                                |> Some
                            else
                                None

                        let repeatState =
                            match weekly, custom with
                            | Some day, None -> day
                            | None, Some date -> date
                            | None, None -> NoRepeat
                            | _ -> failwith "Invalid combination of weekly and custom"

                        { TodoId = id
                          Description = description
                          Status = status
                          Assignee = assignee
                          RepeatState = repeatState }
                }

            return
                todos |> Seq.toList
        }