using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVGearTracker
{
    public enum GearSlot
    {
        MainHand,
        OffHand,
        Head,
        Body,
        Hands,
        Waist,
        Legs,
        Feet,
        Neck,
        Ears,
        Wrists,
        Ring
    }
	[Serializable]
    public struct Statistics
    {
        public int weaponDamage;
		public double autoAttackDelay;
		public int blockRate;
		public int blockStrength;
        public int mainStat;
        public int vit;
        public int det;
        public int crit;
        public int speed;
        public int acc;
        public int itemLevel;
		public int pie;
		public int parry;

		public Statistics(int iLvl, int dmg, int stat, int vitality, int determination, int critrate, int speedstat, int accuracy, int piety, int parrying, int blockrate, int blockstrength, double delay)
		{
			weaponDamage = dmg;
			mainStat = stat;
			vit = vitality;
			det = determination;
			crit = critrate;
			speed = speedstat;
			acc = accuracy;
			itemLevel = iLvl;
			pie = piety;
			parry = parrying;
			blockRate = blockrate;
			blockStrength = blockstrength;
			autoAttackDelay = delay;
		}

		public Statistics(int iLvl, int dmg, int stat, int vitality, int determination, int critrate, int speedstat, int accuracy, int piety, int parrying)
			: this(iLvl, dmg, stat, vitality, determination, critrate, speedstat, accuracy, piety, parrying, 0, 0, 0)
		{
		}
		public Statistics(int iLvl, int dmg, int stat, int vitality, int determination, int critrate, int speedstat, int accuracy)
			: this(iLvl, dmg, stat, vitality, determination, critrate, speedstat, accuracy, 0, 0, 0, 0, 0)
		{
		}

		public static Statistics operator +(Statistics a, Statistics b)
		{
			return new Statistics((a.itemLevel + b.itemLevel) / 2, a.weaponDamage + b.weaponDamage, a.mainStat + b.mainStat, a.vit + b.vit, a.det + b.det, a.crit + b.crit, a.speed + b.speed, a.acc + b.acc, a.pie + b.pie, a.parry + b.parry, a.blockRate+b.blockRate, a.blockStrength+b.blockStrength,a.autoAttackDelay+b.autoAttackDelay);
		}
        public static Statistics operator -(Statistics a, Statistics b)
        {
            return new Statistics((a.itemLevel - b.itemLevel) / 2, a.weaponDamage - b.weaponDamage, a.mainStat - b.mainStat, a.vit - b.vit, a.det - b.det, a.crit - b.crit, a.speed - b.speed, a.acc - b.acc, a.pie - b.pie, a.parry - b.parry, a.blockRate - b.blockRate, a.blockStrength - b.blockStrength, a.autoAttackDelay - b.autoAttackDelay);
        }
        public double Value(StatWeights weights)
        {
			return weaponDamage * weights.wdmgWeight + mainStat * weights.statWeight + det * weights.dtrWeight + crit * weights.critWeight + speed * weights.spdWeight + pie * weights.pieWeight + vit * weights.vitWeight + parry * weights.parryWeight + blockRate * weights.blockRateWeight + blockStrength * weights.blockStrengthWeight;
        }

		public override string ToString()
		{
			return "WDMG: " + weaponDamage + ", Stat: " + mainStat + ", Acc: "+ acc + ", DET: " + det + ", Crit: " + crit + ", Speed: " + speed;
		}
    }

	[Serializable]
    public class Item :IComparable
    {
        public List<Job> canEquip;
        public string name;
        public bool unique;
        public bool twoHand;
        public int sourceTurn;
		public int tomeCost;
		public double tomeTier;
		public double relicTier;
        public GearSlot equipSlot;
        public Statistics itemStats;

        public Item()
        {
            itemStats = new Statistics();
        }

        public override string ToString()
        {
            return name;
        }

		public int CompareTo(object obj)
		{
			if (!(obj is Item))
			{
				return -1;
			}
			if (itemStats.itemLevel > ((Item)obj).itemStats.itemLevel)
			{
				return -1;
			}
			else if (itemStats.itemLevel < ((Item)obj).itemStats.itemLevel)
			{
				return 1;
			}
			else
			{
				return name.CompareTo(((Item)obj).name);
			}
		}

		/*public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}*/

		public static bool operator ==(Item a, Item b)
		{
			return a.GetHashCode() == b.GetHashCode();
		}

		public static bool operator !=(Item a, Item b)
		{
			return a.GetHashCode() != b.GetHashCode();
		}
	}
}
