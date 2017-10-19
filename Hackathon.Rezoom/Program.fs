open Rezoom.SQL
open Rezoom.SQL.Migrations

type MyModel = SQLModel<"."> // find migrations in the project folder, "."

let migrate() =
    // customize the default migration config so that it outputs a message after running a migration
    let config =
        { MigrationConfig.Default with
            LogMigrationRan = fun m -> printfn "Ran migration: %s" m.MigrationName
            AllowRetroactiveMigrations = true
        }
    // run the migrations, creating the database if it doesn't exist
    MyModel.Migrate(config)

[<EntryPoint>]
let main argv =
    migrate()
    stdout.WriteLine("Done running migrations")
    // stdin.ReadLine() |> ignore
    // return 0 status code
    0