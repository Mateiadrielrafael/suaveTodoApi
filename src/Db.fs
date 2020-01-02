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
    open System.Runtime.Serialization

    type DbTodo = DbContext.``public.todosEntity``


    [<DataContract>]
    type Todo =
        { [<field:DataMember(Name = "id")>]
          id: int
          [<field:DataMember(Name = "description")>]
          description: string
          [<field:DataMember(Name = "name")>]
          name: string }


    [<DataContract>]
    type TodoDetails =
        { [<field:DataMember(Name = "description")>]
          description: string
          [<field:DataMember(Name = "name")>]
          name: string }



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


    let updateTodosById (todo: DbTodo) (details: TodoDetails) (ctx: DbContext) =
        todo.Name <- details.name
        todo.Description <- details.description

        ctx.SubmitUpdatesAsync()
 