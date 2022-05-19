module FsharpToolbox.Pkg.Serialization.Json.UnionTypeSerialization

open System.Text.Json.Serialization
open System.Text.Json

type SystemTextJsonSerializer (options: JsonSerializerOptions) =
  static member DefaultOptions =
    JsonSerializerOptions(
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    )

let defaultJsonFSharpConverter =
  JsonFSharpConverter(
    JsonUnionEncoding.UnwrapSingleFieldCases |||
    JsonUnionEncoding.UnwrapOption |||
    JsonUnionEncoding.UnwrapRecordCases |||
    JsonUnionEncoding.NamedFields, "type", "value",
    unionTagCaseInsensitive = true
  )

let GetUnionJsonSerializationOptions (jsonFSharpConverter : JsonFSharpConverter) (options : JsonSerializerOptions) =
  options.Converters.Add(
    jsonFSharpConverter
  )
  options.Converters.Add(
    JsonStringEnumConverter()
  )
  options

let UnionJsonSerializationOptions() =
  GetUnionJsonSerializationOptions defaultJsonFSharpConverter SystemTextJsonSerializer.DefaultOptions

let UnionJsonDeserializationOptions() =
  let options = SystemTextJsonSerializer.DefaultOptions
  options.PropertyNameCaseInsensitive <- true
  GetUnionJsonSerializationOptions defaultJsonFSharpConverter options
