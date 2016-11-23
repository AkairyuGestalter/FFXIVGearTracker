using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV.GearTracking.Core
{
	[Serializable]
    public class Character
    {
        public String charName;
        public Job currentJob;
        public Statistics[] baseStats;
        public StatWeights currentWeights;
        public GearSet[] currentDamage;
        public GearSet[] currentCoilFoodA;
        public GearSet[] currentCoilFoodB;
		public GearSet[] ownedDamage;
		public GearSet[] ownedCoilFoodA;
		public GearSet[] ownedCoilFoodB;
        public GearSet[] progressionDamage;
        public GearSet[] progressionCoilFoodA;
        public GearSet[] progressionCoilFoodB;
        public GearSet[] idealDamage;
        public GearSet[] idealCoilFoodA;
        public GearSet[] idealCoilFoodB;
        public int clearedTurn;
        public double clearedRaid;
		public int[] accuracyNeeds;
		public int[] ownedAccReqListA;
		public int[] ownedAccReqListB;
		public double[] tomeTier;
		public int[] relicTier;
		public List<string> ownedItems;

        public Job CurrentJob
        {
            get { return currentJob; }
            set { currentJob = value; }
        }
        public Character()
        {
			currentDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
			for (int i = 0; i < currentDamage.Length; i++)
			{
				currentDamage[i] = new GearSet();
			}
			currentCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
			for (int i = 0; i < currentCoilFoodA.Length; i++)
			{
				currentCoilFoodA[i] = new GearSet();
			}
			currentCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
			for (int i = 0; i < currentCoilFoodB.Length; i++)
			{
				currentCoilFoodB[i] = new GearSet();
			}
			ownedDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
			for (int i = 0; i < ownedDamage.Length; i++)
			{
				ownedDamage[i] = new GearSet();
			}
			ownedCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
			for (int i = 0; i < ownedCoilFoodA.Length; i++)
			{
				ownedCoilFoodA[i] = new GearSet();
			}
			ownedCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
			for (int i = 0; i < ownedCoilFoodB.Length; i++)
			{
				ownedCoilFoodB[i] = new GearSet();
			}
            progressionDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
            for (int i = 0; i < progressionDamage.Length; i++)
            {
                progressionDamage[i] = new GearSet();
            }
            progressionCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
            for (int i = 0; i < progressionCoilFoodA.Length; i++)
            {
                progressionCoilFoodA[i] = new GearSet();
            }
            progressionCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
            for (int i = 0; i < progressionCoilFoodB.Length; i++)
            {
                progressionCoilFoodB[i] = new GearSet();
            }
            idealDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
            for (int i = 0; i < idealDamage.Length; i++)
            {
                idealDamage[i] = new GearSet();
            }
            idealCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
            for (int i = 0; i < idealCoilFoodA.Length; i++)
            {
                idealCoilFoodA[i] = new GearSet();
            }
            idealCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
            for (int i = 0; i < idealCoilFoodB.Length; i++)
            {
                idealCoilFoodB[i] = new GearSet();
			}
			baseStats = new Statistics[Enum.GetValues(typeof(Job)).Length];
			tomeTier = new double[Enum.GetValues(typeof(Job)).Length];
			relicTier = new int[Enum.GetValues(typeof(Job)).Length];
            accuracyNeeds = Common.accuracyRequirements;
			ownedAccReqListA = Common.accuracyRequirements;
			ownedAccReqListB = Common.accuracyRequirements;
			ownedItems = new List<string>();
        }

        public void SetBaseStats(Statistics stats, Job j)
        {
			baseStats[(int)j] = stats;
			currentDamage[(int)j].baseStats = stats;
			currentDamage[(int)j].CalcTotalStats();
			currentCoilFoodA[(int)j].baseStats = stats;
			currentCoilFoodA[(int)j].CalcTotalStats();
			currentCoilFoodB[(int)j].baseStats = stats;
			currentCoilFoodB[(int)j].CalcTotalStats();
			ownedDamage[(int)j].baseStats = stats;
			ownedDamage[(int)j].CalcTotalStats();
			ownedCoilFoodA[(int)j].baseStats = stats;
			ownedCoilFoodA[(int)j].CalcTotalStats();
			ownedCoilFoodB[(int)j].baseStats = stats;
			ownedCoilFoodB[(int)j].CalcTotalStats();
            progressionDamage[(int)j].baseStats = stats;
            progressionDamage[(int)j].CalcTotalStats();
            progressionCoilFoodA[(int)j].baseStats = stats;
            progressionCoilFoodA[(int)j].CalcTotalStats();
            progressionCoilFoodB[(int)j].baseStats = stats;
            progressionCoilFoodB[(int)j].CalcTotalStats();
            idealDamage[(int)j].baseStats = stats;
            idealDamage[(int)j].CalcTotalStats();
            idealCoilFoodA[(int)j].baseStats = stats;
            idealCoilFoodA[(int)j].CalcTotalStats();
            idealCoilFoodB[(int)j].baseStats = stats;
            idealCoilFoodB[(int)j].CalcTotalStats();
        }

        public override string ToString()
        {
            return charName;
        }

        public static double CalcGearVal(Item i, StatWeights weights)
        {
            //if (Common.SimulateWeights)
            //{
            //    
            //}
            //else
            //{
                return i.itemStats.Value(weights);
            //}
        }

        public double CalcGearDValPerAcc(Item current, Item compare, StatWeights weights)
        {
            if (current.itemStats.acc >= compare.itemStats.acc)
            {
                return 0;
            }
            return (CalcGearVal(compare, weights) - CalcGearVal(current, weights)) / (compare.itemStats.acc - current.itemStats.acc);
        }

        public double CalcGearDValPerTome(Item current, Item compare, StatWeights weights)
        {
            if (compare.tomeCost == 0)
            {
                return 0;
            }
            return (CalcGearVal(compare, weights) - CalcGearVal(current, weights)) / compare.tomeCost;
        }
    }
}
