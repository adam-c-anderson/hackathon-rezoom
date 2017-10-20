open Rezoom
open Rezoom.SQL
open Rezoom.PlanBuilder
open Rezoom.SQL.Plans
open Rezoom.BatchHints
open Rezoom.SQL.Synchronous
open Rezoom.Execution
open FSharp.Control.Tasks.ContextSensitive

[<EntryPoint>]
let main argv =
    Migration.migrate()

    //let oloId = Population.insertEmployer "Olo"
    //printfn "Inserted Olo with id %d" oloId
    //let soId = Population.insertEmployer "StackOverflow"
    //printfn "Inserted SO with id %d" soId

    //let names = ["Adam";"Ben";"Michael";"Sam"]
    // No caching
    //for name in names do
    //    let id = Population.insertPerson "Olo" name |> PlanExecution.execPlanSync 
    //    printfn "Inserted %s with id %d" name id

    // Caches result of getting employer by name because all within the same plan block
    //Population.insertPeopleBatch "Olo" names |> PlanExecution.execPlanSync
    
    //let adams,bens =
    //    //Querying.selectABsSerial () // Serial queries
    //    Querying.selectABsBatch () // Batched queries
    //    |> PlanExecution.execPlanSync
    //printfn "Returned %d Adams and %d Bens" adams.Count bens.Count

    // Vendor-specific SQL
    //Querying.selectPersonChecksum ()
    //|> Seq.iter (fun (row:Querying.SelectPersonChecksum.Row) -> printfn "%d %s %d" row.id row.name row.csum)
    0