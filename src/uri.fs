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

//set of tasks associated with uri
module Uri

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open User
open Sugar

//Records for each task for serialization/deserialization
type QAPair =
    {
        question : string;
        answer : string;
    }

type Triple =
    {
        start : string;
        edge : string;
        ``end`` : string;
    }

type Ability =
    {
        description : string;
        score : float;
    }

// type TaskSet =
//     {
//         user : string;
//         abilities : Ability array
//         questions : QAPair array
//         gist : string
//         prediction : string
//         triples : Triple array
//     }

/// Users have login data and ability data
let SetupUriSchema mongoose = 

    let qaPair =
        !![
            "question" => mongoose?Schema?Types?String;
            "answer" => mongoose?Schema?Types?String;
        ]
        

    let triple =
        !![
            "start" => mongoose?Schema?Types?String;
            "edge" => mongoose?Schema?Types?String;
            "end" => mongoose?Schema?Types?String;
        ]
   
    //see comments in User
    let ability = SetupAbility mongoose

    //A set of tasks for this Uri plus id and abilities of user who generated these tasks
    let taskSet = 
        !![
            "user" => 
                !![
                    "type" => mongoose?Schema?Types?ObjectId;
                    "ref" => "User";
                ]
            "abilities" => [| ability |];
            "questions" => [| qaPair |];
            "gist" => mongoose?Schema?Types?String;
            "prediction" => mongoose?Schema?Types?String;
            "triples" => [| triple |];
        ]
    

    let uriSchema = 
        mongoose?Schema(
            !![
                "uri" => mongoose?Schema?Types?String
                "taskHistory" => [| taskSet |]
            ]
    )
    //return
    mongoose?model("Uri",uriSchema)
