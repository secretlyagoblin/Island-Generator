using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using U3D.Threading.Tasks;

namespace LevelGenerator {
    public abstract class LevelGenerator {

        public GameObject Root;

        protected LevelGeneratorSettings _settings;
        protected MeshTile _meshTile;

        private readonly Material _mat;

        Queue<CleverMesh> _workQueue = new Queue<CleverMesh>();

        // Use this for initialization

        protected LevelGenerator(LevelGeneratorSettings settings) {
            RNG.DateTimeInit();
            _settings = settings;
            _meshTile = new MeshTile(settings.MeshTileData.text);
            _mat = new Material(Shader.Find("Standard"));
            Root = new GameObject();
        }

        public bool DequeueAsyncMesh()
        {
            if(_workQueue == null)
            {
                return false;
            }

            if (_workQueue.Count == 0)
                return false;

            FinaliseMesh(_workQueue.Dequeue());
            return true;
        }

        public abstract void Generate();

        List<Color> _createObjectColors = new List<Color>(100);

        protected GameObject CreateObjectXY(CleverMesh mesh)
        {
            var gameObject = GameObject.Instantiate(_settings.TemplateObject, Root.transform);
            var f = gameObject.GetComponent<MeshFilter>();
            var r = gameObject.GetComponent<MeshRenderer>();
            r.sharedMaterial = _settings.MeshColourMaterial;
            //f.mesh = mesh.Mesh.ToXZMesh(mesh.NodeMetadata.Select(x => -Mathf.InverseLerp(0f,0.05f,Mathf.Min(x.SmoothColor.grayscale,0.05f))*0.2f).ToArray());
            f.mesh = mesh.Mesh.ToXYMesh();
            _createObjectColors.Clear();

            try
            {
                for (int i = 0; i < mesh.NodeMetadata.Length; i++)
                {
                    _createObjectColors.Add(mesh.NodeMetadata[i].SmoothColor);
                }

                if (mesh.NodeMetadata.Length == f.mesh.vertices.Length)
                {
                    f.mesh.SetColors(_createObjectColors);
                }
            }
            catch
            {
                //Debug.LogError("No colours to add");
            }

            return gameObject;
        }

        protected GameObject CreateObjectXZ(CleverMesh mesh)
        {
            var gameObject = GameObject.Instantiate(_settings.TemplateObject, Root.transform);
            var f = gameObject.GetComponent<MeshFilter>();
            var r = gameObject.GetComponent<MeshRenderer>();
            r.sharedMaterial = _settings.MeshColourMaterial;
            f.mesh = mesh.Mesh.ToXZMesh(mesh.NodeMetadata.Select(x => x.Height).ToArray());
            //f.mesh = mesh.Mesh.ToXYMesh();
            _createObjectColors.Clear();

            try
            {
                for (int i = 0; i < mesh.NodeMetadata.Length; i++)
                {
                    _createObjectColors.Add(mesh.NodeMetadata[i].SmoothColor);
                }

                if (mesh.NodeMetadata.Length == f.mesh.vertices.Length)
                {
                    f.mesh.SetColors(_createObjectColors);
                }
            }
            catch
            {
                //Debug.LogError("No colours to add");
            }

            return gameObject;
        }

        protected GameObject CreateRing(CleverMesh mesh)
        {
            var gameObject = GameObject.Instantiate(_settings.TemplateObject, Root.transform);
            var f = gameObject.GetComponent<MeshFilter>();
            var r = gameObject.GetComponent<MeshRenderer>();
            r.sharedMaterial = _settings.MeshColourMaterial;
            //f.mesh = layer5.Mesh.ToXYMesh();
            f.mesh = mesh.RingMesh.ToXYMesh();

            _createObjectColors.Clear();

            try
            {
                for (int i = 0; i < mesh.NodeMetadata.Length; i++)
                {
                    _createObjectColors.Add(mesh.NodeMetadata[i].SmoothColor);
                }

                if (mesh.NodeMetadata.Length == f.mesh.vertices.Length)
                {
                    f.mesh.SetColors(_createObjectColors);
                }
            }
            catch
            {
                Debug.LogError("No colours to add");
            }

            return gameObject;
        }

        protected GameObject CreateBaryObject(CleverMesh mesh)
        {
            var gameObject = GameObject.Instantiate(_settings.TemplateObject, Root.transform);
            var f = gameObject.GetComponent<MeshFilter>();
            var r = gameObject.GetComponent<MeshRenderer>();
            r.sharedMaterial = _settings.MeshColourMaterial;
            //f.mesh = layer5.Mesh.ToXYMesh();
            f.mesh = mesh.GetBarycenterDebugMesh();
            //f.mesh.SetColors(mesh.CellMetadata.Select(x => x.SmoothColor).ToList());

            return gameObject;
        }

        protected IEnumerator CreateSet(CleverMesh parent, Dictionary<int, List<int>> sets, float timeDelay, int batchCount)
        {
            var waitForSeconds = new WaitForSeconds(timeDelay);

            var count = 0;

            foreach (var roomCode in sets)
            {
                if (roomCode.Key == 0)
                {
                    Debug.Log("Nope!!");
                    continue;
                }

                try
                {
                    var layer4 = new CleverMesh(parent, roomCode.Value.Distinct().ToArray(), MeshMasher.NestedMeshAccessType.Triangles);

                    var go = CreateObjectXY(layer4);
                    go.name = "Region " + roomCode.Key;

                }
                catch
                {
                    Debug.Log("MeshCreationFailed");
                }

                count++;

                if (count == batchCount)
                {
                    count = 0;

                    if (timeDelay == 0)
                    {
                        yield return null;
                    }
                    else
                    {
                        yield return waitForSeconds;
                    }
                }
            }
        }

        protected abstract void FinaliseMesh(CleverMesh mesh);

        protected void CreateSetAsync(CleverMesh parent, Queue<int[]> sets, int threadCount, System.Func<CleverMesh, int[], CleverMesh> layerAction)
        {

            var iterator = 0;

            for (int i = 0; i < threadCount; i++)
            {

                Task.Run(() =>
                {
                    while (sets.Count > 0)
                    {
                        var set = sets.Dequeue();

                        try
                        {
                            var cleverMesh = layerAction(parent, set);

                            lock (_workQueue)
                            {
                                _workQueue.Enqueue(cleverMesh);
                            }
                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                    }
                }).ContinueInMainThreadWith((x) => { Debug.Log("Hell yeah we completed that one"); });
            }
        }

        protected void CreateSimpleJobAsync(CleverMesh parent, System.Func<CleverMesh,int[],CleverMesh> layerAction)
        {
            var count = parent.Mesh.Nodes.Count;

            Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    //var cleverMesh = new CleverMesh(parent, new int[] { parent.Mesh.Nodes[i].Index }, type);
                    try
                    {
                        var result = layerAction(parent,new int[] { i });


                        //var layer2cleverMesh = new CleverMesh(cleverMesh, cleverMesh.Mesh.Nodes.ConvertAll(x => x.Index).ToArray(), MeshMasher.NestedMeshAccessType.Triangles);

                        lock (_workQueue)
                        {
                            _workQueue.Enqueue(result);
                        }
                    }
                    catch (System.Exception e)
                    {
                        //var data = parent.GetDataAboutVertex(parent.Mesh.Nodes[i].Index);
                        //Debug.Log(i + " " + data[0] + " " + data[1] + " " + data[2] + " ");
                        Debug.LogError(e);
                        //lock (_workQueue)
                        //{
                        //    _workQueue.Enqueue(cleverMesh);
                        //}
                    }
                }
            });//.ContinueInMainThreadWith((x) => { Debug.Log("Task completed: " + x.IsCompleted); });

        }

        protected IEnumerator CreateSimple(CleverMesh parent, MeshMasher.NestedMeshAccessType type, float timeDelay = 0f)
        {
            var count = parent.Mesh.Nodes.Count;

            var waitForSeconds = new WaitForSeconds(timeDelay);

            for (int i = 0; i < count; i++)
            {

                var cleverMesh = new CleverMesh(parent, new int[] { parent.Mesh.Nodes[i].Index }, type);

                /*
                var cleverMeshMesh = CreateObject(cleverMesh);
                cleverMeshMesh.name = "Cell " + i;
                ;

                if (type == MeshMasher.NestedMeshAccessType.Vertex)
                {
                    var cleverMeshRing = CreateRing(cleverMesh);
                    cleverMeshRing.transform.parent = cleverMeshMesh.transform;
                }
                cleverMeshMesh.transform.Translate(Vector3.back * 0.5f);
                */

                try
                {
                    var layer2cleverMesh = new CleverMesh(cleverMesh, cleverMesh.Mesh.Nodes.ConvertAll(x => x.Index).ToArray(), MeshMasher.NestedMeshAccessType.Triangles);
                    //CreateObject(cleverMesh).name = "Cell " + i;
                    CreateObjectXY(layer2cleverMesh).name = "Cell " + i + " - 2";

                }
                catch (System.Exception e)
                {
                    var cleverMeshMesh = CreateObjectXY(cleverMesh);
                    cleverMeshMesh.name = "Cell " + i;

                    var cleverMeshRing = CreateRing(cleverMesh);
                    cleverMeshRing.transform.parent = cleverMeshMesh.transform;


                    var data = parent.GetDataAboutVertex(parent.Mesh.Nodes[i].Index);
                    Debug.Log(i + " " + data[0] + " " + data[1] + " " + data[2] + " ");
                    Debug.LogError(e);
                }

                yield return waitForSeconds;
            }
        }

        protected CleverMesh CreateSimpleMeshTile(CleverMesh parent, int[] indices)
        {
                return new CleverMesh(parent, indices, MeshMasher.NestedMeshAccessType.Triangles);
        }

    }
}
