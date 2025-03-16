using RougueBit.Play.Interface;
using UnityEngine;
using Zenject;

namespace RougueBit.Play
{
    public class PlayerMover : MonoBehaviour
    {
        [Inject] IPlayManager playManager;

        private Rigidbody rb;
        private float moveSpeed;
        private float moveAcceleration;
        private float rotateSpeed;

        [Inject]
        public void Construct(PlaySceneSO playSceneSO)
        {
            moveSpeed = playSceneSO.PlayerMoveSpeed;
            moveAcceleration = playSceneSO.PlayerAcceleration;
            rotateSpeed = playSceneSO.PlayerRotationSpeed;
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // メモ: 移動は Update() + Time.deltaTimeを使うとカクついてしまうのでFixedUpdate()を使う
        void FixedUpdate()
        {
            Move();
            Rotate();
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
