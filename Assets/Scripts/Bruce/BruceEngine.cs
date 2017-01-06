using UnityEngine;
using System.Collections;

#pragma warning disable 0414


public class BruceEngine : MonoBehaviour
{

    //public GameObject Animations;

    string _stateName = "Default";

    GameObject GameCamera;
    Rigidbody _rb;
    //Animator _animator;

    public float speed = 0.05f;
    public float rSpeed = 0.05f;

    // Use this for initialization
    void Start()
    {
        // Set up gameCamera and RigidBody
        GameCamera = GameObject.Find("Main Camera");
        _rb = GetComponent<Rigidbody>();
        //_animator = Animations.GetComponent<Animator>();

        //Debug.DrawLine(transform.position,groundHit.point);
    }

    // Update is called once per frame

    void FixedUpdate()
    {
        RunState();
    }

    //RUN AND SET STATE

    public void SetState(string setState)
    {
        _stateName = setState;
    }

    void RunState()
    {
        switch (_stateName)
        {
            case "Default": DefaultMovement(); break;
            case "Midair": MidairMovement(); break;
            //case "Glide": GlideMovement(); break;
            case "Jump": JumpMovement(); break;
            //case "Charge": ChargeMovement(); break;
            default: DefaultMovement(); break;
        }
    }

    //ALL STATES

    void DefaultMovement()
    {
        //GameObject.Find("Flyer").GetComponent<MeshRenderer>().enabled = false;

        _rb.useGravity = true;

        Vector3 inputvector = GetMovement();

        //MoveMan(inputvector);

        Vector3 translation = GameCamera.transform.TransformDirection(inputvector);

        translation = new Vector3(translation.x * speed, _rb.velocity.y, translation.z * speed);

        _rb.velocity = translation;
        RotateTowards(translation);

        ObstacleCheck();		
    }

    
    void MidairMovement()
    {
        //GameObject.Find("Flyer").GetComponent<MeshRenderer>().enabled = false;

        _rb.useGravity = true;

        Vector3 inputvector = GetMovement();

        Vector3 translation = GameCamera.transform.TransformDirection(inputvector);

        translation = new Vector3(translation.x * speed, _rb.velocity.y, translation.z * speed);

        _rb.velocity = translation;
        RotateTowards(translation);

        ObstacleCheck();
    }
    

    bool _hasRun = false;

    
    void JumpMovement()
    {
        _rb.useGravity = true;

        Debug.Log("I JUMPED");

        Vector3 inputvector = GetMovement();
        Vector3 translation = GameCamera.transform.TransformDirection(inputvector);

        //_animator.SetTrigger("jump");

        translation = new Vector3(translation.x * speed, 12f, translation.z * speed);

        _rb.velocity = translation;
        RotateTowards(translation);

        //ObstacleCheck();
        SetState("Midair");
    }
    

    /*
    void GlideMovement()
    {
        _rb.useGravity = false;

        GameObject.Find("Flyer").GetComponent<MeshRenderer>().enabled = true;

        float glideSpeed = speed * 1.3f;
        float glideRotationDampening = 0.12f;

        Vector3 inputvector = GetMovement();

        //MoveMan(inputvector);

        Vector3 translation = GameCamera.transform.TransformDirection(inputvector);
        Vector3 glideRotation = new Vector3(translation.x, 0, translation.z);
        RotateTowards(glideRotation, glideRotationDampening);

        Vector3 forwardVector = gameObject.transform.forward;
        Vector3 glideTranslation = new Vector3(forwardVector.x * glideSpeed, -4f, forwardVector.z * glideSpeed);
        _rb.velocity = glideTranslation;

        ObstacleCheck();
    }

    void ChargeMovement()
    {
        _rb.useGravity = true;

        float glideSpeed = speed * 1.6f;
        float glideRotationDampening = 0.3f;

        Vector3 inputvector = GetMovement();

        Vector3 translation = GameCamera.transform.TransformDirection(inputvector);
        Vector3 glideRotation = new Vector3(translation.x, 0, translation.z);
        RotateTowards(glideRotation, glideRotationDampening);

        Vector3 forwardVector = gameObject.transform.forward;
        Vector3 glideTranslation = new Vector3(forwardVector.x * glideSpeed, -4f, forwardVector.z * glideSpeed);
        _rb.velocity = glideTranslation;

        ObstacleCheck();
    }
    */

    //HELPERS

    Vector3 GetMovement()
    {
        float xAxis = 0f;
        float yAxis = 0f;

        xAxis = Input.GetAxis("Horizontal");
        yAxis = Input.GetAxis("Vertical");
        //Debug.Log(new Vector3(xAxis, 0, yAxis));
        return new Vector3(xAxis, 0, yAxis);
    }

    void RotateTowards(Vector3 rTowards, float dampening = 1.0f)
    {

        rTowards = new Vector3(rTowards.x, 0, rTowards.z);

        float step = rSpeed * dampening * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, rTowards, step, 0.0F);
        Debug.DrawRay(transform.position, newDir, Color.red);
        transform.rotation = Quaternion.LookRotation(newDir);

    }
    
    
    void ObstacleCheck()
    {
        Vector3 referencePoint = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 0.95f, gameObject.transform.position.z);
        Ray wallFind = new Ray(referencePoint, gameObject.transform.forward);
        Debug.DrawRay(referencePoint, gameObject.transform.forward, Color.green);

        RaycastHit wallHit;

        if (Physics.Raycast(wallFind, out wallHit, 1.5f))
        {
            Vector3 translation = new Vector3(0, _rb.velocity.y, 0);
            _rb.velocity = translation;
        }
    }
    
    /*
    void MoveMan(Vector3 inputvector)
    {
        if (inputvector.x < -0)
        {
            _animator.SetBool("turnLeft", true);
            _animator.SetBool("turnRight", false);
        }
        else if (inputvector.x > 0)
        {
            _animator.SetBool("turnRight", true);
            _animator.SetBool("turnLeft", false);
        }
        else
        {
            _animator.SetBool("turnLeft", false);
            _animator.SetBool("turnRight", false);
        }
    }
    */
}
