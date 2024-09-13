using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAI : InputDriver
{
    private ActionProbs probs;

    public BaseAI()
    {
        probs = new ActionProbs();
    }

    public override AiAction ProcessInput(NetworkInput input)
    {
        probs.Reset();
        UpdateProbs(input);
        return probs.Choose();
    }

    private void UpdateProbs(NetworkInput input)
    {
        //None
        probs.noneProb = 10;
        if (input.enemyDistance > 0.5f)
            probs.noneProb -= 1;
        else
        {
            if(input.gold < 0.2f)
                probs.noneProb += 9;
        }
        if (input.enemyDistance > 0.8f)
            probs.noneProb -= 1;
        if (input.allyDistance > 0.7f)
            probs.noneProb++;
        if (input.gold < 0.1f)
            probs.noneProb += 5;
        if (input.a5 > 0.01f)
            probs.noneProb += 10;
        //Mine
        if (input.mineLv >= 0.99f)
            probs.mineProb = 0;
        else
        {
            if (input.mineLv < 0.3f && input.gold > 0.3f)
            {
                probs.mineProb += 10;
            }

            if (input.mineLv < 0.9f && input.gold > 0.6f)
            {
                probs.mineProb += 10;
            }

            //si el enemigo esta cerca
            if (input.enemyDistance > 0.7f)
                probs.mineProb -= 2;
            //Si el enemigo tiene unidades y nosotros no
            if (input.a1 < 0.01f && input.e1 > 0.01f)
                probs.mineProb -= 4;
        }

       
        if (input.enemyDistance > 0.9f && input.a1 < 0.01f && input.gold < 0.07f)
            probs.pawnProb += 2;
        if (input.a3 > 0)
            probs.pawnProb--;

        if (input.gold < 0.15f)
            probs.archerProb++;
        if (input.enemyDistance < 0.3f)
            probs.archerProb ++;
        if (input.enemyDistance > 0.8f && input.a1 < 0.01f)
            probs.archerProb--;

        if (input.e1 is > 0.5f and < 0.7f)
            probs.torchProb += 3;

        if (input.e3 > 0f)
            probs.tntProb += 3;

        if (input.e5 > 0f)
        {
            probs.pawnProb--;
            probs.tntProb += 5;
        }
        if (input.gold > 0.2f)
            probs.knightProb += 3;
        
        if (input.gold < 0.1f)
            probs.archerProb = 0;
        if (input.gold < 0.06f)
            probs.pawnProb = 0;
        if (input.gold < 0.2f)
            probs.knightProb = 0;
        if (input.gold < 0.15f){}
            probs.tntProb = probs.torchProb = 0;
 
    }
    private class ActionProbs
    {
        public int noneProb;
        public int mineProb;
        public int pawnProb;
        public int torchProb;
        public int archerProb;
        public int tntProb;
        public int knightProb;

        public ActionProbs()
        {
            Reset();
        }

        private void CheckProbs()
        {
            noneProb = noneProb < 0 ? 0 : noneProb;
            mineProb = mineProb < 0 ? 0 : mineProb;
            pawnProb = pawnProb < 0 ? 0 : pawnProb;
            torchProb = torchProb < 0 ? 0 : torchProb;
            archerProb = archerProb < 0 ? 0 : archerProb;
            tntProb = tntProb < 0 ? 0 : tntProb;
            knightProb = knightProb < 0 ? 0 : knightProb;
        }
        public AiAction Choose()
        {
            CheckProbs();
            int total = noneProb + mineProb + pawnProb + torchProb + archerProb + tntProb + knightProb;
            int choice = Random.Range(0, total);
            int addedProb = noneProb;
            if (noneProb > 0 && choice < addedProb)
                return AiAction.None;
            addedProb += mineProb;
            if (mineProb > 0 && choice < addedProb)
                return AiAction.Mine;
            addedProb += pawnProb;
            if (pawnProb > 0 && choice < addedProb)
                return AiAction.Pawn;
            addedProb += torchProb;
            if (torchProb > 0 && choice < addedProb)
                return AiAction.Torch;
            addedProb += archerProb;
            if (archerProb > 0 && choice < addedProb)
                return AiAction.Archer;
            addedProb += tntProb;
            if (tntProb > 0 && choice < addedProb)
                return AiAction.TNT;
            addedProb += knightProb;
            if (knightProb > 0 && choice < addedProb)
                return AiAction.Knight;

            return AiAction.None;
        }

        public void Reset()
        {
            noneProb = mineProb = pawnProb = torchProb = archerProb = tntProb = knightProb = 1;
        }
    }
}



public enum AiAction
{
    None = 0,
    Mine = 1,
    Pawn = 2,
    Torch = 3,
    Archer = 4,
    TNT = 5,
    Knight = 6
}
public class NetworkInput
{
    public float gold; //0-1000
    public float enemyGold;
    public float allyDistance;
    public float enemyDistance;
    public float e1, e2, e3, e4, e5;
    public float a1, a2, a3, a4, a5;
    public float mineLv;

    public float[] AsArray()
    {
        return new float[] { gold, enemyGold, allyDistance, enemyDistance, e1, e2, e3, e4, e5, a1, a2, a3, a4, a5, mineLv}; 
    }

    public void UnitsFromArray(float[] allys, float[] enemys)
    {
        e1 = allys[0];
        e2 = allys[1];
        e3= allys[2];
        e4 = allys[3];
        e5 = allys[4];
        a1 = enemys[0];
        a2  = enemys[1];
        a3  = enemys[2];
        a4  = enemys[3];
        a5  = enemys[4];
    }
    public void FromArray(float[] inputs)
    {
        gold = inputs[0];
        enemyGold = inputs[1];
        allyDistance = inputs[2];
        enemyDistance = inputs[3];
        e1 = inputs[4];
        e2 = inputs[5];
        e3= inputs[6];
        e4 = inputs[7];
        e5 = inputs[8];
        a1 = inputs[9];
        a2  = inputs[10];
        a3  = inputs[11];
        a4  = inputs[12];
        a5  = inputs[13];
        mineLv = inputs[14];
    }
}