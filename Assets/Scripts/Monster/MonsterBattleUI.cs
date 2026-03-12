using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

public class MonsterBattleUI : MonoBehaviour
{
    [Header("HP Bar")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Monster Info")]
    [SerializeField] private Image monsterImage;
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private TextMeshProUGUI stageLevelText;

    [Header("Battle Info")]
    [SerializeField] private TextMeshProUGUI time;

    private IMonsterService monsterService;
    private IGameService gameService;

    [Inject]
    public void Construct(IMonsterService _monsterService, IGameService _gameService)
    {
        monsterService = _monsterService;
        gameService = _gameService;
    }

    private void Start()
    {
        if (monsterService != null)
        {
            // 몬스터 데이터 변경 구독
            monsterService.TargetMonster.Subscribe(UpdateMonsterInfo);
            // HP 변경 구독
            monsterService.CurrentHp.Subscribe(UpdateHpUI);
        }

        if (gameService is GameManager gm)
        {
            // 스테이지 레벨 변경 구독
            gm.StageLevel.Subscribe(UpdateStageText);
            gm.remainingTime.Subscribe(UpdateRemainingTime);
        }
    }

    private void UpdateRemainingTime(float remainingTime)
    {
        time.text = remainingTime.ToString();
    }

    private void UpdateMonsterInfo(MonsterData data)
    {
        if (data == null) return;
        monsterImage.sprite = data.monsterSprite;
        monsterNameText.text = data.monsterName;
        hpSlider.maxValue = data.maxHp;
    }

    private void UpdateHpUI(int currentHp)
    {
        hpSlider.value = currentHp;
        hpText.text = $"{currentHp} / {hpSlider.maxValue}";
    }

    private void UpdateStageText(int level)
    {
        stageLevelText.text = $"STAGE {level}";
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 구독 해제
        monsterService?.TargetMonster.Unsubscribe(UpdateMonsterInfo);
        monsterService?.CurrentHp.Unsubscribe(UpdateHpUI);
        if (gameService is GameManager gm)
        {
            gm.StageLevel.Unsubscribe(UpdateStageText);
            gm.remainingTime.Unsubscribe(UpdateRemainingTime);
        }
    }
}