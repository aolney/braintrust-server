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
module PageTask

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open User
open Sugar

/// Users have login data and ability data
let SetupTaskSchema mongoose = //removed encryption; TODO does keeping make sense bcrypt =

    let qaPairSchema =
        mongoose?Schema(
            !![
                "question" => mongoose?Schema?Types?String
                "answer" => mongoose?Schema?Types?String
            ]
        )

    let tripleSchema =
        mongoose?Schema(
            !![
                "start" => mongoose?Schema?Types?String
                "edge" => mongoose?Schema?Types?String
                "end" => mongoose?Schema?Types?String
            ]
        )

    let pageTaskSchema = 
        mongoose?Schema(
            !![
                "userid" => mongoose?Schema?Types?String
                "abilities" => [|abilitySchema|]
                "questions" => [| qaPairSchema |]
                "gist" => mongoose?Schema?Types?String
                "prediction" => mongoose?Schema?Types?String
                "triples" => [| tripleSchema |]
            ]
    )

    let pageSchema = 
        mongoose?Schema(
            !![
                "url" => mongoose?Schema?Types?String
                "pageid" => mongoose?Schema?Types?Number //TODO: do we need this in general?
                "taskHistory" => [| pageTaskSchema |]
            ]
    )
    //return
    mongoose?model("PageTasks",pageSchema)
