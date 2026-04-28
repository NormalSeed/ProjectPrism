using System;
using System.Collections.Generic;
using UnityEngine;

public class SpiritSelectPanelPresenter : MonoBehaviour
{
    [SerializeField] private SpiritEntryUI _entryPrefab;
    [SerializeField] private Transform _entryContainer;

    private readonly List<SpiritEntryUI> _entries = new List<SpiritEntryUI>();
    private Action<SpiritData> _onChosen;

    public void Open(List<SpiritData> allSpirits, List<SpiritData> ownedSpirits, Action<SpiritData> onChosen)
    {
        if (allSpirits == null) return;

        _onChosen = onChosen;
        ClearEntries();

        foreach (var spirit in allSpirits)
        {
            var entry = Instantiate(_entryPrefab, _entryContainer);
            bool isOwned = ownedSpirits.Contains(spirit);
            entry.Setup(spirit, isOwned, OnEntrySelected);
            _entries.Add(entry);
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnEntrySelected(SpiritData spirit)
    {
        Close();
        _onChosen?.Invoke(spirit);
    }

    private void ClearEntries()
    {
        foreach (var entry in _entries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }
        _entries.Clear();
    }
}
