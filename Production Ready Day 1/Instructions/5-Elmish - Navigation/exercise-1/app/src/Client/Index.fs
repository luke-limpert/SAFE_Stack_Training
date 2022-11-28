module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model =
    { Todos: Todo List; Input: string; Count: int }


type Msg =
    | Increment
    | Decrement
    | Reset
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo


let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () =
    let model = { Todos = []; Input = ""; Count = 0 }
    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos

    model, cmd


let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
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

    | Increment -> { model with Count = model.Count + 1 }, Cmd.none
    | Decrement -> { model with Count = model.Count - 1 }, Cmd.none
    | Reset -> { model with Count = 0 }, Cmd.none


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


let counterView model dispatch =
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

let todoListView model dispatch =
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
                            Bulma.column [
                                prop.children [
                                    Bulma.title [
                                        text.hasTextCentered
                                        prop.text "Todo app"
                                    ]
                                    todoListView model dispatch
                                ]
                            ]
                        ]
                        Bulma.container [
                            Bulma.column [
                                prop.children [
                                    Bulma.title [
                                        text.hasTextCentered
                                        prop.text "Counter app"
                                    ]
                                    counterView model dispatch
                                ]
                            ]
                        ]
                    ]
                ]
            ]
