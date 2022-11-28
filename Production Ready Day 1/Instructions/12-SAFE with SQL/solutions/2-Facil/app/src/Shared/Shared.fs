namespace Shared

open System

type Status =
    | NotStarted
    | InProgress
    | Done

    static member Serialise = function
        | NotStarted -> "NotStarted"
        | InProgress -> "InProgress"
        | Done -> "Done"

    static member Deserialise = function
        | "NotStarted" -> NotStarted
        | "InProgress" -> InProgress
        | "Done" -> Done
        | x -> failwith $"Invalid status: {x}"

type Day =
    | Monday
    | Tuesday
    | Wednesday
    | Thursday
    | Friday
    | Saturday
    | Sunday

    static member toDisplayValue = function
        | Monday -> "Monday"
        | Tuesday -> "Tuesday"
        | Wednesday -> "Wednesday"
        | Thursday -> "Thursday"
        | Friday -> "Friday"
        | Saturday -> "Saturday"
        | Sunday -> "Sunday"

    static member fromDisplayValue = function
        | "Monday" -> Monday
        | "Tuesday" -> Tuesday
        | "Wednesday" -> Wednesday
        | "Thursday" -> Thursday
        | "Friday" -> Friday
        | "Saturday" -> Saturday
        | "Sunday" -> Sunday
        | invalidDay -> failwith $"Invalid day: {invalidDay}"

type TaskRepeatState =
    | NoRepeat
    | Weekly of Day
    | Custom of DateTime

    static member isNoRepeat = function
        | NoRepeat -> true
        | _ -> false

    static member isWeekly = function
        | Weekly _ -> true
        | _ -> false

    static member getDay = function
        | Weekly day -> day
        | x -> failwith $"Invalid task repeat state: {x}"

    static member isCustom = function
        | Custom _ -> true
        | _ -> false

    static member getDateTime = function
        | Custom dateTime -> dateTime
        | x -> failwith $"Invalid task repeat state: {x}"

    static member DisplayOptions = [ "No repeat"; "Weekly"; "Custom" ]
    static member Default = "No repeat"

    static member fromDisplay = function
        | "No repeat" -> NoRepeat
        | "Weekly" -> Weekly Monday
        | "Custom" -> Custom DateTime.Now
        | x -> failwith $"Invalid task repeat state: {x}"

    static member toDisplay = function
        | NoRepeat -> "No repeat"
        | Weekly day -> "Weekly on " + Day.toDisplayValue day
        | Custom dateTime -> dateTime.ToString("dd/MM/yyyy")



type User =
    { UserId: Guid
      Name: string }

type Todo =
    { TodoId: Guid
      Description: string
      Status: Status
      Assignee: User option
      RepeatState : TaskRepeatState }


module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) assignee repeatState =
        { TodoId = Guid.NewGuid ()
          Description = description
          Status = NotStarted
          Assignee = assignee
          RepeatState = repeatState }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos: unit -> Async<Todo list>
      getUsers: unit -> Async<User list>
      addUser: User -> Async<User>
      addTodo: Todo -> Async<Todo> }
