module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Feliz.UseElmish
open Feliz.Bulma
open Feliz

module Counter =

    type CounterModel = { Count: int }

    type CounterMsg =
        | Increment
        | Decrement
        | Reset

    let initCounter () = { Count = 0 }, Cmd.none

    let updateCounter (msg: CounterMsg) (model: CounterModel) =
        match msg with
        | Increment -> { model with Count = model.Count + 1 }, Cmd.none
        | Decrement -> { model with Count = model.Count - 1 }, Cmd.none
        | Reset -> { model with Count = 0 }, Cmd.none

    let counterView (model: CounterModel) (dispatch: CounterMsg -> unit) =
        Bulma.box [
            Bulma.content [
                prop.style [ style.textAlign.center ]
                prop.text model.Count
            ]
            Bulma.columns [
                Bulma.column [
                    Bulma.button.a [
                        color.isPrimary
                        prop.onClick (fun _ -> dispatch Increment)
                        prop.text "Increment"
                    ]
                ]
                Bulma.column [
                    Bulma.button.a [
                        color.isDanger
                        prop.onClick (fun _ -> dispatch Decrement)
                        prop.text "Decrement"
                    ]
                ]
                Bulma.column [
                    Bulma.button.a [
                        color.isInfo
                        prop.onClick (fun _ -> dispatch Reset)
                        prop.text "Reset"
                    ]
                ]
            ]
        ]

module TodoList =

    type TodoListModel = { Todos: Todo list; Input: string }

    type TodoListMsg =
        | GotTodos of Todo list
        | SetInput of string
        | AddTodo
        | AddedTodo of Todo

    let todosApi =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.buildProxy<ITodosApi>

    let initTodoList () =
        let model = { Todos = []; Input = "" }
        let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos

        model, cmd


    let updateTodoList (msg: TodoListMsg) (model: TodoListModel) =
        match msg with
        | GotTodos todos -> { model with Todos = todos }, Cmd.none
        | SetInput value -> { model with Input = value }, Cmd.none
        | AddTodo ->
            let todo = Todo.create model.Input

            let cmd =
                Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

            { model  with Input = "" }, cmd
        | AddedTodo todo ->
            { model with Todos = model.Todos @ [ todo ] }, Cmd.none

    open Feliz
    open Feliz.Bulma

    let todoView (model: TodoListModel) (dispatch: TodoListMsg -> unit) =
        Bulma.box [
            Bulma.content [
                Html.ol [
                    for todo in model.Todos do
                        Html.li [ prop.text todo.Description ]
                ]
            ]
            Bulma.field.div [
                field.isGrouped
                prop.children [
                    Bulma.control.p [
                        control.isExpanded
                        prop.children [
                            Bulma.input.text [
                                prop.value model.Input
                                prop.placeholder "What needs to be done?"
                                prop.onChange (fun x -> SetInput x |> dispatch)
                            ]
                        ]
                    ]
                    Bulma.control.p [
                        Bulma.button.a [
                            color.isPrimary
                            prop.disabled (Todo.isValid model.Input |> not)
                            prop.onClick (fun _ -> dispatch AddTodo)
                            prop.text "Add"
                        ]
                    ]
                ]
            ]
        ]

open Counter
open TodoList

type Page =
    | CounterPage of CounterModel
    | TodoList of TodoListModel

type Model =
    { CurrentPage: Page }

type Msg =
    | CounterMsg of CounterMsg
    | TodoListMsg of TodoListMsg
    | ChangeToCounter
    | ChangeToTodoList

let init () : Model * Cmd<Msg> =
    let (todoListModel, cmd) = initTodoList()
    let model =
        { CurrentPage = TodoList todoListModel }

    model, (cmd |> Cmd.map TodoListMsg)

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match model.CurrentPage, msg with
    | CounterPage counterModel, CounterMsg counterMsg ->
        let (newCounterModel, cmd) = updateCounter counterMsg counterModel
        { model with CurrentPage = CounterPage newCounterModel }, cmd

    | TodoList todoListModel, TodoListMsg todoListMsg ->
        let (newTodoListModel, cmd) = updateTodoList todoListMsg todoListModel
        { model with CurrentPage = TodoList newTodoListModel }, cmd |> Cmd.map TodoListMsg

    | _, ChangeToTodoList ->
        let (todoListModel, cmd) = initTodoList ()
        let model =
            { CurrentPage = TodoList todoListModel }

        model, (cmd |> Cmd.map TodoListMsg)

    | _, ChangeToCounter ->
        let (counterModel, cmd) = initCounter ()
        let model =
            { CurrentPage = CounterPage counterModel }

        model, cmd

    | _, _ -> failwith "Invalid state"

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
                match model.CurrentPage with
                | TodoList todoListModel ->
                    Bulma.container [
                        Bulma.button.a [
                            prop.onClick (fun _ -> ChangeToCounter |> dispatch )
                            color.isSuccess
                            prop.text "To Counter"
                        ]
                        Bulma.column [
                            prop.children [
                                Bulma.title [
                                    text.hasTextCentered
                                    prop.text "Todo app"
                                ]
                                todoView todoListModel (TodoListMsg >> dispatch)
                            ]
                        ]
                    ]
                | CounterPage counterModel ->
                    Bulma.container [
                        Bulma.button.a [
                            prop.onClick (fun _ -> ChangeToTodoList |> dispatch)
                            color.isSuccess
                            prop.text "To TodoList"
                        ]
                        Bulma.column [
                            prop.children [
                                Bulma.title [
                                    text.hasTextCentered
                                    prop.text "Counter app"
                                ]
                                counterView counterModel (CounterMsg >> dispatch)
                            ]
                        ]
                    ]
            ]
        ]
    ]
