using UnityEngine;
using System.Collections;

public class BruceChecker : MonoBehaviour
{

    public bool PerformLayerMask = false;

    public bool IsGrounded = false;
    public bool IsObstructed = false;

    void Update()
    {
        CheckIsGrounded();
        CheckIsObstructed();
    }

    void CheckIsGrounded()
    {
        RaycastHit hit;
        if (PerformLayerMask)
        {
            if (Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, 0.6f, 1 << 12))
            {
                IsGrounded = true;
            }
            else IsGrounded = false;
        }
        else
        {
            if (Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, 0.6f))
            {
                IsGrounded = true;
            }
            else IsGrounded = false;
        }
    }
    void CheckIsObstructed()
    {
        //var p1 = transform.position + new Vector3(0f, 0.5f, 0f);
        //var p2 = transform.position + new Vector3(0f, -0.5f, 0f);

        /*

        RaycastHit hit;
        if (Physics.CapsuleCast(p1,p2, 1f, transform.forward, out hit, 0.5f))
        {
            IsObstructed = true;
        }
        else IsObstructed = false;
        */

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 1f, transform.forward, out hit, 1f))
        {
            IsObstructed = true;
        }
        else IsObstructed = false;
    }
}
