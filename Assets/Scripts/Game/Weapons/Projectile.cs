using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody m_Rigidbody;
    private WeaponInfo m_WeaponInfo;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void Initalize(WeaponInfo weaponInfo, Vector3 aimDirection)
    {
        m_WeaponInfo = weaponInfo;

        m_Rigidbody.mass = weaponInfo.Mass;
        m_Rigidbody.AddForce(aimDirection * weaponInfo.InitialForce);
    }

	private void OnCollisionEnter(Collision collision)
	{
        foreach(GameObject g in m_WeaponInfo.OnHitSpawnObjects)
        {
            Instantiate(g, transform.position, Quaternion.Euler(0, 0, 0));
        }

        Destroy(gameObject);
	}
}
