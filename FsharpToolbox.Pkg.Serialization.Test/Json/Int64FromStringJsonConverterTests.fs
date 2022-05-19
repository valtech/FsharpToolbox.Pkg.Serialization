module FsharpToolbox.Pkg.Serialization.Test.Json.Int64FromStringJsonConverterTests

open NUnit.Framework
open System.Text.Json
open FsharpToolbox.Pkg.Serialization.Json

[<CLIMutable>]
type TypeWithInt64 = {
  someInt64: int64
}

[<TestFixture>]
type Int64FromStringJsonConverterTests () =

  let getOptions() =
    let options = JsonSerializerOptions()
    options.Converters.Add(Int64FromStringJsonConverter())
    options

  [<Test>]
  member _.``Can deserialize when value is Number`` () =
    let str = "{\"someInt64\":1}"
    let expected = 1L
    let actual = JsonSerializer.Deserialize<TypeWithInt64>(str, getOptions())
    Assert.AreEqual(expected, actual.someInt64)
  
  [<Test>]
  member _.``Can deserialize when value is string`` () =
    let str = "{\"someInt64\":\"1\"}"
    let expected = 1L
    let actual = JsonSerializer.Deserialize<TypeWithInt64>(str, getOptions())
    Assert.AreEqual(expected, actual.someInt64)

  [<TestCase(1L, "1")>]
  [<TestCase(0L, "0")>]
  [<TestCase(-1L, "-1")>]
  [<TestCase(System.Int64.MaxValue, "9223372036854775807")>]
  [<TestCase(System.Int64.MinValue, "-9223372036854775808")>]
  member _.``Always serializes as number`` (int64Value, expectedStringRepresentation) =
    let expected = sprintf "{\"someInt64\":%s}" expectedStringRepresentation
    let options = getOptions()
    let value = {someInt64 = int64Value};
    let actual = JsonSerializer.Serialize(value, options)
    Assert.AreEqual(expected, actual)

  [<Test>]
  member _.``Throws exception when value is overflow`` () =
    let str = "{\"someInt64\":9223372036854775808}" // System.Int64.MaxValue + 1
    let shouldThrow () = JsonSerializer.Deserialize<TypeWithInt64>(str, getOptions())
    Assert.Throws<JsonException>(fun () -> shouldThrow() |> ignore)
    |> ignore
