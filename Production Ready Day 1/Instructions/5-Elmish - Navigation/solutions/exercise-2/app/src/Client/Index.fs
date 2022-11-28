module Index

open Elmish
open Fable.Remoting.Client
open Shared


module Counter =
    open Feliz.Bulma
    open Feliz

    type CounterModel = { Count: int }

    type CounterMsg =
        | Increment
        | Decrement
        | Reset

    let initCounter () = { Count = 0 }, Cmd.none

    let updateCounter (model: CounterModel) (msg: CounterMsg) =
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


    let updateTodoList (model: TodoListModel) (msg: TodoListMsg) =
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
open Feliz.Router

[<RequireQualifiedAccess>]
type Url =
    | Counter
    | TodoList

type Page =
    | Counter of CounterModel
    | TodoList of TodoListModel

type Model =
    { CurrentPage: Page; CurrentUrl: Url }


type Msg =
    | CounterMsg of CounterMsg
    | TodoListMsg of TodoListMsg
    | UrlChanged of Url


let parseUrl = function
  | [ ] -> Url.TodoList
  | [ "counter" ] -> Url.Counter
let init () : Model * Cmd<Msg> =

    let url = parseUrl (Router.currentUrl ())

    match url with
    | Url.Counter ->
        let (counterModel, cmd) = initCounter ()
        { CurrentPage = Counter counterModel; CurrentUrl = Url.Counter }, cmd
    | Url.TodoList ->
        let (todoListModel, cmd) = initTodoList()
        let model =
            { CurrentPage = TodoList todoListModel; CurrentUrl = Url.TodoList }

        model, (cmd |> Cmd.map TodoListMsg)

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match model.CurrentPage, msg with
    | Counter counterModel, CounterMsg counterMsg ->
        let (newCounterModel, cmd) = updateCounter counterModel counterMsg
        { model with CurrentPage = Counter newCounterModel }, cmd

    | TodoList todoListModel, TodoListMsg todoListMsg ->
        let (newTodoListModel, cmd) = updateTodoList todoListModel todoListMsg
        { model with CurrentPage = TodoList newTodoListModel }, cmd |> Cmd.map TodoListMsg

    | _, UrlChanged url ->
        match url with
        | Url.Counter ->
            let (counterModel, cmd) = initCounter ()
            { CurrentPage = Counter counterModel; CurrentUrl = Url.Counter }, cmd
        | Url.TodoList ->
            let (todoListModel, cmd) = initTodoList()
            let model =
                { CurrentPage = TodoList todoListModel; CurrentUrl = Url.TodoList }

            model, (cmd |> Cmd.map TodoListMsg)


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
    React.router [
        router.onUrlChanged ( parseUrl >> UrlChanged >> dispatch )
        router.children [
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
                                    prop.onClick (fun _ -> Router.navigate("counter"))
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
                        | Counter counterModel ->
                            Bulma.container [
                                Bulma.button.a [
                                    prop.onClick (fun _ -> Router.navigate("/"))
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
        ]
    ]
