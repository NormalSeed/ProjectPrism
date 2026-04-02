using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class MonsterManager : MonoBehaviour, IMonsterService
{
    [Header("Visual Effects")]
    [SerializeField] private RectTransform _monsterVisualRoot;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeStrength = 10f;
    [SerializeField] private int _shakeVibrato = 10;
    [SerializeField] private float _deathDelay = 1.2f;

    [Header("Monster List")]
    [SerializeField] private List<MonsterData> _monsters = new();

    public ObservableProperty<int> CurrentHp { get; } = new(0);
    public ObservableProperty<MonsterData> TargetMonster { get; } = new();

    private bool _isDead = false;
    private Tween _activeShakeTween;

    private IGameService _gameService;

    [Inject]
    public void Construct(IGameService gameService)
    {
        _gameService = gameService;
    }

    private void Start()
    {
        if (_gameService != null)
        {
            _gameService.OnDamageCalculated += TakeDamage;
        }

        if (_monsterVisualRoot == null) _monsterVisualRoot = GetComponent<RectTransform>();

        SpawnCurrentStageMonster();
    }

    private void OnDestroy()
    {
        if (_gameService != null)
        {
            _gameService.OnDamageCalculated -= TakeDamage;
        }

        _activeShakeTween.Kill();
    }

    private void SpawnCurrentStageMonster()
    {
        if (_gameService == null || _monsters == null || _monsters.Count == 0) return;

        int index = (_gameService.StageLevel.Value - 1) % _monsters.Count;
        MonsterData data = _monsters[index];

        Spawn(data);
    }

    public void Spawn(MonsterData data)
    {
        if (data == null) return;

        _isDead = false;
        TargetMonster.Value = data;
        CurrentHp.Value = data.maxHp;

        if (_monsterVisualRoot != null)
        {
            _monsterVisualRoot.DOKill();
            _monsterVisualRoot.localScale = Vector3.one;
            _monsterVisualRoot.anchoredPosition = Vector2.zero;
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        CurrentHp.Value = Mathf.Max(0, CurrentHp.Value - damage);
        Debug.Log($"[Battle] {damage} 데미지를 입힘. 남은 HP: {CurrentHp.Value}");

        bool isFatal = CurrentHp.Value <= 0;
        HandleDamageEffects(isFatal).Forget();
    }

    private async UniTaskVoid HandleDamageEffects(bool isFatal)
    {
        if (_monsterVisualRoot == null) return;

        _activeShakeTween?.Kill();
        _monsterVisualRoot.anchoredPosition = Vector2.zero;

        _activeShakeTween = _monsterVisualRoot.DOShakeAnchorPos(_shakeDuration, _shakeStrength, _shakeVibrato);

        if (isFatal)
        {
            _isDead = true;
            _activeShakeTween?.Kill();

            Debug.Log("[Battle] 몬스터 사망");

            await _monsterVisualRoot.DOScale(1.2f, 0.15f).SetEase(Ease.OutQuad).ToUniTask();
            _monsterVisualRoot.DOScale(0f, 0.5f).SetEase(Ease.InBack).ToUniTask().Forget();

            await UniTask.Delay(TimeSpan.FromSeconds(_deathDelay));

            _gameService.ReportMonsterDefeated();
            SpawnCurrentStageMonster();
        }
    }
}
