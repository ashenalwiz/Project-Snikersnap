using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TracingScripts
{
    public class MainMenuHandler : MonoBehaviour
    {
        public static MainMenuHandler Instance;
        public Sprite[] shapeSprites, lineSprites;
        public Transform itemsContent;
        public GameObject ItemPreafab_TextBased, ItemPrefab_SpriteBased;
        [HideInInspector] public string panelName;
        public Slider BGM_Slider, SFX_Slider;
        public List<GameObject> alphabetsPrefab, numbersPrefab, shapesPrefab, linesPrefab = new List<GameObject>();


        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            panelName = "category"; // Ensure category panel is the default
            PanelSceneHandler.panelToOpen = "category"; 
            ShowTracingItems(PanelSceneHandler.categorySelected);
        }



        public void ShowTracingItems(string category)
        {
            PanelSceneHandler.listToTrace.Clear();
            PanelSceneHandler.categorySelected = category;
            foreach (Transform child in itemsContent)
            {
                Destroy(child.gameObject);
            }

            switch (category)
            {
                case "alphabet":
                    for (int i = 0; i < 26; i++)
                    {
                        int num = i + 65;
                        SetItemText(i, num, alphabetsPrefab);
                    }
                    break;
                case "number":
                    for (int i = 0; i <= 9; i++)
                    {
                        int num = i + 48;
                        SetItemText(i, num, numbersPrefab);
                    }
                    break;
                case "shape":
                    for (int i = 0; i < shapeSprites.Length; i++)
                    {
                        SetItemImage(i, shapesPrefab, shapeSprites);
                    }
                    break;
                case "line":
                    for (int i = 0; i < lineSprites.Length; i++)
                    {
                        SetItemImage(i, linesPrefab, lineSprites);
                    }
                    break;
            }
        }

        private void SetItemText(int i, int num, List<GameObject> itemPrefab)
        {
            GameObject _item = Instantiate(ItemPreafab_TextBased, itemsContent);
            _item.GetComponentInChildren<TextMeshProUGUI>().text = Convert.ToChar(num).ToString();
            _item.GetComponent<ButtonHandler>().myItemToTace = itemPrefab[i].name;
            PanelSceneHandler.listToTrace.Add(itemPrefab[i].name);
        }

        private void SetItemImage(int i, List<GameObject> itemPrefab, Sprite[] spritesItem)
        {
            GameObject _item = Instantiate(ItemPrefab_SpriteBased, itemsContent);
            _item.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = spritesItem[i];
            _item.GetComponent<ButtonHandler>().myItemToTace = itemPrefab[i].name;
            PanelSceneHandler.listToTrace.Add(itemPrefab[i].name);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}