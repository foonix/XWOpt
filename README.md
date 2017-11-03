# XWOpt lib

C# Library for editing OPT model data from XWing Special Edition (1998), Tie Fighter Special Edition (1998), X-Wing VS Tie Fighter, X-Wing Alliance.

A Unity plugin is included for loading model data into Unity.

## Requirements:

### XWOpt

 * C# .NET framework 3.5+ or Unity .NET subset 3.5+.
 * As there was no standard vector format in .NET 3.5, `Vector2` and `Vector3` types must be supplied.  Structs are expected with [Xx], [Yy], and [Zz] float fields. `System.Numerics.Vector3` and `UnityEngine.Vector3` are recommended.

### XWOptUnity

 * Tested with Unity 2017.1.1f1 (.NET subset 3.5).

## Usage

```csharp
var opt = new OptFile<Vector2, Vector3>()
{
    logger = msg => Console.WriteLine(msg),
};

opt.Read(@"C:\GOG Games\Star Wars - TIE Fighter (1998)\IVFILES\TIEFTR.OPT")

List<Hardpoint<Vector3>> hardpoints = opt.FindAll<Hardpoint<Vector3>>();
```

## Opt format considerations

* Generally, each OPT represents one ship or weapon model.
* All points in the file share the same origin.
* Point space in OPT files is: forward is -y, right is +x(?), up is +z.  `Vector3`s will likely need to be rotated into the engine's native coordinate system.
* At the top level of the file, each `BranchNode` represent a part of the craft, even if the craft only has one part.
  * Texture data and pallet data can be shared between parts. They are usually defined in the first part that uses them, and refereced in subsequent parts.
  * Each part has exactly one set of `MeshVerticies`, `VertexNormals`, and `VertexUV`s.
  * Polygons are stored as triangles or quads as a list of `int` refences to the part's verticies, normals, and UV coordinates.
  * Hardpoints (Turrets, missile launchers, etc) are stored under the part they are attached to.
  * Parts usually contain a `PartDescriptor` describing its hit box, explosion type, and part type and center point for targeting purposes.
* In some cases there are textures at the top level if they are shared between multiple parts.
* Texture UVs and normals are stored per point on a face, not per vertex.  In some engines (E.G. Unity), this means that vertex data will need to be split (duplicated) in order to use a different UV or or normal on the same vertex.

## Navigating Node Objects

All node objects inherit from `List<BaseNode>` and can be navigated using the index operator or LINQ.

Use `.FindAll<>()` if looking for a specific type of node under a sub-node.

## Testing

The NUnit test suite is run against all OPT files in supported games.  The GOG version is recommended.

For Visual Studio, Install NUnit and NUnit3TestAdapter nuget packages into XWOpt_test project to execute the tests.  Edit `optDirs` in XWOpt_test if you do not have all supported platforms or if they are not in the default GOG install directory.

## License

Copyright 2017 Jason McNew

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## References
* http://www.oocities.org/v_d_d/Xwing_Unofficial_Specs.html
* http://www.oocities.org/v_d_d/OptSpecs.pdf
