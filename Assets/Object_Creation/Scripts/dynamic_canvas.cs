using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class dynamic_canvas : MonoBehaviour
{
   public string spritePath;
   public GameObject ImageObj;
   void Start()
    {

        Sprite loadedSprite = Resources.Load<Sprite>(spritePath);

        if (loadedSprite != null)
        {
            // Применение спрайта к SpriteRenderer
            Image ImagePic = ImageObj.GetComponent<Image>();
            if (ImagePic != null)
            {
                ImagePic.sprite = loadedSprite;
            }
        }
        else
        {
            Debug.LogError("Не удалось загрузить спрайт по пути: " + spritePath);
        }

    }
}
