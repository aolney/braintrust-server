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


let IsLoggedIn req res next = 
        match req?isAuthenticated() |> unbox<bool> with
        | true -> next$() 
        | false -> res?redirect("/") 

let RegisterRoutes ( app : express.Express ) ( passport : obj ) =

    //---------------
    // Human computation API 
    // NOTE: could be a separate application, but no reason to split off right now; needs authentication
    //---------------
    //add tasks (create if not exist, append if exist)
    app.post
        (   U2.Case1 "/tasks/:uri", 
            //if they are not logged in, force them to log in?
            toHandler <| fun req res _ -> 
                match req?isAuthenticated() |> unbox<bool> with
                | true -> 
                    // PageTasks.findById( 
                    //     !![
                    //         "page_id" => req.params?page_id
                    //     ],
                    //     fun err pageTask ->
                    //         () //we want to append to the list of tasks for this page
                    //         pageTask.save( fun err -> if err then res.send( err ))
                    //         res.json( !! [ "message" => "tasks updated"]) |> box
                    //     )
                    res.json( !! [ "message" => "TODO get task database response"]) |> box
                | false -> res.json ( !![ "message" => "please log in"]) |> box

        ) 
        |> ignore

    //---------------
    // General log in
    //---------------
    //home page -- if they are already logged in, we force them to the profile page
    app.get
        (   U2.Case1 "/", 
            toHandler <| fun req res _ -> 
                match req?isAuthenticated() |> unbox<bool> with
                | true -> res.render("profile.ejs", !! [ "user" => req.user]) |> box
                | false -> res.render ("index.ejs", !![]) |> box
        ) |> ignore
    // app.get
    //     (   U2.Case1 "/", 
    //         toHandler <| fun _ res _ -> 
    //             res.render ("index.ejs", !![]) |> box
    //     ) |> ignore


    //profile
    app.get
        (   U2.Case1 "/profile", 
            [| 
                (toHandler <| IsLoggedIn );
                (toHandler <| fun req res _ -> 
                    res.render("profile.ejs", !! [ "user" => req.user]) |> box)
            |] 
        ) |> ignore

    //logout
    app.get
        (   U2.Case1 "/logout", 
            toHandler <| fun req res _ -> 
                req?logout() |> ignore
                res.redirect("/") |> box
        ) |> ignore

    //------------
    // First login
    //------------
    //local login
    app.get
        (   U2.Case1 "/login", 
            toHandler <| fun req res _ -> 
                res.render("login.ejs", !! [ "message" => req?flash("loginMessage")]) |> box
        ) |> ignore

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

    //local signup
    app.get
        (   U2.Case1 "/signup", 
            toHandler <| fun req res _ -> 
                res.render("signup.ejs", !! [ "message" => req?flash("signupMessage")]) |> box
        ) |> ignore


    // process the local signup form
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


    // GOOGLE
    //authenticate with google
    app.get
        (   U2.Case1 "/auth/google", 
            passport?authenticate("google",
                !! [
                    "scope" => [|"profile";"email"|];
                ]
            ) |> unbox<express.RequestHandler>
        ) |> ignore

    //google callback after authentication
    app.get
        (   U2.Case1 "/auth/google/callback", 
            passport?authenticate("google",
                !! [
                    "successRedirect" => "/profile";
                    "failureRedirect" => "/";
                ]
            ) |> unbox<express.RequestHandler>
        ) |> ignore



    // =============================================================================
    // AUTHORIZE (ALREADY LOGGED IN / CONNECTING OTHER SOCIAL ACCOUNT) =============
    // =============================================================================

    // locally --------------------------------
    app.get
        ( U2.Case1 "/connect/local", 
            toHandler <| fun req res _ -> 
                res.render("connect-local.ejs", !! [ "message" => req?flash("loginMessage")]) |> box
        ) |> ignore
    app.post
        (   U2.Case1 "/connect/local", 
            passport?authenticate("local-signup",  //weird this is authenticate not authorize?
                !! [
                    "successRedirect" => "/profile";
                    "failureRedirect" => "/connect/local";
                    "failureFlash" => true;
                ]
            ) |> unbox<express.RequestHandler>
        ) |> ignore
    

    // google --------------------------------
    app.get
        (   U2.Case1 "/connect/google", 
            passport?authorize("google",
                !! [
                    "scope" => [|"profile";"email"|];
                ]
            ) |> unbox<express.RequestHandler>
        ) |> ignore

    //google callback
    app.get
        (   U2.Case1 "/connect/google/callback", 
            passport?authorize("google",
                !! [
                    "successRedirect" => "/profile";
                    "failureRedirect" => "/";
                ]
            ) |> unbox<express.RequestHandler>
        ) |> ignore
// =============================================================================
// UNLINK ACCOUNTS =============================================================
// =============================================================================
// used to unlink accounts. for social accounts, just remove the token
// for local account, remove email and password
// user account will stay active in case they want to reconnect in the future

    app.get
        (   U2.Case1 "/unlink/local", 
            [| 
                (toHandler <| IsLoggedIn );
                (toHandler <| fun req res _ -> 
                    let user = req?user
                    user?local?email <- ()
                    user?local?password <- ()
                    user?save( fun err ->
                        res.redirect("/profile") |> box) )
            |] 
        ) |> ignore

    app.get
        (   U2.Case1 "/unlink/google", 
            [| 
                (toHandler <| IsLoggedIn );
                (toHandler <| fun req res _ -> 
                    let user = req?user
                    user?google?token <- ()
                    user?save( fun err ->
                        res.redirect("/profile") |> box) )
            |] 
        ) |> ignore