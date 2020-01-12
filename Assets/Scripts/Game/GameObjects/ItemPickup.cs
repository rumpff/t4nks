using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        Weapon
    };
    public enum PickupState
    {
        Active,
        Cooldown
    };

    [SerializeField]
    private PickupType m_PickupType;
    private PickupItem m_PickupItem;

    [SerializeField]
    [Tooltip("Only needed for weapon pickups")]
    private WeaponProperties m_WeaponType;

    private Transform m_ObjectParent;

    private GameObject m_ExplosionObject;
    private ExplosionProperties m_ExplosionProperties;

    // Animation Values
    private const float BaseHeight = 6.0f;
    private const float WaveHeight = 2.0f;
    private const float WaveRotation = 20.0f;
    private const float HeightSpeed = 5.0f;
    private const float RotateSpeed = 40.0f;

    // Start is called before the first frame update
    private void Start()
    {
        m_ObjectParent = transform.Find("PickupObject");
        m_ObjectParent.GetComponent<ItemPickupCollider>().PickupEvent += OnPickup;

        m_ExplosionObject = Resources.Load("Prefabs/Explosions/PickupExplosion") as GameObject; 
        m_ExplosionProperties = Resources.Load("Properties/Explosions/PickupExplosion") as ExplosionProperties;

        // Assign the pickup item
        switch (m_PickupType)
        {
            case PickupType.Health:
                m_PickupItem = new HealthPickup();
                break;

            case PickupType.Weapon:
                m_PickupItem = new WeaponPickup();
                break;

            default:
                Debug.LogError("Missing pickup type!");
                break;
        }

        m_PickupItem.Init(this);

        // Set the model
        GameObject pickupModel = m_PickupItem.GetPickupModel();
        GameObject obj = Instantiate(pickupModel, m_ObjectParent);

        obj.transform.localPosition = Vector3.zero;
    }

    private void OnDestroy()
    {
        m_ObjectParent.GetComponent<ItemPickupCollider>().PickupEvent -= OnPickup;
    }

    // Update is called once per frame
    private void Update()
    {
        switch (m_PickupItem.GetState())
        {
            case PickupState.Active:
                UpdatePickupModelTransform();
                break;

            case PickupState.Cooldown:
                HidePickupModel();
                break;
        }
    }

    private void UpdatePickupModelTransform()
    {
        m_ObjectParent.transform.localPosition = new Vector3()
        {
            x = 0,
            y = (BaseHeight + (Mathf.Sin(Time.time * HeightSpeed) * WaveHeight)),
            z = 0
        };

        m_ObjectParent.transform.localEulerAngles = new Vector3()
        {
            x = Mathf.Sin(Time.time) * WaveRotation,
            y = Time.time * RotateSpeed,
            z = 0
        };

        m_ObjectParent.transform.localScale = Vector3.one;
    }

    private void HidePickupModel()
    {
        m_ObjectParent.transform.localScale = Vector3.zero;
    }

    private void OnPickup(Player p)
    {
        StartCoroutine(m_PickupItem.OnPickup(p));
    }

    public void CreatePickupExplosion(Player owner)
    {
        Explosion explsn = (Instantiate(m_ExplosionObject, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject).GetComponent<Explosion>();
        explsn.Initalize(owner, m_ExplosionProperties, new HitProperties(false));
    }


    public WeaponProperties WeaponProperties
    {
        get { return m_WeaponType; }
    }
}

public class PickupItem
{
    protected ItemPickup m_ItemPickup;
    protected ItemPickup.PickupState m_State;

    public virtual void Init(ItemPickup itemPickup)
    {
        m_ItemPickup = itemPickup;
        m_State = ItemPickup.PickupState.Active;
    }

    public virtual GameObject GetPickupModel()
    {
        Debug.LogError("Pickup model has not been set!");
        return null;
    }

    public virtual ItemPickup.PickupState GetState()
    {
        return m_State;
    }

    public virtual IEnumerator OnPickup(Player p)
    {
        Debug.LogError("No pickup behaviour has been set!");
        yield return null;
    }
}

public class HealthPickup : PickupItem
{
    public override GameObject GetPickupModel()
    {
        return Resources.Load("Prefabs/GameObjects/HealthBlock") as GameObject;
    }

    public override IEnumerator OnPickup(Player p)
    {
        if (m_State == ItemPickup.PickupState.Cooldown)
            yield break;

        m_State = ItemPickup.PickupState.Cooldown;

        // Add health to the player
        if (p != null)
            p.Health.AddHealth(GameManager.I.Rules.HealthPickupHealthAmount);

        // Handle the pickup
        m_ItemPickup.CreatePickupExplosion(p);

        yield return new WaitForSeconds(GameManager.I.Rules.HealthPickupRespawnTime);

        m_State = ItemPickup.PickupState.Active;
    }
}

public class WeaponPickup : PickupItem
{
    public override GameObject GetPickupModel()
    {
        return m_ItemPickup.WeaponProperties.BarrelPrefab;
    }

    public override IEnumerator OnPickup(Player p)
    {
        if (m_State == ItemPickup.PickupState.Cooldown)
            yield break;

        m_State = ItemPickup.PickupState.Cooldown;

        if (p != null)
            p.Weapon.SwitchWeapon(m_ItemPickup.WeaponProperties);

        // Handle the pickup
        m_ItemPickup.CreatePickupExplosion(p);

        yield return new WaitForSeconds(GameManager.I.Rules.HealthPickupRespawnTime);

        m_State = ItemPickup.PickupState.Active;
    }
}