using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TracingScripts
{
    public class Game_Manager: MonoBehaviour
    {
        public static Game_Manager Instance;
        public bool isGameEnded;
        public List<GameObject> itemToTracePrefabs = new List<GameObject>();

        public List<string> listToTrace;

        private void Awake()
        {
            Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < itemToTracePrefabs.Count; i++)
            {
                if (itemToTracePrefabs[i].name == PanelSceneHandler.itemToTrace)
                {
                    Instantiate(itemToTracePrefabs[i]);
                }
            }

            listToTrace = PanelSceneHandler.listToTrace;
        }

        // Update is called once per frame
        void Update()
        {
            if (PathGenerateHandler.Instance != null)
            {
                if (!GetComponent<TouchMovementHandler>().enabled)
                {
                    GetComponent<TouchMovementHandler>().enabled = true;
                }
            }
        }
    }
}