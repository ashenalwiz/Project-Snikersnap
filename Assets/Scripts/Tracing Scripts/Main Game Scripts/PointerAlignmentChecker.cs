using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerAlignmentChecker : MonoBehaviour
{
    public GameObject myMask; 

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        pos.z = 0;

        if(TouchMovementHandler.Instance.isAlignet)
        {
            GameObject go = Instantiate(myMask, pos, Quaternion.identity);
            go.transform.parent = GameObject.Find("Masks").transform;
        }
        if(Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            DestroyPointer();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "myPath")
        {
            TouchMovementHandler.Instance.isAlignet = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "myPath")
        {
            if(TouchMovementHandler.Instance.isAlignet)
            {
                TouchMovementHandler.Instance.isAlignet = false;
            }
        }
    }

    void DestroyPointer()
    {
        if(TouchMovementHandler.Instance.PointerGO != null)
        {
            if(GameObject.Find("Masks").transform.childCount > 0)
            {
                foreach(Transform child in GameObject.Find("Masks").transform)
                {
                    Destroy(child.gameObject);
                }
            }

            Destroy(TouchMovementHandler.Instance.PointerGO);
        }
    }
}
