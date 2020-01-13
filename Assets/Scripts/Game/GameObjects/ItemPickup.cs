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
    private GameObject m_PickupModel;

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
        m_PickupModel = Instantiate(pickupModel, m_ObjectParent);

        List<Vector3> offsets = m_PickupItem.GetLocalTransformOffset();

        m_PickupModel.transform.localPosition = offsets[0];
        m_PickupModel.transform.localEulerAngles = offsets[1];
        m_PickupModel.transform.localScale = offsets[2];

        // Set light
        transform.GetComponentInChildren<Light>().color = m_PickupItem.LightColor;
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

    public void CreatePickupExplosion(Player player)
    {
        Explosion explosion = (Instantiate(m_ExplosionObject, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject).GetComponent<Explosion>();
        explosion.Initalize(player, m_ExplosionProperties, new HitProperties(false));
    }


    public WeaponProperties WeaponProperties
    {
        get { return m_WeaponType; }
    }

    public GameObject PickupModel
    {
        get { return m_PickupModel; }
    }
}

public class PickupItem
{
    public Color LightColor { get; protected set; }

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

    public virtual List<Vector3> GetLocalTransformOffset()
    {
        List<Vector3> transformOffset = new List<Vector3>();

        transformOffset.Add(Vector3.zero);
        transformOffset.Add(Vector3.zero);
        transformOffset.Add(Vector3.one);

        return transformOffset;
    }

    public virtual IEnumerator OnPickup(Player p)
    {
        Debug.LogError("No pickup behaviour has been set!");
        yield return null;
    }
}

public class HealthPickup : PickupItem
{
    public override void Init(ItemPickup itemPickup)
    {
        base.Init(itemPickup);

        LightColor = Color.cyan;
    }

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
    public override void Init(ItemPickup itemPickup)
    {
        base.Init(itemPickup);

        LightColor = new Color(0.968f, 0.545f, 0.172f);
    }

    public override GameObject GetPickupModel()
    {
        return m_ItemPickup.WeaponProperties.BarrelPrefab;
    }

    public override List<Vector3> GetLocalTransformOffset()
    {
        List<Vector3> transformOffset = new List<Vector3>();
        float scale = 2.5f;

        // Position the barrel in the center
        float z = -(Vector3.Distance(m_ItemPickup.PickupModel.transform.position, m_ItemPickup.PickupModel.transform.Find("BarrelEnd").position));

        transformOffset.Add(new Vector3(0,0,z));
        transformOffset.Add(Vector3.zero);
        transformOffset.Add(Vector3.one * scale);

        return transformOffset;
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