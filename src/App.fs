﻿// Learn more about F# at http://fsharp.org
open System
open FSharpPlus.Operators
open Suave
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Filters
open Chiron

module Utils = 
    open System.Text
    open Db.Types

    let todoToRecord (todo: DbTodo) =
        { id = todo.Id
          description = todo.Description
          name = todo.Name }
          
    let inline parseJson (input: byte array) = input |> Encoding.UTF8.GetString |> Json.parse |> Json.deserialize
    
    let respondWithTodo = todoToRecord >> Json.serialize >> Json.format >> OK

module App =
    open Utils
    open Db

    let withTodoById f (id): WebPart =
        let ctx = Context.getContext()
        let dbTodo = ctx |> Queries.getTodosById id

        match dbTodo with 
        | Some inner -> f (inner, ctx)
        | None -> id |> sprintf "Cannot find todo with id %i" |> NOT_FOUND 

    let todoById = 
        withTodoById (fun (inner, _) -> respondWithTodo inner)

    let updateTodo =
        withTodoById (fun (todo, dbContext) ->
            fun ctx -> async {
                let body: Types.TodoDetails = parseJson ctx.request.rawForm

                do! Queries.updateTodo todo body dbContext

                return! respondWithTodo todo ctx 
            }) 

    let patchTodo = withTodoById (fun (todo, dbContext) ->
            fun ctx -> async {
                let body: Types.PartialTodoDetails = parseJson ctx.request.rawForm

                do! Queries.patchTodo todo body dbContext

                return! respondWithTodo todo ctx
            })

    let deleteTodo = withTodoById (fun (todo, dbContext) ->
            fun ctx -> async {
                do! Queries.deleteTodo todo dbContext

                return! respondWithTodo todo ctx
            })

    let listTodos _ = 
        Context.getContext() 
        |> Queries.getAllTodos 
        |>> todoToRecord 
        |> Json.serialize 
        |> Json.format 
        |> OK

    let createTodo ctx = async {
            let dbContext = Context.getContext()
            let details: Types.TodoDetails = ctx.request.rawForm |> parseJson
            
            let! todo = Queries.createTodo details dbContext

            return! respondWithTodo todo ctx 
        }

    let mainWebPart: WebPart = choose [
        pathScan "/todos/%i" (fun (id) -> choose [
            GET >=>  todoById id
            PUT >=>  updateTodo id
            PATCH >=> patchTodo id
            DELETE >=> deleteTodo id
            ])
        path "/todos/" >=> choose [
            GET >=> warbler listTodos
            POST >=> createTodo
        ]]

[<EntryPoint>]
let main _ =

    let handleErrors (e: Exception) (message: string): WebPart = 
        sprintf "%s: %s" message e.Message |> BAD_REQUEST  
    let config = { defaultConfig with errorHandler = handleErrors }

    startWebServer config App.mainWebPart

    0 // return an integer exit code
