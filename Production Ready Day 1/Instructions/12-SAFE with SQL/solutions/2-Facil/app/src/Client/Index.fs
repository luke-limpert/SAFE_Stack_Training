module Index

open Elmish
open Fable.Remoting.Client
open Shared
open System

type Model =
    { Todos: Todo list
      TodoInput: string
      AssignCheckBox: bool
      RepeatOptions: string list
      SelectedRepeatOption: string
      Days: Day list
      SelectedDay: Day
      SelectedDate: DateTime
      Users: User list
      SelectedUser: User option
      UserInput: string }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    | SetAssign of bool
    | SetRepeatOption of string
    | SetWeeklyOption of Day
    | SetDate of DateTime
    | SetUser of User
    | GotUsers of User list
    | SetUserInput of string
    | AddUser
    | GotUser of User

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () : Model * Cmd<Msg> =
    let model =
        { Todos = []
          TodoInput = ""
          AssignCheckBox = false
          RepeatOptions = TaskRepeatState.DisplayOptions
          SelectedRepeatOption = TaskRepeatState.Default
          Days =
            [ Monday
              Tuesday
              Wednesday
              Thursday
              Friday
              Saturday
              Sunday ]
          SelectedDay = Monday
          SelectedDate = DateTime.Now
          Users = []
          SelectedUser = None
          UserInput = "" }

    let cmd =
        Cmd.OfAsync.perform todosApi.getTodos () GotTodos

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with TodoInput = value }, Cmd.none
    | AddTodo ->
        let repeatState =
            if model.SelectedRepeatOption = "No repeat" then NoRepeat
            elif model.SelectedRepeatOption = "Weekly" then Weekly model.SelectedDay
            else Custom model.SelectedDate
        let todo = Todo.create model.TodoInput model.SelectedUser repeatState
        let cmd =
            Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with TodoInput = "" }, cmd
    | AddedTodo todo ->
        { model with Todos = model.Todos @ [ todo ] }, Cmd.none
    | SetRepeatOption repeatOption ->
        { model with SelectedRepeatOption = repeatOption }, Cmd.none
    | SetWeeklyOption day ->
        { model with SelectedDay = day }, Cmd.none
    | SetDate date ->
        { model with SelectedDate = date }, Cmd.none
    | SetAssign checkbox ->
        let cmd =
            if checkbox then
                Cmd.OfAsync.perform todosApi.getUsers () GotUsers
            else
                Cmd.none
        { model with AssignCheckBox = checkbox }, cmd
    | GotUsers users ->
        { model with Users = users }, Cmd.none
    | SetUser user ->
        { model with SelectedUser = Some user }, Cmd.none
    | SetUserInput value ->
        { model with UserInput = value }, Cmd.none
    | AddUser ->
        let cmd =
            Cmd.OfAsync.perform todosApi.addUser { UserId = Guid.NewGuid (); Name = model.UserInput } GotUser
        { model with UserInput = "" }, cmd
    | GotUser user ->
        { model with Users = model.Users @ [ user ] }, Cmd.none



open Feliz
open Feliz.Bulma

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let getUserByName users name =
    users |> List.find (fun user -> user.Name = name)

let userForm (inputValue: string) dispatch =
    Bulma.box [
        Bulma.field.div [
            Bulma.label "Name"
            Bulma.control.div [
                Bulma.input.text [
                    prop.value inputValue
                    prop.placeholder "Add users to assign todos to"
                    prop.onChange (SetUserInput >> dispatch)
                ]
            ]
        ]
        Bulma.button.button [
            prop.text "Add user"
            color.isInfo
            button.isFullWidth
            prop.onClick (fun _ -> dispatch AddUser)
        ]
    ]

let todoForm model dispatch =
    Bulma.box [
        prop.children [
            Bulma.field.div [
                Bulma.label "Description"
                Bulma.control.div [
                    Bulma.input.text [
                        prop.value model.TodoInput
                        prop.placeholder "What needs to be done?"
                        prop.onChange (SetInput >> dispatch)
                    ]
                ]
            ]
            Bulma.field.div [
                Bulma.label [
                    Bulma.text.span "Assign"
                    Bulma.input.checkbox [
                        prop.value model.AssignCheckBox
                        prop.style [ style.width 30; style.height 30 ]
                        prop.onChange (SetAssign >> dispatch)
                    ]
                ]
            ]
            if model.AssignCheckBox then
                Bulma.field.div [
                    Bulma.label "Assignee"
                    Bulma.control.div [
                        Bulma.select [
                            select.isFullWidth
                            prop.placeholder "What needs to be done?"
                            prop.onChange (getUserByName model.Users >> SetUser >> dispatch)
                            prop.children [
                                for user in model.Users do
                                    Html.option [
                                        prop.key user.UserId
                                        prop.text user.Name
                                        prop.value user.Name
                                    ]
                                ]
                        ]
                    ]
                ]
            Bulma.field.div [
                Bulma.label "Repeat"
                Bulma.control.div [
                    Bulma.select [
                        select.isFullWidth
                        prop.placeholder "What needs to be done?"
                        prop.onChange (SetRepeatOption >> dispatch)
                        prop.value model.SelectedRepeatOption
                        prop.children [
                            for repeatOption in model.RepeatOptions do
                                Html.option [
                                    prop.key repeatOption
                                    prop.text repeatOption
                                    prop.value repeatOption
                                ]
                            ]
                    ]
                ]
            ]
            match TaskRepeatState.fromDisplay model.SelectedRepeatOption with
            | Weekly _ ->
                Bulma.field.div [
                    Bulma.label "Day"
                    Bulma.control.div [
                        Bulma.select [
                            select.isFullWidth
                            prop.onChange (Day.fromDisplayValue >> SetWeeklyOption >> dispatch)
                            prop.children [
                                for day in model.Days do
                                    Html.option [
                                        let dayValue = Day.toDisplayValue day
                                        prop.key dayValue
                                        prop.text dayValue
                                        prop.value dayValue
                                    ]
                                ]
                        ]
                    ]
                ]
            | Custom _ ->
                Bulma.field.div [
                    Bulma.label "Date"
                    Bulma.control.div [
                        Bulma.input.date [
                            prop.onChange (SetDate >> dispatch)
                            prop.value model.SelectedDate
                        ]
                    ]
                ]
            | _ -> ()

            Bulma.control.p [
                Bulma.button.a [
                    color.isPrimary
                    button.isFullWidth
                    prop.disabled (Todo.isValid model.TodoInput |> not)
                    prop.onClick (fun _ -> dispatch AddTodo)
                    prop.text "Add"
                ]
            ]
        ]
    ]


let todos (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.table [
            table.isFullWidth
            prop.children [
                Html.thead [
                    Html.tr [
                        Html.th [
                            prop.text "Description"
                        ]
                        Html.th [
                            prop.text "Assignee"
                        ]
                        Html.th [
                            prop.text "Repeat"
                        ]
                        Html.th [
                            prop.text "Status"
                        ]
                    ]
                ]
                Html.tbody [
                    for todo in model.Todos do
                        Html.tr [
                            Html.td [
                                prop.text todo.Description
                            ]
                            Html.td [
                                prop.text (todo.Assignee |> Option.map (fun user -> user.Name) |> Option.defaultValue "No one assigned")
                            ]
                            Html.td [
                                prop.text (todo.RepeatState |> TaskRepeatState.toDisplay)
                            ]
                            Html.td [
                                prop.text (todo.Status |> Status.Serialise)
                            ]
                        ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    Bulma.container [ navBrand ]
                ]
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.columns [
                        Bulma.column [
                            Bulma.title [
                                prop.text "Add User"
                                text.hasTextCentered
                            ]
                            userForm model.UserInput dispatch
                            Bulma.title [
                                prop.text "Add Todo"
                                text.hasTextCentered
                            ]
                            todoForm model dispatch
                        ]
                        Bulma.column [
                            prop.children [
                                Bulma.title [
                                    text.hasTextCentered
                                    prop.text "Todos"
                                ]
                                todos model dispatch
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
