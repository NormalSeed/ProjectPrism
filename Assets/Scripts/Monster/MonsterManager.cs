using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public interface IMonsterService
{
    ObservableProperty<int> CurrentHp { get; }
    ObservableProperty<MonsterData> TargetMonster { get; }
    void Spawn(MonsterData data);
    void TakeDamage(int damage);
}

public class MonsterManager : MonoBehaviour, IMonsterService
{
    [Header("Visual Effects")]
    [SerializeField] private RectTransform monsterVisualRoot;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float deathDelay = 1.2f;

    [Header("Monster List")]
    [SerializeField] private List<MonsterData> monsters = new();

    // UI에서 직접 구독할 수 있는 Observable Property
    public ObservableProperty<int> CurrentHp { get; } = new(0);
    public ObservableProperty<MonsterData> TargetMonster { get; } = new();

    private bool isDead = false;
    private Tween activeShakeTween;

    private IGameService gameService;

    [Inject]
    public void Construct(IGameService _gameService)
    {
        gameService = _gameService;
    }

    private void Start()
    {
        if (gameService != null)
        {
            gameService.OnDamageCalculated += TakeDamage;
        }

        if (monsterVisualRoot == null) monsterVisualRoot = GetComponent<RectTransform>();

        SpawnCurrentStageMonster();
    }

    private void OnDestroy()
    {
        if (gameService != null)
        {
            gameService.OnDamageCalculated -= TakeDamage;
        }

        activeShakeTween.Kill();
    }

    private void SpawnCurrentStageMonster()
    {
        if (gameService == null || monsters == null || monsters.Count == 0) return;

        int index = (gameService.StageLevel.Value - 1) % monsters.Count;
        MonsterData data = monsters[index];

        Spawn(data);
    }

    public void Spawn(MonsterData data)
    {
        if (data == null) return;

        isDead = false;
        TargetMonster.Value = data;
        CurrentHp.Value = data.maxHp;

        // 스폰 연출
        if (monsterVisualRoot != null)
        {
            monsterVisualRoot.DOKill(); // 기존 트윈 제거
            monsterVisualRoot.localScale = Vector3.one;
            monsterVisualRoot.anchoredPosition = Vector2.zero;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        CurrentHp.Value = Mathf.Max(0, CurrentHp.Value - damage);
        Debug.Log($"[Battle] {damage} 데미지를 입힘. 남은 HP: {CurrentHp.Value}");

        bool isFatal = CurrentHp.Value <= 0;
        HandleDamageEffects(isFatal).Forget();
    }

    private async UniTaskVoid HandleDamageEffects(bool isFatal)
    {
        if (monsterVisualRoot == null) return;

        // 피격시 흔들림
        // 기존 흔들림이 있다면 멈추고 새로 시작
        activeShakeTween?.Kill();
        monsterVisualRoot.anchoredPosition = Vector2.zero; // 위치 초기화

        activeShakeTween = monsterVisualRoot.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato);

        // 사망 처리
        if (isFatal)
        {
            isDead = true;
            activeShakeTween?.Kill();   // 사망 시 흔들림 즉시 중단

            Debug.Log("[Battl] 몬스터 사망");

            await monsterVisualRoot.DOScale(1.2f, 0.15f).SetEase(Ease.OutQuad).ToUniTask();
            monsterVisualRoot.DOScale(0f, 0.5f).SetEase(Ease.InBack).ToUniTask().Forget();

            await UniTask.Delay(TimeSpan.FromSeconds(deathDelay));

            gameService.ReportMonsterDefeated();
            SpawnCurrentStageMonster();
        }
    }
}
