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

    type Todo = DbContext.``public.todosEntity``

module Queries =
    open Context
    open Types

    let getTodosById id (ctx: DbContext): Todo option =
        query {
            for todo in ctx.Public.Todos do
                where (todo.Id = id)
                select todo
        }
        |> Seq.tryHead
 