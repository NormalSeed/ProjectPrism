using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonRoomNodeUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private GameObject _clearedIndicator;

    private string _roomId;

    public void Setup(DungeonRoomData room, bool isSelectable, Action<string> onSelected)
    {
        _roomId = room.Id;

        _label.text = GetRoomLabel(room.Type);
        _background.color = GetRoomColor(room.Type);

        if (_clearedIndicator != null)
            _clearedIndicator.SetActive(room.IsCleared);

        _button.interactable = isSelectable;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onSelected?.Invoke(_roomId));
    }

    private static string GetRoomLabel(RoomType type) => type switch
    {
        RoomType.Battle   => "⚔",
        RoomType.Elite    => "☠",
        RoomType.Boss     => "👑",
        RoomType.Treasure => "💎",
        RoomType.Rest     => "🔥",
        _                 => "?"
    };

    private static Color GetRoomColor(RoomType type) => type switch
    {
        RoomType.Battle   => new Color(0.8f, 0.3f, 0.3f),
        RoomType.Elite    => new Color(0.6f, 0.2f, 0.6f),
        RoomType.Boss     => new Color(0.9f, 0.1f, 0.1f),
        RoomType.Treasure => new Color(0.9f, 0.8f, 0.2f),
        RoomType.Rest     => new Color(0.2f, 0.7f, 0.4f),
        _                 => Color.gray
    };
}
