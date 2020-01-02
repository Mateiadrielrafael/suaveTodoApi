// Learn more about F# at http://fsharp.org

open Suave
open Suave.Successful
open Suave.Operators
open Suave.Filters
open Suave.RequestErrors

module App =
    let todoById (id) = 
        let todo = Db.Context.getContext() |> Db.Queries.getTodosById id

        match todo with 
        | Some inner -> OK <| sprintf "%A" inner.Name
        | None -> id |> sprintf "Cannot find todo with id %i" |> NOT_FOUND 

    let mainWebPart: WebPart = choose [
        GET >=> choose [
            pathScan "/todos/%i" todoById
        ]]

[<EntryPoint>]
let main _ =
    startWebServer defaultConfig App.mainWebPart

    0 // return an integer exit code
