// Learn more about F# at http://fsharp.org

open Suave
open Suave.Successful

[<EntryPoint>]
let main argv =
    
    startWebServer defaultConfig (OK "hello world")

    0 // return an integer exit code
