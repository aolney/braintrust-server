(*Copyright 2017 Andrew M. Olney

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*)

#r "../node_modules/fable-core/Fable.Core.dll"
#load "../node_modules/fable-import-express/Fable.Import.Express.fs"

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import

let app = express.Invoke()

// Handle request using plain `string` route specification
app.get
  ( U2.Case1 "/hello/:name", 
    fun (req:express.Request) (res:express.Response) _ ->
      res.send(sprintf "Goodbye %O" req.``params``?name) |> box)
|> ignore

// Get PORT environment variable or use default
let port =
  match unbox Node.``process``.env?PORT with
  | Some x -> x | None -> 8080

// Start the server on the port
app.listen(port, unbox (fun () ->
  printfn "Server started: http://localhost:%i/" port))
|> ignore

