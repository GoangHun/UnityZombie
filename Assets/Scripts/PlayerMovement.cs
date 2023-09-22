using UnityEngine;
using UnityEngine.UIElements;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    public float rotateSpeed = 180f; // 좌우 회전 속도

    private Vector3 direction;

    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터
    private Camera worldCam;
    public LayerMask layerMask;

    private void Start() {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        worldCam = Camera.main;
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate() {
        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행
        Move();
        Rotate();
	}

	private void Update()
	{
        
        var forward = worldCam.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        var right = worldCam.transform.right;
        right.y = 0f;
        right.Normalize();


		direction = forward * playerInput.move;
        direction += right * playerInput.rotate;

        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }
		playerAnimator.SetFloat("Move", direction.magnitude);
	}

	// 입력값에 따라 캐릭터를 앞뒤로 움직임
	private void Move() {
		var pos = playerRigidbody.position;
        pos += direction * moveSpeed * Time.deltaTime;
		playerRigidbody.MovePosition(pos);
    }

    // 입력값에 따라 캐릭터를 좌우로 회전
    private void Rotate() {
		Ray ray = worldCam.ScreenPointToRay(Input.mousePosition);
		
		if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, layerMask))
		{
			Vector3 lookPoint = hitInfo.point;
			lookPoint.y = transform.position.y;
            var look = lookPoint - playerRigidbody.position;
            playerRigidbody.MoveRotation(Quaternion.LookRotation(look));
			//transform.LookAt(lookPoint);
		}
	}
}