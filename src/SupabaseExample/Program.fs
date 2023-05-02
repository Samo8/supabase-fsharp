open Supabase
open GoTrue
open Postgrest
open Postgrest.Common

let client = initialize "https://<project-id>.supabase.co" "<api-key>"

type Test = {
    id: int
    name: string
}

let user =
    client.GoTrueConnection
    |> signInWithEmail "<email>" "<password>" None
    |> Async.RunSynchronously

match user with
| Ok r ->
    let films =
        (withAuth r.accessToken client).PostgrestConnection
        |> from "test"
        |> Client.select Columns.All
        |> PostgrestFilterBuilder.execute<Test list>
        |> Async.RunSynchronously
    match films with
    | Ok ok2   -> printfn $"{ok2}"
    | Error e2 -> printfn $"{e2}"
| Error e -> printfn $"{e}"