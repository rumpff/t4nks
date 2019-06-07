using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttitudeIndicator : MonoBehaviour
{
    public Transform yerr;

    private int m_PlayerIndex;
    private GameManager m_GameManager;
    private Camera m_Camera;
    private Camera m_PlayerCamera;

    private List<Line> m_Lines;

    // Player values
    private Vector2 m_PlayerRawAim;
    private Vector3 m_PlayerEulerAngles;

    [SerializeField]
    private Material m_LineMaterial;
    private Vector3 m_LineOrigin;

    public void Initalize(int playerIndex, GameManager gameManager)
    {
        m_PlayerIndex = playerIndex;
        m_GameManager = gameManager;
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
    }

    private void OnPostRender()
    {
        UpdatePlayerValues();
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
        }
    }

    #region line generation

    private void AddLine(Line line)
    {
        m_Lines.Add(line);
    }

    private void GenerateLines()
    {
        // Generate all elements
        GenerateAimCompass();
        GenerateWorldCompass();
        GenerateDegreesPitch();

        // Render the lines
        RenderLines(m_Lines);

        // Reset the list
        m_Lines = new List<Line>();
    }

    private void GenerateDegreesPitch()
    {
        float baseAngle = -m_PlayerEulerAngles.z;
        Vector2 center = Vector2.one * 0.5f;
        float triangleLength = 0.2f;

        #region calculate triangle center
        {
            // Create a point so far in the distance that it is technicly a point on the horizon
            Vector3 horizon = transform.position + (transform.forward * 250.0f);
            horizon.y = 0.0f;

            if (m_PlayerIndex == 0)
                horizon = yerr.position;

            center = m_PlayerCamera.WorldToScreenPoint(horizon);

            if (m_PlayerIndex == 0)
                Debug.Log(center);
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

            AddLine(new Line(center + left, center + top));
            AddLine(new Line(center + right, center + top));
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

            AddLine(new Line(center + left, center + left * 4));
            AddLine(new Line(center + right, center + right * 4));
        }
        #endregion

    }
    private void GenerateAimCompass()
    {
        #region indicator arrow
        {
            float length = 0.05f;

            // Left side
            AddLine(new Line(
                new Vector2(0, 0.8f),
                new Vector2(Mathf.Cos(135 * Mathf.Deg2Rad) * length, Mathf.Sin(135 * Mathf.Deg2Rad) * length + 0.8f)));

            // Right side
            AddLine(new Line(
                new Vector2(0, 0.8f),
                new Vector2(Mathf.Cos(45 * Mathf.Deg2Rad) * length, Mathf.Sin(45 * Mathf.Deg2Rad) * length + 0.8f)));
        }
        #endregion
        #region ruler lines
        {
            int rulerAmount = 24;
            float lineSpacing = 1.25f;

            for (int i = 0; i <= rulerAmount; i++)
            {
                float x = -(m_PlayerRawAim.x + (i - (rulerAmount / 2.0f)) / (rulerAmount / 2.0f)) * lineSpacing;
                float yLength = 0.03f;

                if(i == 0 || i == rulerAmount || i == rulerAmount / 2.0f)
                {
                    yLength = 0.05f;
                }

                Line line = new Line(x, 0.78f, x, 0.78f - yLength);
                AddLine(line);
            }
        }
        #endregion
    }
    private void GenerateWorldCompass()
    {
        Vector2 compassCenter = new Vector2(0, -1.25f);
        float innerRadius = 0.5f;

        #region indicator arrow
        {
            Vector2 arrowPoint = compassCenter;
            arrowPoint.y += (innerRadius - 0.03f);

            float arrowLength = 0.05f;

            // Left side
            AddLine(new Line(
                arrowPoint,
                new Vector2(Mathf.Cos(-135 * Mathf.Deg2Rad) * arrowLength, Mathf.Sin(-135 * Mathf.Deg2Rad) * arrowLength + arrowPoint.y)));

            // Right side
            AddLine(new Line(
                arrowPoint,
                new Vector2(Mathf.Cos(-45 * Mathf.Deg2Rad) * arrowLength, Mathf.Sin(-45 * Mathf.Deg2Rad) * arrowLength + arrowPoint.y)));
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

                AddLine(line);
            }
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
            GL.Color(lines[i].color);

            Vector3 start = lines[i].start;
            Vector3 end = lines[i].end;

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

public struct Line
{
    public Vector2 start;
    public Vector2 end;
    public Color color;

    public Line(Vector2 a, Vector2 b, Color c)
    {
        start = a;
        end = b;
        color = c;
    }

    public Line(Vector2 a, Vector2 b)
    {
        start = a;
        end = b;
        color = Color.white;
    }

    public Line(float x1, float y1, float x2, float y2)
    {
        start = new Vector2(x1, y1);
        end = new Vector2(x2, y2);
        color = Color.white;
    }

    public Line(float x1, float y1, float x2, float y2, Color c)
    {
        start = new Vector2(x1, y1);
        end = new Vector2(x2, y2);
        color = c;
    }
}