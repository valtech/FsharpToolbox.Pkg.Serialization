module FsharpToolbox.Pkg.Serialization.Test.Json.NodaTimeSerialization

open NUnit.Framework
open NodaTime

open FsharpToolbox.Pkg.Serialization.Json

type WithTime = {
  theTime: Instant
}

[<TestFixture>]
type UnionTypeSerializationTests () =
  [<Test>]
  member _.``Get same value when serializing and deserializing NodaTime.Instant`` () =
    let expected = {
      theTime = SystemClock.Instance.GetCurrentInstant()
    }
    let actual =
      expected
      |> Serializer.jsonSerialize
      |> Serializer.jsonDeserialize<WithTime> Serializer.DeserializeSettings.AllowAll

    Assert.AreEqual(expected, actual)
