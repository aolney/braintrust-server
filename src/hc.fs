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

//Human computation functions
module Hc

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Uri



let SelectFromTaskSets (taskSets : TaskSet[])=
//Consider both consensus and individual skill
    let gistHash = new System.Collections.Generic.Dictionary<string,float>()
    let predictionHash = new System.Collections.Generic.Dictionary<string,float>()
    let tripleHash = new System.Collections.Generic.Dictionary<Triple,float>()

    //For identical elements, HC score is product of skills that produced element
    //Example: "hi"-8, "hi"-2 = "hi"-16
    let weightedFreq  (hash:System.Collections.Generic.Dictionary<'a,float>) key score =
        match hash.TryGetValue(key)  with
        | true, value -> hash.[key] <- value * score
        | false, _ -> hash.Add( key, score)

    //From all listed abilities, derive a single score. TODO replace with real abilities
    let compositeAbility (abilities : Ability array) =
        let abilityMap = abilities |> abilityArrayToMap
        match abilityMap.TryFind "test" with //using fake "test" ability; real abilities would be based on reading comprehension and motivation
        | Some(value) -> value
        | None -> 1.0

    
    for taskSet in taskSets do
        let abilityScore = taskSet.abilities |> compositeAbility
        weightedFreq gistHash taskSet.gist abilityScore
        weightedFreq predictionHash taskSet.prediction abilityScore
        //triples are a bit interesting; we could try to combine triples across taskSets or 
        //just select the taskSet with the best triples or do both, e.g. take the triples from the best 
        //taskSet but then add back the best triple not in that taskset
        for triple in taskSet.triples do
            weightedFreq tripleHash triple abilityScore

    //We need another pass with weighted triples to get taskSet.triples scores
    let maxScore,maxTriples =
        taskSets
        |> Seq.map( fun taskSet -> 
            let tripleScore = taskSet.triples |> Seq.sumBy( fun triple -> tripleHash.[triple])
            tripleScore,taskSet.triples
        )
        |> Seq.maxBy fst

    //Remove maxTriples from tripleHash so we can select some non-included triples
    maxTriples |> Seq.iter( fun t -> tripleHash.Remove(t) |> ignore)

    //get best of what's left
    let sortedTriples = tripleHash |> Seq.sortByDescending( fun (KeyValue(k,v)) -> v ) 

    //Add some of these; TODO need a criteria for # to take
    let sortedLength = sortedTriples |> Seq.length
    let takeCount =
        if sortedLength >= 2 then 
            2
        else if sortedLength <> 0 then
            sortedLength - 1
        else
            0
    let finalTriples = Array.append maxTriples (sortedTriples |> Seq.take takeCount |> Seq.map(fun (KeyValue(k,v)) -> k) |> Seq.toArray )
    
    let finalGistPair = gistHash |> Seq.maxBy( fun (KeyValue(k,v)) -> v )
    let finalPredictionPair = predictionHash |> Seq.maxBy( fun (KeyValue(k,v)) -> v )
    
    //all questions should be the same, only answers change so we strip out answers
    //we don't care about questions from an hc perspective
    let finalQuestions = taskSets.[0].questions |> Array.map( fun qa -> {qa with answer=""})

    //return a taskset that is the product of this HC; user values are empty on purpose
    {
        user="";
        abilities=[||];
        questions=finalQuestions
        gist = finalGistPair.Key;
        prediction = finalPredictionPair.Key;
        triples = finalTriples;
    }

//We have nothing, so ask the user to do everything. TODO invoke AI here (would be for non-NEETS)
let EmptyTaskSet() =
    //return a taskset that is the product of this HC; user values are empty on purpose
    {
        user="";
        abilities=[||];
        questions=[||];
        gist = "";
        prediction = "";
        triples = [||];
    }

let GetTaskSetForHumanComputation uri user =
    match uri with
    | None -> EmptyTaskSet()
    | Some (u) ->
        let taskSets = u?taskHistory |> unbox<TaskSet[]>

        //We can do HC if we have taskSets; if not we punt
        if taskSets.Length > 0 then
            SelectFromTaskSets taskSets
        else
            EmptyTaskSet()