using Cinemachine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using Unity.Mathematics;
using System;
using UnityEngine.InputSystem;
using static Fusion.NetworkBehaviour;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using Zenject;
using SimpleFPS;
using System.Collections.Generic;

namespace FourFathers
{
	public enum MoveType
	{
		DOWNED = -1,
		CROUCH = 0,
		WALK = 1,
	}

	public class PlayerController : NetworkBehaviour
	{
		public PlayerRef PlayerRef {  get; private set; }

		[SerializeField]
		public bool IsAlive()
		{
			return m_isAlive;
		}

		[Header("Components")]
		public SimpleKCC KCC;
		public Animator FirstPersonAnimator;
		public Animator ThirdPersonAnimator;
		public HitboxRoot HitboxRoot;
		public GameObject ReviveInteractableTrigger;

		[Header("Controller Setup")]
		[SerializeField]
		private float m_forwardMoveSpeed = 6f;
		[SerializeField]
		private float m_backwardMoveSpeed = 4f;
		[SerializeField]
		private float m_sprintSpeed = 10f;
		[SerializeField]
		private float m_crouchMoveSpeed = 1f;
		[SerializeField]
		private float m_backwardCrouchMoveSpeed = 1f;
		[SerializeField]
		private float m_downedMoveSpeed = 1f;

		public float JumpForce = 10f;
		public Transform CameraHandle;
		public GameObject FirstPersonRoot;
		public GameObject FirstPersonBody;
		public GameObject ThirdPersonBody;
		public NetworkObject SprayPrefab;
		
		[SerializeField]
		private float CrouchColliderHeight = 1f;
		[SerializeField]
		private float WalkColliderHeight = 1.8f;
		[SerializeField]
		private float DownColliderHeight = .75f;


		[Header("Movement")]
		public float UpGravity = 15f;
		public float DownGravity = 25f;
		public float GroundAcceleration = 55f;
		public float GroundDeceleration = 25f;
		public float AirAcceleration = 25f;
		public float AirDeceleration = 1.3f;

		[SerializeField]
		private MoveType m_moveType;

		[Header("Camera Settings")]
		[SerializeField]
		private CinemachineVirtualCamera m_firstPersonCamera;

		[SerializeField]
		private CinemachineVirtualCamera m_thirdPersonCamera;

		[SerializeField]
		private CinemachineVirtualCamera UICamera;

		[SerializeField]
		private CinemachineImpulseSource m_fpsCameraImpulse;

		[SerializeField]
		private float m_bobForceModifier = -.1f;

		[SerializeField, Range(0,1)]
		private float m_bobForceFalling = .1f;

		[SerializeField]
		private float m_bobCooldown = .2f;

		[SerializeField]
		private Vector2 VerticalCameraClamp = Vector2.zero;

		[SerializeField, Networked]
		private Vector2 m_cameraPitch { get; set; }

		public GameObject DeathCameraThirdPerson;

		[Header("Tools")]
		[SerializeField]
		private List<ToolBase> m_equipableTools = new List<ToolBase>();

		[Networked]
		private NetworkButtons m_previousButtons { get; set; }
		[Networked]
		private int m_jumpCount { get; set; }
		[Networked]
		private Vector3 _moveVelocity { get; set; }
		[Networked, OnChangedRender(nameof(OnAliveChanged))]
		private bool m_isAlive { get; set;}
		[Networked, OnChangedRender(nameof(OnCrouchChanged))]
		private bool m_isCrouching { get; set; }
		[Networked, OnChangedRender(nameof(OnDownedChanged))]
		private bool m_isDowned { get; set; }

		private int _visibleJumpCount;
		private Vector3 m_cameraOrigin = Vector3.zero;
		private float m_bobTimer = 0f;
		private bool m_isSprinting = false;
		private ChangeDetector _changeDetector;
		private Collider? m_lastInteractableCollider;
		private Collider? m_lastLookedAtInteractable;
		private ToolBase m_equippedTool;

		[Inject]
		private UIManager m_uIManager;
		[Inject]
		private PlayerManager m_playerManager;
		[Inject]
		private InteractableManager m_interactableManager;

		public override void Spawned()
		{
			name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";
			//transform.SetParent(Runner.transform);

			// Enable first person visual for local player, third person visual for proxies.
			SetFirstPersonVisuals(HasInputAuthority);
			DeathCameraThirdPerson.SetActive(false);

			if (HasInputAuthority == false)
			{
				// Virtual cameras are enabled only for local player.
				var virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);
				for (int i = 0; i < virtualCameras.Length; i++)
				{
					virtualCameras[i].enabled = false;
				}
			}

			m_isAlive = true;
			m_cameraOrigin = CameraHandle.position;
			_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
			PlayerRef = Object.InputAuthority;
			m_playerManager.AddPlayer(Object.InputAuthority, this);
			m_equippedTool = m_equipableTools[0];
		}

		public override void FixedUpdateNetwork()
		{
			//if (_sceneObjects.Gameplay.State == EGameplayState.Finished)
			//{
			//	// After gameplay is finished we still want the player to finish movement and not stuck in the air.
			//	MovePlayer();
			//	return;
			//}

			if (m_isAlive == false)
			{
				// We want dead body to finish movement - fall to ground etc.
				MovePlayer();

				// Disable physics casts and collisions with other players.
				KCC.SetColliderLayer(LayerMask.NameToLayer("Ignore Raycast"));
				KCC.SetCollisionLayerMask(LayerMask.GetMask("Default"));

				HitboxRoot.HitboxRootActive = false;

				// Force enable third person visual for local player.
				SetFirstPersonVisuals(false);
				return;
			}

			if (GetInput(out NetworkedInputStruct input))
			{
				// Input is processed on InputAuthority and StateAuthority.
				ProcessInput(input);
			}
			else
			{
				// When no input is available, at least continue with movement (e.g. falling).
				MovePlayer();
			}


			// Camera is set based on KCC look rotation.
			Vector2 pitchRotation = KCC.GetLookRotation(true, false);
			CameraHandle.localRotation = Quaternion.Euler(pitchRotation);
			CameraHandle.position = new Vector3(CameraHandle.position.x, m_isCrouching ? transform.position.y + 1.1f : transform.position.y + 1.6f, CameraHandle.position.z);
			m_cameraPitch = pitchRotation;

			m_bobTimer += Runner.DeltaTime;
			if (m_bobTimer > m_bobCooldown)
			{
				float bobForce = (_moveVelocity.magnitude / m_forwardMoveSpeed);
				m_fpsCameraImpulse.GenerateImpulseWithForce(bobForce * -.1f);
				m_bobTimer = 0f;
			}

			if (!KCC.IsGrounded)
			{
				if(KCC.RealVelocity.y > 0)
					m_fpsCameraImpulse.GenerateImpulseWithForce(m_bobForceFalling);
				else
					m_fpsCameraImpulse.GenerateImpulseWithForce(-m_bobForceFalling);
			}


		}

		public override void Render()
		{
			//if (_sceneObjects.Gameplay.State == EGameplayState.Finished)
				//return;

			var moveVelocity = GetAnimationMoveVelocity();
			float velocityX = m_isSprinting ? moveVelocity.x : moveVelocity.x * .5f;
			float velocityZ = m_isSprinting ? moveVelocity.z : moveVelocity.z * .5f;

			// Set animation parameters.
			if (!HasInputAuthority || m_isDowned)
			{
				ThirdPersonAnimator.SetBool("IsAlive", true);
				ThirdPersonAnimator.SetBool("IsGrounded", KCC.IsGrounded);
				//ThirdPersonAnimator.SetBool("IsReloading", false);
				ThirdPersonAnimator.SetFloat("MoveX", velocityX, 0.05f, Time.deltaTime);
				ThirdPersonAnimator.SetFloat("MoveZ", velocityZ, 0.05f, Time.deltaTime);
				ThirdPersonAnimator.SetFloat("MoveSpeed", moveVelocity.magnitude/m_sprintSpeed);
				float2 remapPitch = m_cameraPitch.x > 0 ? 
					math.remap(0, 72, 0, 1, m_cameraPitch) :
					math.remap(-89, 0, -1, 0, m_cameraPitch);
				ThirdPersonAnimator.SetFloat("LookPitch", remapPitch.x);
				ThirdPersonAnimator.SetFloat("MoveType", GetMoveType());
			}
			else
			{
				FirstPersonAnimator.SetBool("IsAlive", true);
				FirstPersonAnimator.SetBool("IsGrounded", KCC.IsGrounded);
				//FirstPersonAnimator.SetBool("IsReloading", false);
				FirstPersonAnimator.SetFloat("MoveX", moveVelocity.x, 0.05f, Time.deltaTime);
				FirstPersonAnimator.SetFloat("MoveZ", moveVelocity.z, 0.05f, Time.deltaTime);
				FirstPersonAnimator.SetFloat("MoveSpeed", moveVelocity.magnitude);
				FirstPersonAnimator.SetFloat("MoveType", GetMoveType());
			}

			if (m_isAlive == false)
			{
				// Disable UpperBody (override) and Look (additive) layers. Death animation is full-body.

				//int upperBodyLayerIndex = ThirdPersonAnimator.GetLayerIndex("UpperBody");
				//ThirdPersonAnimator.SetLayerWeight(upperBodyLayerIndex, Mathf.Max(0f, ThirdPersonAnimator.GetLayerWeight(upperBodyLayerIndex) - Time.deltaTime));

				//int lookLayerIndex = ThirdPersonAnimator.GetLayerIndex("Look");
				//ThirdPersonAnimator.SetLayerWeight(lookLayerIndex, Mathf.Max(0f, ThirdPersonAnimator.GetLayerWeight(lookLayerIndex) - Time.deltaTime));

				ThirdPersonAnimator.enabled = false;
			}

			if (_visibleJumpCount < m_jumpCount)
			{
				ThirdPersonAnimator.SetTrigger("Jump");
			}

			_visibleJumpCount = m_jumpCount;
		}

		public void Kill()
		{
			if(!HasStateAuthority)
			{
				return;
			}

			Die();
		}

		public void Down()
		{
			if (!HasStateAuthority)
			{
				return;
			}

			Downed();
		}

		public void Respawn()
		{
			if (!HasStateAuthority)
			{
				return;
			}

			Debug.Log("Respawning...");

			Revive();
			m_isAlive = true;
		}

		public void Revive()
		{
			m_isDowned = false;
		}

		private void LateUpdate()
		{
			if (HasInputAuthority == false)
				return;

			// Camera is set based on interpolated KCC look rotation.
			var pitchRotation = KCC.GetLookRotation(true, false);
			CameraHandle.localRotation = Quaternion.Euler(pitchRotation);

			//Vector3 centerPoint = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(10);
			//Transform flashlightTransform = FlashlightToggle.m_firstPersonLightTransform;

			//var targetForwardRotation = Quaternion.LookRotation(centerPoint - flashlightTransform.position, CameraHandle.transform.up);

			//// Smoothly rotate towards the target point.
			//if(Cursor.lockState == CursorLockMode.Locked )
			//	flashlightTransform.rotation = Quaternion.Lerp(flashlightTransform.rotation, targetForwardRotation, 5f * Time.deltaTime);
		}

		private void ProcessInput(NetworkedInputStruct input)
		{
			// Processing input - look rotation, jump, movement, weapon fire, weapon switching, weapon reloading, spray decal.
			// Debug.Log(string.Format( "Look Rotation: {0}", input.LookRotationDelta));
			KCC.AddLookRotation(input.LookRotationDelta, -VerticalCameraClamp.x, VerticalCameraClamp.y);

			// It feels better when player falls quicker
			KCC.SetGravity(KCC.RealVelocity.y >= 0f ? -UpGravity : -DownGravity);

			var inputDirection = KCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
			var jumpImpulse = 0f;

			if (input.Buttons.WasPressed(m_previousButtons, EInputButton.Jump) && KCC.IsGrounded && !m_isDowned)
			{
				jumpImpulse = JumpForce;
			}

			if (KCC.HasJumped)
			{
				m_jumpCount++;
			}

			if (input.Buttons.IsSet(EInputButton.Fire))
			{
				bool justPressed = input.Buttons.WasPressed(m_previousButtons, EInputButton.Fire);
				bool justReleased = input.Buttons.WasReleased(m_previousButtons, EInputButton.Fire);

				if (m_equippedTool.HoldToUse && justPressed)
				{
					if(justPressed)
						m_equippedTool.StartUse();
					else if(justReleased)
						m_equippedTool.StopUse();
				}
				else if(!m_equippedTool.HoldToUse && justPressed)
				{
					m_equippedTool.Use();
				}


			}
			else if (input.Buttons.IsSet(EInputButton.Reload))
			{
				//Weapons.Reload();
			}

			if (input.Buttons.WasPressed(m_previousButtons, EInputButton.Suicide))
			{
				//TODO - Remove debug input
				m_playerManager.DownPlayer(PlayerRef);
			}

			if (HasInputAuthority && Runner.IsForward)
			{
				RaycastHit interactionHit;
				Vector3? interactDireaction = KCC.LookDirection;
				if (Physics.Raycast(CameraHandle.position, interactDireaction.Value, out interactionHit, 3f, LayerMask.GetMask("Interactable"), QueryTriggerInteraction.Collide))
				{
					//Player looking at new interactable collider;
					if(m_lastLookedAtInteractable != interactionHit.collider)
					{
						m_interactableManager.ShowInteractPrompt(interactionHit.collider);
					}
					m_lastLookedAtInteractable = interactionHit.collider;


					if (input.Buttons.IsSet(EInputButton.Interact) &&
						m_lastInteractableCollider != interactionHit.collider)
					{
						m_lastInteractableCollider = interactionHit.collider;
						m_interactableManager.InteractStart(m_lastInteractableCollider, interactDireaction);
					}
				}
				else
				{
					m_lastLookedAtInteractable = null;
					m_interactableManager.HideInteractPrompt();
					StopInteractable();
				}

				bool justReleased = input.Buttons.WasReleased(m_previousButtons, EInputButton.Interact);
				if (justReleased)
				{
					StopInteractable();
				}
			}


			Vector3 cameraForward = CameraHandle.TransformDirection(Vector3.forward).normalized;
			float movementDOT = Vector3.Dot(inputDirection.normalized, cameraForward);

			m_isSprinting = input.Buttons.IsSet(EInputButton.Sprint) && movementDOT >= .1f && !m_isDowned && !m_isCrouching;
			m_isCrouching = input.Buttons.IsSet(EInputButton.Crouch) && !m_isDowned;

			if (input.Buttons.WasPressed(m_previousButtons, EInputButton.Spray) && HasStateAuthority)
			{
				//if (Runner.GetPhysicsScene().Raycast(CameraHandle.position, KCC.LookDirection, out var hit, 2.5f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
				//{
				//	// When spraying on the ground, rotate it so it aligns with player view.
				//	var sprayOrientation = hit.normal.y > 0.9f ? KCC.TransformRotation : Quaternion.identity;
				//	Runner.Spawn(SprayPrefab, hit.point, sprayOrientation * Quaternion.LookRotation(-hit.normal));
				//}
			}

			float speed = CalculateSpeed(movementDOT);
			MovePlayer(inputDirection * speed, jumpImpulse);
			HandleStamina();

			// Store input buttons when the processing is done - next tick it is compared against current input buttons.
			m_previousButtons = input.Buttons;
		}

		private void StopInteractable()
		{
			if (m_lastInteractableCollider == null)
				return;

			if (m_lastInteractableCollider != null)
				m_interactableManager.InteractStop(m_lastInteractableCollider);
			m_lastInteractableCollider = null;
		}

		private void HandleStamina()
		{
			float? currentStamina = m_playerManager.GetPlayerStamina(PlayerRef);

			if (currentStamina == null)
				return;

			if (m_isSprinting && currentStamina.Value > 0)
			{
				m_playerManager.DrainPlayerStamina(PlayerRef, Runner.DeltaTime);
			}
			else
			{
				m_playerManager.RegenerateStamina(PlayerRef, Runner.DeltaTime);
			}
		}

		private float CalculateSpeed(float movementDOT)
		{
			float speed = movementDOT >= .1f ? m_forwardMoveSpeed : m_backwardMoveSpeed;
			if (m_isCrouching)
				speed = movementDOT >= .1f ? m_crouchMoveSpeed : m_backwardCrouchMoveSpeed;
			if (m_isDowned)
				speed = m_downedMoveSpeed;

			float? currentStamina = m_playerManager.GetPlayerStamina(PlayerRef);
			if (m_isSprinting && currentStamina != null && currentStamina.Value > 0)
				speed = m_sprintSpeed;

			return speed;
		}

		private void MovePlayer(Vector3 desiredMoveVelocity = default, float jumpImpulse = default)
		{
			float acceleration = 1f;

			if (desiredMoveVelocity == Vector3.zero)
			{
				// No desired move velocity - we are stopping.
				acceleration = KCC.IsGrounded == true ? GroundDeceleration : AirDeceleration;
			}
			else
			{
				acceleration = KCC.IsGrounded == true ? GroundAcceleration : AirAcceleration;
			}

			_moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);
			KCC.Move(_moveVelocity, jumpImpulse);
		}

		private void SetFirstPersonVisuals(bool firstPerson)
		{
			m_firstPersonCamera.enabled = firstPerson;
			FirstPersonRoot.SetActive(firstPerson);
			FirstPersonBody.SetActive(firstPerson);
			ThirdPersonBody.SetActive(firstPerson == false);
		}

		private Vector3 GetAnimationMoveVelocity()
		{
			if (KCC.RealSpeed < 0.01f)
				return default;

			var velocity = KCC.RealVelocity;

			// We only care about X an Z directions.
			velocity.y = 0f;

			if (velocity.sqrMagnitude > 1f)
			{
				velocity.Normalize();
			}

			// Transform velocity vector to local space.
			return transform.InverseTransformVector(velocity);
		}

		private void Die()
		{
			m_isAlive = false;
			ReviveInteractableTrigger.SetActive(false);
		}

		private void Downed()
		{
			m_isDowned = true;
		}

		private float GetMoveType()
		{
			m_moveType = MoveType.WALK;
			if (m_isCrouching)
				m_moveType = MoveType.CROUCH;
			if (m_isDowned)
				m_moveType = MoveType.DOWNED;

			return (float)m_moveType;
		}

		private void OnAliveChanged()
		{
			if(HasInputAuthority)
			{
				m_uIManager.ToggleDeathView(!m_isAlive);
				DeathCameraThirdPerson.SetActive(!m_isAlive);

				if (m_isAlive)
				{
					SetFirstPersonVisuals(true);
				}
			}

			if (m_isAlive)
			{
				ThirdPersonAnimator.enabled = true;
				KCC.SetColliderLayer(LayerMask.NameToLayer("PlayerKCC"));
				KCC.SetCollisionLayerMask(LayerMask.GetMask("Default", "PlayerKCC"));
			}
		}
		
		private void OnCrouchChanged()
		{
			KCC.SetHeight(m_isCrouching ? CrouchColliderHeight : WalkColliderHeight);
		}

		private void OnDownedChanged()
		{
			if (HasInputAuthority)
			{
				SetFirstPersonVisuals(!m_isDowned);
				DeathCameraThirdPerson.SetActive(m_isDowned);
			}

			if (m_isDowned)
				ThirdPersonAnimator.enabled = true;

			ThirdPersonAnimator.SetLayerWeight(1, m_isDowned ? 0 : 1);
			ThirdPersonAnimator.SetLayerWeight(2, m_isDowned ? 0 : 1);

			FirstPersonAnimator.SetLayerWeight(1, m_isDowned ? 0 : 1);
			FirstPersonAnimator.SetLayerWeight(2, m_isDowned ? 0 : 1);

			ReviveInteractableTrigger.SetActive(m_isDowned);

		}
	}
}
