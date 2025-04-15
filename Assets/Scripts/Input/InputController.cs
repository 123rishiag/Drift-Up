using UnityEngine;

public class InputController : MonoBehaviour
{
    public InputControls inputControls;

    private void Awake()
    {
        inputControls = new InputControls();
    }
    private void OnEnable()
    {
        inputControls.Enable();
    }
    private void OnDisable()
    {
        inputControls.Disable();
    }
}
