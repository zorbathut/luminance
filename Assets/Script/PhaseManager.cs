using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System.Collections.Generic;

public class PhaseManager : MonoBehaviour
{
    public List<Spawner> m_Spawners;

    public bool m_First;
    public PhaseManager m_Chain;

    public float m_ProgressionPermitted = 1;

    public Text m_SuccessText;
    public Text m_ContinueText;

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

    void EndPhase()
    {
        Assert.IsNull(m_Chain);
        if (!m_Chain)
        {
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

    public void NotifyGrabbed(Notamari notamari)
    {
        if (notamari.GetDebrisCount() >= m_DebrisTotal)
        {
            EndPhase();
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

        float saturationTime = 3f;
        float saturationDelayTime = 1.2f;
        float successTime = 2f;
        float successDelayTime = 1.2f;
        float continueTime = 2f;

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
}
