using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{
    public GameObject firePosition; //발사 위치
    public GameObject bombFactory; //투척 무기 오브젝트
    public float throwPower = 15f; //투척 파워
    public GameObject bulletEffect; //피격 이펙트 오브젝트
    ParticleSystem ps; //피격 에픽트 파티클 시스템
    public int weaponPower = 5; //발사 무기 공격력
    Animator anim; //애니메이터 변수
    public Text wModeText; //무기 모드 텍스트
    public GameObject[] eff_Flash; //총 발사 효과 오브젝트 배열
    //무기 아이콘 스프라이트 변수
    public GameObject weapon01;
    public GameObject weapon02;
    //크로스헤어 스프라이트 변수
    public GameObject crosshair01;
    public GameObject crosshair02;
    //마우스 오른쪽 버튼 클릭 아이콘 스프라이트 변수
    public GameObject weapon01_R;
    public GameObject weapon02_R;
    //마우스 우클릭 줌 모드 스프라이트 변수
    public GameObject crosshair02_zoom;


    //무기 모드 변수
    enum WeaponMode
    {
        Normal,
        Sniper
    }
    WeaponMode wMode;
    bool ZoomMode = false; //카메라 확대 확인용 변수


    void Start()
    {
        //피격 이펙트 오브젝트에서 파티클 시스템 컴포넌트 
        ps = bulletEffect.GetComponent<ParticleSystem>();
        //애니메이터 컴포넌트 가져오기
        anim = GetComponentInChildren<Animator>();
        //무기 기본 모드를 노멀 모드로 설정
        wMode = WeaponMode.Normal;
    }

    void Update()
    {
        //게임 상태가 '게임 중' 상태일 때만 조작할 수 있게 한다
        if (GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

        //노멀 모드 : 마우스 오른쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던진다
        //스나이퍼 모드 : 마우스 오른쪽 버튼을 누르면 화면 확대

        //마우스 오른쪽 버튼 입력
        if(Input.GetMouseButtonDown(1))
        {
            switch(wMode)
            {
                case WeaponMode.Normal:
                    //수류탄 오브젝트를 생성한 후 수류탄의 생성 위치를 발사 위치로 한다
                    GameObject bomb = Instantiate(bombFactory);
                    bomb.transform.position = firePosition.transform.position;

                    //수류탄 오브젝트의 Rigidbody 컴포넌트 
                    Rigidbody rb = bomb.GetComponent<Rigidbody>();

                    //카메라의 정면 방향으로 수류탄에 물리적 힘을 가함
                    rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);

                    break;
                case WeaponMode.Sniper:
                    //카메라를 확대하고 줌모드 상태로 변경
                    if(!ZoomMode)
                    {
                        Camera.main.fieldOfView = 15f;
                        ZoomMode = true;
                        //줌 모드일 때 크로스헤어 변경
                        crosshair02_zoom.SetActive(true);
                        crosshair02.SetActive(false);
                    }
                    //그렇지 않으면 카메라를 원래 상태로 되돌리고 줌 모드 상태 해제
                    else
                    {
                        Camera.main.fieldOfView = 60f;
                        ZoomMode = false;
                        //크로스헤어를 스나이퍼 모드로 돌려놓음
                        crosshair02_zoom.SetActive(false);
                        crosshair02.SetActive(true);
                    }
                    break;
            }
        }    

        //마우스 왼쪽 버튼을 누르면 시선이 바라보는 방향으로 총 발사
        //마우스 왼쪽 버튼 입력
        if(Input.GetMouseButtonDown(0))
        {
            //만일 이동 블렌드 트리 파라미터의 값이 0이라면, 공격 애니메이션 실행
            if(anim.GetFloat("MoveMotion") == 0)
            {
                anim.SetTrigger("Attack");
            }
            //레이를 생성한 후 발사될 위치와 진행 방향 설정
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            //레이가 부딪힌 대상의 정보를 저장할 변수를 생성
            RaycastHit hitInfo = new RaycastHit();

            //레이를 발사한 후 만일 부딪힌 물체가 있으면 피격 이벤트 실행
            if(Physics.Raycast(ray, out hitInfo))
            {
                //만일 레이에 부딪힌 대상의 레이어가 Enemy라면 데미지 함수 실행
                if(hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    EnemyFSM eFSM = hitInfo.transform.GetComponent<EnemyFSM>();
                    eFSM.HitEnemy(weaponPower);
                }
                //그렇지 않다면 레이에 부딪힌 지점에 피격 이펙트 플레이
                else
                {
                    //피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동
                    bulletEffect.transform.position = hitInfo.point;

                    //피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킨다
                    bulletEffect.transform.forward = hitInfo.normal;

                    //피격 이펙트 플레이
                    ps.Play();
                }
            }
            //총 이펙트 실시
            StartCoroutine(ShootEffectOn(0.05f));
        }
        //만일 키보드의 숫자 1번 입력을 받으면, 무기 모드를 일반 모드로 변경
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            wMode = WeaponMode.Normal;

            //스나이퍼 모드에서 일반 모드 키 입력 시 zoom 비활성화, 줌모드 해제
            crosshair02_zoom.SetActive(false);
            //카메라 화면을 다시 원래대로 돌려준다
            Camera.main.fieldOfView = 60f;
            //일반 모드 텍스트 출력
            wModeText.text = "Normal Mode";
            //1번 스프라이트는 활성화되고, 2번 스프라이트는 비활성화된다
            weapon01.SetActive(true);
            weapon02.SetActive(false);
            crosshair01.SetActive(true);
            crosshair02.SetActive(false);
            //Weapon01_R는 활성화되고, Weapon02_R은 비활성화
            weapon01_R.SetActive(true);
            weapon02_R.SetActive(false);
        }
        //만일 키보드의 숫자 2번 입력을 받으면 무기 모드를 스나이퍼 모드로 변경
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            wMode = WeaponMode.Sniper;
            //스나이퍼 모드 텍스트 출력
            wModeText.text = "Sniper Mode";
            //1번 스프라이트는 비활성화되고, 2번 스프라이트는 활성화된다
            weapon01.SetActive(false);
            weapon02.SetActive(true);
            crosshair01.SetActive(false);
            crosshair02.SetActive(true);
            //Weapon01_R은 비활성화, Weapon02_R은 활성화
            weapon01_R.SetActive(false);
            weapon02_R.SetActive(true);
        }
    }
    //총구 이펙트 코루틴 함수
    IEnumerator ShootEffectOn(float duration)
    {
        //랜덤하게 숫자를 뽑음
        int num = Random.Range(0, eff_Flash.Length);
        //이펙트 오브젝트 배열에서 뽑힌 숫자에 해당하는 이펙트 오브젝트 활성화
        eff_Flash[num].SetActive(true);
        //지정한 시간만큼 대기
        yield return new WaitForSeconds(duration);
        //이펙트 오브젝트를 다시 비활성화
        eff_Flash[num].SetActive(false);
    }
}
