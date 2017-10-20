// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open Impl
open Rezoom
open Rezoom.Execution

type BrandId = BrandId of Guid
    with static member MakeBrandId () = BrandId (Guid.NewGuid())

type UserId = UserId of Guid
    with static member MakeUserId () = UserId (Guid.NewGuid())

let brand1Id = BrandId.MakeBrandId()
let brand2Id = BrandId.MakeBrandId()

//////////////////////////////////////////
//////////////////////////////////////////

let brands = dict [ brand1Id, "Brand 1"
                    brand2Id, "Brand 2" ]

let users = dict [ UserId.MakeUserId(), ("User 1", brand1Id )
                   UserId.MakeUserId(), ("User 2", brand1Id )
                   UserId.MakeUserId(), ("User 3", brand1Id )
                   UserId.MakeUserId(), ("User 4", brand1Id )
                   UserId.MakeUserId(), ("User 5", brand1Id )
                   UserId.MakeUserId(), ("User 6", brand2Id )
                   UserId.MakeUserId(), ("User 7", brand2Id )
                   UserId.MakeUserId(), ("User 8", brand2Id )
                   UserId.MakeUserId(), ("User 9", brand2Id )
                   UserId.MakeUserId(), ("User 10", brand2Id ) ]



let getBrand id =
    printfn "Get Brand with Id %A" id
    brands.[id]

let getUser id =
    printfn "Get User with Id %A" id
    users.[id]



let getBrands ids =
    printfn "\nGet Batch of Brands\n"
    ids |> Seq.map getBrand

let getUsers ids =
    printfn "\nGet Batch of Users\n"
    ids |> Seq.map getUser

//////////////////////////////////////////
//////////////////////////////////////////
        
type StepBatch<'a, 'b>(runBatch: 'a seq -> 'b seq) =
    let userIds = ResizeArray<'a>()

    //evaluates the first time the result of AddToBatch is executed
    let batchResults = lazy (runBatch userIds |> Seq.toArray) 
    
    member this.AddToBatch(i) =
        let index = userIds.Count
        userIds.Add(i)
        fun () -> batchResults.Value.[index]

type UserStepBatch() = inherit StepBatch<UserId, string*BrandId>(getUsers)
type BrandStepBatch() = inherit StepBatch<BrandId, string>(getBrands)

let getCache<'a> () = 

    //cache identifier
    let category = box typeof<'a>
    
    //item identifier within a cache
    let identity = box "ignorefornow"

    { new CacheInfo() with
        override this.Category = category
        override this.Identity = identity
        override this.Cacheable = true
        // Result can be cached without any dependencies.
        override this.DependencyMask = BitMask.Zero
        // Running this does not invalidate any dependencies.
        override this.InvalidationMask = BitMask.Zero
    }
    
type UserErrand(arg : UserId) =
    inherit SynchronousErrand<string*BrandId>()
    
    override this.CacheInfo = getCache<UserStepBatch> ()
    override this.CacheArgument = box arg
    
    override this.Prepare(context : ServiceContext) =
        // Get the batch for this execution step.
        let batch = context.GetService<StepLocal<UserStepBatch>, UserStepBatch>()
        // Add ourselves to the batch, return the result getter.
        batch.AddToBatch arg
        
type BrandErrand(arg : BrandId) =
    inherit SynchronousErrand<string>()
    
    override this.CacheInfo = getCache<BrandStepBatch> ()
    override this.CacheArgument = box arg
    
    override this.Prepare(context : ServiceContext) =
        // Get the batch for this execution step.
        let batch = context.GetService<StepLocal<BrandStepBatch>, BrandStepBatch>()
        // Add ourselves to the batch, return the result getter.
        batch.AddToBatch arg
                
let retrieveUser id = UserErrand id |> Plan.ofErrand
let retrieveBrand id = BrandErrand id |> Plan.ofErrand

[<EntryPoint>]
let main argv = 
    
    //pretend we want to get a list of users and their brands
    //users come from one service, brands come from another, so this requires two steps:
    //1. Get Each User
    //2. For each user, get their brand

    //10 users
    //2 brands

    //naive implementation results in 20 service calls (1 for each user and then once for the brand for each user)
    //non-naive implmentation is messy and difficult to follow for collating users and brands without unneeded service calls
    
    let userIds = users.Keys |> Seq.toArray
    let adminIds = userIds |> Seq.take 5 |> Seq.toArray

    let normalUsers, adminUsers = (plan {
                                        let normalUsers = ResizeArray<UserId*string*BrandId*string>()

                                        for userId in batch userIds do
                                            let! (userName, brandId) = retrieveUser userId
                                            let! brandName = retrieveBrand brandId
                                            normalUsers.Add (userId, userName, brandId, brandName)

                                        let adminUsers = ResizeArray<UserId*string*BrandId*string>()

                                        for adminId in batch adminIds do
                                            let! (userName, brandId) = retrieveUser adminId
                                            let! brandName = retrieveBrand brandId
                                            adminUsers.Add (adminId, userName, brandId, brandName)

                                        return normalUsers, adminUsers
                                     }
                                     |> execute ExecutionConfig.Default).Result

    printfn "\n\n---------------------Normal Users------------------"

    for user in normalUsers do
        printfn "\n%A" user

    printfn "\n\n---------------------Admin Users------------------"

    for admin in adminUsers do
        printfn "\n%A" admin

    printfn "\n"
    0 // return an integer exit code
