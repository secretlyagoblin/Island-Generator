using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class HexInterpolator : MonoBehaviour
{

    public Mesh Sphere;
    public Material Material;
    // Start is called before the first frame update




    // Update is called once per frame
    void OnSceneGUI(SceneView sceneView)
    {
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        Debug.Log("Hit");

        if (_plane.Raycast(ray, out _enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(_enter);

            

            Graphics.DrawMeshNow(Sphere, hitPoint, Quaternion.identity);

            Gizmos.DrawWireSphere(hitPoint, 5);
        }
    }

    Plane _plane = new Plane(Vector3.up, Vector3.zero);
    float _enter = 0.0f;
     

    private void OnEnable()
    {

        Camera.onPreCull -= DrawWithCamera;
        Camera.onPreCull += DrawWithCamera;
    }

    private void OnDisable()
    {

        Camera.onPreCull -= DrawWithCamera;
    }

    private void DrawWithCamera(Camera camera)
    {

        if (!camera) return;

        var ray = camera.ScreenPointToRay(new Vector3(0.5f, 0.5f));

        if (_plane.Raycast(ray, out _enter))
        {
            Draw(camera, Matrix4x4.TRS(ray.GetPoint(_enter), Quaternion.identity, Vector3.one));
        }
    }

    private void Draw(Camera camera, Matrix4x4 matrix)
    {

        if (!Sphere) return;
        
            Graphics.DrawMesh(Sphere, matrix, Material, gameObject.layer, camera);
        
    }
}
