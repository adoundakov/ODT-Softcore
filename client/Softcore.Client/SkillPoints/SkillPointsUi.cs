using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// Builds the few UI pieces at runtime instead of shipping an asset bundle (plans/8-skill-points-plan.md
/// §4.4): a square text button styled from an existing TMP text, and a label cloned from one.
/// </summary>
internal static class SkillPointsUi
{
    public static readonly Color ButtonBackground = new Color(0.08f, 0.08f, 0.08f, 0.85f);
    public static readonly Color ButtonText = new Color(0.85f, 0.85f, 0.8f, 1f);

    /// <summary>A <see cref="Button"/> with a dark background and one centred character, font copied from <paramref name="fontFrom"/>.</summary>
    public static Button MakeButton(Transform parent, string name, string label, TMP_Text fontFrom, Vector2 anchor, Vector2 position, float size, UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(size, size);

        var image = go.GetComponent<Image>();
        image.color = ButtonBackground;

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        var textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;

        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.font = fontFrom.font;
        text.fontSharedMaterial = fontFrom.fontSharedMaterial;
        text.fontSize = size * 0.8f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = ButtonText;
        text.raycastTarget = false;
        text.text = label;

        return button;
    }

    /// <summary>Clones a text object (RectTransform + TMP only) next to itself under the same parent.</summary>
    public static TMP_Text CloneText(TMP_Text source, string name, Vector2 anchor, Vector2 position)
    {
        var go = Object.Instantiate(source.gameObject, source.transform.parent, false);
        go.name = name;

        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = ((RectTransform)source.transform).sizeDelta;

        var text = go.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }
}
