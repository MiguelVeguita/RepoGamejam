using System;
using UnityEngine;

public class GrabObjects : MonoBehaviour
{
    [SerializeField] GameObject point_ref;
    private GameObject object_ref;
    [SerializeField] float throwForce = 10f;
    string[] tags = { "star", "ship", "moon", "alien", "saturn" };

    private Rigidbody rb_object_ref;
   
    private bool isCurrentlyHolding = false;

    void Start()
    {
    }

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
        if (object_ref != null && !isCurrentlyHolding)
        {
            PlayerControllerAlt.OnGrab -= GrabObject;
        }
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (isCurrentlyHolding)
        {
            return;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (collision.gameObject.tag == tags[i])
            {
                if (object_ref != null && object_ref != collision.gameObject)
                {
                    PlayerControllerAlt.OnGrab -= GrabObject;
                }

                object_ref = collision.gameObject;
                rb_object_ref = object_ref.GetComponent<Rigidbody>();
                
                PlayerControllerAlt.OnGrab -= GrabObject;
                PlayerControllerAlt.OnGrab += GrabObject;
                //OnGrabSound.Invoke(0);
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (isCurrentlyHolding)
        {
            return;
        }

        if (object_ref == collision.gameObject)
        {
            PlayerControllerAlt.OnGrab -= GrabObject;
           // OnGrabSound.Invoke(0);
            object_ref = null;
            rb_object_ref = null;
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (isCurrentlyHolding)
        {
            return;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (other.gameObject.tag == tags[i])
            {
                if (object_ref != null && object_ref != other.gameObject)
                {
                    PlayerControllerAlt.OnGrab -= GrabObject;
                }

                object_ref = other.gameObject;
                rb_object_ref = object_ref.GetComponent<Rigidbody>();

                PlayerControllerAlt.OnGrab -= GrabObject;
                PlayerControllerAlt.OnGrab += GrabObject;
                //OnGrabSound.Invoke(0);
                return;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isCurrentlyHolding)
        {
            return;
        }

        if (object_ref == other.gameObject)
        {
            PlayerControllerAlt.OnGrab -= GrabObject;
            // OnGrabSound.Invoke(0);
            object_ref = null;
            rb_object_ref = null;
        }
    }
    public void GrabObject()
    {
        if (object_ref == null)
        {
            return;
        }
        if (isCurrentlyHolding)
        {
            return;
        }
        if (rb_object_ref == null)
        {
            rb_object_ref = object_ref.GetComponent<Rigidbody>();
            if (rb_object_ref == null)
            {
                return;
            }
        }

        isCurrentlyHolding = true;

        object_ref.transform.SetParent(point_ref.transform);
        object_ref.transform.localPosition = Vector3.zero;
        object_ref.transform.localRotation = Quaternion.identity;

        rb_object_ref.isKinematic = true;
    }

    public void DropObject()
    {
        if (!isCurrentlyHolding || object_ref == null)
        {
            return;
        }

        if (rb_object_ref == null || rb_object_ref.gameObject != object_ref)
        {
            rb_object_ref = object_ref.GetComponent<Rigidbody>();
            if (rb_object_ref == null)
            {
                if (object_ref != null) object_ref.transform.SetParent(null);
                isCurrentlyHolding = false;
                return;
            }
        }

        object_ref.transform.SetParent(null);
        rb_object_ref.isKinematic = false;
    }

    public void ThrowObject()
    {
        if (!isCurrentlyHolding || object_ref == null)
        {
            return;
        }

        if (rb_object_ref == null || rb_object_ref.gameObject != object_ref)
        {
            rb_object_ref = object_ref.GetComponent<Rigidbody>();
            if (rb_object_ref == null)
            {
                if (object_ref != null) object_ref.transform.SetParent(null);
                isCurrentlyHolding = false;
                object_ref = null;
                return;
            }
        }

        DropObject();

        Vector3 throwDirection = point_ref.transform.forward;
        rb_object_ref.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        isCurrentlyHolding = false;

        PlayerControllerAlt.OnGrab -= GrabObject;
        object_ref = null;
        rb_object_ref = null;
    }
}