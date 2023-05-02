namespace Supabase

open System.Text.RegularExpressions
open Functions.Connection
open GoTrue.Connection
open Postgrest.Connection
open Realtime.Connection
open Storage.Connection

/// Contains connections for all client libraries and initialization functions
[<AutoOpen>]
module SupabaseClient =
    /// Represents Supabase client
    type Client =
        { Url:                 string
          ApiKey:              string
          PostgrestConnection: PostgrestConnection
          GoTrueConnection:    GoTrueConnection
          RealtimeConnection:  RealtimeConnection
          StorageConnection:   StorageConnection  
          FunctionConnection:  FunctionsConnection }
        
    /// Represents Supabase client with mutable connection data
    type MutableClient =
        { mutable Url:                 string
          mutable ApiKey:              string
          mutable PostgrestConnection: PostgrestConnection
          mutable GoTrueConnection:    GoTrueConnection
          mutable RealtimeConnection:  RealtimeConnection
          mutable StorageConnection:   StorageConnection
          mutable FunctionConnection:  FunctionsConnection }
    
    /// Builds url for postgrest-fsharp
    let private buildPostgrestUrl (url: string): string = $"{url}/rest/v1"
    
    /// Builds url for gotrue-fsharp
    let private buildGoTrueUrl(url: string): string = $"{url}/auth/v1"
    
    /// Builds url for realtime-fsharp
    let private buildRealtimeUrl(url: string): string =
        let wsUrl = url.Replace("http", "ws")
        $"{wsUrl}/realtime/v1"
    
    /// Builds url for storage-fsharp
    let private buildStorageUrl(url: string): string = $"{url}/storage/v1"
    
    /// Build url for functions-fsharp
    let private buildFunctionsUrl(url: string) =
        let isPlatform = Regex(@"(supabase\.co)|(supabase\.in)").Match url
        match isPlatform.Success with
        | true ->
            let parts = url.Split "."
            $"{parts[0]}.functions.{parts[1]}.{parts[2]}";
        | _    ->
            $"{url}/functions/v1"
    
    /// Initializes connection with given `url` and `apiKey` 
    let initialize (url: string) (apiKey: string): Client =
        let postgrestUrl = buildPostgrestUrl url
        let goTrueUrl = buildGoTrueUrl url
        let realtimeUrl = buildRealtimeUrl url
        let storageUrl = buildStorageUrl url
        let functionsUrl = buildFunctionsUrl url
        
        let initialHeaders = Map [ "apiKey", apiKey
                                   "Authorization", $"Bearer {apiKey}" ]
                
        {   Url = url
            ApiKey = apiKey
            PostgrestConnection = postgrestConnection {
                url     postgrestUrl
                headers initialHeaders
            }
            GoTrueConnection = goTrueConnection {
                url     goTrueUrl
                headers initialHeaders
            }
            RealtimeConnection = realtimeConnection {
                yield realtimeUrl, (Map ["apiKey", apiKey])
            }
            StorageConnection = storageConnection {
                url    storageUrl
                headers initialHeaders
            }
            FunctionConnection = functionsConnection {
                url     functionsUrl
                headers initialHeaders
            }
        }
        
    /// Adds given token to headers of connections
    let withAuth (bearer: string) (client: Client): Client =
        let postgrestConnection = Postgrest.Common.updateBearer bearer client.PostgrestConnection
        let realtimeConnection = Realtime.Common.updateBearer bearer client.RealtimeConnection
        let storageConnection = Storage.Common.updateBearer bearer client.StorageConnection
        let functionConnection = Functions.Common.updateBearer bearer client.FunctionConnection
        
        { client with
            PostgrestConnection = postgrestConnection
            RealtimeConnection  = realtimeConnection
            StorageConnection   = storageConnection
            FunctionConnection  = functionConnection }
        
    /// Adds given token to headers of connections for `MutableClient`
    let withAuthMutable (bearer: string) (client: MutableClient): unit =
        let postgrestConnection = Postgrest.Common.updateBearer bearer client.PostgrestConnection
        let realtimeConnection = Realtime.Common.updateBearer bearer client.RealtimeConnection
        let storageConnection = Storage.Common.updateBearer bearer client.StorageConnection
        let functionConnection = Functions.Common.updateBearer bearer client.FunctionConnection
        
        client.PostgrestConnection <- postgrestConnection
        client.RealtimeConnection  <- realtimeConnection
        client.StorageConnection   <- storageConnection
        client.FunctionConnection  <- functionConnection