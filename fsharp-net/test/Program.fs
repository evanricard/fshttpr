// Learn more about F# at http://fsharp.org

#r gitpod-fsharp-template\test\bin\Debug\netcoreapp3.1\FSharp.Data.dll

open System
open FSharp.Data

type RaceType = JsonProvider<"./races.json">
type AllRacesType = JsonProvider<"./all-races.json">

[<EntryPoint>]
let main argv =
    let allRaces = AllRacesType.GetSample()
    //printfn "%A" allRaces
    
    allRaces.Results |> Array.map (fun race -> race.Name) |> Array.iter (printfn "%A")

    let allRaceUrls = allRaces.Results |> Array.map (fun race -> race.Name, "http://www.dnd5eapi.co" + race.Url)
    
    let getAndPrintSpeed (name, url) =
        let docAsync = 
            RaceType.AsyncLoad(url) |> Async.RunSynchronously
        printfn "Name: %s, Speed: %i" name docAsync.Speed

    allRaceUrls |> Array.iter getAndPrintSpeed

    printfn "Hello World from F#!"
    let racesample = RaceType.GetSample()

    let wbReq = 
        "http://www.dnd5eapi.co/api/races/half-orc" + 
        ""

    let docAsync = 
        RaceType.AsyncLoad(wbReq) |> Async.RunSynchronously

    printfn "%A" docAsync

    let speed = docAsync.Speed

    0 // return an integer exit code
