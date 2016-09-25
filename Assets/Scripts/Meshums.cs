using UnityEngine;
using System.Collections.Generic;
using Nurbz;

public class Meshums : MonoBehaviour
{

    public Material Material;

    List<Vector3> _points = new List<Vector3>();
    List<int> _indices = new List<int>();



    // Use this for initialization
    void Start()
    {

        Vector3[] vertices3D = new Vector3[] {
            new Vector3(8.220409f,0.0f,12.651987f),
            new Vector3(6.664282f,0.0f,9.810364f),
            new Vector3(1.725271f,0.0f,9.404418f),
            new Vector3(-3.619686f,0.0f,8.321895f),
            new Vector3(-5.243471f,0.0f,5.412615f),
            new Vector3(-2.943109f,0.0f,3.991803f),
            new Vector3(1.860586f,0.0f,5.54793f),
            new Vector3(3.552028f,0.0f,6.698111f),
            new Vector3(6.664282f,0.0f,5.480272f),
            new Vector3(5.784732f,0.0f,3.179911f),
            new Vector3(3.552028f,0.0f,0.744235f),
            new Vector3(1.792929f,0.0f,-2.368019f),
            new Vector3(3.078425f,0.0f,-4.871353f),
            new Vector3(6.393651f,0.0f,-5.480272f),
            new Vector3(8.085093f,0.0f,-9.133787f),
            new Vector3(8.829328f,0.0f,-12.246041f),
            new Vector3(12.27987f,0.0f,-14.140456f),
            new Vector3(18.910323f,0.0f,-13.666852f),
            new Vector3(23.172757f,0.0f,-11.163518f),
            new Vector3(24.25528f,0.0f,-7.036399f),
            new Vector3(25.946722f,0.0f,-3.585857f),
            new Vector3(30.344472f,0.0f,-2.232704f),
            new Vector3(34.539248f,0.0f,-3.247569f),
            new Vector3(40.222494f,0.0f,-5.344957f),
            new Vector3(40.696097f,0.0f,-8.727841f),
            new Vector3(38.395736f,0.0f,-11.434149f),
            new Vector3(35.012852f,0.0f,-11.907753f),
            new Vector3(32.847806f,0.0f,-14.817033f),
            new Vector3(33.456725f,0.0f,-17.929287f),
            new Vector3(35.486456f,0.0f,-20.229648f),
            new Vector3(39.613575f,0.0f,-21.244513f),
            new Vector3(41.913936f,0.0f,-19.485413f),
            new Vector3(40.290151f,0.0f,-17.996944f),
            new Vector3(36.501321f,0.0f,-17.320367f),
            new Vector3(36.839609f,0.0f,-15.155321f),
            new Vector3(42.38754f,0.0f,-14.208114f),
            new Vector3(44.958532f,0.0f,-10.080995f),
            new Vector3(44.349612f,0.0f,-6.630453f),
            new Vector3(43.334747f,0.0f,-3.247569f),
            new Vector3(43.402405f,0.0f,0.270631f),
            new Vector3(43.673036f,0.0f,4.939011f),
            new Vector3(46.108712f,0.0f,6.833426f),
            new Vector3(46.17637f,0.0f,9.878022f),
            new Vector3(45.29682f,0.0f,12.85496f),
            new Vector3(41.237359f,0.0f,14.546402f),
            new Vector3(35.621771f,0.0f,13.463879f),
            new Vector3(30.885733f,0.0f,14.208114f),
            new Vector3(28.382399f,0.0f,16.508475f),
            new Vector3(26.149695f,0.0f,18.944152f),
            new Vector3(21.075369f,0.0f,20.500279f),
            new Vector3(18.977981f,0.0f,19.688386f),
            new Vector3(18.70735f,0.0f,17.52334f),
            new Vector3(22.360865f,0.0f,16.37316f),
            new Vector3(25.337803f,0.0f,14.884691f),
            new Vector3(27.976453f,0.0f,11.501806f),
            new Vector3(31.697625f,0.0f,11.163518f),
            new Vector3(38.192763f,0.0f,11.637122f),
            new Vector3(41.305017f,0.0f,11.231176f),
            new Vector3(40.899071f,0.0f,6.630453f),
            new Vector3(37.922132f,0.0f,2.232704f),
            new Vector3(31.494652f,0.0f,1.082523f),
            new Vector3(27.096903f,0.0f,1.962073f),
            new Vector3(22.428523f,0.0f,3.991803f),
            new Vector3(19.316269f,0.0f,6.630453f),
            new Vector3(19.654557f,0.0f,11.772437f),
            new Vector3(13.700681f,0.0f,14.884691f),
            new Vector3(10.114824f,0.0f,18.538206f),
            new Vector3(10.859058f,0.0f,21.718117f),
            new Vector3(9.032301f,0.0f,25.980551f),
            new Vector3(4.296263f,0.0f,26.645509f),
            new Vector3(1.368136f,0.0f,30.464805f),
            new Vector3(-0.286892f,0.0f,35.684509f),
            new Vector3(-3.978879f,0.0f,40.522285f),
            new Vector3(-1.687301f,0.0f,45.36006f),
            new Vector3(-2.57847f,0.0f,52.489412f),
            new Vector3(-6.143146f,0.0f,57.454497f),
            new Vector3(-14.545598f,0.0f,56.945258f),
            new Vector3(-22.820739f,0.0f,52.489412f),
            new Vector3(-25.494247f,0.0f,44.214271f),
            new Vector3(-26.640036f,0.0f,36.702988f),
            new Vector3(-22.3115f,0.0f,29.700946f),
            new Vector3(-21.165711f,0.0f,22.698903f),
            new Vector3(-25.748866f,0.0f,18.243057f),
            new Vector3(-33.642078f,0.0f,17.606508f),
            new Vector3(-38.479854f,0.0f,19.134226f),
            new Vector3(-45.099967f,0.0f,19.516156f),
            new Vector3(-48.537333f,0.0f,12.896043f),
            new Vector3(-46.627685f,0.0f,6.275929f),
            new Vector3(-43.063009f,0.0f,3.220492f),
            new Vector3(-40.516811f,0.0f,0.546985f),
            new Vector3(-43.699558f,0.0f,-3.145001f),
            new Vector3(-50.192362f,0.0f,-7.600847f),
            new Vector3(-55.666686f,0.0f,-4.29079f),
            new Vector3(-61.77756f,0.0f,-8.873945f),
            new Vector3(-62.923349f,0.0f,-17.912946f),
            new Vector3(-58.594813f,0.0f,-20.077214f),
            new Vector3(-51.46546f,0.0f,-22.241482f),
            new Vector3(-38.989093f,0.0f,-21.732242f),
            new Vector3(-32.623599f,0.0f,-24.787679f),
            new Vector3(-32.24167f,0.0f,-30.007384f),
            new Vector3(-29.440853f,0.0f,-34.972469f),
            new Vector3(-33.769388f,0.0f,-40.574103f),
            new Vector3(-40.007572f,0.0f,-43.37492f),
            new Vector3(-43.991423f,0.0f,-48.080685f),
            new Vector3(-41.633832f,0.0f,-52.167174f),
            new Vector3(-36.918652f,0.0f,-52.324347f),
            new Vector3(-32.989335f,0.0f,-47.451994f),
            new Vector3(-27.016773f,0.0f,-43.994195f),
            new Vector3(-24.973528f,0.0f,-38.650324f),
            new Vector3(-24.50201f,0.0f,-32.363417f),
            new Vector3(-19.000967f,0.0f,-28.276927f),
            new Vector3(-18.686621f,0.0f,-21.99002f),
            new Vector3(-24.187665f,0.0f,-22.147192f),
            new Vector3(-26.702428f,0.0f,-26.233682f),
            new Vector3(-30.003054f,0.0f,-20.26112f),
            new Vector3(-33.30368f,0.0f,-18.217875f),
            new Vector3(-35.189753f,0.0f,-10.987932f),
            new Vector3(-31.889126f,0.0f,-6.901442f),
            new Vector3(-25.130701f,0.0f,-6.587097f),
            new Vector3(-25.602219f,0.0f,-2.029089f),
            new Vector3(-30.474572f,0.0f,2.371746f),
            new Vector3(-32.832162f,0.0f,5.829545f),
            new Vector3(-30.160227f,0.0f,10.073207f),
            new Vector3(-21.201384f,0.0f,10.387552f),
            new Vector3(-19.629657f,0.0f,4.729336f),
            new Vector3(-22.301593f,0.0f,-1.400399f),
            new Vector3(-22.615938f,0.0f,-8.158824f),
            new Vector3(-19.472485f,0.0f,-13.18835f),
            new Vector3(-11.456678f,0.0f,-15.860285f),
            new Vector3(-3.912389f,0.0f,-14.288558f),
            new Vector3(3.317554f,0.0f,-23.404574f),
            new Vector3(10.075979f,0.0f,-25.762164f),
            new Vector3(10.861843f,0.0f,-19.789602f),
            new Vector3(4.26059f,0.0f,-12.559659f),
            new Vector3(-0.611763f,0.0f,-7.215788f),
            new Vector3(-10.513642f,0.0f,-2.343435f),
            new Vector3(-11.613851f,0.0f,5.672372f),
            new Vector3(-8.784742f,0.0f,11.173416f),
            new Vector3(-0.165157f,0.0f,15.845201f),
            new Vector3(5.197135f,0.0f,15.020233f)
        };

        var polyLine = new Polyline3(vertices3D,true);

        var innerPoly = polyLine.OffsetInPlane(-0.2f);

        var innerLines = innerPoly.GetLines();


        foreach(var line in innerLines)
        {
            MeshMaker(line, Vector3.up);

        };






        var outerPoly = polyLine.OffsetInPlane(0.2f);

        var triangulator = new Triangulatron(outerPoly);
        var bits = triangulator.Triangulate();

        var count = _points.Count;

        for (var x = 0; x < bits.Length; x++)
        {

            bits[x] += count;
        }

        _points.AddRange(outerPoly.Vectors);
        _indices.AddRange(bits);

        outerPoly.Flip();

        var outerLines = outerPoly.GetLines();

        foreach (var line in outerLines)
        {
            MeshMaker(line, Vector3.up);

        };



        outerPoly.Flip();

        outerLines = outerPoly.GetLines();




        for (var i = 0; i< innerLines.Length; i++)
        {
            MeshMakerCap(innerLines[i], outerLines[i], Vector3.up);

        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = _points.ToArray();
        msh.triangles = _indices.ToArray();
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        //AutoWeldBox.AutoWeld(msh, 0.1f, 5f);

        // Set up game object with mesh;
        var renderer = gameObject.AddComponent<MeshRenderer>();
        var filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = msh;
        renderer.sharedMaterial = Material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;


        //offsetPolyline.Debugdraw();



    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MeshMaker(Line3 line, Vector3 extrusionAngle)
    {

        var count = _points.Count;

        Vector3[] points = { line.start, line.start + extrusionAngle, line.end + extrusionAngle, line.end };
        int[] indices = { count, count + 1, count + 2, count, count + 2, count + 3 };

        _points.AddRange(points);
        _indices.AddRange(indices);

    }

    public void MeshMaker(Line3 lineA, Line3 lineB)
    {

        var count = _points.Count;

        Vector3[] points = { lineA.start, lineB.start, lineB.end, lineA.end };
        int[] indices = { count, count + 1, count + 2, count, count + 2, count + 3 };

        _points.AddRange(points);
        _indices.AddRange(indices);

    }

    public void MeshMakerCap(Line3 lineA, Line3 lineB, Vector3 extrusionAngle)
    {

        var count = _points.Count;

        Vector3[] points = { lineA.start+ extrusionAngle, lineB.start + extrusionAngle, lineB.end + extrusionAngle, lineA.end + extrusionAngle };
        int[] indices = { count, count + 1, count + 2, count, count + 2, count + 3 };

        _points.AddRange(points);
        _indices.AddRange(indices);

    }
}

