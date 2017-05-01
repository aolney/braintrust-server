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

module Routes 

// #r "../node_modules/fable-core/Fable.Core.dll"
// #load "../node_modules/fable-import-express/Fable.Import.Express.fs"
// #load "sugar.fsx"

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Sugar

type Request = express.Request
type Response = express.Response
let inline toHandler (fn : Request -> Response -> (obj -> unit) -> obj) : express.RequestHandler =
    System.Func<_,_,_,_>(fn) 



let RegisterRoutes ( app : express.Express ) ( passport : obj ) =

    //home page
    app.get
        (   U2.Case1 "/", 
            toHandler <| fun _ res _ -> 
                res.render ("index.ejs", !![]) |> box
        ) |> ignore

    //login
    app.get
        (   U2.Case1 "/login", 
            toHandler <| fun req res _ -> 
                res.render("login.ejs", !! [ "message" => req?flash("loginMessage")]) |> box
        ) |> ignore

    // process the login form
    //This works, so the basic syntax is OK. Seems like passport is broken
    // app.post
    //     ( U2.Case1 "/login", 
    //         toHandler <| fun _ res _ -> 
    //             res.render ("index.ejs", !![]) |> box
    //     ) |> ignore

    app.post
        (   U2.Case1 "/login", 
            passport?authenticate("local-login",
                !! [
                    "successRedirect" => "/profile";
                    "failureRedirect" => "/login";
                    "failureFlash" => true;
                ]
            ) |> unbox<express.RequestHandler>
        ) |> ignore

    //signup
    app.get
        (   U2.Case1 "/signup", 
            toHandler <| fun req res _ -> 
                res.render("signup.ejs", !! [ "message" => req?flash("signupMessage")]) |> box
        ) |> ignore


    // process the signup form
    app.post
        (   U2.Case1 "/signup", 
            passport?authenticate("local-signup",
                !! [
                    "successRedirect" => "/profile";
                    "failureRedirect" => "/signup";
                    "failureFlash" => true;
                ]
            ) |> unbox<express.RequestHandler>
        ) |> ignore

    //profile
    app.get
        (   U2.Case1 "/profile", 
            [|(toHandler <| fun req res next ->
                match req?isAuthenticated() |> unbox<bool> with
                | true -> next$() 
                | false -> res.redirect("/") |> box);
            (toHandler <| fun req res _ -> 
                res.render("profile.ejs", !! [ "user" => req.user]) |> box)|] 
        ) |> ignore

    //logout
    app.get
        (   U2.Case1 "/logout", 
            toHandler <| fun req res _ -> 
                req?logout() |> ignore
                res.redirect("/") |> box
        ) |> ignore



