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

module Passport 

// #r "../node_modules/fable-core/Fable.Core.dll"
// #load "sugar.fsx"
// #load "user.fsx"

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Sugar
open User

let passportLocal = importAll<obj> "passport-local"

//let LocalStrategy = passportLocal?Strategy

let SetupPassport passport userApi = 

    // session state maintenance
    // passport?serializeUser <- fun user ``done`` -> ``done``(null, user?id) 

    // passport?deserializeUser <- fun id ``done`` -> userApi?findById( id, fun err user -> ``done``(err,user) )

    passport?serializeUser( fun user ``done`` -> ``done``$(null, user?id) |> ignore ) |> ignore

    passport?deserializeUser( fun id ``done`` -> userApi?findById( id, fun err user -> ``done``$(err,user) |> ignore ) |> ignore ) |> ignore

    //signup local

    passport?``use``(
        "local-signup", 
        createNew passportLocal?Strategy (
            !![
                "usernameField" => "email"
                "passwordField" => "password"
                "passReqToCallback" => true
            ],
            fun req email password _done ->
                //check if user exists
                userApi?findOne( 
                    !![
                        "local.email" => email
                    ],
                    fun err user ->
                        Browser.console.log("checking signup")
                        if err then 
                            _done(err)
                        else if user then
                            _done$( null, false, req?flash("signupMessage", "That email is already taken"))
                        else
                            //create a user
                            let newUser = createNew userApi ()
                            newUser?local?email <- email
                            newUser?local?password <- newUser?generateHash(password)
                            newUser?save( fun err ->
                                if err then 
                                    err |> unbox |> raise
                                else
                                    _done$(null,newUser)
                            ) 
                ) |> ignore
        ) |> ignore
    ) |> ignore
    
    //login local
    passport?``use``(
        "local-login", 
        createNew passportLocal?Strategy (
            !![
                "usernameField" => "email"
                "passwordField" => "password"
                "passReqToCallback" => true
            ],
            fun req email password _done ->
                //check if user exists
                userApi?findOne( 
                    !!["local.email" => email],
                    fun err user ->
                        Browser.console.log("checking login")
                        if err then 
                            _done(err) 
                        else if not <| user then
                            _done$( null, false, req?flash("loginMessage", "User not found")) 
                        else if user?validPassword(password) |> unbox<bool> |> not then
                            _done$( null, false, req?flash("loginMessage", "Invalid password")) 
                        else
                            _done$( null, user ) 
                ) |> ignore
        ) |> ignore
    ) |> ignore

    //return nothing

