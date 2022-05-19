namespace FsharpToolbox.Pkg.Serialization.Json

open System.Text.Json.Serialization
open System.Text.Json

// adapted from https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-core-3-1#allow-or-write-numbers-in-quotes
// replace with built in settings when migrated to .net 5
// OR remove when all external ids are treated as strings
type StringFromNumberJsonConverter() =
  inherit JsonConverter<string>()

  override _.Read
    (reader: byref<System.Text.Json.Utf8JsonReader>,
    _typeToConvert: System.Type,
    _options: System.Text.Json.JsonSerializerOptions) =
    if
      reader.TokenType = JsonTokenType.Number
    then
      reader.ValueSpan.ToArray()
      |> System.Text.Encoding.UTF8.GetString
    else
      reader.GetString()


  override _.Write(writer, value, _options) =
    writer.WriteStringValue(value)
