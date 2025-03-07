using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    public static MainMenuHandler Instance;
    public Sprite[] shapeSprites, lineSprites;
    public Transform itemsContent;
    public GameObject ItemPreafab_TextBased, ItemPrefab_SpriteBased;
    [HideInInspector] public string panelName;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ShowTracingItems(string category)
    {
        foreach (Transform child in itemsContent)
        {
            Destroy(child.gameObject);
        }

        switch(category)
        {
            case "alphabet":
                for(int i = 0; i < 26; i++)
                {
                    int num = i + 65;
                    SetItemText(i, num);
                }
                break;
            case "number":
                for (int i = 0; i <= 9; i++)
                {
                    int num = i + 48;
                    SetItemText(i, num);
                }
                break;
            case "shape":
                for (int i = 0; i < shapeSprites.Length; i++)
                {
                    SetItemImage(i, shapeSprites);
                }
                break;
            case "line":
                for (int i = 0; i < lineSprites.Length; i++)
                {
                    SetItemImage(i, lineSprites);
                }
                break;            
        }
    }

    private void SetItemText(int i, int num)
    {
        GameObject _item = Instantiate(ItemPreafab_TextBased, itemsContent);
        _item.GetComponentInChildren<TextMeshProUGUI>().text = Convert.ToChar(num).ToString();
    }

    private void SetItemImage(int i, Sprite[] spritesItem)
    {
        GameObject _item = Instantiate(ItemPrefab_SpriteBased, itemsContent);
        _item.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = spritesItem[i];

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
