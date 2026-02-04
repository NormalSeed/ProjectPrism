using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Visual Elements")]
    [SerializeField] private GameObject obstacleIcon;
    [SerializeField] private GameObject crystalIcon;
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private GameObject emitterIcon;

    // 모든 시각 요소 초기화
    public void ResetTile()
    {
        obstacleIcon.SetActive(false);
        crystalIcon.SetActive(false);
        emitterIcon.SetActive(false);
        crystalText.text = "";
    }

    // 장애물 설정
    public void SetAsObstacle()
    {
        ResetTile();
        obstacleIcon.SetActive(true);
    }

    // 크리스탈 설정
    public void SetAsCrystal(int count)
    {
        ResetTile();
        crystalIcon.SetActive(true);
        crystalText.text = count.ToString();
    }

    // 광원 설정 (방향에 맞춰 회전)
    public void SetAsEmitter(Vector2Int direction)
    {
        ResetTile();
        emitterIcon.SetActive(true);

        // 방향 벡터를 각도로 변환 (오른쪽이 0도 기준)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        emitterIcon.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
