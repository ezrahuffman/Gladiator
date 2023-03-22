using System;
using Unity.VisualScripting;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool punchRight;
		public bool punchLeft;
		public bool crouch;
		public bool flipJump;



		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;


		[SerializeField]

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED

        public void Awake()
        {
			// TODO: Maybe don't search for this
			InputAction jumpAction = null;
            foreach (var item in GetComponent<PlayerInput>().actions)
            {
				if (item.name == "Jump")
                {
					jumpAction = item;
                }

			}


			if (jumpAction != null)
			{
				jumpAction.performed +=
					context =>
					{
						if (context.interaction is PressInteraction)
						{
							JumpInput(true);
						}
						else if (context.interaction is HoldInteraction || context.interaction is MultiTapInteraction)
						{
							FlipJumpInput(true);
						}
					};
			}
			
		}

		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		//public void OnJump(InputValue value)
		//{
		//	//value.GetType()
		//	JumpInput(value.isPressed);
		//}



        //public void OnFlipJump(InputValue value)
        //{
        //    FlipJumpInput(value.isPressed);
        //}


        public void OnCrouch(InputValue value)
        {
			CrouchInput(value.isPressed);
        }


        public void OnPunchRight(InputValue value)
		{
			PunchRightInput(value.isPressed);
		}

		public void OnPunchLeft(InputValue inputValue) 
		{
			PunchLeftInput(inputValue.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			Debug.Log($"set jump {newJumpState}");

			jump = newJumpState;

		}
        private void FlipJumpInput(bool newFlipJumpState)
        {
			Debug.Log($"set flipJump {newFlipJumpState}");
			flipJump = newFlipJumpState;
        }
        private void CrouchInput(bool newCrouchState)
        {
			Debug.Log($"isCrouchPressed: {newCrouchState}");
            crouch = newCrouchState;
        }

		public void PunchRightInput(bool newPunchRightState)
		{
			punchRight = newPunchRightState;
		}
		
		public void PunchLeftInput(bool newPunchLeftState)
		{
			punchLeft = newPunchLeftState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}