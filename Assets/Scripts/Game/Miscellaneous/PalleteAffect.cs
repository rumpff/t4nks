using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalleteAffect : MonoBehaviour
{
    [SerializeField]
    private ColorPallete.PalleteColor m_PalleteColor;
    private MeshRenderer m_MeshRenderer;

    private void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetMaterial(Material m)
    {
        m_MeshRenderer.material = m;
    }

    public ColorPallete.PalleteColor PalleteColor
    {
        get { return m_PalleteColor; }
    }
}
