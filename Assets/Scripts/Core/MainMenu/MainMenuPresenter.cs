using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] private List<SpiritData> _allSpirits = new List<SpiritData>();
    [SerializeField] private List<SpiritData> _ownedSpirits = new List<SpiritData>();
    [SerializeField] private SpiritSelectPanelPresenter _spiritSelectPanel;

    private ISceneService _sceneService;

    [Inject]
    public void Construct(ISceneService sceneService)
    {
        _sceneService = sceneService;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (_spiritSelectPanel != null)
            _spiritSelectPanel.Open(_allSpirits, _ownedSpirits, OnSpiritChosen);
    }

    public void OnSpiritChosen(SpiritData spirit)
    {
        PlayerPrefs.SetString(GameConsts.SelectedSpiritKey, spirit.name);
        PlayerPrefs.Save();
        _sceneService.LoadGameScene();
    }

    public bool IsOwned(SpiritData spirit) => _ownedSpirits.Contains(spirit);
    public List<SpiritData> GetAllSpirits() => _allSpirits;
    public List<SpiritData> GetOwnedSpirits() => _ownedSpirits;
}
