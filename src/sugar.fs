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

module Sugar

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
let inline (!!) x = createObj x
let inline (=>) x y = x ==> y

let [<Emit("this")>] jsThis<'T> : 'T = jsNative //not available in my version of fable    

