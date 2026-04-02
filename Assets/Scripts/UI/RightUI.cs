using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class RightUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button inventoryButton;

    private SpiritInventoryUI inventoryUI;

    [Inject]
    public void Construct(SpiritInventoryUI ui)
    {
        inventoryUI = ui;
    }

    private void Start()
    {
        inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
    }

    private void OnInventoryButtonClicked()
    {
        inventoryUI.OnInventoryButtonClicke();
    }
}
