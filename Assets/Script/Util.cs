using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

static class Util
{
    // shamelessly ganked from https://stackoverflow.com/questions/5817490/implementing-box-mueller-random-number-generator-in-c-sharp
    public static float NextGaussian()
    {
        float u, v, S;

        do
        {
            u = 2f * Random.value - 1f;
            v = 2f * Random.value - 1f;
            S = u * u + v * v;
        }
        while (S >= 1f);

        float fac = Mathf.Sqrt(-2f * Mathf.Log(S) / S);
        return u * fac;
    }

    // like gaussian, but with built-in offset/variance and with a min/max
    public static float NextGaussianClamp(float offset, float variance, float min, float max)
    {
        Assert.IsTrue(min <= max);
        if (min > max)
        {
            // wat
            return (min + max) / 2; // okay sure whatever don't crash plzkthx
        }

        while (true)
        {
            float value = offset + variance * NextGaussian();
            if (value >= min && value <= max)
            {
                return value;
            }

            // in theory we should eventually escape from looping too much
            // but that's not yet implemented
        }
    }
}
