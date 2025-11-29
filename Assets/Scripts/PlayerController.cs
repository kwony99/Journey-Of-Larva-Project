using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check (BoxCast)")]
    public Vector2 boxSize = new Vector2(0.8f, 0.2f); // 발바닥 크기
    public float castDistance = 0.2f; // 감지 거리 (조금 넉넉하게)
    public LayerMask groundLayer; 

    // 내부 변수들
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private bool isGrounded;
    private Vector2 surfaceNormal;
    private float jumpCooldown;
    private bool isKnockedBack;
    private float defaultGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        if (jumpCooldown > 0) jumpCooldown -= Time.deltaTime;

        CheckGround();
        ProcessInput();
        UpdateAnimation();
    }

    void CheckGround()
    {
        if (jumpCooldown > 0)
        {
            isGrounded = false;
            surfaceNormal = Vector2.up;
            return;
        }

        // [핵심 수정 1] 박스 시작점을 몸통 중심부(위쪽)로 올립니다.
        // position(발끝) + 위로 0.3만큼
        Vector2 boxOrigin = (Vector2)transform.position + Vector2.up * 0.3f;
        
        // 위에서 시작했으니 거리도 그만큼 더 길게(castDistance + 0.3f) 쏴야 바닥에 닿습니다.
        RaycastHit2D hit = Physics2D.BoxCast(boxOrigin, boxSize, 0f, Vector2.down, castDistance + 0.3f, groundLayer);
        
        isGrounded = hit.collider != null;

        if (isGrounded)
        {
            surfaceNormal = hit.normal;
        }
        else
        {
            surfaceNormal = Vector2.up;
        }

        // [핵심 수정 2] 회전 코드 삭제 -> 항상 똑바로 서 있게 고정
        transform.rotation = Quaternion.identity; 
    }

    void ProcessInput()
    {
        if (isKnockedBack) return;

        float moveInput = Input.GetAxisRaw("Horizontal");

        // [점프]
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpCooldown = 0.2f;
            isGrounded = false;
            rb.gravityScale = defaultGravity;
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("DoJump");
            return;
        }

        // [이동]
        if (isGrounded && moveInput != 0)
        {
            rb.gravityScale = defaultGravity;

            // 1. 경사면 방향 계산
            Vector2 slopeDir = Vector2.Perpendicular(surfaceNormal).normalized;
            Vector2 moveDir = slopeDir * -moveInput;

            rb.linearVelocity = moveDir * moveSpeed;

            // [핵심 수정: 강력한 접착력 적용]
            // 오르막이든 내리막이든, 땅에 붙어있다면 무조건 바닥으로 강하게 누릅니다.
            // 단, 너무 세게 누르면 오르막길 속도가 느려질 수 있으니 적당한 값(-5f ~ -10f)을 줍니다.
            // 튕겨나가는 힘을 이길만큼 충분히 강해야 합니다.
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 5f); 
        }
        else if (!isGrounded)
        {
            // 공중 이동
            rb.gravityScale = defaultGravity;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else // [지상 정지]
        {
            // 멈춰있을 때 미끄러짐 방지
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f; 
        }

        // [방향 반전]
        if (moveInput > 0) sr.flipX = false;
        else if (moveInput < 0) sr.flipX = true;
    }

    void UpdateAnimation()
    {
        anim.SetFloat("Speed", rb.linearVelocity.magnitude > 0.1f ? rb.linearVelocity.magnitude : 0f);
        anim.SetBool("IsGrounded", isGrounded);
        
        // [필수] 낙하 상태(Fall)로 넘어가기 위해 수직 속도 전달
        anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
    }
    public void ApplyKnockback(Vector2 knockbackForce)
    {
        StopAllCoroutines();
        isKnockedBack = true;
        rb.gravityScale = defaultGravity;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);
        StartCoroutine(ResetKnockbackRoutine());
    }

    IEnumerator ResetKnockbackRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        isKnockedBack = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // 기즈모도 실제 박스 위치랑 똑같이 그려서 확인
        Vector2 boxOrigin = (Vector2)transform.position + Vector2.up * 0.3f;
        Gizmos.DrawWireCube(boxOrigin + Vector2.down * (castDistance + 0.3f), boxSize);
    }
}