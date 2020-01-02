module Db

open FSharp.Data.Sql

[<Literal>]
let ConnectionString = "Server=127.0.0.1; Database=todo_api_db; User Id=suave; Password=1234;"

type Sql = SqlDataProvider<ConnectionString=ConnectionString, DatabaseVendor=Common.DatabaseProviderTypes.POSTGRESQL, CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL>

type DbContext = Sql.dataContext

type Todo = DbContext.``public.todosEntity``
