using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Debris : MonoBehaviour
{
    Light m_Light;
    Renderer m_Renderer;

    Color m_BaseColor;
    float m_BaseIntensity;

    void Awake()
    {
        m_Light = GetComponent<Light>();
        Assert.IsNotNull(m_Light);
        if (m_Light)
        {
            m_BaseIntensity = m_Light.intensity;
        }

        m_Renderer = GetComponent<Renderer>();
        Assert.IsNotNull(m_Renderer);
    }

    public void SetColor(float H)
    {
        m_BaseColor = HSVToRGB(H, 1f, 1f);

        SetIntensity(1);
    }

    public void SetIntensity(float intensity)
    {
        Assert.IsNotNull(m_Light);
        if (m_Light)
        {
            m_Light.color = m_BaseColor;
            m_Light.intensity = m_BaseIntensity * intensity;
        }

        Assert.IsNotNull(m_Renderer);
        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_TintColor", m_BaseColor * intensity);
        }
    }

    // ganked shamelessly from http://answers.unity3d.com/questions/701956/hsv-to-rgb-without-editorguiutilityhsvtorgb.html
    private static Color HSVToRGB(float H, float S, float V)
    {
        if (S == 0f)
        {
            return new Color(V, V, V);
        }
        else if (V == 0f)
        {
            return Color.black;
        }
        else
        {
            Color col = Color.black;
            float Hval = H * 6f;
            int sel = Mathf.FloorToInt(Hval);
            float mod = Hval - sel;
            float v1 = V * (1f - S);
            float v2 = V * (1f - S * mod);
            float v3 = V * (1f - S * (1f - mod));

            switch (sel + 1)
            {
            case 0:
                col.r = V;
                col.g = v1;
                col.b = v2;
                break;
            case 1:
                col.r = V;
                col.g = v3;
                col.b = v1;
                break;
            case 2:
                col.r = v2;
                col.g = V;
                col.b = v1;
                break;
            case 3:
                col.r = v1;
                col.g = V;
                col.b = v3;
                break;
            case 4:
                col.r = v1;
                col.g = v2;
                col.b = V;
                break;
            case 5:
                col.r = v3;
                col.g = v1;
                col.b = V;
                break;
            case 6:
                col.r = V;
                col.g = v1;
                col.b = v2;
                break;
            case 7:
                col.r = V;
                col.g = v3;
                col.b = v1;
                break;
            }
            col.r = Mathf.Clamp(col.r, 0f, 1f);
            col.g = Mathf.Clamp(col.g, 0f, 1f);
            col.b = Mathf.Clamp(col.b, 0f, 1f);
            return col;
        }
    }
}
