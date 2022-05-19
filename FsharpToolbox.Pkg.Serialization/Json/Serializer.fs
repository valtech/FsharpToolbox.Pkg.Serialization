module FsharpToolbox.Pkg.Serialization.Json.Serializer

open System.Linq
open System.Text.Json
open FsharpToolbox.Pkg.Serialization.Json
open FsharpToolbox.Pkg.Serialization.Json.UnionTypeSerialization
open NodaTime.Serialization.SystemTextJson

let private addNodaTimeConverters (options: JsonSerializerOptions) =
  options.ConfigureForNodaTime NodaTime.DateTimeZoneProviders.Tzdb

let private serializeOptions =
  UnionJsonSerializationOptions() |> addNodaTimeConverters

let updateSerializeOptions (options: JsonSerializerOptions) =
  options
  |> GetUnionJsonSerializationOptions defaultJsonFSharpConverter
  |> addNodaTimeConverters

let jsonSerialize obj = JsonSerializer.Serialize(obj, serializeOptions)

[<System.Flags>]
type DeserializeSettings =
  | Default = 0
  | AllowInt64FromString = 1
  | AllowStringFromNumber = 2
  | AllowAll = 3 // NOTE: Sum of all flags

let private generateJsonSerializerOptions (setting: DeserializeSettings) =
  let options = UnionJsonDeserializationOptions() |> addNodaTimeConverters

  if setting.HasFlag(DeserializeSettings.AllowInt64FromString) then
    options.Converters.Add(Int64FromStringJsonConverter())
  if setting.HasFlag(DeserializeSettings.AllowStringFromNumber) then
    options.Converters.Add(StringFromNumberJsonConverter())
  options

let private deserializeOptionsCache : Map<DeserializeSettings, JsonSerializerOptions> =
  // NOTE: Find max value of DeserializeSettings variants
  let maxEnumValue = int (System.Enum.GetValues(typeof<DeserializeSettings>).Cast<DeserializeSettings>().Max())

  [0 .. maxEnumValue]
  |> List.map enum<DeserializeSettings>
  |> List.map (fun setting -> (setting, generateJsonSerializerOptions setting))
  |> Map.ofList

let jsonDeserialize<'t> (settings: DeserializeSettings) (content: string) =
  let options = deserializeOptionsCache.[settings]
  JsonSerializer.Deserialize<'t>(content, options)

let tryDeserialize<'T> (settings: DeserializeSettings) (content : string) : Result<'T, exn> =
  try
    jsonDeserialize<'T> settings content
    |> Ok
  with
    | ex -> Error ex
