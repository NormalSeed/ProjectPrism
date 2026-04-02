public interface IMonsterService
{
    ObservableProperty<int> CurrentHp { get; }
    ObservableProperty<MonsterData> TargetMonster { get; }
    void Spawn(MonsterData data);
    void TakeDamage(int damage);
}
