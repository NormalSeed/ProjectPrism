using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MonsterBattleUI : MonoBehaviour
{
    [Header("HP Bar")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _hpText;

    [Header("Monster Info")]
    [SerializeField] private Image _monsterImage;
    [SerializeField] private TextMeshProUGUI _monsterNameText;
    [SerializeField] private TextMeshProUGUI _stageLevelText;

    [Header("Battle Info")]
    [SerializeField] private TextMeshProUGUI _time;

    private IMonsterService _monsterService;
    private IGameService _gameService;

    [Inject]
    public void Construct(IMonsterService monsterService, IGameService gameService)
    {
        _monsterService = monsterService;
        _gameService = gameService;
    }

    private void Start()
    {
        if (_monsterService != null)
        {
            _monsterService.TargetMonster.Subscribe(UpdateMonsterInfo);
            _monsterService.CurrentHp.Subscribe(UpdateHpUI);
        }

        if (_gameService != null)
        {
            _gameService.StageLevel.Subscribe(UpdateStageText);
            _gameService.RemainingTime.Subscribe(UpdateRemainingTime);
        }
    }

    private void UpdateRemainingTime(float remainingTime)
    {
        _time.text = remainingTime.ToString();
    }

    private void UpdateMonsterInfo(MonsterData data)
    {
        if (data == null) return;
        if (_monsterImage != null) _monsterImage.sprite = data.monsterSprite;
        _monsterNameText.text = data.monsterName;
        _hpSlider.maxValue = data.maxHp;
    }

    private void UpdateHpUI(int currentHp)
    {
        _hpSlider.value = currentHp;
        _hpText.text = $"{currentHp} / {_hpSlider.maxValue}";
    }

    private void UpdateStageText(int level)
    {
        _stageLevelText.text = $"STAGE {level}";
    }

    private void OnDestroy()
    {
        _monsterService?.TargetMonster.Unsubscribe(UpdateMonsterInfo);
        _monsterService?.CurrentHp.Unsubscribe(UpdateHpUI);
        if (_gameService != null)
        {
            _gameService.StageLevel.Unsubscribe(UpdateStageText);
            _gameService.RemainingTime.Unsubscribe(UpdateRemainingTime);
        }
    }
}
