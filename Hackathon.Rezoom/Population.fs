module Population

open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Plans
open Rezoom.SQL.Synchronous
open Rezoom.PlanBuilder

type private InsertEmployer = SQL<"""
    insert into employer row
        name = @name;
    select scope_identity() as id;
    """>

type private SelectEmployerByName = SQL<"""
    select *
    from employer
    where name = @name
    limit 1;
    """>

type private InsertPerson = SQL<"""
    insert into person row
        employerId = @employerId,
        name = @name;
    select scope_identity() as id;
    """>

// Example of synchronous, direct execution
let insertEmployer name =
    use context = new ConnectionContext()
    InsertEmployer.Command(name = name).ExecuteScalar(context)

// Example of a plan that can take advantage of caching
let insertPerson employerName personName =
    plan {
        let! employer = SelectEmployerByName.Command(name = employerName).ExactlyOne()
        return! InsertPerson.Command(employerId = employer.id, name = personName).Scalar()
    }

// Uses nested plan which will always return the same employer, so only executes SelectEmployerByName once
let insertPeople employerName (personNames:seq<string>) =
    plan {
        for personName in personNames do
            // There isn't a yield! implemented yet
            let! _ = insertPerson employerName personName
            ()
    }

// Uses nested plan which will always return the same employer, so only executes SelectEmployerByName once
// Also now batches inserts
let insertPeopleBatch employerName (personNames:seq<string>) =
    plan {
        for personName in batch personNames do
            // There isn't a yield! implemented yet
            let! _ = insertPerson employerName personName
            ()
    }

// Uses nested plan which will always return the same employer, so only executes SelectEmployerByName once
// Also batches inserts AND returns the results!
let insertPeopleBatch' employerName (personNames:seq<string>) =
    personNames
    |> Seq.map (fun personName -> insertPerson employerName personName)
    |> Seq.toList
    |> Plan.concurrentList