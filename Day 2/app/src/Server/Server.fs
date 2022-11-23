module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Bogus
open Shared

let faker = Faker "en"
let generatePerson () = 
    {
        Name = faker.Name.FirstName()
        Age = faker.Random.Int(18,35)
    }

let people = ResizeArray()
let save (person : Person) = async {
    people.Add person
}

let getFakePeople () = async {
    return [
        for i in 1 .. 10 do generatePerson ()
    ]
}

let getRealPeople () = async {
    return people |> List.ofSeq
}

let personApi = 
    {
        GetPeople = getRealPeople
        SavePerson = save 
    }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue personApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
