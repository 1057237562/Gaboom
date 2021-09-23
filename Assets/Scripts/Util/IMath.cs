using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Gaboom.Util
{
    public class IMath
    {
        public static float Sigmoid(float input)
        {
            return 1 / (1 + Mathf.Exp(-input)) - 0.5f;
        }

        public static float Sigmoid(float input,float duration)
        {
            return duration / (1 + Mathf.Exp(-input)) - duration / 2;
        }
    }
}
