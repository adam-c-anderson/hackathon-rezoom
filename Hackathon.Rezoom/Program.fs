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
         for name in ["Adam";"Ben";"Michael";"Sam"] do
            do! insertPerson.Command(name).Plan()
         }

[<EntryPoint>]
let main argv =
    migrate()
    stdout.WriteLine("Done running migrations")
    let (adams,bens) = (
        plan {
            let! adams,bens = selectPersonByName.Command("Adam").Plan(),selectPersonByName.Command("Ben").Plan()
            return (adams,bens)
        }
        |> execute ExecutionConfig.Default).Result
    Seq.iter2 (fun (a:selectPersonByName.Row) (b:selectPersonByName.Row) -> printfn "%s %s" a.name b.name ) adams bens    
    stdout.WriteLine("Done populating")
    // stdin.ReadLine() |> ignore
    // return 0 status code
    0