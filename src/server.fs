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

module Server

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Sugar

//import node modules
let app = express.Invoke()
let mongoose = importAll<obj> "mongoose"
let bcrypt = importAll<obj> "bcrypt-nodejs"
let passport = importAll<obj> "passport"
let connectFlash = importAll<obj> "connect-flash"
let morgan = importAll<obj> "morgan"
let cookieParser = importAll<obj> "cookie-parser"
let bodyParser = importAll<obj> "body-parser"
let session = importAll<obj> "express-session"

//acquire database
mongoose?connect(Auth.mongoConnection) |> ignore

//Setup userSchema
let user = User.SetupUserSchema mongoose bcrypt
// let res = 
//   user?findOne( 
//     !![
//         "local.email" => "fart@yourmom.com"
//     ],
//     fun err user -> Browser.console.log("found")
//   )

//configure passport
Passport.SetupPassport passport user |> ignore

//set up express
app.``use``( morgan$("dev") |> unbox<express.RequestHandler> ) |> ignore
app.``use``( cookieParser$() |> unbox<express.RequestHandler> ) |> ignore
app.``use``( bodyParser?json() |> unbox<express.RequestHandler> ) |> ignore
app.``use``( bodyParser?urlencoded( !![ "extended" => true ]) |> unbox<express.RequestHandler> ) |> ignore

app.set( "view engine", "ejs") |> ignore

app.``use``( session$( !![ "secret" => "booboo" ] ) |> unbox<express.RequestHandler> ) |> ignore
app.``use``( passport?initialize$() |> unbox<express.RequestHandler> ) |> ignore
app.``use``( passport?session$() |> unbox<express.RequestHandler> ) |> ignore
app.``use``( connectFlash$() |> unbox<express.RequestHandler> ) |> ignore

Routes.RegisterRoutes app passport

// Get PORT environment variable or use default
let port =
  match unbox Node.Globals.``process``.env?PORT with
  | Some x -> x 
  | None -> 8080

// Start the server on the port
app.listen(port, unbox (fun () ->
  printfn "BrainTrust server started: http://localhost:%i/" port)) |> ignore

//start docker interactive shell
//dotnet restore sln (not persisted) or persisted
//dotnet restore --no-cache --packages .nuget/packages BrainTrustServer.sln
//dotnet fable npm-run watch

//only if ports are mapped in docker (like you might for webpack-dev-server) in a new terminal
//docker exec -it bash into new docker shell

//in either case to run server so
//npm run start
//use launch script from vscode

//sudo service mongod start
//yarn
//dotnet restore
//dotnet fable npm-run build (or watch)
//dotnet fable npm-run start (starts server)
