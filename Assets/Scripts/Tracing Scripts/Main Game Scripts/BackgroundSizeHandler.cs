using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TracingScripts
{
    public class BackgroundSizeHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            ResizeSpriteToScreen();
        }

        private void ResizeSpriteToScreen()
        {
            var sr = this.GetComponent<SpriteRenderer>();

            if (sr == null) return;

            transform.localScale = new Vector3(1, 1, 1);

            var width = sr.sprite.bounds.size.x;
            var height = sr.sprite.bounds.size.y;

            var worldScreenHeight = Camera.main.orthographicSize * 2.0;
            var worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            float myX = (float)worldScreenWidth / width;
            float myY = (float)worldScreenHeight / height;

            transform.localScale = new Vector3(myX, myY, 0);
        }
    }
}