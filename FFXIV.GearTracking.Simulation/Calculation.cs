using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.Simulation
{
    public static class Calculation
    {
        public static StatWeights CalcStatWeights(Job j, Statistics totalStats, bool simulateWeights = false)
        {
            Simulation sim;
            StatWeights gearWeights;
            if (simulateWeights)
            {
                DateTime start = DateTime.Now;
                switch (j)
                {
                    case Job.BlackMage:
                        sim = new BLMSimulation();
                        gearWeights = sim.RunSimulation(totalStats);
                        break;
                    case Job.Bard:
                        sim = new BRDSimulation();
                        gearWeights = sim.RunSimulation(totalStats);
                        break;
                    case Job.Summoner:
                        sim = new SMNSimulation();
                        gearWeights = sim.RunSimulation(totalStats);
                        break;
                    default:
                        gearWeights = CalcStatWeightsSimple(j, totalStats);
                        break;
                }
                DateTime end = DateTime.Now;
                TimeSpan duration = end - start;
            }
            else
            {
                gearWeights = CalcStatWeightsSimple(j, totalStats);
            }
            gearWeights = MitigationWeights(j, totalStats, gearWeights);
            return gearWeights;
        }
        public static StatWeights CalcStatWeightsSimple(Job j, Statistics totalStats)
        {
            double baseDmgVal, wdmDmgVal, statDmgVal, detDmgVal, critDmgVal;
            
            baseDmgVal = Core.Common.CalculateDamage(totalStats.weaponDamage, totalStats.mainStat, totalStats.det, totalStats.crit, true);
            wdmDmgVal = Core.Common.CalculateDamage(totalStats.weaponDamage + 1, totalStats.mainStat, totalStats.det, totalStats.crit, true);
            statDmgVal = Core.Common.CalculateDamage(totalStats.weaponDamage, totalStats.mainStat + 1, totalStats.det, totalStats.crit, true);
            detDmgVal = Core.Common.CalculateDamage(totalStats.weaponDamage, totalStats.mainStat, totalStats.det + 1, totalStats.crit, true);
            critDmgVal = Core.Common.CalculateDamage(totalStats.weaponDamage, totalStats.mainStat, totalStats.det, totalStats.crit + 1, true);

            if (j == Job.Scholar || j == Job.WhiteMage)
            {
                baseDmgVal += Core.Common.CalculateHealing(j, totalStats.weaponDamage, totalStats.mainStat, totalStats.det, totalStats.crit);
                wdmDmgVal += Core.Common.CalculateHealing(j, totalStats.weaponDamage + 1, totalStats.mainStat, totalStats.det, totalStats.crit);
                statDmgVal += Core.Common.CalculateHealing(j, totalStats.weaponDamage, totalStats.mainStat + 1, totalStats.det, totalStats.crit);
                detDmgVal += Core.Common.CalculateHealing(j, totalStats.weaponDamage, totalStats.mainStat, totalStats.det + 1, totalStats.crit);
                critDmgVal += Core.Common.CalculateHealing(j, totalStats.weaponDamage, totalStats.mainStat, totalStats.det, totalStats.crit + 1);
            }

            double statDiff = statDmgVal - baseDmgVal;
            if (statDiff == 0)
            {
                statDiff = 0.0000000000001;
            }

            StatWeights gearWeights = new StatWeights((wdmDmgVal - baseDmgVal) / statDiff, 1, (detDmgVal - baseDmgVal) / statDiff, (critDmgVal - baseDmgVal) / statDiff, 0);
            gearWeights.spdWeight = (j == Job.BlackMage ? (gearWeights.dtrWeight + 3 * gearWeights.critWeight) : Math.Min(gearWeights.critWeight, gearWeights.dtrWeight)) * 0.25;

            if (j == Job.Scholar)
            {
                gearWeights.spdWeight = gearWeights.spdWeight * 2;
                gearWeights.pieWeight = (Math.Min(gearWeights.critWeight, gearWeights.dtrWeight) + 3 * gearWeights.spdWeight) * 0.25;
            }
            else if (j == Job.WhiteMage)
            {
                gearWeights.pieWeight = (3 * gearWeights.dtrWeight + gearWeights.critWeight) * 0.25;
                gearWeights.critWeight *= 0.25;
                gearWeights.spdWeight = (Math.Min(gearWeights.dtrWeight, gearWeights.pieWeight) + gearWeights.critWeight) * 0.5;
            }
            else if (j == Job.Dragoon)
            {
                gearWeights.spdWeight *= 3.0;
            }
            else if (j == Job.Bard)
            {
                gearWeights.critWeight = (3 * gearWeights.dtrWeight + gearWeights.critWeight) * 0.25;
            }
            return gearWeights;
        }
        private static StatWeights MitigationWeights(Job j, Statistics totalStats, StatWeights gearWeights)
        {
            if (j == Job.Warrior || j == Job.Paladin)
            {
                double baseHPval, vitHPVal, parryHPval, blockRateHPVal, blockStrHPVal, spdHPVal, strHPval;
                baseHPval = Core.Common.CalculateEHP(j, totalStats.vit, totalStats.mainStat, totalStats.parry, totalStats.blockRate, totalStats.blockStrength, totalStats.speed);
                strHPval = Core.Common.CalculateEHP(j, totalStats.vit, totalStats.mainStat + 1, totalStats.parry, totalStats.blockRate, totalStats.blockStrength, totalStats.speed);
                vitHPVal = Core.Common.CalculateEHP(j, totalStats.vit + 1, totalStats.mainStat, totalStats.parry, totalStats.blockRate, totalStats.blockStrength, totalStats.speed);
                parryHPval = Core.Common.CalculateEHP(j, totalStats.vit, totalStats.mainStat, totalStats.parry + 1, totalStats.blockRate, totalStats.blockStrength, totalStats.speed);
                blockRateHPVal = Core.Common.CalculateEHP(j, totalStats.vit, totalStats.mainStat, totalStats.parry, totalStats.blockRate + 10, totalStats.blockStrength, totalStats.speed);
                blockStrHPVal = Core.Common.CalculateEHP(j, totalStats.vit, totalStats.mainStat, totalStats.parry, totalStats.blockRate, totalStats.blockStrength + 10, totalStats.speed);
                spdHPVal = Core.Common.CalculateEHP(j, totalStats.vit, totalStats.mainStat, totalStats.parry, totalStats.blockRate, totalStats.blockStrength, totalStats.speed + 1);

                double vitDiff = vitHPVal - baseHPval;
                double strDiff = strHPval - baseHPval;
                gearWeights.vitWeight = Core.Common.VitPerSTR;
                gearWeights.parryWeight = (parryHPval - baseHPval) / vitDiff * gearWeights.vitWeight;
                gearWeights.blockRateWeight = (blockRateHPVal - baseHPval) / 10.0 / vitDiff * gearWeights.vitWeight;
                gearWeights.blockStrengthWeight = (blockStrHPVal - baseHPval) / 10.0 / vitDiff * gearWeights.vitWeight;
                /*
                double strDefWeight = strDiff / vitDiff;

                gearWeights.wdmgWeight /= 1 + strDefWeight;
                gearWeights.dtrWeight /= 1 + strDefWeight;
                gearWeights.critWeight /= 1 + strDefWeight;
                gearWeights.spdWeight /= 1 + strDefWeight;*/
                gearWeights.spdWeight += (spdHPVal - baseHPval) / vitDiff * gearWeights.vitWeight;

                //gearWeights.parryWeight /= 1 + strDefWeight;
                //gearWeights.blockRateWeight /= 1 + strDefWeight;
                //gearWeights.blockStrengthWeight /= 1 + strDefWeight;
            }
            /*else
            {
                gearWeights.statWeight *= 1.75;
            }*/
            return gearWeights;
        }
    }
}
