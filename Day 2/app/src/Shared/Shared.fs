namespace Shared

open System

type Person =
    { 
        Name : string
        Age : int 
    }

type IPersonApi = 
    { 
        GetPeople : unit -> Async<Person list>
        SavePerson : Person -> Async<unit>
    }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

