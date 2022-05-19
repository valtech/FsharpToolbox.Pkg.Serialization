module FsharpToolbox.Pkg.Serialization.Test.Json.UnionTypeSerializationTests

open NUnit.Framework
open FsharpToolbox.Pkg.Serialization.Json.UnionTypeSerialization
open System.Text.Json

type TypeWithOption = {
  myVal : string option
}

type SomeRecord = {
  someStringVal : string
  someIntVal : int
  someOptionalIntVal : int option
}

type TypeWithOptionalRecordType = {
  myVal : SomeRecord option
}

type Status =
  | OkStatus
  | ErrorStatus

type SearchOption =
  | Id of int
  | Term of string
  | Status of Status

type Something =
  | First
  | Second of int

type SomethingOptions = {
  a : Something
  b : Something
  c : Something option
}

type SomeEnum =
  | Success = 1
  | Failure = 2

[<TestFixture>]
type UnionTypeSerializationTests () =

  // Deserialize

  [<Test>]
  member _.``When value is null, returns None`` () =
    let str = "{\"myVal\": null}"
    let actual = JsonSerializer.Deserialize<TypeWithOption>(str, UnionJsonDeserializationOptions())
    match actual.myVal with
      | None -> ()
      | Some v -> failwithf "Expected None, was %A" v

  [<Test>]
  member _.``When value is nothing, returns None`` () =
    let str = "{}"
    let actual = JsonSerializer.Deserialize<TypeWithOption>(str, UnionJsonDeserializationOptions())
    match actual.myVal with
      | None -> ()
      | Some v -> failwithf "Expected None, was %A" v

  [<Test>]
  member _.``When value is not null, returns Some`` () =
    let str = "{\"myVal\": \"something\"}"
    let actual = JsonSerializer.Deserialize<TypeWithOption>(str, UnionJsonDeserializationOptions())
    match actual.myVal with
      | None -> failwithf "Expected Some"
      | Some v -> Assert.AreEqual("something", v)

  [<Test>]
  member _.``When record value is not null, returns Some`` () =
    let str = "{\"myVal\": {\"someStringVal\":\"something\", \"someIntVal\": 1}}"
    let actual = JsonSerializer.Deserialize<TypeWithOptionalRecordType>(str, UnionJsonDeserializationOptions())
    match actual.myVal with
      | None -> failwithf "Expected Some"
      | Some v ->
        Assert.AreEqual("something", v.someStringVal)
        Assert.AreEqual(1, v.someIntVal)
        match v.someOptionalIntVal with
          | None -> ()
          | Some v -> failwithf "Expected None, was %A" v


  [<Test>]
  member _.``Union type with int`` () =
    let str = "{\"type\":\"Id\",\"value\":1}"
    let actual = JsonSerializer.Deserialize<SearchOption>(str, UnionJsonDeserializationOptions())
    let expected : SearchOption = Id 1
    Assert.AreEqual(expected, actual)

  [<Test>]
  member _.``Union type with small first letter`` () =
    let str = "{\"type\":\"id\",\"value\":1}"
    let actual = JsonSerializer.Deserialize<SearchOption>(str, UnionJsonDeserializationOptions())
    let expected : SearchOption = Id 1
    Assert.AreEqual(expected, actual)

  [<Test>]
  member _.``Union type with string small first letter`` () =
    let str = "{\"type\":\"term\",\"value\":\"expected\"}"
    let actual = JsonSerializer.Deserialize<SearchOption>(str, UnionJsonDeserializationOptions())
    let expected : SearchOption = Term "expected"
    Assert.AreEqual(expected, actual)

  [<Test>]
  member _.``Union type Enum-value`` () =
    let str = "{\"type\":\"status\",\"value\":{\"type\":\"okstatus\"}}"
    let actual = JsonSerializer.Deserialize<SearchOption>(str, UnionJsonDeserializationOptions())
    let expected : SearchOption = Status Status.OkStatus
    Assert.AreEqual(expected, actual)

  [<Test>]
  member _.``Deserialize to record type with different enum types`` () =
    let str = "{\"a\":{\"type\":\"First\"},\"b\":{\"type\":\"Second\",\"value\":1},\"c\":null}"
    let expected = {a = First; b = Something.Second 1; c = None }
    let actual = JsonSerializer.Deserialize<SomethingOptions>(str, UnionJsonDeserializationOptions())
    Assert.AreEqual(actual, expected)

  [<Test>]
  member _.``Handles deserialization when optional value is not given in input`` () =
    let str = "{\"a\":{\"type\":\"First\"},\"b\":{\"type\":\"Second\",\"value\":1}}"
    let expected = {a = First; b = Something.Second 1; c = None }
    let actual = JsonSerializer.Deserialize<SomethingOptions>(str, UnionJsonDeserializationOptions())
    Assert.AreEqual(actual, expected)

  // Serialize

  [<Test>]
  member _.``Serialize Enum value`` () =
    let expected = "\"Success\""
    let obj = SomeEnum.Success
    let actual = JsonSerializer.Serialize(obj, UnionJsonSerializationOptions())
    Assert.AreEqual(expected, actual)

  [<Test>]
  member _.``Serialize Enum value in record`` () =
    let expected = "{\"expected\":\"Failure\"}"
    let obj = {| expected = SomeEnum.Failure |}
    let actual = JsonSerializer.Serialize(obj, UnionJsonSerializationOptions())
    Assert.AreEqual(expected, actual)


  [<Test>]
  member _.``When value is None, returns null`` () =
    let str = "{\"myVal\":null}"
    let obj = {myVal = None}
    let actual = JsonSerializer.Serialize(obj, UnionJsonSerializationOptions())
    Assert.AreEqual(str, actual)

  [<Test>]
  member _.``When one value is None, returns null for that property`` () =
    let expected = "{\"myVal\":{\"someStringVal\":\"something\",\"someIntVal\":1,\"someOptionalIntVal\":null}}"
    let obj = {
                myVal = Some {
                  someStringVal = "something"
                  someIntVal = 1
                  someOptionalIntVal = None
                }
              }
    let actual = JsonSerializer.Serialize(obj, UnionJsonSerializationOptions())
    Assert.AreEqual(expected, actual)

  [<Test>]
  member _.``Serialize to record type with DUs and one optional as null`` () =
    let expected = "{\"a\":{\"type\":\"First\"},\"b\":{\"type\":\"Second\",\"value\":1},\"c\":null}"
    let obj : SomethingOptions = {a = First; b = Something.Second 1; c = None }
    let actual = JsonSerializer.Serialize(obj, UnionJsonSerializationOptions())
    Assert.AreEqual(actual, expected)

  [<Test>]
  member _.``Serialize to record type with DUs and one optional as value`` () =
    let expected = "{\"a\":{\"type\":\"First\"},\"b\":{\"type\":\"Second\",\"value\":1},\"c\":{\"type\":\"First\"}}"
    let obj : SomethingOptions = {a = Something.First; b = Something.Second 1; c = Some First }
    let actual = JsonSerializer.Serialize(obj, UnionJsonSerializationOptions())
    Assert.AreEqual(actual, expected)

  [<Test>]
  member _.``Serializer Union type Enum-value`` () =
    let expected = "{\"type\":\"Status\",\"value\":{\"type\":\"OkStatus\"}}"
    let obj : SearchOption = Status Status.OkStatus
    let actual = JsonSerializer.Serialize(obj, UnionJsonSerializationOptions())
    Assert.AreEqual(expected, actual)
