using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideManager : MonoBehaviour
{
    [NonSerialized] public SideState state;
    public Tower tower;
    public bool blueSide;
    [Header("Scene components")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private SpriteRenderer mine;
    [Header("Sprites")]
    [SerializeField] private Sprite[] mineSprites;
    //[SerializeField] private Sprite towerUp, towerDown, castleUp, castleDown;
    [Header("Minions")]
    [SerializeField] private MinionController minionPrefab;
    [SerializeField] private List<Minion> minionStats;
    private bool _defeated;
   
    [Header("Mine")]
    public int[] goldAmounts = new int[] { 30, 45, 60 };
    public int[] levelUpCosts = new int[] { 300, 600 };
    public Animator goldAnim;
    public float goldTime = 1.5f, goldAnimTime = 0.62f;
    public TextMeshProUGUI goldCostText;
    public Image goldImage;
    [NonSerialized] public bool finished;
    [NonSerialized] public Action<MinionController> OnMinionSpawn;

    void Awake()
    {
        Reset();
        tower.onDamage += OnTowerDamage;
        tower.onDestroy += OnTowerDeath;
    }

    public void Reset()
    {
        finished = false;
        state = new SideState();
        _defeated = false;
        if(goldText!= null)
            goldText.text = state.gold.ToString();
        mine.sprite = mineSprites[state.goldMineLV];
        if (blueSide && goldImage != null)
        {
            goldImage.sprite = mineSprites[state.goldMineLV + 1];
            goldCostText.text = levelUpCosts[0].ToString();
        }
        tower.Reset();
        tower.life = SideState.MaxTowerLife;
        StopAllCoroutines();
        StartCoroutine(GetGold());
    }

    public void OnTowerDamage()
    {
        state.towerLife = tower.life;
    }

    public void OnTowerDeath()
    {
        finished = true;
        //Debug.Log(transform.parent.parent.name + " end game " + (blueSide ? "Red side wins" : "Blue side wins"));
    }

    public void Finish()
    {
        finished = true;
        _defeated = true;
    }

    public IEnumerator GetGold()
    {
        do
        {
            yield return new WaitForSeconds(goldTime);
            state.gold += goldAmounts[state.goldMineLV];
            if(goldText!=null)
                goldText.text = state.gold.ToString();
            StartCoroutine(GoldSpawn());
        } while (!_defeated);
    }
    
    public void LevelUpMine()
    {
        if (state.goldMineLV >= SideState.MaxMineLevel)
            return;
        if (!Buy(levelUpCosts[state.goldMineLV]))
            return;
        state.goldMineLV++;
        mine.sprite = mineSprites[state.goldMineLV];
        if (blueSide)
        {
            if(state.goldMineLV < SideState.MaxMineLevel  && goldImage != null)
            {
                goldImage.sprite = mineSprites[state.goldMineLV +1];
                goldCostText.text = levelUpCosts[state.goldMineLV].ToString();
            }
        }
    }

    public bool Buy(int amount)
    {
        if (amount > state.gold)
            return false;
        state.gold -= amount;
        if(goldText!=null)
            goldText.text = state.gold.ToString();
        return true;
    }
    public void EnemySpawn()
    {
        SpawnMinion(UnityEngine.Random.Range(0, minionStats.Count));
        Invoke(nameof(EnemySpawn), 5f);
    }
    public void SpawnMinion(int index)
    {
        if (tower.IsDestroyed())
            return;
        if (!Buy(minionStats[index].goldCost))
            return;
        var minion = Instantiate(minionPrefab, spawnPoint1);
        minion.stats = minionStats[index];
        minion.blueSide = blueSide;
        if (!blueSide)
            minion.transform.localScale = new Vector3(-1, 1, 1);
        int scale = blueSide ? 1 : -1;
        if(minion.stats.attackType != Attack.Melee)
        {
            minion.transform.Translate(new Vector3(UnityEngine.Random.Range(-0.05f, -0.15f) * scale, 0, 0));
        }
        else
        {
            minion.transform.Translate(new Vector3(UnityEngine.Random.Range(0.05f, 0.15f) * scale, 0, 0));
        }
        minion.name = minionStats[index].name + "_" + (blueSide ? "blue" : "red");
        OnMinionSpawn?.Invoke(minion);
    }

    public IEnumerator GoldSpawn()
    {
        goldAnim.gameObject.SetActive(true);
        goldAnim.Play("GoldAnimation", -1, 0f);
        yield return new WaitForSeconds(goldAnimTime);
        goldAnim.gameObject.SetActive(false);
    }
}

[Serializable]
public class SideState
{
    public const int MaxTowerLife = 1000;
    public const int MaxCastleLife = 2000;
    public const int MaxMineLevel = 2;
    public int goldMineLV;
    public int towerLife;
    public int castleLife;
    public int gold;
    public SideState()
    {
        goldMineLV = 0;
        towerLife = MaxTowerLife;
        castleLife = MaxCastleLife;
        gold = 0;
    }


}
