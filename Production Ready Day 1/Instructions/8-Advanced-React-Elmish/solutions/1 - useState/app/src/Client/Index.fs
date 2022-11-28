module Index

open Elmish
open System

type Address =
    { AddressLine1 : string
      City : string
      PostCode: string }

type Person =
    { FirstName : string
      LastName : string
      Address : Address }

type Model =
    { Person : Person option }

type Msg =
    | ProvidePersonDetails of Person

let init () : Model * Cmd<Msg> =
    let model =
        { Person = None }

    model, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | ProvidePersonDetails person ->
        { model with Person = Some person }, Cmd.none

let createPerson (firstName, lastName, addressLine1, city, postcode) =
    { FirstName = firstName
      LastName = lastName
      Address =
        { AddressLine1 = addressLine1
          City = city
          PostCode = postcode } }

open Feliz
open Feliz.Bulma

let input (label: string) (changeHandler: string -> unit) (inputValue: string) =
    Bulma.field.div [
        Bulma.label label
        Bulma.control.div [
            Bulma.input.text [
                prop.onChange changeHandler
                prop.value inputValue
          ]
        ]
    ]

[<ReactComponent>]
let Form (props: {| SubmitPerson : Person -> unit |}) =
    let firstName, setFirstName = React.useState ""
    let lastName, setLastName = React.useState ""
    let addressLine1, setAddressLine1 = React.useState ""
    let city, setCity = React.useState ""
    let postCode, setPostCode = React.useState ""

    let handleSubmit _ =
        { FirstName = firstName
          LastName = lastName
          Address = {
              AddressLine1 = addressLine1
              City = city
              PostCode = postCode }
        } |> props.SubmitPerson


    Bulma.box [
        prop.children [
            input "First name" setFirstName firstName
            input "Last name" setLastName lastName
            input "Address line 1" setAddressLine1 addressLine1
            input "City" setCity city
            input "Post code" setPostCode postCode
            Bulma.button.button [
                color.isInfo
                prop.onClick handleSubmit
                prop.text "Submit"
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
            Bulma.heroBody [
                Bulma.container [
                    Bulma.columns [
                        Bulma.column [
                            column.is5
                            prop.children [
                                Bulma.title [
                                    text.hasTextCentered
                                    prop.text "Form"
                                ]
                                Form {| SubmitPerson = ProvidePersonDetails >> dispatch |}
                            ]
                        ]
                        Bulma.column [
                            column.is7
                            prop.children [
                                Bulma.title [
                                    text.hasTextCentered
                                    prop.text "Submitted person"
                                ]
                                Bulma.box [
                                    prop.children [
                                        match model.Person with
                                        | None ->
                                            Html.div "No person details provided"
                                        | Some person ->
                                            Bulma.table [
                                                Html.thead [
                                                    Html.tr [
                                                        Html.th "First name"
                                                        Html.th "Last name"
                                                        Html.th "Address Line"
                                                        Html.th "City"
                                                        Html.th "Post code"
                                                    ]
                                                ]
                                                Html.tbody [
                                                    Html.tr [
                                                        Html.td person.FirstName
                                                        Html.td person.LastName
                                                        Html.td person.Address.AddressLine1
                                                        Html.td person.Address.City
                                                        Html.td person.Address.PostCode
                                                    ]
                                                ]
                                            ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
