using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class Player : MonoBehaviour
{
    private readonly string m_DriveableTag = "Driveable";

    [SerializeField]
    private XboxController m_Controller;

    [SerializeField]
    private Collider[] m_TireColliders; // Used for initalization only
    private TankTire[] m_TankTires;

    private Rigidbody m_RigidBody;

    private void Start()
    {
        m_RigidBody = GetComponent<Rigidbody>();
        m_TankTires = CreateTankTires(m_TireColliders);
    }

    private void Update()
    {
        //if(IsOnDriveable())
        {
            transform.Translate(new Vector3(XCI.GetAxis(XboxAxis.LeftStickX, m_Controller), 0, XCI.GetAxis(XboxAxis.LeftStickY, m_Controller)) * 0.4f);
        }
        //else
        {

        }

    }

    /*
    private bool IsOnDriveable()
    {
        foreach(TankTire t in m_TankTires)
        {
            if (t.IsOnDriveable)
                return true;
        }

        return false;
    }
    */

    /*
    private void OnCollisionEnter(Collision collision)
    {
        foreach(Collider t in m_TankTires)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                if(t.GetType() == collision.contacts[0].thisCollider)
            }

            if (collision.transform.tag == m_DriveableTag)
                IsOnDriveable = true;
        }       
    }
    */

    private TankTire[] CreateTankTires(Collider[] colliders)
    {
        TankTire[] tires = new TankTire[colliders.Length];

        for (int i = 0; i < colliders.Length; i++)
        {
            tires[i] = new TankTire(colliders[i]);
        }

        return tires;
    }
}


public struct TankTire
{
    public Collider Collider;
    public bool OnDriveable;

    public TankTire(Collider collider)
    {
        Collider = collider;
        OnDriveable = false;
    }
}