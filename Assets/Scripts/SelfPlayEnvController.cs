using System;
using UnityEngine;

public class SelfPlayEnvController : MonoBehaviour
{

    [SerializeField] SelfPlayEnemy[] enemies;

    public void TakeDmg(SelfPlayEnemy agent, float dmg)
    {
        SelfPlayEnemy otherAgent = enemies[0] == agent ? enemies[1] : enemies[0];

        //otherAgent.AddReward(dmg);
        //agent.AddReward(-dmg);
    }

    public void ReducedToNoHealth(SelfPlayEnemy agent, float killReward)
    {
        SelfPlayEnemy otherAgent = enemies[0] == agent ? enemies[1] : enemies[0];

        otherAgent.SetReward(1);
        agent.SetReward(-1);

        agent.EndEpisode();
        otherAgent.EndEpisode();
    }

    // End episode for both agents and reward the winner. 
    // Reward is 1 for winner and -1 for loser and 0 for draw, this is for the elo system used
    // when self play is enabled.
    internal void EndEpisode(SelfPlayEnemy callingAgent, bool lost = false)
    {
        SelfPlayEnemy enemy1 = enemies[0];
        SelfPlayEnemy enemy2 = enemies[1];

        SelfPlayEnemy winner;
        if (!lost)
        {
            winner = enemy1.healthSystem.Health > enemy2.healthSystem.Health ? enemy1 : enemy2;
        }
        else
        {
            winner = callingAgent == enemy1 ? enemy2 : enemy1;
        }

        bool hasWinner = lost || enemy1.healthSystem.Health != enemy2.healthSystem.Health;

        foreach (var enemy in enemies)
        {
            if (hasWinner)
            {
                if (winner == enemy)
                {
                    enemy.SetReward(1f);
                }
                else
                {
                    enemy.SetReward(-1f);
                }
            }
            
            enemy.EndEpisode();
        }
    }
}
