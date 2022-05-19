namespace FsharpToolbox.Pkg.Serialization.Json

open System.Text.Json.Serialization
open System.Text.Json

// adapted from https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-core-3-1#allow-or-write-numbers-in-quotes
// replace with built in settings when migrated to .net 5
// OR remove when all external ids are treated as strings
type Int64FromStringJsonConverter() =
  inherit JsonConverter<int64>()

  override _.Read
    (reader: byref<System.Text.Json.Utf8JsonReader>,
    _typeToConvert: System.Type,
    _options: System.Text.Json.JsonSerializerOptions) =
    if
      reader.TokenType = JsonTokenType.String
    then      
      let (couldParse, parsedValue) = System.Int64.TryParse(reader.GetString())
      if couldParse
      then parsedValue
      else reader.GetInt64()
    else
      reader.GetInt64()

  override _.Write(writer, value, _options) =
    writer.WriteNumberValue(value)