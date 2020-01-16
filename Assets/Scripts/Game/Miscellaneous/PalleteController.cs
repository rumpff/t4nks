using System;
using System.Collections.Generic;
using UnityEngine;

public class PalleteController : MonoBehaviour
{
    private Dictionary<ColorPallete.PalleteColor, Material> m_Materials;
    private PalleteAffect[] m_PalleteAffects;

    private void Start()
    {
        InitMaterials();
        GetPalletAffectables();
        ApplyMaterials(m_PalleteAffects);
    }

    private void InitMaterials()
    {
        Material source = Resources.Load("Materials/ToonMat") as Material;
        m_Materials = new Dictionary<ColorPallete.PalleteColor, Material>();

        foreach (ColorPallete.PalleteColor palleteColor in (ColorPallete.PalleteColor[])Enum.GetValues(typeof(ColorPallete.PalleteColor)))
        {
            m_Materials.Add(palleteColor, new Material(source));
        }
    }

    private void GetPalletAffectables()
    {
        m_PalleteAffects = GetComponentsInChildren<PalleteAffect>();
    }

    private void ApplyMaterials(PalleteAffect[] palleteAffects)
    {
        foreach(PalleteAffect p in palleteAffects)
        {
            p.SetMaterial(m_Materials[p.PalleteColor]);
        }
    }

    public void UpdateColors( ColorPallete pallete )
    {
        foreach (ColorPallete.PalleteColor palleteColor in (ColorPallete.PalleteColor[])Enum.GetValues(typeof(ColorPallete.PalleteColor)))
        {
            Color color;

            switch (palleteColor)
            {
                case ColorPallete.PalleteColor.Primary:
                    color = pallete.Primary;
                    break;

                case ColorPallete.PalleteColor.Secundary:
                    color = pallete.Secundary;
                    break;

                case ColorPallete.PalleteColor.Accesory:
                    color = pallete.Accesories;
                    break;

                case ColorPallete.PalleteColor.Highlight:
                    color = pallete.Highlight;
                    break;


                default:
                    color = Color.magenta;
                    break;

            }

            m_Materials[palleteColor].color = color;
        }
    }
}
