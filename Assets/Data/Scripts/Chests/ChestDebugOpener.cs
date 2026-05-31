using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Chests
{
    public class ChestDebugOpener : MonoBehaviour
    {
        [SerializeField] private string _chestId = "Chest_1";
        [SerializeField] private InputAction _openAction = new(
            "Open Chest",
            InputActionType.Button,
            "<Keyboard>/space");

        private IChestService _chestService;

        [Inject]
        public void Construct(IChestService chestService)
        {
            _chestService = chestService;
        }

        private void OnEnable()
        {
            if (_openAction == null)
            {
                return;
            }

            _openAction.performed += OnOpenPerformed;
            _openAction.Enable();
        }

        private void OnDisable()
        {
            if (_openAction == null)
            {
                return;
            }

            _openAction.performed -= OnOpenPerformed;
            _openAction.Disable();
        }

        private void OnOpenPerformed(InputAction.CallbackContext context)
        {
            _chestService?.TryOpenChest(_chestId, out _);
        }
    }
}
