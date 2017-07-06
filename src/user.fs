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

// #r "../node_modules/fable-core/Fable.Core.dll"
// #load "sugar.fsx"
module User

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Sugar

/// Users have login data and ability data
let SetupUserSchema mongoose bcrypt =
    let abilitySchema =
        mongoose?Schema(
            !![
                "name" => mongoose?Schema?Types?String
                "score" => mongoose?Schema?Types?Number
            ]
        )
    let userSchema = 
        mongoose?Schema(
            !![
                "abilities" => [|abilitySchema|] //not sure if this is the correct array syntax
                "local" => 
                    !![
                        "email" => mongoose?Schema?Types?String
                        "password" => mongoose?Schema?Types?String
                    ];
                "facebook" => 
                    !![
                        "id" => mongoose?Schema?Types?String
                        "token" => mongoose?Schema?Types?String
                        "email" => mongoose?Schema?Types?String
                        "name" => mongoose?Schema?Types?String  
                    ];   
                "twitter" => 
                    !![
                        "id" => mongoose?Schema?Types?String
                        "token" => mongoose?Schema?Types?String
                        "displayName" => mongoose?Schema?Types?String
                        "username" => mongoose?Schema?Types?String  
                    ];   
                "google" => 
                    !![
                        "id" => mongoose?Schema?Types?String
                        "token" => mongoose?Schema?Types?String
                        "email" => mongoose?Schema?Types?String
                        "name" => mongoose?Schema?Types?String  
                    ];     
            ]
    )

    // generating a hash
    userSchema?methods?generateHash <- fun password -> bcrypt?hashSync(password, bcrypt?genSaltSync(8), null)
    
    // checking if password is valid
    userSchema?methods?validPassword <- fun password -> bcrypt?compareSync(password, jsThis?local?password)

    //return
    mongoose?model("User",userSchema)
