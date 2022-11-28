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

    [<ReactComponent>]
    let Counter () =
        let model, dispatch = React.useElmish (initCounter, updateCounter, [| |])

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


    [<ReactComponent>]
    let Todo () =
        let model, dispatch = React.useElmish (initTodoList, updateTodoList, [| |])
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
                                prop.onChange (SetInput >> dispatch)
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
    | CounterPage
    | TodoList

type Model =
    { CurrentPage: Page }


type Msg =
    | ChangeToCounter
    | ChangeToTodoList


let init () : Model * Cmd<Msg> =
    let model = { CurrentPage = TodoList }

    model, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match model.CurrentPage, msg with
    | _, ChangeToTodoList ->
        let model =
            { CurrentPage = TodoList }
        model, Cmd.none

    | _, ChangeToCounter ->
        let model =
            { CurrentPage = CounterPage }
        model, Cmd.none

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
                | TodoList ->
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
                                Todo ()
                            ]
                        ]
                    ]
                | CounterPage ->
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
                                Counter ()
                            ]
                        ]
                    ]
            ]
        ]
    ]
