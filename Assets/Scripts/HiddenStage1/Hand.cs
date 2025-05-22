using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public SpriteRenderer sr;
    private Camera mainCamera;
    public Transform gunPoint;
    public GameObject bulletPrefab;


    SpriteRenderer player;
    Vector3 rightpos = new Vector3(-0.175f, -0.295f, 0);
    Vector3 rightposReverse = new Vector3(-0.295f, -0.295f, 0);
    //Quaternion leftRot = Quaternion.Euler(0, 0, -35);
    //Quaternion leffRotReverse = Quaternion.Euler(0, 0, -135);
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentsInParent<SpriteRenderer>()[1];//손 스프라이트렌더러
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        bool isReverse = player.flipX;
        transform.localPosition = isReverse ? rightpos : rightposReverse;

        // 마우스 위치 가져오기 (월드 좌표)
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z; // Z 값 고정 (2D 게임이라 필요함)

        // 마우스를 바라보도록 회전
        Vector3 direction = mousePosition - transform.position;//마우스 위치 벡터
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;//마우스 위치 x,y가 x축과 y축 사이의 각도 구하기

        if (isReverse)
        {
            angle += 180;
        }//왼쪽으로 반전됐을 경우 각도 정반대로 돌려서 총구가 계속 마우스 향하도록 하는 코드

        gunPoint.localPosition = isReverse ? new Vector3(-Mathf.Abs(gunPoint.localPosition.x), gunPoint.localPosition.y, 0)
                                       : new Vector3(Mathf.Abs(gunPoint.localPosition.x), gunPoint.localPosition.y, 0);

        transform.localRotation = Quaternion.Euler(0, 0, angle);//나온 각도만큼 회전
        sr.flipX = isReverse;
        sr.sortingOrder = isReverse ? 99 : 101;
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }
    public void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.identity);

        // 총알의 방향을 마우스 방향으로 설정
        Vector3 fireDirection = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - gunPoint.position).normalized;

        fireDirection.z = 0; // 2D 게임이라 z값을 0으로 고정
        fireDirection.Normalize();

        // 🔹 총알을 방향에 맞게 회전시키기
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle-90);

        bullet.GetComponent<Bullet>().SetDirection(fireDirection);
        if (MainSoundManager.instance != null)
            MainSoundManager.instance.PlaySFX("Pistol");
    }
}
