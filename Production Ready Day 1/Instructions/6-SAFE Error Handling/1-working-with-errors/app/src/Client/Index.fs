module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Elmish.SweetAlert
open Fable.SimpleJson

type Model = { Todos: Todo list; Input: string; InputError : AddTodoError option }

type Msg =
    | GetTodos
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

type System.Exception with
    member this.GetPropagatedError() =
        match this with
        | :? ProxyRequestException as exn ->
            let response = exn.ResponseText |> Json.parseAs<{| error:string; ignored : bool; handled : bool |}>
            response.error
        | ex ->
            ex.Message

let init () =
    let model = { Todos = []; Input = ""; InputError = None }
    model, Cmd.ofMsg GetTodos

let update msg model =
    match msg with
    | GetTodos ->
        { model with Todos = [] }, Cmd.OfAsync.perform todosApi.getTodos () GotTodos
    | GotTodos todos ->
        { model with Todos = todos }, Cmd.none
    | SetInput value ->
        { model with Input = value; InputError = None }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input
        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo
        model, cmd
    | AddedTodo todo ->
        { model with Todos = model.Todos @ [ todo ] }, Cmd.none

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

let containerBox model dispatch =
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
                            if model.InputError.IsSome then color.isDanger
                        ]

                        match model.InputError with
                        | Some error ->
                            Bulma.help [
                                color.isDanger
                                prop.text error.Description
                            ]
                        | None ->
                            ()
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.onClick (fun _ -> dispatch AddTodo)
                        prop.text "Add"
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isInfo
                        prop.onClick (fun _ -> dispatch GetTodos)
                        prop.text "Refresh"
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
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "Working with Errors!"
                            ]
                            containerBox model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]
