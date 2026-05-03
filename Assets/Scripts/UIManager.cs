using UnityEngine;

public class UIManager : MonoBehaviour
{
    public PlayerData playerData;
    public TMPro.TextMeshProUGUI leafText;
    public TMPro.TextMeshProUGUI coinText;
    public TMPro.TextMeshProUGUI bushText;

    private void Start()
    {
        UpdateCurrencyUI();
        UpdateBrickUI();
    }

    public void UpdateCurrencyUI()
    {
        if (leafText != null)
            leafText.text = "" + playerData.leaf.ToString();
        if (coinText != null)
            coinText.text = "" + playerData.coin.ToString();
    }

    public void UpdateBrickUI()
    {
        if (bushText != null)
        {
            var bushData = playerData.ownedBricks.Find(b => b.data != null && b.data.brickType == BrickType.Bush);
            if (bushData != null)
            {
                bushText.text = "" + bushData.quantity.ToString();
            }
            else
            {
                bushText.text = "0";
            }
        }
    }
}