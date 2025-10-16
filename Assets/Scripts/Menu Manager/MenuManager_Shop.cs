using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;

using VInspector;
using static Constants;

// Partial class for managing the shop menu, including buying and selecting cosmetics and skills
public sealed partial class MenuManager : MonoBehaviour
{
    [Tab("Shop Menu")]
    [SerializeField] SpriteAtlas spriteAtlas;
    [SerializeField] GameObject shopCosmetics, shopSkills;
    [SerializeField] Image cosmeticPreviewImage;
    [SerializeField] GameObject cosmeticBuyButton;
    [SerializeField] TextMeshProUGUI[] cosmeticStyleTexts;
    [SerializeField] TextMeshProUGUI[] skillLevelTexts;
    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] GameObject[] skillBuyButtons;
    [SerializeField] AudioClip[] buySounds;
    int selectedItemIndex, selectedItemCost;

    // These stats are saved and loaded using PlayerPrefs
    bool[] birdsBought = new bool[MaxBirdTypes], backgroundsBought = new bool[MaxBackgroundTypes], obstaclesBought = new bool[MaxObstacleTypes];
    int coin;
    int birdSelected, backgroundSelected, obstacleSelected;
    int skill1Level, skill2Level;

    enum ShopType : byte { Bird = 1, Background = 2, Obstacle = 3, Skill = 4 }
    ShopType shopType;

    // Opens the cosmetics shop (Birds, Backgrounds, Obstacles) and sets up UI
    public void OpenCosmeticShop(int index)
    {
        shopMenu.SetActive(false);
        shopCosmetics.SetActive(true);
        cosmeticBuyButton.SetActive(false);
        shopType = (ShopType)index;

        // Determine item count and set text visibility (how many)
        int itemCount = shopType == ShopType.Bird ? MaxBirdTypes : shopType == ShopType.Background ? MaxBackgroundTypes : shopType == ShopType.Obstacle ? MaxObstacleTypes : 0;
        for (int i = 0; i < cosmeticStyleTexts.Length; i++)
            cosmeticStyleTexts[i].gameObject.SetActive(i < itemCount);

        // Update selected item cost based on shop type
        selectedItemCost = shopType == ShopType.Bird ? BirdUnlockCost : shopType == ShopType.Background ? BackgroundUnlockCost : ObstacleUnlockCost;
        // Set initial selected item index to current selection
        selectedItemIndex = shopType == ShopType.Bird ? birdSelected : shopType == ShopType.Background ? backgroundSelected : obstacleSelected;
        // Adjust preview image size for obstacles (taller)
        cosmeticPreviewImage.rectTransform.sizeDelta = shopType == ShopType.Obstacle ? new Vector2(141, 512) : new Vector2(512, 512);

        UpdateCosmeticItemUI();  // Update item UI (colors, preview image, buy button)
    }

    // Updates cosmetic item UI: text colors (yellow for bought+selected, white for bought, grey for locked), preview image, and buy button
    void UpdateCosmeticItemUI()
    {
        int selectedIndex = shopType == ShopType.Bird ? birdSelected : shopType == ShopType.Background ? backgroundSelected : obstacleSelected;

        for (int i = 0; i < cosmeticStyleTexts.Length; i++)
            if (cosmeticStyleTexts[i].gameObject.activeSelf)
                cosmeticStyleTexts[i].color = i == selectedIndex && (shopType == ShopType.Bird ? birdsBought[i] : shopType == ShopType.Background ? backgroundsBought[i] : obstaclesBought[i]) ? Color.yellow : (shopType == ShopType.Bird ? birdsBought[i] : shopType == ShopType.Background ? backgroundsBought[i] : obstaclesBought[i]) ? Color.white : Color.grey;

        cosmeticPreviewImage.sprite = spriteAtlas.GetSprite(shopType.ToString() + selectedItemIndex);
    }

    // Selects a cosmetic item and updates UI if the item is bought
    public void SelectCosmeticItem(int index)
    {
        selectedItemIndex = index;

        // Update selected index based on shop type if the item is bought
        bool isBought = shopType == ShopType.Bird ? birdsBought[index] : shopType == ShopType.Background ? backgroundsBought[index] : obstaclesBought[index];
        if (isBought)  // If bought, select it
        {
            cosmeticBuyButton.SetActive(false);
            if (shopType == ShopType.Bird) birdSelected = index;
            else if (shopType == ShopType.Background) backgroundSelected = index;
            else if (shopType == ShopType.Obstacle) obstacleSelected = index;
        }
        else cosmeticBuyButton.SetActive(true);

        UpdateCosmeticItemUI();
    }

    public void BuySelectedItem()
    {
        if (coin >= selectedItemCost)
        {
            audioSource.PlayOneShot(buySounds[Random.Range(0, buySounds.Length)]);
            coin -= selectedItemCost;
            coinText.text = coin.ToString();
            cosmeticBuyButton.SetActive(false);

            if (shopType == ShopType.Bird) { birdsBought[selectedItemIndex] = true; birdSelected = selectedItemIndex; }
            else if (shopType == ShopType.Background) { backgroundsBought[selectedItemIndex] = true; backgroundSelected = selectedItemIndex; }
            else if (shopType == ShopType.Obstacle) { obstaclesBought[selectedItemIndex] = true; obstacleSelected = selectedItemIndex; }

            UpdateCosmeticItemUI();
        }
        else audioSource.PlayOneShot(cantDoThatSound);
    }

    public void OpenSkillShop()
    {
        shopMenu.SetActive(false);
        shopSkills.SetActive(true);
        UpdateSkillUI();
    }

    public void BuySkill(int index)
    {
        if (coin >= SkillUnlockCost)
        {
            audioSource.PlayOneShot(buySounds[Random.Range(0, buySounds.Length)]);
            coin -= SkillUnlockCost;
            coinText.text = coin.ToString();

            if (index == 0) skill1Level++;
            else if (index == 1) skill2Level++;
            UpdateSkillUI();
        }
        else audioSource.PlayOneShot(cantDoThatSound);
    }

    // Helper to update skill UI: levels text and buy button states
    void UpdateSkillUI()
    {
        skillLevelTexts[0].text = skill1Level + " / 3";
        skillLevelTexts[1].text = skill2Level + " / 3";
        skillBuyButtons[0].SetActive(skill1Level < 3);
        skillBuyButtons[1].SetActive(skill2Level < 3);
    }
}