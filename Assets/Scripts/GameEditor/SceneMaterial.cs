using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaboom.Scene
{

    public class SceneMaterial : MonoBehaviour
    {
        public static SceneMaterial Instance { get; private set; }

        public List<GameObject> prefabs = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }
    }
}
