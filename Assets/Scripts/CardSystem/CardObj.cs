using System;
using System.Collections.Generic;
using DG.Tweening;
using Telepathy;
using UnityEngine;
using UColor = UnityEngine.Color;

namespace FishONU.CardSystem
{
    [Serializable]
    public struct UFaceSpritePair
    {
        public Face face;
        public Sprite sprite;
    }

    [System.Serializable]
    public struct UColorMaterialPair
    {
        public Color color;
        public Material material;
    }


    // TODO: 如果有性能问题就考虑在这使用对象池吧
    public class CardObj : MonoBehaviour
    {
        public bool IsHover { get; private set; }
        public bool isDrag = false;

        public CardData data;

        public Action<CardObj> OnCardClick;


        [Header("引用")] [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("卡面颜色")] public UColor red = new UColor(255f, 0, 0);
        public UColor blue = new UColor(0, 0, 255f);
        public UColor green = new UColor(0, 255f, 0);
        public UColor yellow = new UColor(255f, 255f, 0);
        public UColor black = new UColor(0, 0, 0);

        [Header("卡面贴图")] [SerializeField] private List<UFaceSpritePair> faceSprites;
        [SerializeField] private List<UColorMaterialPair> colorMaterials;

        void Start()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Log.Error("CardObj 未找到 SpriteRenderer");
                    Destroy(gameObject);
                }
            }
        }


        public void Load(CardData cardData)
        {
            if (cardData == null)
            {
                Debug.LogError("CardObj.Load: cardInfo is null");
            }

            data = cardData;

            if (data == null)
            {
                Debug.LogError("CardObj.Load: cardInfo is null");
                return;
            }

            // 显示对应贴图
            foreach (var pair in faceSprites)
            {
                if (pair.face != data.face) continue;
                spriteRenderer.sprite = pair.sprite;
                break;
            }

            foreach (var pair in colorMaterials)
            {
                if (pair.color != data.color) continue;
                spriteRenderer.material = pair.material;
                break;
            }
        }

        public CardData GetCardInfo()
        {
            return data;
        }

        public void FadeOutAndDestroy(float time = 0f)
        {
            if (spriteRenderer == null) Destroy(gameObject);

            transform.DOKill();

            if (time == 0f) Destroy(gameObject);

            spriteRenderer.DOFade(0, time).OnComplete(() => Destroy(gameObject));
        }

        private void OnMouseEnter()
        {
            IsHover = true;
        }

        private void OnMouseExit()
        {
            IsHover = false;
        }

        private void OnMouseDown()
        {
            OnCardClick?.Invoke(this);
        }
    }
}