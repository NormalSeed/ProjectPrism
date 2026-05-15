using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpiritEntryUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _pieceInfoText;
    [SerializeField] private TextMeshProUGUI _passiveText;

    private SpiritData _data;
    private Action<SpiritData> _onSelected;

    public void Setup(SpiritData data, bool isOwned, Action<SpiritData> onSelected)
    {
        if (data == null) return;

        _data = data;
        _onSelected = onSelected;

        if (_nameText != null)
            _nameText.text = data.spiritName;

        if (_icon != null && data.spiritIcon != null)
            _icon.sprite = data.spiritIcon;

        float alpha = isOwned ? 1f : 0.35f;
        if (_canvasGroup != null)
            _canvasGroup.alpha = alpha;

        if (_button != null)
            _button.interactable = isOwned;

        if (_pieceInfoText != null)
        {
            int mirrors = data.GetPieceCount(PieceType.Mirror);
            int prisms = data.GetPieceCount(PieceType.Prism);
            _pieceInfoText.text = $"거울 {mirrors} / 프리즘 {prisms}";
        }

        if (_passiveText != null)
        {
            string passiveName = data.PassiveEffect != null
                ? data.PassiveEffect.EffectName
                : data.passiveSkillDescription;
            _passiveText.text = passiveName;
        }
    }

    public void OnClick()
    {
        _onSelected?.Invoke(_data);
    }
}
