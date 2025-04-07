using UnityEngine;

public class FortuneWheelAward : MonoBehaviour
{
    [SerializeField] private AwardType _awardType;

    private enum AwardType
    {
        Money25,
        Money100,
        Money150,
        Money250,
        Skin,
        Respin,
        IncomeLevel,
        LuckLevel,
    }

    public void GetAward()
    {
        switch(_awardType)
        {
            case AwardType.Money25:
                GetMoney(25);
                break;

            case AwardType.Money100:
                GetMoney(100);
                break;

            case AwardType.Money150:
                GetMoney(150);
                break;

            case AwardType.Money250:
                GetMoney(250);
                break;

            case AwardType.Skin:
                GetSkin();
                break;

            case AwardType.Respin:
                GetRespin();
                break;

            case AwardType.IncomeLevel:
                GetIncomeLevel();
                break;

            case AwardType.LuckLevel:
                GetLuckLevel();
                break;
        }

        Debug.Log("Stop " + _awardType);
    }

    private void GetMoney(int count)
    {
        GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + count);
    }

    private void GetSkin()
    {
        Debug.Log("Unlock Skin");
    }

    private void GetRespin()
    {
        PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) + 1);

        GameManager.Instance.UIController.UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());
    }

    private void GetIncomeLevel()
    {
        GameManager.Instance.EnableIncomeMode(isGift: true);

    }

    private void GetLuckLevel()
    {
        GameManager.Instance.EnableLuckMode(isGift: true);
    }
}
