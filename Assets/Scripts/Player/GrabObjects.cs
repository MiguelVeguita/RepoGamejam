using UnityEngine;

public class GrabObjects : MonoBehaviour
{
    [SerializeField] GameObject point_ref;
    private GameObject object_ref;
    [SerializeField] float throwForce = 10f;
    string[] tags = { "star", "ship", "moon", "alien", "saturn" };
   
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        PlayerControllerAlt.OnThrow = ThrowObject;
    }
    private void OnDisable()
    {
        PlayerControllerAlt.OnThrow -= ThrowObject;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (object_ref != null) return;
        for (int i = 0; i < tags.Length; i++) 
        {
            if (collision.gameObject.tag == tags[i])
            {
                //collision.transform.position = point_ref.transform.position;
                object_ref = collision.gameObject;
                rb = object_ref.GetComponent<Rigidbody>();
                // rb = null;
                rb.isKinematic = true;
                
                Debug.Log("detectado");
                PlayerControllerAlt.OnGrab = GrabObject;
                //PlayerControllerAlt.OnThrow -= ThrowObject;
            }
        }

        
        
    }
    private void OnCollisionExit(Collision collision)
    {
        if (object_ref == null) return;

        for (int i = 0; i < tags.Length; i++)
        {

            if (collision.gameObject.tag == tags[i])
            {
                rb = object_ref.GetComponent<Rigidbody>();

                PlayerControllerAlt.OnGrab -= GrabObject;
                //PlayerControllerAlt.OnThrow = ThrowObject;
                //DropObject();
                // Debug.Log("saltao");
            }


        }
        
    }

    public void GrabObject()
    {
      
            object_ref.transform.SetParent(point_ref.transform);
            object_ref.transform.localPosition = Vector3.zero;
            object_ref.transform.localRotation = Quaternion.identity;
        
        
        // rb.useGravity = false;
        // rb = null;


        // rb.isKinematic = true;

        
        //Debug.Log("agarrao?");
     
    }
    
    public void DropObject()
    {
      
        object_ref.transform.SetParent(null);
        


    }
    //collision.transform.position = point_ref.transform.position;
    // collision.transform.SetParent(point_ref.transform.parent);
    public void ThrowObject()
    {
        DropObject();
        Vector3 throwDirection = point_ref.transform.forward;
        rb.isKinematic = false;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        object_ref = null;
        rb = null;
        Debug.Log("Objeto " + object_ref.name + " lanzado con fuerza: " + throwForce + " en dirección " + throwDirection);
    }
}
