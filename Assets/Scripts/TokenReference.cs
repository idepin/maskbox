using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokenReference : MonoBehaviour
{
    [SerializeField] private GameObject isFlippedGameObject;
    [SerializeField] private GameObject isNotFlippedGameObject;
    [SerializeField] private List<Image> tokenImages;
    public void Setup(Sprite tokenSprite, bool isFlipped = false)
    {
        foreach (var img in tokenImages)
        {
            img.sprite = tokenSprite;
        }
        isFlippedGameObject.SetActive(isFlipped);
        isNotFlippedGameObject.SetActive(!isFlipped);
    }
}
