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
    { FirstName : string
      LastName : string
      AddressLine1 : string
      City : string
      PostCode : string
      Person : Person option }

type Msg =
    | SetFirstName of string
    | SetLastName of string
    | SetAddressLine1 of string
    | SetCity of string
    | SetPostCode of string
    | ProvidePersonDetails of Person

let init () : Model * Cmd<Msg> =
    let model =
        { FirstName = ""
          LastName = ""
          AddressLine1 = ""
          City = ""
          PostCode = ""
          Person = None }

    model, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SetFirstName firstName ->
        { model with FirstName = firstName }, Cmd.none
    | SetLastName lastName ->
        { model with LastName = lastName }, Cmd.none
    | SetAddressLine1 addressLine1 ->
        { model with AddressLine1 = addressLine1 }, Cmd.none
    | SetCity city ->
        { model with City = city }, Cmd.none
    | SetPostCode postCode ->
        { model with PostCode = postCode }, Cmd.none
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


let input (label: string) (inputValue: string) (changeHandler: string -> unit) =
    Bulma.field.div [
        Bulma.label label
        Bulma.control.div [
            Bulma.input.text [
                prop.onChange changeHandler
                prop.value inputValue
          ]
        ]
    ]

let form model dispatch =
    Bulma.box [
        prop.children [
            input "First name" model.FirstName (SetFirstName >> dispatch)
            input "Last name" model.LastName (SetLastName >> dispatch)
            input "Address line1" model.AddressLine1 (SetAddressLine1 >> dispatch)
            input "City" model.City (SetCity >> dispatch)
            input "Post code" model.PostCode (SetPostCode >> dispatch)
            Bulma.button.button [
                color.isInfo
                prop.onClick (fun _ ->
                    (model.FirstName, model.LastName, model.AddressLine1, model.City, model.PostCode)
                        |> createPerson
                        |> ProvidePersonDetails
                        |> dispatch)
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
                                form model dispatch
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
