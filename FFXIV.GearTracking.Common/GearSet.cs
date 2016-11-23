using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace FFXIV.GearTracking.Core
{
	[Serializable]
    public struct StatWeights
    {
        public double wdmgWeight;
        public double statWeight;
        public double critWeight;
        public double dtrWeight;
        public double spdWeight;
		public double pieWeight;
		public double parryWeight;
		public double vitWeight;
		public double blockRateWeight;
		public double blockStrengthWeight;

		public StatWeights(double WD, double mainStat, double dtr, double crt, double spd)
			: this(WD, mainStat, dtr, crt, spd, 0, 0, 0, 0, 0)
		{}

		public StatWeights(double WD, double mainStat, double dtr, double crt, double spd, double pie, double parry, double vit, double blockrate, double blockstrength)
		{
			this.wdmgWeight = WD;
			this.statWeight = mainStat;
			this.dtrWeight = dtr;
			this.critWeight = crt;
			this.spdWeight = spd;
			this.pieWeight = pie;
			this.parryWeight = parry;
			this.vitWeight = vit;
			this.blockRateWeight = blockrate;
			this.blockStrengthWeight = blockstrength;
		}

		public static StatWeights operator +(StatWeights a, StatWeights b)
		{
			return new StatWeights((a.wdmgWeight + b.wdmgWeight) / 2.0, (a.statWeight + b.statWeight) / 2.0, (a.dtrWeight + b.dtrWeight) / 2.0, (a.critWeight + b.critWeight) / 2.0, 
				(a.spdWeight + b.spdWeight) / 2.0, (a.pieWeight + b.pieWeight) / 2.0, (a.parryWeight + b.parryWeight) / 2.0, (a.vitWeight + b.vitWeight) / 2.0,
				(a.blockRateWeight + b.blockRateWeight) / 2.0, (a.blockStrengthWeight + b.blockStrengthWeight) / 2.0);
		}

		public string ToString(Job j)
		{
			StringBuilder weightString = new StringBuilder();
			weightString.AppendFormat("  WDMG:\t\t{1:0.0000}\n  DET:\t\t{3:0.0000}\n  Crit:\t\t{5:0.0000}\n  Speed:\t\t{7:0.0000}", "WDMG:", wdmgWeight, "DET:", dtrWeight, "Crit:", critWeight, "Speed:", spdWeight);
			if (j == Job.Scholar || j == Job.WhiteMage)
			{
				weightString.AppendFormat("\n  PIE:\t\t{1:0.0000}", "PIE:", pieWeight);
			}
			else if (j == Job.Warrior || j == Job.Paladin)
			{
				weightString.AppendFormat("\n  VIT:\t\t{1:0.0000}\n  Parry:\t\t{3:0.0000}", "VIT:", vitWeight, "Parry:", parryWeight);
				if (j == Job.Paladin)
				{
					weightString.AppendFormat("\n  Block Rate:\t{1:0.0000}\n  Block Strength:\t{3:0.0000}", "Block Rate:", blockRateWeight, "Block Strength", blockStrengthWeight);
				}
			}
			return weightString.ToString();
		}
    }
	[Serializable]
    public class GearSet
    {
        public Item mainHand;
        public Item offHand;
        public Item head;
        public Item body;
        public Item hands;
        public Item waist;
        public Item legs;
        public Item feet;
        public Item neck;
        public Item ears;
        public Item wrists;
        public Item leftRing;
        public Item rightRing;
        public Food meal;
        public Statistics baseStats;
        public Statistics gearStats;
        public Statistics totalStats;
        public StatWeights gearWeights;
		public int totalTomeCost;

        public GearSet()
            : this(new Statistics())
        {
        }
        public GearSet(Statistics baseStatistics)
        {
            baseStats = baseStatistics;
            mainHand = new Item();
            offHand = new Item();
            head = new Item();
            body = new Item();
            hands = new Item();
            waist = new Item();
            legs = new Item();
            feet = new Item();
            neck = new Item();
            ears = new Item();
            wrists = new Item();
            leftRing = new Item();
            rightRing = new Item();
            meal = new Food();
            CalcGearStats();
            CalcTotalStats();
        }

        public double Value(StatWeights weights)
        {
            return totalStats.weaponDamage * weights.wdmgWeight + totalStats.mainStat * weights.statWeight + totalStats.det * weights.dtrWeight + totalStats.crit * weights.critWeight + totalStats.speed * weights.spdWeight + totalStats.pie * weights.pieWeight + totalStats.parry * weights.parryWeight + totalStats.vit * weights.vitWeight;
        }

        public Statistics CalcGearStats()
        {
            gearStats = mainHand.itemStats + offHand.itemStats + head.itemStats + body.itemStats + hands.itemStats + waist.itemStats + legs.itemStats + feet.itemStats + neck.itemStats + ears.itemStats + wrists.itemStats + leftRing.itemStats + rightRing.itemStats;
			totalTomeCost = mainHand.tomeCost + offHand.tomeCost + head.tomeCost + body.tomeCost + hands.tomeCost + waist.tomeCost + legs.tomeCost + feet.tomeCost + neck.tomeCost + ears.tomeCost + wrists.tomeCost + leftRing.tomeCost + rightRing.tomeCost;
			return gearStats;
        }

        public Statistics CalcTotalStats()
        {
            totalStats = baseStats + gearStats;
            try
            {
				Statistics foodStats = new Statistics(totalStats.itemLevel, 0, 0, Math.Min((int)(totalStats.vit * meal.vitPct), meal.vitCap), Math.Min((int)(totalStats.det * meal.detPct), meal.detCap), Math.Min((int)(totalStats.crit * meal.critPct), meal.critCap), Math.Min((int)(totalStats.speed * meal.speedPct), meal.speedCap), Math.Min((int)(totalStats.acc * meal.accPct), meal.accCap));
				totalStats += foodStats;
            }
            catch { }
            return totalStats;
        }

		public bool IsEqual(GearSet compareSet)
		{
			if (mainHand == compareSet.mainHand && offHand == compareSet.offHand && head == compareSet.head && body == compareSet.body && hands == compareSet.hands &&
				waist == compareSet.waist && legs == compareSet.legs && feet == compareSet.feet && neck == compareSet.neck && ears == compareSet.ears && leftRing == compareSet.leftRing &&
				rightRing == compareSet.rightRing && meal == compareSet.meal)
				return true;
			return false;
		}

		public GearSet Clone()
		{
			GearSet newSet = new GearSet();
			newSet.baseStats = baseStats;
			newSet.mainHand = mainHand;
			newSet.offHand = offHand;
			newSet.head = head;
			newSet.body = body;
			newSet.hands = hands;
			newSet.waist = waist;
			newSet.legs = legs;
			newSet.feet = feet;
			newSet.neck = neck;
			newSet.ears = ears;
			newSet.wrists = wrists;
			newSet.leftRing = leftRing;
			newSet.rightRing = rightRing;
			newSet.meal = meal;
			newSet.gearStats = gearStats;
			newSet.totalStats = totalStats;
			newSet.gearWeights = gearWeights;
			newSet.totalTomeCost = totalTomeCost;
			return newSet;
		}
    }
}
