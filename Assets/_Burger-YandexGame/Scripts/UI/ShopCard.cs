using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCard : MonoBehaviour
{
    [field: SerializeField] public Button CardButton {  get; set; }
    [field: SerializeField] public TMP_Text CardButtonText { get; set; }
    [field: SerializeField] public int SkinID { get; set; }
    [SerializeField] private int _price = 100;


    private void Start()
    {
        CardButtonText.text = _price.ToString();
        CardButton.onClick.AddListener(ChangeOrBuySkin);

        if(SkinID == 0 && GameManager.Instance.CurrentSkinID == 0)
        {
            CardButtonText.text = "Selected";
            CardButton.interactable = false;
        }
        else if(GameManager.Instance.CurrentSkinID == SkinID)
        {
            CardButtonText.text = "Selected";
            CardButton.interactable = false;
        }
        else if(GameManager.Instance.PurchasedSkins.Contains(SkinID))
        {
            CardButtonText.text = "Select";
            CardButton.interactable = true;
        }
    }

    private void ChangeOrBuySkin()
    {
        if(GameManager.Instance.PurchasedSkins.Contains(SkinID))
        {
            GameManager.Instance.ChangeSkin(SkinID);
            GameManager.Instance.UIController.ResetAllButtons();
            CardButtonText.text = "Selected";
            CardButton.interactable = false;
        }
        else
        {
            if(PlayerPrefs.GetInt(SaveData.MoneyKey) >= _price) 
            {
                GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) - _price);
                GameManager.Instance.PurchasedSkins.Add(SkinID);
                CardButtonText.text = "Select";
            }
        }
    }
}
