using System;
using System.Collections.Generic;

namespace BubbleBots.Gameplay.Models
{
    public class PlayerRoster
    {
        public List<BubbleBot> bots;
        public int currentBot;
        public void TakeDamage(int damage)
        {
            if (currentBot < bots.Count)
            {
                bots[currentBot].hp = Math.Max(0, bots[currentBot].hp - damage);
            }
        }
        public bool IsDead(int botIndex)
        {
            return bots[botIndex].hp <= 0;
        }

        public bool AreAllBotsDead()
        {
            bool allBotsDead = true;
            for (int i = 0; i < bots.Count; ++i)
            {
                if (bots[i].hp > 0)
                {
                    allBotsDead = false;
                    break;
                }
            }
            return allBotsDead;
        }
        public void ResetRoster()
        {
            currentBot = 0;
            for (int i = 0; i < bots.Count; ++i)
            {
                bots[i].hp = bots[i].maxHp;
            }
        }
    }
}
