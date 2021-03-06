using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7f; //이동 속도 변수
    CharacterController cc; //캐릭터 콘트롤러 변수
    float gravity = -20f; //중력 변수
    float yVelocity = 0; //수직 속력 변수
    public float jumpPower = 10f; //점프력 변수
    public bool isJumping = false; //점프 상태 변수
    public int hp = 20; //플레이어 체력 변수\
    int maxHp = 20; //최대 체력 변수
    public Slider hpSlider; //hp 슬라이더 변수
    public GameObject hitEffect; //Hit 효과 오브젝트
    Animator anim; //애니메이터 변수

    void Start()
    {
        //캐릭터 컨트롤러 컴포넌트
        cc = GetComponent<CharacterController>();

        //애니메이터 받아오기
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        //게임 상태가 '게임 중' 상태일 때만 조작할 수 있게 한다
        if(GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

        //1. 사용자 입력
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //2. 이동 방향 설정
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        //이동 블렌딩 트리를 호출하고 벡터의 크기 값을 넘겨준다
        anim.SetFloat("MoveMotion", dir.magnitude);

        //2-1. 메인 카메라를 기준으로 방향 변환
        dir = Camera.main.transform.TransformDirection(dir);

        //2-2. 점프 중이고, 다시 바닥에 착지했다면
        if(isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            //점프 전 상태로 초기화
            isJumping = false;
            //캐릭터 수직 속도를 0
            yVelocity = 0;
        }

        //2-3. 키보드 space 버튼 입력, 점프를 안 한 상태라면
        if(Input.GetButtonDown("Jump") && !isJumping)
        {
            //캐릭터 수직 속도에 점프력을 적용하고 점프 상태로 변경
            yVelocity = jumpPower;
            isJumping = true;
        }

        //2-4. 캐릭터 수직 속도에 중력 값을 적용
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        //3. 속도에 맞춰 이동
        cc.Move(dir * moveSpeed * Time.deltaTime);

        //4. 현재 플레이어 hp(%)를 hp 슬라이더의 value에 반영
        hpSlider.value = (float)hp / (float)maxHp;
    }

    //플레이어 피격 함수
    public void DamageAction(int damage)
    {
        //에너미의 공격력만큼 플레이어의 체력을 깎는다
        hp -= damage;

        //플레이어의 체력이 0보다 크면 피격 효과 출력
        if(hp > 0)
        {
            //피격 이벤트 코루틴 시작
            StartCoroutine(PlayHitEffect());
        }
    }

    //피격 효과 코루틴 함수
    IEnumerator PlayHitEffect()
    {
        //1. 피격 UI를 활성화
        hitEffect.SetActive(true);
        //2. 0.3초간 대기
        yield return new WaitForSeconds(0.3f);
        //3. 피격 UI 비활성화
        hitEffect.SetActive(false);
    }
}
