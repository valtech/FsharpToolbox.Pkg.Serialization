# Introduction 
Package with serialization helpers for `System.Text.Json`. These packages
contain default settings for working with JSON serialization in the FsharpToolbox.

### Usage

#### JSON (de)serialization

Use the FsharpToolbox default JSON serialization by adding the `FsharpToolbox.Pkg.Serialization`,
opening the `FsharpToolbox.Pkg.Serialization.Json`
module and calling these helper functions:

 - `Serializer.jsonSerialize`
 - `Serializer.jsonDeserialize`
 - `Serializer.tryDeserialize<'T>`

The deserialization functions accept a `DeserializeSettings` that controls
how lenient the deserializer should be, whether it allows a string to be parsed
into an number field and vice versa.

#### Giraffe JSON serialization

Configure Giraffe to use the FsharpToolbox default serialization by adding either the
`FsharpToolbox.Pkg.Serialization.Giraffe6` package to
your Giraffe project and in your setup, call the `AddGiraffeJsonSerialization` method
(you can optionally pass a `DeserializeSettings`to control whether certain conversions
should be allowed, `DeserializeSettings.Default` is the default):

```fsharp
open FsharpToolbox.Pkg.Serialization.Giraffe

let configureServices _context (services : IServiceCollection) (configuration : IConfiguration) =
    services
        .AddGiraffe()
        .AddGiraffeJsonSerialization(DeserializeSettings.Default)
```
