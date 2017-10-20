module PlanExecution

open Rezoom.Execution

let execPlan plan = execute ExecutionConfig.Default plan

let execPlanAsync plan = execPlan plan |> Async.AwaitTask

let execPlanSync plan = execPlanAsync plan |> Async.RunSynchronously