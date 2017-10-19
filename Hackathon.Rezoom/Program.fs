open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Migrations
open Rezoom.PlanBuilder
open Rezoom.SQL.Plans
open Rezoom.BatchHints
open Rezoom.Execution
open Rezoom.SQL.Synchronous
open System.Threading

type MyModel = SQLModel<"."> // find migrations in the project folder, "."

type insertPerson = SQL<"""
    insert into person
    row
        name = @name
    """>

type selectPersonByName = SQL<"""
    select *
    from person
    where name = @name
""">

//type selectPersonChecksum = SQL<"select *, checksum(*) as csum from person">
type selectPersonChecksum = SQL<"vendor tsql {
        select *, checksum(*) as csum from person
    } imagine {
        select *, cast(0 as int) as csum from person
    }">

let migrate() =
    // customize the default migration config so that it outputs a message after running a migration
    let config =
        { MigrationConfig.Default with
            LogMigrationRan = fun m -> printfn "Ran migration: %s" m.MigrationName
            AllowRetroactiveMigrations = true
        }
    // run the migrations, creating the database if it doesn't exist
    MyModel.Migrate(config)

let populatePerson() =
    
    plan {
         for name in batch ["Adam";"Ben";"Michael";"Sam"] do
            do! insertPerson.Command(name).Plan()
         }

[<EntryPoint>]
let main argv =
    migrate()
    stdout.WriteLine("Done running migrations")
    //populatePerson() |> execute ExecutionConfig.Default |> ignore
    //stdout.WriteLine("Done populating")
    //let (adams,bens,sams) = (
    //    plan {
    //        let! adams,bens = selectPersonByName.Command("Adam").Plan(),selectPersonByName.Command("Ben").Plan()
    //        let! adams2,sams = selectPersonByName.Command("Adam").Plan(),selectPersonByName.Command("Sam").Plan()
    //        let! michaels,michaels2 = selectPersonByName.Command("Michael").Plan(),selectPersonByName.Command("Michael").Plan()
    //        let! adams3,adams4 = selectPersonByName.Command("Adam").Plan(),selectPersonByName.Command("Adam").Plan()
    //        return (adams,bens,sams)
    //    }
    //    |> execute ExecutionConfig.Default).Result
    //Seq.iter2 (fun (a:selectPersonByName.Row) (b:selectPersonByName.Row) -> printfn "%s %s" a.name b.name ) adams bens    
    selectPersonChecksum.Command().Execute(new ConnectionContext())
    |> Seq.iter (fun (row:selectPersonChecksum.Row) -> printfn "%d %s %d" row.id row.name row.csum)
    0