using System.Collections.Generic;
using System.Security;
using FishONU.CardSystem;
using FishONU.CardSystem.CardArrangeStrategy;
using FishONU.Utils;
using UnityEngine;
using Color = FishONU.CardSystem.Color;

namespace FishONU.GamePlay.GameState
{
    /// <summary>
    /// 游戏刚开始的准备阶段
    ///
    /// 进行初始化牌堆、洗牌、发牌等
    /// </summary>
    public class PrepareState : GameState
    {
        protected override void OnServerEnter(GameStateManager manager)
        {
            Debug.Log("服务端进入准备阶段");

            // 初始化牌堆
            // 标准的游戏卡牌共108张，其中包括数字牌76张，功能牌24张和变色牌8张
            // 数字牌包括0-9十个数字，分为红黄蓝绿四种颜色，其中1-9数字每种颜色各有两张，0每种颜色只有一张，共计76张
            // 功能牌有三种：跳过(Skip)、反转(Reverse)和+2(Draw two)，分为红黄蓝绿四种颜色，每种颜色各有2张，共计24张
            // 万能牌有两种：变色(Wild)和变色+4(Wild Draw 4)，牌面都是黑色（也可视为无色），每种各有4张，共计8张。

            #region 抽牌堆初始化

            var drawPileOwnerInventory = manager.drawPile.GetComponent<OwnerInventory>();

            drawPileOwnerInventory.Cards.Clear();

            // 生成牌堆
            var deck = CardDataFactory.CreateStandardDeck();

            // 洗牌
            deck.FisherYatesShuffle();


            // 我不知道什么 b bug 导致 AddRange 后没有 Callback
            // 可能是因为同一帧就不同步了吧，但是底下 players 的 AddRange 是同步的
            drawPileOwnerInventory.Cards.AddRange(deck);
            // manager.drawPile.GetComponent<SecretInventory>()
            //     ?.RpcManualSyncCardView(drawPileOwnerInventory.Cards.Count);


            //  发牌
            foreach (var player in manager.players)
            {
                var playerInventory = player.GetComponent<OwnerInventory>();
                var newCards = new List<CardData>(7);

                for (int i = 0; i < 7; i++)
                {
                    if (drawPileOwnerInventory.Cards.TryPop(out var card))
                    {
                        newCards.Add(card);
                    }
                    else
                    {
                        Debug.Log("牌堆不足");
                        // TODO: 结束游戏
                    }
                }

                playerInventory.Cards.AddRange(newCards);
            }

            #endregion

            #region 弃牌堆初始化

            var discardPileInventory = manager.discardPile.GetComponent<DiscardInventory>();
            discardPileInventory.Cards.Clear();

            #endregion

            #region 抽牌开始流程

            CardData firstCard;
            while (true)
            {
                if (drawPileOwnerInventory.Cards.TryPop(out firstCard))
                {
                    if (firstCard.face == Face.WildDrawFour || firstCard.color == Color.Black)
                    {
                        drawPileOwnerInventory.Cards.InsertRandom(firstCard);
                        continue;
                    }

                    break;
                }
                else
                {
                    Debug.Log("牌堆不足");
                    // TODO: 结束游戏
                    break;
                }
            }

            discardPileInventory.Cards.Add(firstCard);

            Debug.Log($"抽到第一张牌：{firstCard.color.ToString()} {firstCard.face.ToString()}");

            #endregion

            // 开始游戏，进入出牌阶段
        }

        protected override void OnClientEnter(GameStateManager manager)
        {
            base.OnClientEnter(manager);

            Debug.Log("客户端进入准备阶段");
        }
    }
}