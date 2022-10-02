using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeaponIndicator : MonoBehaviour
{
    public LineRenderer renderer;
    public int resolution = 100;
    public float radius = 2f;
    public float groundOffset = 2f;
    public float completeValue = 0.98f;

    private Camera camera;
    private Vector3[] points;

    private Color2 enabledColor = new Color2(Color.white, Color.white);
    private Color2 disabledColor = new Color2(Color.clear, Color.clear);
    private bool enabled = false;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        points = new Vector3[resolution];
        renderer.positionCount = resolution;
        // Generate points for the renderer.
        for (int i = 0; i < resolution; i++)
        {
            var radians = 2 * Mathf.PI / resolution * i;
            var pos = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f);
            points[i] = pos * radius;
        }

        GameManager.Instance.sceneChange += delegate ()
        {
            // Assume we start at full bore.
            enabled = false;
            renderer.SetPositions(points);
            renderer.loop = true;
            renderer.startColor = Color.clear;
            renderer.endColor = Color.clear;
        };
    }

    void Update()
    {
        // Track the mouse
        var mousePos = Input.mousePosition;
        var worldMousePos = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camera.transform.position.y + groundOffset));

        renderer.transform.position = worldMousePos;
    }

    public void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);
        renderer.loop = value > completeValue;
        renderer.positionCount = Mathf.RoundToInt(resolution * value);
        for (int i = 0; i < renderer.positionCount; i++)
        {
            renderer.SetPosition(i, points[i]);
        }

        if (!enabled && value < completeValue)
        {
            enabled = true;
            renderer.DOColor(disabledColor, enabledColor, 0.3f)
                          .SetUpdate(true);
        }

        if (enabled && value >= completeValue)
        {
            enabled = false;
            renderer.DOColor(enabledColor, disabledColor, 0.3f)
              .SetUpdate(true);
        }
    }
}

