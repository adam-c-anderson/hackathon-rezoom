module Querying

open Rezoom.SQL
open Rezoom.SQL.Plans
open Rezoom.SQL.Synchronous
open Rezoom.PlanBuilder

type SelectPersonByName = SQL<"""
    select *
    from person
    where name = @name
""">

type SelectPersonChecksum = SQL<"""
    vendor tsql {
        select *, checksum(*) as csum from person
    } imagine {
        select *, cast(0 as int) as csum from person
    }""">

let selectABsSerial () =
    plan {
        let! adams = SelectPersonByName.Command(name = "Adam").Plan()
        let! bens = SelectPersonByName.Command(name = "Ben").Plan()
        return (adams, bens)
    }
    
let selectABsBatch () =
    plan {
        let! adams,bens = 
            SelectPersonByName.Command(name = "Adam").Plan(),
            SelectPersonByName.Command(name = "Ben").Plan()
        return adams,bens
    }

let selectPersonChecksum () =
    use context = new ConnectionContext()
    SelectPersonChecksum.Command().Execute(context)