﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleEasing;

public class AttitudeIndicator : MonoBehaviour
{
    private int m_PlayerIndex;
    private GameManager m_GameManager;
    private Camera m_Camera;
    private Camera m_PlayerCamera;
    private float m_PlayerHeight;
    private Vector3 m_PlayerForward;

    [SerializeField]
    private Canvas m_AttitudeCanvas;

    private List<Line> m_Lines;

    // Player values
    private Vector2 m_PlayerRawAim;
    private Vector3 m_PlayerEulerAngles;
    private float m_PlayerVelocity;
    private float m_AimDistance;

    [SerializeField]
    private Material m_LineMaterial;
    private Vector3 m_LineOrigin;

    private bool m_IsActive;
    private float m_Interpolation;
    private float m_InterpolationReversed;
    private float m_InterpolationTimer;

    // Texts
    private TextMeshProUGUI m_AimDistanceText;
    private TextMeshProUGUI m_AimXText;
    private TextMeshProUGUI m_AimYText;
    private TextMeshProUGUI m_WorldCompassText;
    private TextMeshProUGUI m_HeightText;
    private TextMeshProUGUI m_VelocityText;

    public void Initalize(int playerIndex, GameManager gameManager)
    {
        m_PlayerIndex = playerIndex;
        m_GameManager = gameManager;

        InitText(ref m_AimDistanceText);
        InitText(ref m_AimXText);
        InitText(ref m_AimYText);
        InitText(ref m_WorldCompassText);
        InitText(ref m_HeightText);
        InitText(ref m_VelocityText);
    }

    private void InitText(ref TextMeshProUGUI text)
    {
        GameObject prefab = Resources.Load("Prefabs/UI/AttiduteText") as GameObject;

        GameObject t = Instantiate(prefab, m_AttitudeCanvas.transform);
        text = t.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Make sure that all the camera values are correct
        m_Camera = GetComponent<Camera>();
        m_Camera.nearClipPlane = 0;
        m_Camera.orthographicSize = 1;

        m_Lines = new List<Line>();
    }

    private void Update()
    {
        // Prevent the camrea from rotating
        transform.rotation = Quaternion.identity;

        UpdatePlayerValues();
        UpdateInterpolation();

        // Generate all elements
        GenerateRectile();
        GenerateAimCompass();
        GenerateHeightCompass();
        GenerateVelocityCompass();
        GenerateWorldCompass();
        GenerateDegreesPitch();
    }

    private void OnPostRender()
    {
        GenerateLines();
    }

    private void UpdatePlayerValues()
    {
        GamePlayer p = m_GameManager.Players[m_PlayerIndex];
        if (p.Player != null)
        {
            m_PlayerRawAim = p.Player.Weapon.RawAim;
            m_PlayerEulerAngles = p.Player.transform.eulerAngles;
            m_PlayerCamera = p.Camera.Camera;
            m_PlayerHeight = p.Player.GetHeightFromGround();
            m_PlayerVelocity = p.Player.transform.InverseTransformDirection(p.Player.Rigidbody.velocity).z;
            m_AimDistance = p.Player.Weapon.AimDistance();
            m_PlayerForward = p.Player.transform.forward;
        }
    }

    private void UpdateInterpolation()
    {
        int state = 0;

        if (m_GameManager.Players[m_PlayerIndex].Player != null)
            state = m_GameManager.Players[m_PlayerIndex].Player.PInput.Zoom ? 1 : -1;

        m_InterpolationTimer += state * Time.deltaTime * 2.5f;
        m_InterpolationTimer = Mathf.Clamp01(m_InterpolationTimer);

        m_Interpolation = Easing.Ease(EaseType.EaseInOutQuart, m_InterpolationTimer, 0, 1, 1);
        m_InterpolationReversed = Easing.Ease(EaseType.EaseInOutQuart, m_InterpolationTimer, 1, -1, 1);
    }

    private Vector2 CalculateRectilePosition()
    {
        if (m_GameManager.Players[m_PlayerIndex].State != PlayerState.Alive)
            return Vector2.one * 5;

        Vector3 physicalPosition = m_GameManager.Players[m_PlayerIndex].Player.Weapon.AimPosition();
        Vector3 viewportPosition = m_GameManager.Players[m_PlayerIndex].Camera.Camera.WorldToViewportPoint(physicalPosition);

        // Prevent the rectile from coming back when the position is behind the camera
        viewportPosition *= Mathf.Sign(viewportPosition.z);

        float aspect = (float)m_PlayerCamera.pixelWidth / (float)m_PlayerCamera.pixelHeight;

        viewportPosition.x *= (aspect * 2);
        viewportPosition.x -= aspect;

        viewportPosition.y *= 2;
        viewportPosition.y -= 1;


        return viewportPosition;
    }


    #region line generation

    private void AddLine(Line line)
    {
        m_Lines.Add(line);
    }

    private void AddArrow(Vector2 position, float angle, float length)
    {
        // Left side
        AddLine(new Line(
            position,
            position + new Vector2(Mathf.Cos((45 + angle + 180) * Mathf.Deg2Rad) * length, 
                                    Mathf.Sin((45 + angle + 180) * Mathf.Deg2Rad) * length)));

        // Right side
        AddLine(new Line(
            position,
            position + new Vector2(Mathf.Cos((-45 + angle + 180) * Mathf.Deg2Rad) * length,
                                    Mathf.Sin((-45 + angle + 180) * Mathf.Deg2Rad) * length)));
    }

    private void GenerateLines()
    {


        // Render the lines
        RenderLines(m_Lines);

        // Reset the list
        m_Lines = new List<Line>();
    }

    
    private void GenerateRectile()
    {
        float length = 0.08f;
        float baseDistance = Mathf.Sqrt(length * length * 2.0f);
        float distance = baseDistance + (m_InterpolationReversed * 0.1f);
        float baseAngle = 45 + (m_Interpolation * 45);

        Vector2 rectilePos = CalculateRectilePosition();

        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90) + baseAngle;


            Vector2 offset = new Vector2()
            {
                x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                y = Mathf.Sin(angle * Mathf.Deg2Rad) * distance,
            };
            AddArrow(rectilePos + offset, angle, length);
        }

        #region text
        {
            // Set the alignment
            m_AimDistanceText.alignment = TextAlignmentOptions.Center;

            // Position
            m_AimDistanceText.rectTransform.anchoredPosition = rectilePos;

            // Scale
            m_AimDistanceText.rectTransform.localScale = new Vector3(m_Interpolation, m_Interpolation, 1.0f);

            // Text
            m_AimDistanceText.text = (m_AimDistance).ToString("00");
        }
        #endregion

    }
    private void GenerateDegreesPitch()
    {
        float baseAngle = -m_PlayerCamera.transform.eulerAngles.z;
        Vector2 center = Vector2.zero;
        float triangleLength = 0.2f;

        #region calculate triangle center
        {
            // Create a point so far in the distance that it is technicly a point on the horizon
            Vector3 forwardiWan = m_PlayerForward;
            if (Input.GetKey(KeyCode.Q)) forwardiWan = m_PlayerCamera.transform.forward;

            Vector3 horizon = m_PlayerCamera.transform.position + (forwardiWan * 2500.0f);
            horizon.y = 0.0f;

            float aspect = (float)m_PlayerCamera.pixelWidth / (float)m_PlayerCamera.pixelHeight;
            center = m_PlayerCamera.WorldToViewportPoint(horizon);

            center.x *= (aspect * 2);
            center.x -= aspect;

            center.y *= 2;
            center.y -= 1;
        }
        #endregion

        #region center triangle
        {
            Vector2 top = new Vector2
            {
                x = Mathf.Cos((baseAngle + 90) * Mathf.Deg2Rad) * triangleLength,
                y = Mathf.Sin((baseAngle + 90) * Mathf.Deg2Rad) * triangleLength
            };

            Vector2 left = new Vector2
            {
                x = Mathf.Cos((baseAngle + 180) * Mathf.Deg2Rad) * triangleLength,
                y = Mathf.Sin((baseAngle + 180) * Mathf.Deg2Rad) * triangleLength
            };

            Vector2 right = new Vector2
            {
                x = Mathf.Cos(baseAngle * Mathf.Deg2Rad) * triangleLength,
                y = Mathf.Sin(baseAngle * Mathf.Deg2Rad) * triangleLength
            };

            AddLine(new Line(center + left, center + top, m_Interpolation));
            AddLine(new Line(center + right, center + top, m_Interpolation));
        }
        #endregion
        #region outer lines
        {
            Vector2 left = new Vector2
            {
                x = Mathf.Cos((baseAngle + 180) * Mathf.Deg2Rad) * triangleLength,
                y = Mathf.Sin((baseAngle + 180) * Mathf.Deg2Rad) * triangleLength
            };

            Vector2 right = new Vector2
            {
                x = Mathf.Cos(baseAngle * Mathf.Deg2Rad) * triangleLength,
                y = Mathf.Sin(baseAngle * Mathf.Deg2Rad) * triangleLength
            };

            AddLine(new Line(center + left, center + left * 4, m_Interpolation));
            AddLine(new Line(center + right, center + right * 4, m_Interpolation));
        }
        #endregion

    }
    private void GenerateAimCompass()
    {
        #region horizontal aim
        {
            float y = 0.8f;

            #region indicator arrow
            {
                float length = 0.05f * m_Interpolation;
                Vector2 position = new Vector2(0, y + 0.02f);

                AddArrow(position, -90, length);
            }
            #endregion
            #region ruler lines
            {
                int rulerAmount = 24;
                float lineSpacing = 1.25f * m_Interpolation;

                for (int i = 0; i <= rulerAmount; i++)
                {
                    float x = -(m_PlayerRawAim.x + (i - (rulerAmount / 2.0f)) / (rulerAmount / 2.0f)) * lineSpacing;
                    float yLength = 0.03f;

                    if (i == 0 || i == rulerAmount || i == rulerAmount / 2.0f)
                    {
                        yLength += 0.02f;
                    }

                    yLength *= (m_Interpolation > 0.001f ? 1 : 0);

                    Line line = new Line(x, y, x, y - yLength);
                    AddLine(line);
                }
            }
            #endregion
            #region text
            {
                // Set the alignment
                m_AimXText.alignment = TextAlignmentOptions.Center;

                // Position
                m_AimXText.rectTransform.anchoredPosition = new Vector2()
                {
                    x = 0,
                    y = y + 0.10f
                };

                // Scale
                m_AimXText.rectTransform.localScale = new Vector3(m_Interpolation, m_Interpolation, 1.0f);

                // Text
                m_AimXText.text = (m_PlayerRawAim.x).ToString("0.000");
            }
            #endregion
        }
        #endregion

        #region vertical aim
        {
            float y = 0.6f;

            #region indicator arrow
            {
                float length = 0.05f * m_Interpolation;
                Vector2 position = new Vector2(0, y - 0.02f);

                AddArrow(position, 90, length);
            }
            #endregion
            #region ruler lines
            {
                int rulerAmount = 24;
                float lineSpacing = 1.25f * m_Interpolation;

                for (int i = 0; i <= rulerAmount; i++)
                {
                    float x = (m_PlayerRawAim.y + (i - (rulerAmount / 2.0f)) / (rulerAmount / 2.0f)) * lineSpacing;
                    float yLength = 0.03f;

                    if (i == 0 || i == rulerAmount || i == rulerAmount / 2.0f)
                    {
                        yLength += 0.02f;
                    }

                    yLength *= (m_Interpolation > 0.001f ? 1 : 0);

                    Line line = new Line(x, y + yLength, x, y);
                    AddLine(line);
                }
            }
            #endregion
            #region text
            {
                // Set the alignment
                m_AimXText.alignment = TextAlignmentOptions.Center;

                // Position
                m_AimYText.rectTransform.anchoredPosition = new Vector2()
                {
                    x = 0,
                    y = y - 0.10f
                };

                // Scale
                m_AimYText.rectTransform.localScale = new Vector3(m_Interpolation, m_Interpolation, 1.0f);

                // Text
                m_AimYText.text = (m_PlayerRawAim.y * -1.0f).ToString("0.000");
            }
            #endregion
        }
        #endregion
    }
    private void GenerateHeightCompass()
    {
        float x = 1.4f;
        #region ruler lines
        {
            int rulerAmount = 100;
            float lineSpacing = 0.2f;


            for (int i = 0; i < rulerAmount; i++)
            {
;
                //float y = (m_PlayerHeight + (i - (rulerAmount / 2.0f)) / (rulerAmount / 2.0f)) * lineSpacing;
                float y = (-(m_PlayerHeight / 10.0f) + (i * lineSpacing));
                float xLength = 0.03f;

                if (i == 0)
                {
                    xLength += 0.03f;
                }

                xLength *= m_Interpolation;

                Line line = new Line(x, y, x - xLength, y);
                AddLine(line);
            }
        }
        #endregion
        #region indicator arrow
        {
            Vector2 position = new Vector2()
            {
                x = x + 0.04f,
                y = 0f
            };

            float arrowLength = 0.05f * m_Interpolation;

            AddArrow(position, -180, arrowLength);
        }
        #endregion
        #region text
        {
            // Set the alignment
            m_HeightText.alignment = TextAlignmentOptions.Left;

            // Position
            m_HeightText.rectTransform.anchoredPosition = new Vector2()
            {
                x = x + 0.12f,
                y = 0f
            };

            // Scale
            m_HeightText.rectTransform.localScale = new Vector3(m_Interpolation, m_Interpolation, 1.0f);

            // Text
            m_HeightText.text = m_PlayerHeight.ToString("0.00 m");
        }
        #endregion

    }
    private void GenerateVelocityCompass()
    {
        float x = -1.4f;
        #region ruler lines
        {
            int rulerAmount = 50;
            float lineSpacing = 0.2f;


            for (int i = -rulerAmount; i < rulerAmount; i++)
            {
                //float y = (m_PlayerHeight + (i - (rulerAmount / 2.0f)) / (rulerAmount / 2.0f)) * lineSpacing;
                float y = (-(m_PlayerVelocity / 10.0f) + (i * lineSpacing));
                float xLength = 0.03f;

                if (i == 0)
                {
                    xLength += 0.03f;
                }

                xLength *= m_Interpolation;

                Line line = new Line(x, y, x + xLength, y);
                AddLine(line);
            }
        }
        #endregion

        #region indicator arrow
        {
            Vector2 position = new Vector2()
            {
                x = x - 0.04f,
                y = 0f
            };

            float arrowLength = 0.05f * m_Interpolation;

            AddArrow(position, 0, arrowLength);
        }
        #endregion
        #region text
        {
            // Set the alignment
            m_VelocityText.alignment = TextAlignmentOptions.Right;

            // Position
            m_VelocityText.rectTransform.anchoredPosition = new Vector2()
            {
                x = x - 0.12f,
                y = 0f
            };

            // Scale
            m_VelocityText.rectTransform.localScale = new Vector3(m_Interpolation, m_Interpolation, 1.0f);

            // Text
            m_VelocityText.text = (m_PlayerVelocity * 3.6f).ToString("0.00 km/h");
        }
        #endregion
    }
    private void GenerateWorldCompass()
    {
        Vector2 compassCenter = new Vector2(0, -1.25f);
        float innerRadius = 0.5f * m_Interpolation;

        #region indicator arrow
        {
            Vector2 arrowPoint = compassCenter;
            arrowPoint.y += (innerRadius - 0.03f);

            float arrowLength = 0.05f;

            AddArrow(arrowPoint, 90, arrowLength);
        }
        #endregion

        #region compass ring
        {
            int rulerAmount = 36;
            float lineLength = 0.05f;

            for (int i = 0; i < rulerAmount; i++)
            {
                // The angle the line represents
                float angle = (360 / rulerAmount) * i;

                // The angle of the line in the compass
                float angleLocation = angle + m_PlayerEulerAngles.y;

                float length = lineLength;

                if (angle % 90 == 0)
                    length += 0.03f;
                else if (angle % 45 == 0)
                    length += 0.15f;

                Line line = new Line();

                line.start.x = Mathf.Cos(angleLocation * Mathf.Deg2Rad) * innerRadius;
                line.start.y = Mathf.Sin(angleLocation * Mathf.Deg2Rad) * innerRadius;

                line.end.x = Mathf.Cos(angleLocation * Mathf.Deg2Rad) * (innerRadius + length);
                line.end.y = Mathf.Sin(angleLocation * Mathf.Deg2Rad) * (innerRadius + length);

                // Set the global position of the compass
                line.start += compassCenter;
                line.end += compassCenter;

                line.interpolation = m_Interpolation;

                AddLine(line);
            }
        }
        #endregion

        #region text
        {
            // Set the alignment
            m_WorldCompassText.alignment = TextAlignmentOptions.Center;

            // Position
            m_WorldCompassText.rectTransform.anchoredPosition = new Vector2()
            {
                x = 0f,
                y = compassCenter.y + (innerRadius - 0.12f)
            };

            // Text
            m_WorldCompassText.text = m_PlayerEulerAngles.y.ToString("000°");
        }
        #endregion
    }

    #endregion
    #region rendering

    private void RenderLines(List<Line> lines)
    {
        m_LineOrigin = transform.position;

        GL.Begin(GL.LINES);
        m_LineMaterial.SetPass(0);

        for (int i = 0; i < lines.Count; i++)
        {
            Line l = lines[i].Lerp();

            GL.Color(l.color);

            Vector3 start = l.start;
            Vector3 end = l.end;

            // Correct for the position of the camera
            start += m_LineOrigin;
            end += m_LineOrigin;

            GL.Vertex(start);
            GL.Vertex(end);
        }

        GL.End();
    }

    #endregion
}

public class Line
{
    public Vector2 start;
    public Vector2 end;
    public Color color;
    public float interpolation;

    public Line()
    {
        start = Vector2.zero;
        end = Vector2.zero;
        color = Color.white;
        interpolation = 1;
    }

    public Line(Vector2 a, Vector2 b, Color c)
    {
        start = a;
        end = b;
        color = c;
        interpolation = 1;
    }

    public Line(Vector2 a, Vector2 b)
    {
        start = a;
        end = b;
        color = Color.white;
        interpolation = 1;
    }

    public Line(float x1, float y1, float x2, float y2)
    {
        start = new Vector2(x1, y1);
        end = new Vector2(x2, y2);
        color = Color.white;
        interpolation = 1;
    }

    public Line(float x1, float y1, float x2, float y2, Color c)
    {
        start = new Vector2(x1, y1);
        end = new Vector2(x2, y2);
        color = c;
        interpolation = 1;
    }

    public Line(Vector2 a, Vector2 b, Color c, float i)
    {
        start = a;
        end = b;
        color = c;
        interpolation = i;
    }

    public Line(Vector2 a, Vector2 b, float i)
    {
        start = a;
        end = b;
        color = Color.white;
        interpolation = i;
    }

    public Line(float x1, float y1, float x2, float y2, float i)
    {
        start = new Vector2(x1, y1);
        end = new Vector2(x2, y2);
        color = Color.white;
        interpolation = i;
    }

    public Line(float x1, float y1, float x2, float y2, Color c, float i)
    {
        start = new Vector2(x1, y1);
        end = new Vector2(x2, y2);
        color = c;
        interpolation = i;
    }

    public Line Lerp()
    {
        Line l = this;

        l.end = Vector2.Lerp(l.start, l.end, this.interpolation);

        return l;
    }
}