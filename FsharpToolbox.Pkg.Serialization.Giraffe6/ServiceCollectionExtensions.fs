namespace FsharpToolbox.Pkg.Serialization.Giraffe

open FsharpToolbox.Pkg.Serialization.Json
open FsharpToolbox.Pkg.Serialization.Json.Serializer
open Microsoft.Extensions.DependencyInjection
open FsharpToolbox.Pkg.Serialization.Json.UnionTypeSerialization

[<AutoOpen>]
module ServiceCollectionExtensions =
  type IServiceCollection with

    // Should be called after AddGiraffe()
    member services.AddGiraffeJsonSerialization (?settings0: DeserializeSettings) =
      let settings = defaultArg settings0 DeserializeSettings.Default

      let options = UnionJsonDeserializationOptions()
      if settings.HasFlag(DeserializeSettings.AllowInt64FromString) then
        options.Converters.Add(Int64FromStringJsonConverter())
      if settings.HasFlag(DeserializeSettings.AllowStringFromNumber) then
        options.Converters.Add(StringFromNumberJsonConverter())

      let serializer =
        options
        |> Giraffe.SystemTextJson.Serializer

      services.AddSingleton<Giraffe.Json.ISerializer>(serializer) |> ignore
      services
