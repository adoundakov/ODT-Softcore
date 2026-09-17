using TMPro;
using UnityEngine;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// "available points: N" in the skills screen header, between the "current:" and "remaining:" XP texts.
/// Lives on the <c>PlayerExperiencePanel</c>'s GameObject so a re-shown screen reuses it.
/// </summary>
internal class AvailablePointsLabel : MonoBehaviour
{
    private static readonly Vector2 Anchor = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 Position = new Vector2(0f, -19f); // same line as the two XP texts

    private TMP_Text _text;

    public void Bind(TMP_Text cloneFrom)
    {
        if (_text == null)
        {
            _text = SkillPointsUi.CloneText(cloneFrom, "Softcore Available Points", Anchor, Position);
        }

        Render();
    }

    private void OnEnable() => SkillPointsClient.StateChanged += Render;
    private void OnDisable() => SkillPointsClient.StateChanged -= Render;

    private void Render()
    {
        if (_text == null) return;

        if (!SkillPointsClient.Enabled)
        {
            _text.gameObject.SetActive(false);
            return;
        }

        var available = SkillPointsClient.State.Available;
        var value = available > 0 ? $"<color=#85FF9E>{available}</color>" : available.ToString();
        _text.text = $"<color=#949286>available skill points:</color> {value}";
        _text.gameObject.SetActive(true);
    }
}
