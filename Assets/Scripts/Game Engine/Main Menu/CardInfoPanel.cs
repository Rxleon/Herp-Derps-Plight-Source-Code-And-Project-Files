﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardInfoPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Proeprties + Component References
    #region
    [Header("Properties")]
    [HideInInspector] public CardDataSO dataSoRef;
    [HideInInspector] public CardData cardDataRef;
    [HideInInspector] public int copiesCount = 0;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private CardPanelLocation location;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI energyCostText;
    public TextMeshProUGUI copiesCountText;

    [Header("Image Components")]
    [SerializeField] private Image talentUnderlay;
    [SerializeField] private Image talentOverlay;
    [SerializeField] private Image rarityOverlay;
    [SerializeField] private Image cardTypeImage;
    #endregion

    // Setup + Initialization
    #region
    public void BuildCardInfoPanelFromCardDataSO(CardDataSO data)
    {
        dataSoRef = data;
        cardNameText.text = data.cardName;
        energyCostText.text = data.cardEnergyCost.ToString();

        talentOverlay.color = ColorLibrary.Instance.GetTalentColor(data.talentSchool);
        talentOverlay.color = new Color(talentOverlay.color.r, talentOverlay.color.g, talentOverlay.color.b, 0.5f);

        rarityOverlay.color = ColorLibrary.Instance.GetRarityColor(data.rarity);
        rarityOverlay.color = new Color(rarityOverlay.color.r, rarityOverlay.color.g, rarityOverlay.color.b, 0.5f);

        cardTypeImage.sprite = SpriteLibrary.Instance.GetCardTypeImageFromTypeEnumData(data.cardType);     

        copiesCount++;
        copiesCountText.text = "x" + copiesCount.ToString();
    }
    public void BuildCardInfoPanelFromCardData(CardData data)
    {
        cardDataRef = data;
        cardNameText.text = data.cardName;
        energyCostText.text = data.cardBaseEnergyCost.ToString();

        talentOverlay.color = ColorLibrary.Instance.GetTalentColor(data.talentSchool);
        talentOverlay.color = new Color(talentOverlay.color.r, talentOverlay.color.g, talentOverlay.color.b, 0.5f);

        rarityOverlay.color = ColorLibrary.Instance.GetRarityColor(data.rarity);
        rarityOverlay.color = new Color(rarityOverlay.color.r, rarityOverlay.color.g, rarityOverlay.color.b, 0.5f);

        cardTypeImage.sprite = SpriteLibrary.Instance.GetCardTypeImageFromTypeEnumData(data.cardType);

        copiesCount++;
        copiesCountText.text = "x" + copiesCount.ToString();
    }
    #endregion

    // Input listeners
    #region
    public void OnPointerEnter(PointerEventData eventData)
    {
        talentUnderlay.color = hoverColor;
        if (location == CardPanelLocation.CharacterInfoWindow)
        {
            MainMenuController.Instance.BuildAndShowCardViewModelPopup(dataSoRef);
            KeyWordLayoutController.Instance.BuildAllViewsFromKeyWordModels(dataSoRef.keyWordModels);
        }
        else if (location == CardPanelLocation.ChooseCardScreen)
        {
            LootController.Instance.BuildAndShowCardViewModelPopup(cardDataRef);
            KeyWordLayoutController.Instance.BuildAllViewsFromKeyWordModels(cardDataRef.keyWordModels);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        talentUnderlay.color = normalColor;
        if (location == CardPanelLocation.CharacterInfoWindow)
        {
            MainMenuController.Instance.HidePreviewCard();
            KeyWordLayoutController.Instance.FadeOutMainView();
        }
        else if (location == CardPanelLocation.ChooseCardScreen)
        {
            LootController.Instance.HidePreviewCard();
            KeyWordLayoutController.Instance.FadeOutMainView();
        }

    }
    #endregion
}

public enum CardPanelLocation
{
    None = 0,
    CharacterInfoWindow = 1,
    ChooseCardScreen = 2,
}
