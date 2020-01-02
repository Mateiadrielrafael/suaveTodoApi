module Db

open FSharp.Data.Sql

[<Literal>]
let ConnectionString = "Server=127.0.0.1; Database=todo_api_db; User Id=suave; Password=1234;"

type Sql = SqlDataProvider<ConnectionString=ConnectionString, DatabaseVendor=Common.DatabaseProviderTypes.POSTGRESQL, CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL>

module Context =
    type DbContext = Sql.dataContext

    let getContext() = Sql.GetDataContext()


module Types =
    open Context
    open Chiron

    type DbTodo = DbContext.``public.todosEntity``


    type Todo =
        { id: int
          description: string
          name: string }

        static member ToJson(todo: Todo) =
            json {
                do! Json.write "name" todo.name
                do! Json.write "description" todo.description
                do! Json.write "id" todo.id
            }

        static member FromJson(_: Todo) =
            json {
                let! name = Json.read "name"
                let! description = Json.read "description"
                let! id = Json.read "id"

                return { name = name
                         description = description
                         id = id }
            }

    type TodoDetails =
        { description: string
          name: string }
        static member FromJson(_: TodoDetails) =
            json {
                let! name = Json.read "name"
                let! description = Json.read "description"
                return { name = name
                         description = description }
            }

    type PartialTodoDetails =
        { description: string option
          name: string option }
        static member FromJson(_: PartialTodoDetails) =
            json {
                let! name = Json.tryRead "name"
                let! description = Json.tryRead "description"
                return { name = name
                         description = description }
            }


module Queries =
    open Context
    open Types

    let getTodosById id (ctx: DbContext): DbTodo option =
        query {
            for todo in ctx.Public.Todos do
                where (todo.Id = id)
                select todo
        }
        |> Seq.tryHead

    let getAllTodos (ctx: DbContext): DbTodo list =
        query {
            for todo in ctx.Public.Todos do
                select todo
        }
        |> Seq.toList

    let updateTodo (todo: DbTodo) (details: TodoDetails) (ctx: DbContext) =
        todo.Name <- details.name
        todo.Description <- details.description

        ctx.SubmitUpdatesAsync()

    let patchTodo (todo: DbTodo) (details: PartialTodoDetails) (ctx: DbContext) =
        Option.iter (fun name -> todo.Name <- name) details.name
        Option.iter (fun description -> todo.Description <- description) details.description

        if Option.orElse details.name details.description |> Option.isSome
        then ctx.SubmitUpdatesAsync()
        else Async.result()

    let deleteTodo (todo: DbTodo) (ctx: DbContext) =
        todo.Delete()

        ctx.SubmitUpdatesAsync()

    let createTodo (details: TodoDetails) (ctx: DbContext) =
        async {
            let todo = ctx.Public.Todos.Create()

            do! updateTodo todo details ctx

            return todo
        }
 