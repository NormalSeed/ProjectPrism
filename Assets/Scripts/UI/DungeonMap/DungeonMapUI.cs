using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class DungeonMapUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform _nodesContainer;
    [SerializeField] private DungeonRoomNodeUI _roomNodePrefab;

    private IDungeonMapService _dungeonMapService;
    private IGameService _gameService;
    private DungeonMapPresenter _presenter;

    [Inject]
    public void Construct(IDungeonMapService dungeonMapService, IGameService gameService)
    {
        _dungeonMapService = dungeonMapService;
        _gameService = gameService;
    }

    private void Start()
    {
        _presenter = new DungeonMapPresenter(_dungeonMapService);
        _gameService.OnRoomCompleted += OpenMap;
        _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_gameService != null)
            _gameService.OnRoomCompleted -= OpenMap;
    }

    private void OpenMap()
    {
        _panel.SetActive(true);
        RefreshNodes();
    }

    private void RefreshNodes()
    {
        foreach (Transform child in _nodesContainer)
            Destroy(child.gameObject);

        var currentRoom = _presenter.GetCurrentRoom();
        var nextChoices = _presenter.GetNextRoomChoices();
        var nextIds = new HashSet<string>(nextChoices.ConvertAll(r => r.Id));

        foreach (var room in _presenter.GetAllRooms())
        {
            var node = Instantiate(_roomNodePrefab, _nodesContainer);
            bool isSelectable = nextIds.Contains(room.Id);
            node.Setup(room, isSelectable, OnRoomSelected);
        }
    }

    private void OnRoomSelected(string roomId)
    {
        _presenter.SelectRoom(roomId);
        _panel.SetActive(false);
    }
}
