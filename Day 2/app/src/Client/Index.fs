module Index

open Elmish
open Fable.Remoting.Client
open Shared

open System

open Feliz
open Feliz.Bulma

let personApi = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IPersonApi>

type Model =
    { 
        Name : string
        Age : int
        People : Person list 
        Loading : bool
        Saving : bool
    }
    member this.Validate =
      match this with
      | { Name = "" } -> Error ("Name Missing", Bulma.color.isDanger)
      | { Age = age } when age < 1 -> Error ("No Age", Bulma.color.isDanger)
      | { Age = age } when age < 18 -> Error ("Too young", Bulma.color.isWarning)
      | _ -> Ok ()

type Msg =
    | SetName of string
    | SetAge of int
    | SavePerson
    | LoadAllPeople
    | PeopleLoaded of Person list
    | PersonSaved of unit

let init () : Model * Cmd<Msg> =
    let model = 
        { 
            Name = ""
            Age = 0
            People = []
            Loading = false
            Saving = false 
        }
    model, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SetName name ->
        let newModel = { model with Name = name }
        newModel, Cmd.none
    | SetAge age ->
        let newModel = { model with Age = age }
        newModel, Cmd.none
    | SavePerson ->
        let cmd = Cmd.OfAsync.perform personApi.SavePerson ({ Name = model.Name; Age = model.Age}) PersonSaved
        { model with 
            Saving = true }, cmd
    | LoadAllPeople ->
        let cmd = Cmd.OfAsync.perform personApi.GetPeople () PeopleLoaded
        { model with 
            Loading = true }, cmd
    | PeopleLoaded people -> 
        { model with 
            People = people; Loading = false}, Cmd.none
    | PersonSaved _ -> 
        { model with 
            Saving = false
        }, Cmd.ofMsg LoadAllPeople


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
            Bulma.fieldBody [
                 Bulma.field.div [
                    Bulma.control.div [
                        prop.children children
                    ]
                 ]
            ]
        ]
    ]


let view (model : Model) (dispatch : Msg -> unit) =
    Bulma.section [
        let validationText, validationColor =
            match model.Validate with
            | Ok () -> "Valid form", Bulma.color.isSuccess
            | Error (text, color) -> text, color
        Bulma.notification [
            validationColor
            prop.text validationText
        ]
        Bulma.box [
            Bulma.columns [
                Bulma.column [
                    horizontalField "Name" [ Bulma.input.text [ prop.value model.Name; prop.onChange (SetName >> dispatch) ] ]
                ]
                Bulma.column [
                    horizontalField "Age" [ Bulma.input.number [ prop.value (string model.Age); prop.onChange(fun (e: string) -> dispatch (SetAge (int e))) ] ]
                ]
            ]
            Bulma.button.button [
                if model.Saving 
                then Bulma.button.isLoading
                else Bulma.color.isPrimary
                prop.onClick (fun _ -> dispatch SavePerson)
                prop.disabled (String.IsNullOrWhiteSpace model.Name || model.Age < 1)
                prop.children [
                    Bulma.icon [ Html.i [ prop.className "fas fa-user" ] ]
                    Html.span "Save Person"
                ]
            ]
        ]
        Bulma.box [
            Bulma.button.button [
                if model.Loading 
                then Bulma.button.isLoading
                else Bulma.color.isPrimary
                prop.onClick (fun _ -> dispatch LoadAllPeople)
                prop.children [
                    Bulma.icon [ Html.i [ prop.className "fas fa-user" ] ]
                    Html.span "Load all people"
                ]
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
                            Html.td [ prop.text person.Name ]
                            Html.td [ prop.text person.Age ]
                        ]
                ]
            ]
        ]
    ]

