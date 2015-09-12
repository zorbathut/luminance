using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System.Collections.Generic;

// This is kind of the "level" manager - each section is handled mostly independently. It's not really "level" because that has its own meaning in Unity, though, so I kinda had to invent a term.
public class PhaseManager : MonoBehaviour
{
    public List<Spawner> m_Spawners;

    public bool m_First;
    public PhaseManager m_Chain;

    public float m_ProgressionPermitted = 1;

    public Text m_SuccessText;
    public Text m_ContinueText;

    public Transform m_Shatter;
    public float m_ShatterSize = 1;

    public AnimationCurve m_PCCutsceneHigh;
    public AnimationCurve m_PCCutsceneLow;

    int m_DebrisTotal = 0;
    bool m_AllowRestart = false;

    void Start()
    {
        foreach (Spawner spawner in m_Spawners)
        {
            m_DebrisTotal += spawner.m_Spawns;
        }

        if (m_First)
        {
            BeginPhase();
        }
    }

    void BeginPhase()
    {
        foreach (Spawner spawner in m_Spawners)
        {
            spawner.Spawn();
        }
    }

    void EndPhase(Notamari notamari)
    {
        if (m_Chain)
        {
            StartCoroutine(PhaseChangeCutscene(notamari));
        }
        else
        {
            ShatterWorld(true, notamari.transform.position);

            StartCoroutine(EndGameCutscene());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Notamari notamari = other.GetComponent<Notamari>();
        if (notamari)
        {
            notamari.SetPhaseManager(this, Mathf.RoundToInt(m_DebrisTotal / m_ProgressionPermitted));
        }
    }

    void OnTriggerExit(Collider other)
    {
        Notamari notamari = other.GetComponent<Notamari>();
        if (notamari)
        {
            notamari.UnsetPhaseManager(this);
        }
    }

    public void NotifyGrabbed(Notamari notamari)
    {
        if (notamari.GetDebrisCount() >= m_DebrisTotal)
        {
            EndPhase(notamari);
        }
    }

    void Update()
    {
        if (m_AllowRestart && Input.GetKeyDown(KeyCode.Space))  // hardcoded key assignments, aww yeah
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    //////////////////
    // Cutscenes
    //

    void ShatterWorld(bool keepColliders, Vector3 epicenter)
    {
        foreach (Collider collider in transform.parent.GetComponentsInChildren<Collider>())
        {
            if (!keepColliders && !collider.isTrigger)
            {
                Destroy(collider);
            }

            if (collider.GetComponent<Renderer>())
            {
                // Is visible, so let's break it into cubes

                // Dunno how expensive this is, and we use it all over the place, so let's cache it
                Vector3 scale = collider.transform.localScale;

                int cubesX = Mathf.RoundToInt(scale.x / m_ShatterSize);
                int cubesY = Mathf.RoundToInt(scale.y / m_ShatterSize);
                int cubesZ = Mathf.RoundToInt(scale.z / m_ShatterSize);

                // pointless microoptimization - do y's loop first because our objects tend to be flat
                for (int ty = 0; ty < cubesY; ++ty)
                {
                    float ypos = (ty + 0.5f) / cubesY - 0.5f;
                    for (int tx = 0; tx < cubesX; ++tx)
                    {
                        float xpos = (tx + 0.5f) / cubesX - 0.5f;
                        for (int tz = 0; tz < cubesZ; ++tz)
                        {
                            float zpos = (tz + 0.5f) / cubesZ - 0.5f;
                            Vector3 worldTarget = collider.transform.TransformPoint(new Vector3(xpos, ypos, zpos));
                            Rigidbody shatter = ((Transform)Instantiate(m_Shatter, worldTarget, collider.transform.rotation)).GetComponent<Rigidbody>();

                            // These are hardcoded because I would have had to change values in three different managers, which seemed silly for this project.
                            shatter.AddExplosionForce(Util.NextGaussianClamp(1000, 200, 500, 1500), epicenter, 40);
                            shatter.AddTorque(Random.rotation.eulerAngles * 10);
                        }
                    }
                }

                // but we really don't want the old one to render anymore
                Destroy(collider.GetComponent<Renderer>());
            }
        }

    }
        
    float Intensify(float value, float power)
    {
        // this is basically Mathf.Pow, except it leaves the sign alone
        return Mathf.Pow(Mathf.Abs(value), power) * Mathf.Sign(value);
    }

    IEnumerator EndGameCutscene()
    {
        // This is all pretty hacked-together, I just don't want to try putting it in a state machine
        Camera camera = Camera.main;
        ColorCorrectionCurves colorCurves = camera.GetComponent<ColorCorrectionCurves>();

        float preDelayTime = 1f;
        float saturationTime = 3f;
        float saturationDelayTime = 1.2f;
        float successTime = 2f;
        float successDelayTime = 1.2f;
        float continueTime = 2f;

        yield return new WaitForSeconds(preDelayTime);

        float startTime = Time.time;
        while (Time.time < startTime + saturationTime)
        {
            // yield goes first to guarantee we max out the animation
            yield return 0;

            // parenthesis are important to avoid floating-point inaccuracy!
            float progress = Mathf.Clamp((saturationTime - (Time.time - startTime)) / saturationTime, 0, 1);

            // strongly recommended that you consult with google calculator or wolfram alpha or similar in order to graph these
            // math pulled pretty much out of nowhere, I guess it looked good
            // in retrospect I probably should have used curves, but using curves to create curves seemed . . . complicated
            // even though it really wouldn't have been
            // welp
            // TODO: use curves
            float progressMunged = (Intensify(progress * 2 - 1, 2) + 1) / 2;

            float darkness = progressMunged;
            float lightness = Mathf.Sqrt(progressMunged);

            AnimationCurve curve = AnimationCurve.Linear(0, -darkness * (1 - darkness), lightness * lightness, 1);

            colorCurves.redChannel = curve;
            colorCurves.greenChannel = curve;
            colorCurves.blueChannel = curve;
            colorCurves.UpdateParameters();
        }

        m_AllowRestart = true;
        yield return new WaitForSeconds(saturationDelayTime);

        startTime = Time.time;
        while (Time.time < startTime + successTime)
        {
            // this is copypastier than I like :/
            // yield goes first to guarantee we max out the animation
            yield return 0;

            // parenthesis are important to avoid floating-point inaccuracy!
            float progress = Mathf.Clamp((successTime - (Time.time - startTime)) / successTime, 0, 1);

            Assert.IsNotNull(m_SuccessText);
            if (m_SuccessText)
            {
                m_SuccessText.color = new Color(0, 0, 0, 1 - progress);
            }
        }

        yield return new WaitForSeconds(successDelayTime);

        startTime = Time.time;
        while (Time.time < startTime + continueTime)
        {
            // this is copypastier than I like :/
            // yield goes first to guarantee we max out the animation
            yield return 0;

            // parenthesis are important to avoid floating-point inaccuracy!
            float progress = Mathf.Clamp((continueTime - (Time.time - startTime)) / continueTime, 0, 1);

            Assert.IsNotNull(m_ContinueText);
            if (m_ContinueText)
            {
                m_ContinueText.color = new Color(0, 0, 0, 1 - progress);
            }
        }
    }


    IEnumerator PhaseChangeCutscene(Notamari notamari)
    {
        // The phase change cutscene is (1) meant to be pretty, and (2) meant to disguise the framerate hitch caused by spawning a ton of physics items
        // If I couldn't disguise the hitch like this I would probably have to generate physics items in stasis over a period of time, or generate them at level load, or something
        // But I can!
        // So I do.

        // We're using animation curves here because the EndGameCutscene is way too ugly for me to be happy with.

        float flashDuration = 0.05f;

        ColorCorrectionCurves colorCurves = Camera.main.GetComponent<ColorCorrectionCurves>();
        float startTime = Time.time;

        while ((Time.time - startTime) < m_PCCutsceneLow[m_PCCutsceneLow.length - 1].time)
        {
            // yield goes first to guarantee we max out the animation
            yield return 0;

            AnimationCurve curve = AnimationCurve.Linear(0, m_PCCutsceneLow.Evaluate(Time.time - startTime), m_PCCutsceneHigh.Evaluate(Time.time - startTime), 1);

            colorCurves.redChannel = curve;
            colorCurves.greenChannel = curve;
            colorCurves.blueChannel = curve;
            colorCurves.UpdateParameters();
        }

        yield return new WaitForSeconds(flashDuration);

        // Destroy all colliders that are involved in this phase
        ShatterWorld(false, notamari.transform.position);

        // Clear the notamari
        notamari.Empty();

        // Begin the next segment
        m_Chain.BeginPhase();

        // Reset colors
        AnimationCurve origCurve = AnimationCurve.Linear(0, 0, 1, 1);
        colorCurves.redChannel = origCurve;
        colorCurves.greenChannel = origCurve;
        colorCurves.blueChannel = origCurve;
        colorCurves.UpdateParameters();
    }
}
