using R3;
using RougueBit.Play.Interface;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace RougueBit.Play
{
    public class PlayerMover : MonoBehaviour
    {
        [Inject] IPlayManager playManager;

        private PlaySceneSO playSceneSO;
        private Rigidbody rb;
        private Animator animator;
        private float moveSpeed;
        private float moveAcceleration;
        private float rotateSpeed;

        private static readonly int MoveHash = Animator.StringToHash("Move");

        [Inject]
        public void Construct(PlaySceneSO playSceneSO)
        {
            this.playSceneSO = playSceneSO;
            moveSpeed = playSceneSO.PlayerMoveSpeed;
            moveAcceleration = playSceneSO.PlayerAcceleration;
            rotateSpeed = playSceneSO.PlayerRotationSpeed;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            Observable.FromEvent<PlayState>(
                h => playManager.OnPlayStateChanged += h,
                h => playManager.OnPlayStateChanged -= h
            ).Subscribe(state => {
                if (state == PlayState.SetPlayer)
                {
                    // IsKenematicがfalseだとtransformで位置を変更できない
                    rb.position = new Vector3(playSceneSO.PlayerStartPosition.x, 0, playSceneSO.PlayerStartPosition.y);
                }
            }).AddTo(this);
        }

        // メモ: 移動は Update() + Time.deltaTimeを使うとカクついてしまうのでFixedUpdate()を使う
        void FixedUpdate()
        {
            Move();
            Rotate();
            animator.SetFloat(MoveHash, Mathf.Clamp(rb.linearVelocity.magnitude / moveSpeed , 0f, 1f));
        }

        private void Move()
        {
            var moveInput = playManager.PlayInputs.Main.Move.ReadValue<Vector2>();

            var targetSpeed = moveInput.magnitude * moveSpeed;
            var currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

            var speed = Mathf.Abs(targetSpeed - currentSpeed) > 0.1f ? targetSpeed : Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * moveAcceleration);

            rb.linearVelocity = new(moveInput.x * speed, rb.linearVelocity.y, moveInput.y * speed);
        }

        private void Rotate()
        {
            var moveInput = playManager.PlayInputs.Main.Move.ReadValue<Vector2>();

            if (moveInput.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 moveDirection = new(moveInput.x, 0, moveInput.y);
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
        }
    }
}
