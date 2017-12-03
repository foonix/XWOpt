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

### XWOpt
```cs
var opt = new OptFile<Vector2, Vector3>()
{
    logger = msg => Console.WriteLine(msg),
};

opt.Read(@"C:\GOG Games\Star Wars - TIE Fighter (1998)\IVFILES\TIEFTR.OPT")

List<Hardpoint<Vector3>> hardpoints = opt.FindAll<Hardpoint<Vector3>>();
```
### XWOptUnity

#### Example code
```cs
public class CraftLoader : MonoBehaviour
{

    public static string OptDir = @"C:\GOG Games\Star Wars - X-Wing (1998)\IVFiles\";
    public string OptFile = "TIEFTR.OPT";
    public int skin = 0;
    private CraftFactory craft;

    public List<GameObject> sFoils;
    public List<GameObject> blasters;
    public List<GameObject> torpedoLaunchers;

    void InitSFoils(GameObject part, PartDescriptor<Vector3> descriptor)
    {
        if(descriptor.PartType == PartType.RotWing)
        {
            sFoils.Add(part);
        }

        // Set up box collider
        BoxCollider box = part.AddComponent<BoxCollider>();
        box.center = descriptor.HitboxCenterPoint;
        // Span size components can become negative due to vector rotation into Unity's coordinate space.
        var span = descriptor.HitboxSpan;
        box.size = new Vector3(Math.Abs(span.x), Math.Abs(span.y), Math.Abs(span.z));
    }

    void GetBlasters(GameObject weapon, PartDescriptor<Vector3> parent, Hardpoint<Vector3> hardpoint)
    {
        if (hardpoint.WeaponType == WeaponType.RebelLaser)
        {
            blasters.Add(weapon);
            return;
        }
        // Discard unused hardpoints
        //Destroy(weapon);
    }

    void Start()
    {
        // Load OPT model data into this object.
        craft = new CraftFactory(OptDir + OptFile) {
            PartBase = new GameObject("Provided part base object"),
            CraftBase = new GameObject("Provided craft base object"),
            TargetPointBase = new GameObject("Provided Targeting Point"),
            TargetingGroupBase = new GameObject("Provided targeting group"),
            HardpointBase = new GameObject("Provided Hardpoint"),
            ProcessPart = new ProcessPartHandler(InitSFoils),
            ProcessHardpoint = new ProcessHardpointHandler(GetBlasters),
        };
        var optCraft = craft.CreateCraftObject(skin);
        optCraft.name = OptFile;
        optCraft.GetComponent<Transform>().parent = gameObject.GetComponent<Transform>();
        optCraft.GetComponent<Transform>().localPosition = Vector3.zero;
        gameObject.name = OptFile;
    }
}
```

`CraftFactory` loads an OPT file, does the work of converting to unity format, and provides a function for instatiating craft objects.  A set of base objects are instatiated to form the new craft.  Craft instantiated from the same factory will share the same skins, meshes, and other 3d graphics objects.  Two craft instantiated with different "skins" will share the same textures except for textures that differ between skins.

## Opt format considerations

* Generally, each OPT file represents one ship or projectile model.
* All points in the file share the same origin.
* Point space in OPT files is: forward is -y, right is +x, up is +z.  `Vector3`s will likely need to be rotated into the engine's native coordinate system.
* At the top level of the file, each `BranchNode` represent a part of the craft, even if the craft only has one part.
  * Texture data and pallet data can be shared between parts. They are usually defined in the first part that uses them, and refereced in subsequent parts.
  * Each part has exactly one set of `MeshVerticies`, `VertexNormals`, and `VertexUV`s.
  * Polygons are stored as triangles or quads as a list of `int` refences to the part's verticies, normals, and UV coordinates.
  * Hardpoints (Turrets, missile launchers, etc) are stored under the part they are attached to.
  * Parts usually contain a `PartDescriptor` describing its hit box, explosion type, and part type and center point for targeting purposes.
* In some cases there are textures at the top level if they are shared between multiple parts.
* Texture UVs and normals are stored per point on a face, not per vertex.  In some engines (E.G. Unity), this means that vertex data will need to be split (duplicated) in order to use a different UV or or normal on the same vertex.

### Skins

`skin` for some OPT models changes the color and/or logos on a craft for different flight groups.  EG in XWing.opt, 0 = Red, 1 = Gold, 2 = Blue, and 3 = Green.

![Skins 0-3 for TIEFTR.OPT and XWING.OPT (TIE98)](https://i.imgur.com/8Yjh3E8.png)

## Navigating Node Objects

All node objects with children implement a recursive `IEnumerable` interface.  For non-recursive child enumeration, enumerate directly on the child fields, `OptFile.RootNodes` and `BranchNode.Children`.

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
