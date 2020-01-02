// Learn more about F# at http://fsharp.org
open System

// suave overwrites some stuff from f#+, so the order matters
open FSharpPlus.Operators
open Suave
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Json
open Suave.Filters

module Utils = 
    open System.Text
    open Db.Types

    let jsonToString json = json |> toJson |> Encoding.UTF8.GetString

    let todoToRecord (todo: DbTodo) =
        { id = todo.Id
          description = todo.Description
          name = todo.Name }

module App =
    open Utils
    open Db

    let withTodoById f (id): WebPart =
        let ctx = Context.getContext()
        let dbTodo = ctx |> Queries.getTodosById id

        match dbTodo with 
        | Some inner -> f (inner, ctx, id)
        | None -> id |> sprintf "Cannot find todo with id %i" |> NOT_FOUND 

    let todoById = 
        (fun (inner, _, _) -> inner |> todoToRecord |> jsonToString |> OK)  |> withTodoById

    let updateTodo =
        (fun (todo, dbContext, id) ->
            fun ctx -> async {
                    let body: Types.TodoDetails = ctx.request.rawForm |> fromJson

                    do! Queries.updateTodosById todo body dbContext

                    let newBody: Types.Todo = {
                        name = body.name
                        description = body.description
                        id = id
                    } 

                    let withNewBody = newBody |> toJson |> ok
                    return! withNewBody ctx
                }        
        ) |> withTodoById

    let mainWebPart: WebPart = choose [
        GET >=> pathScan "/todos/%i" todoById
        PUT >=> pathScan "/todos/%i" updateTodo]

[<EntryPoint>]
let main _ =

    let handleErrors (e: Exception) (message: string): WebPart = 
        sprintf "%s: %s" message e.Message |> BAD_REQUEST  
    let config = { defaultConfig with errorHandler = handleErrors }

    startWebServer config App.mainWebPart

    0 // return an integer exit code
