using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsBoard : MonoBehaviour
{
    [SerializeField] Transform container;
    [SerializeField] GameObject PointsPrefab;

    public void AddItem(string text)
    {
        if (container.gameObject.activeSelf)
        {
            GameObject item = Instantiate(PointsPrefab, container);
            TMP_Text textComponent = item.GetComponent<TMP_Text>();
            textComponent.text = text;
            StartCoroutine(FadeTextToZeroAlpha(2, textComponent));
        }
        else
        {
            Debug.LogWarning("Container is not active. Unable to add item.");
        }
    }

    private IEnumerator FadeTextToZeroAlpha(float t, TMP_Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
        DestroyImmediate(i.transform.gameObject, true);
    }
}
