﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AssemblyHijack.Automation
{
    internal class MergeCard : Runnable
    {
        private Card target = null;
        private IList<Card> children = new List<Card>();
        private int expectedCoin = 0;

        protected override bool Check()
        {
            if (!Game.runtimeData.user.inventory.isFull)
                return false;

            children.Clear();
            expectedCoin = 0;
            int childrenBonus = 0;
            StringBuilder cardNames = new StringBuilder();
            foreach (var card in Game.runtimeData.user.inventory.cards.Values)
            {
                if (card.inUse || card.bookmark)
                    continue;

                if (MyGameConfig.merge.cards.Contains(card.monsterId))
                {
                    childrenBonus += card.bonus;
                    children.Add(card);

                    if (cardNames.Length > 0)
                        cardNames.Append(",");
                    cardNames.AppendFormat("[{0}]", card.name);

                    if (children.Count >= 5)
                        break;
                }
            }

            target = null;
            if (children.Count < 1)
            {
                Debug.Log(String.Format("沒有飼料卡可供餵食！"));
                return false;
            }

            foreach (var cardId in Game.runtimeData.teamCardIds)
            {
                var card = Game.runtimeData.user.inventory.GetCard(cardId);
                Debug.Log(String.Format("[{0}] {1} / {2}", card.name, card.accumulativeLevelExp, card.accumulativeNextLevelMinExp));
                if (card.accumulativeLevelExp < card.accumulativeNextLevelMinExp)
                {
                    expectedCoin = card.mergeCoin * children.Count + (card.bonus + childrenBonus) * Core.Config.UPGRADE_CARDBOUNS_COST;
                    Debug.Log(String.Format("升級[{0}]預估需要[{1:#,0}]錢！", card.name, expectedCoin));

                    if (Game.runtimeData.user.coin >= expectedCoin)
                    {
                        Debug.Log(String.Format("判定升級[{0}], 餵食{1}", card.name, cardNames));

                        target = card;
                        return true;
                    }
                }
            }

            Debug.Log(String.Format("找不到合適的卡片進行升級！"));

            return false;
        }

        protected override void Execute(Action next)
        {
            StringBuilder cardNames = new StringBuilder();
            foreach (var card in children)
            {
                if (cardNames.Length > 0)
                    cardNames.Append("\n");

                cardNames.AppendFormat("{0}", card.name);
            }

            MyDialog.ShowWaiting("正在花費{0:#,0}升級[{1}]\n{2}", expectedCoin, target.name, cardNames);
            Game.SetMonsterUpgradeTarget(target);
            Game.SetMonsterUpgradeChildren(children.ToArray());
            Game.UpgradeMonster(() =>
                {
                    MyDialog.Close();
                    next();
                });
        }
    }
}