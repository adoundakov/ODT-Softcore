using EFT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// The +/− buttons on one skill row of the list view, added by <see cref="Patches.SkillPanelButtonsPatch"/>
/// and kept on the row's GameObject so a re-shown row reuses them. Visibility follows
/// <see cref="SkillPointsClient.State"/>: "+" while points are available and the skill is under the cap,
/// "−" only with deallocation enabled and something allocated.
/// </summary>
internal class SkillPointButtons : MonoBehaviour
{
    // Stacked on the right edge of the 96×96 skill icon, which sits at the row's bottom-left corner
    private const float Size = 22f;
    private static readonly Vector2 PlusPosition = new Vector2(74f, 74f);
    private static readonly Vector2 MinusPosition = new Vector2(74f, 0f);

    private Skill _skill;
    private Button _plus;
    private Button _minus;

    public void Bind(Skill skill, TMP_Text fontFrom)
    {
        _skill = skill;
        if (_plus == null)
        {
            _plus = SkillPointsUi.MakeButton(transform, "Softcore Plus", "+", fontFrom, Vector2.zero, PlusPosition, Size, () => Allocate(1));
            _minus = SkillPointsUi.MakeButton(transform, "Softcore Minus", "−", fontFrom, Vector2.zero, MinusPosition, Size, () => Allocate(-1));
        }

        Render();
    }

    private void OnEnable() => SkillPointsClient.StateChanged += Render;
    private void OnDisable() => SkillPointsClient.StateChanged -= Render;

    private void Allocate(int delta)
    {
        // Apply() inside recomputes the skill's buffs and fires its UI events; StateChanged re-renders every row
        SkillPointsClient.Allocate(_skill.Id, delta);
    }

    private void Render()
    {
        if (_plus == null || _skill == null) return;

        var state = SkillPointsClient.State;
        var enabled = SkillPointsClient.Enabled && !_skill.Locked;
        var allocated = SkillLevels.AllocatedFor(_skill.Id);

        _plus.gameObject.SetActive(enabled && state.Available > 0 && _skill.Level < state.MaxLevel);
        _minus.gameObject.SetActive(enabled && state.DeallocationEnabled && allocated > 0);
    }
}
