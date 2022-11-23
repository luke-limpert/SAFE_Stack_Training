module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Person = 
    {
        Name : string
        Age : int
    }

type Model = 
    { 
        Name : string
        Age : int
        People : Person List
    }

type Msg = // replace with meaningful messages
    | SetName of string
    | SetAge of int
    | AddPerson of Person

let people = [
    {Name = "Jack"; Age = 10}
    {Name = "Jill"; Age = 11}
]

let init () : Model * Cmd<Msg> =
    { Name = ""; Age = 0; People = people}, Cmd.none

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with 
    | SetName name -> 
        { model with
            Name = name }, Cmd.none
    | SetAge age -> 
        { model with
            Age = age }, Cmd.none
    | AddPerson person -> 
        { model with 
            People = person :: model.People }, Cmd.none

open Feliz
open Feliz.Bulma

let horizontalField (label: string) (children: ReactElement list) =
    Bulma.field.div [
        Bulma.field.isHorizontal
        prop.children [
            Bulma.fieldLabel [
                Bulma.fieldLabel.isNormal
                prop.children [
                    Bulma.label [ prop.text label ]
                    ]
                ]
            Bulma.control.div [
                prop.children children
            ]
        ]
    ]

let view (model: Model) dispatch =
    Bulma.section [
        Bulma.field.div [
            Bulma.field.isHorizontal
            prop.children [
                Bulma.notification [
                    Bulma.color.isPrimary
                    prop.text "Notification"
                ]
                Bulma.box [
                    Bulma.columns [
                        Bulma.column [
                            horizontalField "Name" [ 
                                Bulma.input.text [ 
                                    prop.onChange (fun (typedValue: string) -> typedValue |> SetName |> dispatch)] ]
                            horizontalField "Age" [ 
                                Bulma.input.number [
                                    prop.onChange (fun (typedValue: int) -> typedValue |> SetAge |> dispatch)
                                ] ]
                        ]

                        Bulma.column [
                            horizontalField "Date" [ Bulma.input.datetimeLocal [] ]
                            horizontalField "Select" [ 
                                Bulma.select [
                                    Html.option "Option 1"
                                    Html.option "Option 2"
                                ] ]
                        ]
                    ]
                ]
            ]
        ]
        Bulma.button.button [
            if false then Bulma.button.isLoading
            Bulma.color.isPrimary
            prop.children [
                Bulma.icon [
                    Html.i [
                        prop.className "fas fa-user"
                    ]
                ]
                Html.span "Button"
            ]
            prop.onClick (fun _ -> 
                let newPerson = 
                    {
                        Name = model.Name
                        Age = model.Age
                    }
                newPerson |> AddPerson |> dispatch
                )
        ]
        Bulma.table [
            Html.thead [
                Html.tr [
                    Html.th "Name"
                    Html.th "Age"
                ]
            ]
            Html.tbody [
                for person in model.People do
                    Html.tr [
                        Html.td person.Name
                        Html.td person.Age
                    ]
            ]
        ]
    ]