using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Renders an integer using the classic bitmap digit sprites (0-9).
public class ScoreDisplay : MonoBehaviour
{
    public Sprite[] digitSprites; // index = digit
    public float digitWidth = 24f;
    public float digitHeight = 36f;
    public float spacing = 2f;

    readonly List<Image> images = new List<Image>();

    public void SetValue(int value)
    {
        string s = Mathf.Max(0, value).ToString();
        while (images.Count < s.Length)
        {
            var go = new GameObject("Digit", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(digitWidth, digitHeight);
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            images.Add(img);
        }

        float totalW = s.Length * digitWidth + (s.Length - 1) * spacing;
        for (int i = 0; i < images.Count; i++)
        {
            bool used = i < s.Length;
            images[i].gameObject.SetActive(used);
            if (!used) continue;
            images[i].sprite = digitSprites[s[i] - '0'];
            ((RectTransform)images[i].transform).anchoredPosition =
                new Vector2(-totalW / 2f + digitWidth / 2f + i * (digitWidth + spacing), 0f);
        }
    }
}
