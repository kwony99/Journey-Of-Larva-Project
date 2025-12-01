using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("1. 장애물 프리팹 목록")]
    [Tooltip("생성할 장애물들을 여기에 드래그해서 넣어주세요.")]
    public GameObject[] prefabs; 

    [Header("2. 시간 간격 설정 (불규칙 리듬 핵심)")]
    [Tooltip("최소 몇 초를 기다릴지 (겹치지 않으려면 1.5 이상 추천)")]
    public float minTime = 2.0f; 
    
    [Tooltip("최대 몇 초까지 기다릴지 (불규칙함을 위해 minTime과 차이를 크게 두세요)")]
    public float maxTime = 5.0f; 

    [Header("3. 위치 설정")]
    [Tooltip("카메라 중심에서 오른쪽으로 얼마나 떨어져서 생성할지 (화면 밖: 15~20 추천)")]
    public float spawnXDistance = 18.0f; 
    
    [Tooltip("장애물이 생성될 높이 (바닥 높이에 맞춰 조절하세요)")]
    public float spawnYHeight = -2.5f; 

    void Start()
    {
        // 게임 시작 시 생성 루틴 가동
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true) // 무한 반복
        {
            // 1. 랜덤한 대기 시간 계산 (최소~최대 사이에서 뽑기)
            float waitTime = Random.Range(minTime, maxTime);

            // 2. 계산된 시간만큼 대기 (이 시간 동안 장애물 사이 거리가 벌어짐)
            yield return new WaitForSeconds(waitTime);

            // 3. 생성 위치 계산 (현재 카메라 위치 기준 + 오른쪽 거리)
            // transform.position.x는 카메라가 계속 이동하므로 계속 바뀝니다.
            Vector3 spawnPos = new Vector3(transform.position.x + spawnXDistance, spawnYHeight, 0);

            // 4. 프리팹 목록에서 랜덤하게 하나 뽑기
            // (prefabs 배열이 비어있으면 오류가 나므로 예외 처리)
            if (prefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, prefabs.Length);
                GameObject selectedPrefab = prefabs[randomIndex];

                // 5. 실제 생성 (Instantiate)
                Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    // 에디터에서 생성 위치를 눈으로 확인하기 위한 기능 (게임 플레이엔 영향 없음)
    void OnDrawGizmos()
    {
        // 빨간색 공으로 표시
        Gizmos.color = Color.red;
        
        // 현재 설정된 생성 위치 계산
        Vector3 spawnPos = new Vector3(transform.position.x + spawnXDistance, spawnYHeight, 0);
        
        // 위치에 원 그리기
        Gizmos.DrawWireSphere(spawnPos, 0.5f);
        
        // 바닥 높이 가이드 선 그리기
        Gizmos.DrawLine(spawnPos + Vector3.left, spawnPos + Vector3.right);
    }
}