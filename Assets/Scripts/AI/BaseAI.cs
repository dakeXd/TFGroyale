using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAI : InputDriver
{
    private ActionProbs probs;

    private void Start()
    {
        probs = new ActionProbs();
    }
    public override AiAction ProcessInput(NetworkInput input)
    {
        probs.Reset();
        //DO something
        return probs.Choose();
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
    public float gold;
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