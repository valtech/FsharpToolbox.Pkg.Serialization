module FsharpToolbox.Pkg.Serialization.Test.Json.SerializerTests

open System.Text.Json
open NUnit.Framework
open FsharpToolbox.Pkg.Serialization.Json
open FsharpToolbox.Pkg.Serialization.Json.Serializer

[<CLIMutable>]
type TypeWithInt64 = {
  someInt64: int64
}

[<CLIMutable>]
type TypeWithString = {
  someString: string
}

[<CLIMutable>]
type TypeWithTwoStrings = {
  someOtherString: string
  someString: string
}


[<CLIMutable>]
type TypeWithBoth = {
  someInt64: int64
  someString: string
}

[<CLIMutable>]
type TypeWithMaybeString = {
  someMaybeString: string option
}

type RecordWithCamelCaseField = { theField: string }
type RecordWithPascalCaseField = { TheField: string }

[<TestFixture>]
type SerializerTests () =

  [<Test>]
  member _.``Can deserialize numbers to strings when allowed`` () =
    let str = "{\"someInt64\":\"1\"}"
    let expected = 1L
    let actual = tryDeserialize<TypeWithInt64> DeserializeSettings.AllowInt64FromString str
    match actual with
    | Ok v -> Assert.AreEqual(expected, v.someInt64)
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Cannot deserialize numbers from strings when disallowed`` () =
    let str = "{\"someInt64\":\"1\"}"
    let actual = Serializer.tryDeserialize<TypeWithInt64> DeserializeSettings.Default str
    match actual with
    | Error _ -> ()
    | other -> Assert.Fail(sprintf "Expected Error, was %A" other)

  [<TestCase(DeserializeSettings.AllowInt64FromString)>]
  [<TestCase(DeserializeSettings.Default)>]
  member _.``Can deserialize when value is number`` (allowStringsToInt64s) =
    let str = "{\"someInt64\":1}"
    let expected = 1L

    let actual = Serializer.tryDeserialize<TypeWithInt64> allowStringsToInt64s str
    match actual with
    | Ok v -> Assert.AreEqual(expected, v.someInt64)
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<TestCase(DeserializeSettings.AllowStringFromNumber)>]
  [<TestCase(DeserializeSettings.Default)>]
  member _.``Can deserialize when value is string`` (allowNumberToString) =
    let str = "{\"someString\":\"1\"}"
    let expected = "1"

    let actual = Serializer.tryDeserialize<TypeWithString> allowNumberToString str
    match actual with
    | Ok v -> Assert.AreEqual(expected, v.someString)
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<TestCase("-1")>]
  [<TestCase("1")>]
  [<TestCase("0.04")>]
  [<TestCase("10.0")>]
  [<TestCase("-10.0")>]
  member _.``Can deserialize to string when value is number`` (input) =
    let str = sprintf "{\"someString\":%s}" input
    let actual = Serializer.tryDeserialize<TypeWithString> (DeserializeSettings.AllowInt64FromString ||| DeserializeSettings.AllowStringFromNumber) str
    match actual with
    | Ok v -> Assert.AreEqual(input, v.someString)
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Can deserialize with AllowAll`` () =
    let str = "{\"someString\":1,\"someInt64\":\"1\"}"
    let expected = {
      someString = "1"
      someInt64 = 1L
    }
    let actual = tryDeserialize<TypeWithBoth> DeserializeSettings.AllowAll str
    match actual with
    | Ok v -> Assert.AreEqual(expected, v)
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Must deserialize missing field as None`` () =
    let str = "{}"
    let expected = {
      someMaybeString = None
    }
    let actual = tryDeserialize<TypeWithMaybeString> DeserializeSettings.AllowAll str
    match actual with
    | Ok v -> Assert.AreEqual(expected, v)
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Must deserialize present field as Some`` () =
    let str = "{ \"someMaybeString\": \"text\" }"
    let expected = {
      someMaybeString = Some "text"
    }
    let actual = tryDeserialize<TypeWithMaybeString> DeserializeSettings.AllowAll str
    match actual with
    | Ok v -> Assert.AreEqual(expected, v)
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Should not deserialize with null field to a non-nullable target`` () =
    let str = "{ \"someString\": null}"
    let actual = tryDeserialize<TypeWithString> DeserializeSettings.AllowAll str
    match actual with
    | Ok v -> Assert.Fail $"Expected Error, was %A{v}"
    | Error e ->
      Assert.AreEqual(typedefof<JsonException>, e.GetType())
      Assert.IsTrue(e.Message.Contains("someString"))

  [<Test>]
  member _.``Should not deserialize with missing field`` () =
    let str = "{}"
    let actual = tryDeserialize<TypeWithString> DeserializeSettings.AllowAll str
    match actual with
    | Ok v -> Assert.Fail $"Expected Error, was %A{v}"
    | Error e ->
      Assert.AreEqual(typedefof<JsonException>, e.GetType())
      Assert.IsTrue(e.Message.Contains("someString"))

  [<Test>]
  member _.``Should not deserialize with missing field with one remaining field`` () =
    let str = "{ \"someOtherString\": \"otherString\" }"
    let actual = tryDeserialize<TypeWithTwoStrings> DeserializeSettings.AllowAll str
    match actual with
    | Ok v -> Assert.Fail $"Expected Error, was %A{v}"
    | Error e ->
      Assert.AreEqual(typedefof<JsonException>, e.GetType())
      Assert.IsTrue(e.Message.Contains("someString"))

  [<Test>]
  member _.``Should not deserialize with missing field and one bonus field`` () =
    let str = "{ \"someUnexpectedString\": \"unexpectedValue\" }"
    let actual = tryDeserialize<TypeWithString> DeserializeSettings.AllowAll str
    match actual with
    | Ok v -> Assert.Fail $"Expected Error, was %A{v}"
    | Error e ->
      Assert.AreEqual(typedefof<JsonException>, e.GetType())
      Assert.IsTrue(e.Message.Contains("someString"))

  [<Test>]
  member _.``Can deserialize camelCase JSON property into camelCase field`` () =
    let str = "{ \"theField\": \"foo\" }"
    let result = tryDeserialize<RecordWithCamelCaseField> DeserializeSettings.Default str
    match result with
    | Ok record -> Assert.That(record.theField, Is.EqualTo("foo"))
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Can deserialize camelCase JSON property into PascalCase field`` () =
    let str = "{ \"theField\": \"foo\" }"
    let result = tryDeserialize<RecordWithPascalCaseField> DeserializeSettings.Default str
    match result with
    | Ok record -> Assert.That(record.TheField, Is.EqualTo("foo"))
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Can deserialize PascalCase JSON property into camelCase field`` () =
    let str = "{ \"TheField\": \"foo\" }"
    let result = tryDeserialize<RecordWithCamelCaseField> DeserializeSettings.Default str
    match result with
    | Ok record -> Assert.That(record.theField, Is.EqualTo("foo"))
    | other -> Assert.Fail $"Expected Ok, was %A{other}"

  [<Test>]
  member _.``Can deserialize PascalCase JSON property into PascalCase field`` () =
    let str = "{ \"TheField\": \"foo\" }"
    let result = tryDeserialize<RecordWithPascalCaseField> DeserializeSettings.Default str
    match result with
    | Ok record -> Assert.That(record.TheField, Is.EqualTo("foo"))
    | other -> Assert.Fail $"Expected Ok, was %A{other}"
