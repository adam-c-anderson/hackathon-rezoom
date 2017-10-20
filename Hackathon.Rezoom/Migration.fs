module Migration

open Rezoom.SQL
open Rezoom.SQL.Migrations

type RzSampleModel = SQLModel<"."> // find migrations in the project folder, "."

let migrate() =
    let config =
        { MigrationConfig.Default with
            LogMigrationRan = fun m -> printfn "Ran migration: %s" m.MigrationName
            //AllowRetroactiveMigrations = true
        }
    RzSampleModel.Migrate(config)
    printfn "Finished running migrations"