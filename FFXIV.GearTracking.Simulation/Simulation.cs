using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.Simulation
{
	public abstract class Simulation
	{
		protected const int SimIterations = 25;
		protected const int SimMinutes = 13;
		protected double baseSkillDmg = -1;
		protected double spdReduction = -1;
		protected Random RNG;

		protected Job simJob;

		public Simulation(Job j)
		{
			simJob = j;
		}

		public virtual StatWeights RunSimulation(Statistics stats)
		{
            double baseDmg, wDmg, statDmg, critDmg, dtrDmg, spdNormDmg, spdDiffDmg, spdMinStatDmg, spdMinDmg, spdMinNormDmg;
			double baseDmgNoIgnore, wDmgNoIgnore, statDmgNoIgnore, critDmgNoIgnore, dtrDmgNoIgnore, spdNormDmgNoIgnore, spdDiffDmgNoIgnore, spdMinStatDmgNoIgnore, spdMinDmgNoIgnore, spdMinNormDmgNoIgnore;
			int spdNorm = 0, spdDiff = 0, spdMin = 0, spdMinNorm = 0, baseGCDs, spdNormGCDs, spdDiffGCDs, spdMinNormGCDs, spdMinGCDs;
            int spdNormNoIgnore = 0, spdDiffNoIgnore = 0, spdMinNoIgnore = 0, spdMinNormNoIgnore = 0, baseGCDsNoIgnore, spdNormGCDsNoIgnore, spdDiffGCDsNoIgnore, spdMinNormGCDsNoIgnore, spdMinGCDsNoIgnore;

			double avgAutoDelay = 0.0;
			int weaponCount = 0;

			foreach (Item i in Core.Common.gearDictionary.Values)
			{
				if (i.equipSlot == GearSlot.MainHand && i.canEquip.Contains(simJob))
				{
					avgAutoDelay += i.itemStats.autoAttackDelay;
					weaponCount++;
				}
			}
			avgAutoDelay /= (double)weaponCount;
			stats.autoAttackDelay = (double)Math.Round(avgAutoDelay, 2);

			baseDmg = RunSimOnce(stats, out baseGCDs, true);
			wDmg = RunSimOnce(stats + new Statistics(0, 1, 0, 0, 0, 0, 0, 0), true);
			statDmg = RunSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, 0, 0), true);
			critDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 1, 0, 0), true);
            dtrDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 1, 0, 0, 0), true);
            do
            {
                spdMinNorm++;
                spdMinNormDmg = RunSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMinNorm, 0), out spdMinNormGCDs, true);
            } while (spdMinNormGCDs >= baseGCDs || spdMinNormDmg >= baseDmg);
            do
            {
                spdMin++;
                spdMinDmg = RunSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMinNorm + spdMin, 0), out spdMinGCDs, true);
            } while (spdMinGCDs >= spdMinNormGCDs || spdMinDmg >= spdMinNormDmg);
            spdMinStatDmg = RunSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, -1 * (spdMinNorm + spdMin), 0), true);
            do
            {
                spdNorm++;
                spdNormDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdNorm, 0), out spdNormGCDs, true);
            } while (spdNormGCDs <= baseGCDs || spdNormDmg <= baseDmg);
            do
            {
                spdDiff++;
                spdDiffDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdDiff + spdNorm, 0), out spdDiffGCDs, true);
            } while (spdDiffGCDs <= spdNormGCDs || spdDiffDmg <= spdNormDmg);

			baseDmgNoIgnore = RunSimOnce(stats, out baseGCDsNoIgnore, false);
			wDmgNoIgnore = RunSimOnce(stats + new Statistics(0, 1, 0, 0, 0, 0, 0, 0), false);
			statDmgNoIgnore = RunSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, 0, 0), false);
			critDmgNoIgnore = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 1, 0, 0), false);
            dtrDmgNoIgnore = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 1, 0, 0, 0), false);
            do
            {
                spdMinNormNoIgnore++;
                spdMinNormDmgNoIgnore = RunSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMinNormNoIgnore, 0), out spdMinNormGCDsNoIgnore, false);
            } while ((spdMinNormGCDsNoIgnore >= baseGCDsNoIgnore || spdMinNormDmgNoIgnore >= baseDmgNoIgnore) && spdMinNormNoIgnore < 10);
            do
            {
                spdMinNoIgnore++;
                spdMinDmgNoIgnore = RunSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMinNormNoIgnore + spdMinNoIgnore, 0), out spdMinGCDsNoIgnore, false);
            } while ((spdMinGCDsNoIgnore >= spdMinNormGCDsNoIgnore || spdMinDmgNoIgnore >= spdMinNormDmgNoIgnore) && spdMinNoIgnore < 10);
            spdMinStatDmgNoIgnore = RunSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, -1 * (spdMinNormNoIgnore + spdMinNoIgnore), 0), false);
            do
            {
                spdNormNoIgnore++;
                spdNormDmgNoIgnore = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdNormNoIgnore, 0), out spdNormGCDsNoIgnore, false);
            } while ((spdNormGCDsNoIgnore <= baseGCDsNoIgnore || spdNormDmgNoIgnore <= baseDmgNoIgnore) && spdNormNoIgnore < 10);
            do
            {
                spdDiffNoIgnore++;
                spdDiffDmgNoIgnore = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdDiffNoIgnore + spdNormNoIgnore, 0), out spdDiffGCDsNoIgnore, false);
            } while ((spdDiffGCDsNoIgnore <= spdNormGCDsNoIgnore || spdDiffDmgNoIgnore <= spdNormDmgNoIgnore) && spdDiffNoIgnore < 10);

            double wdmgDelta = wDmg - baseDmg;
            double statDelta = statDmg - baseDmg;
            double critDelta = critDmg - baseDmg;
            double dtrDelta = dtrDmg - baseDmg;
            double spdDelta = spdDiffDmg - spdNormDmg;
            double spdMinDelta = baseDmg - spdMinDmg;
            double spdDiffMinDelta = spdDiffDmg - spdMinDmg;
            double spdMinStatDelta = spdMinStatDmg - spdMinDmg;

			double statDeltaNoIgnore = statDmgNoIgnore - baseDmgNoIgnore;
			double critDeltaNoIgnore = critDmgNoIgnore - baseDmgNoIgnore;
			double dtrDeltaNoIgnore = dtrDmgNoIgnore - baseDmgNoIgnore;
			double wdmgDeltaNoIgnore = wDmgNoIgnore - baseDmgNoIgnore;
            double spdDeltaNoIgnore = spdDiffDmgNoIgnore - spdNormDmgNoIgnore;
            double spdMinDeltaNoIgnore = baseDmgNoIgnore - spdMinDmgNoIgnore;
            double spdDiffMinDeltaNoIgnore = spdDiffDmgNoIgnore - spdMinDmgNoIgnore;
            double spdMinStatDeltaNoIgnore = spdMinStatDmgNoIgnore - spdMinDmgNoIgnore;

			double wdmgWeight = ((wdmgDelta / statDelta) + (wdmgDeltaNoIgnore / statDeltaNoIgnore)) / 2.0;
			double critweight = ((critDelta / statDelta) + (critDeltaNoIgnore / statDeltaNoIgnore)) / 2.0;
			double dtrweight = ((dtrDelta / statDelta) + (dtrDeltaNoIgnore / statDeltaNoIgnore)) / 2.0;
			
			double spdWeightIgnore = Math.Max((spdDelta / statDelta) / (spdNorm + spdDiff), 0);
			double spdWeightNoIgnore = Math.Max((spdDeltaNoIgnore / statDeltaNoIgnore) / (spdNormNoIgnore + spdDiffNoIgnore), 0);
			double spdWeight = (spdWeightIgnore + spdWeightNoIgnore + 2.0 * Math.Min(spdWeightIgnore, spdWeightNoIgnore)) / 4.0;
			
			double spdWeightMinIgnore = Math.Max((spdMinDelta / spdMinStatDelta) / (spdMinNorm + spdMin), 0);
			double spdWeightMinNoIgnore = Math.Max((spdMinDeltaNoIgnore / spdMinStatDeltaNoIgnore) / (spdMinNormNoIgnore + spdMinNoIgnore), 0);
			double spdWeightMin = (spdWeightMinIgnore + spdWeightMinNoIgnore + 2.0 * Math.Min(spdWeightMinIgnore, spdWeightMinNoIgnore)) / 4.0;
			
			double spdWeightDiffMinIgnore = Math.Max((spdDiffMinDelta / spdMinStatDelta) / (spdNorm + spdDiff + spdMinNorm + spdMin), 0);
			double spdWeightDiffMinNoIgnore = Math.Max((spdDiffMinDeltaNoIgnore / spdMinStatDeltaNoIgnore) / (spdNormNoIgnore + spdDiffNoIgnore + spdMinNormNoIgnore + spdMinNoIgnore), 0);
			double spdWeightDiffMin = (spdWeightDiffMinIgnore + spdWeightDiffMinNoIgnore + 2.0 * Math.Min(spdWeightDiffMinIgnore, spdWeightDiffMinNoIgnore)) / 4.0;
            
			double avgSpdWeight = (spdWeight + spdWeightMin + spdWeightDiffMin) / 3.0;

            return new StatWeights(wdmgWeight, 1, dtrweight, critweight, avgSpdWeight);
		}
		protected double RunSimOnce(Statistics stats, bool ignoreResources = false)
		{
			int GCDs = 0;
			return RunSimOnce(stats, out GCDs, ignoreResources);
		}
        public virtual double SetValue(GearSet set)
        {
            double dmgVal = RunSimOnce(set.totalStats, true);
            double dmgValNoIgnore = RunSimOnce(set.totalStats);
            return ((dmgVal / SimIterations) + (dmgValNoIgnore / SimIterations)) / 2.0;
        }
        public double CompareSets(GearSet startSet, GearSet compareSet)
        {
            return SetValue(compareSet) - SetValue(startSet);
        }
		
		protected abstract double RunSimOnce(Statistics stats, out int GCDCount, bool ignoreResources = false);

		protected virtual void ResetCachedValues()
		{
			baseSkillDmg = -1;
			spdReduction = -1;
		}
		protected virtual double GCDSpeed(int speed)
		{
			if (spdReduction < 0)
			{
				spdReduction = Core.Common.CalculateSpdReduction(speed);
			}
			return (double)Math.Round(2.5 - spdReduction, 3);
		}
		protected virtual double CastSpeed(double castTime, int speed, bool round = true)
		{
			if (spdReduction < 0)
			{
				spdReduction = Core.Common.CalculateSpdReduction(speed);
			}
			return (round ? (double)Math.Round(castTime - spdReduction, 3) : castTime - spdReduction);
		}
		protected double GetNextServerTick(double time)
		{
			return time + 3.0;
		}
		protected virtual int MPTicAmt(int maxMP)
		{
			return (int)(maxMP * 0.02);
		}
		protected virtual int TPTicAmt()
		{
			return 60;
		}
	}
}
