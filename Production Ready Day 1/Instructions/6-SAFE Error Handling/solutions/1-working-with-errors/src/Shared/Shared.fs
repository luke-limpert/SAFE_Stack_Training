namespace Shared

open System

type AddTodoError =
    | TooShort
    | ForbiddenWord
    member this.Description = match this with TooShort -> "Task is too short!" | ForbiddenWord -> "The text contains a forbidden word."

type Todo = { Id: Guid; Description: string }

module Todo =
    let validate (todo: Todo) =
        if String.IsNullOrWhiteSpace todo.Description then Error TooShort
        elif todo.Description.Contains "swearword" then Error ForbiddenWord
        else Ok todo

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos: unit -> Async<Todo list>
      addTodo: Todo -> Async<Result<Todo, AddTodoError>> }