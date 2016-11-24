using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.Simulation
{
    public class BLMSimulation : Simulation
    {
        public BLMSimulation()
            : base(Job.BlackMage)
        {
        }

        public override StatWeights RunSimulation(Statistics stats)
        {
            double baseDmg, wDmg, statDmg, critDmg, dtrDmg, spdNormDmg, spdDiffDmg, spdMinDmg, spdMinNormDmg, spdMinStatDmg;
            int spdNorm = 0, spdDiff = 0, spdMin = 0, spdMinNorm = 0, baseGCDs, spdNormGCDs, spdDiffGCDs, spdMinGCDs, spdMinNormGCDs, spdMinStatGCDs;
            double baseTime = 0, spdNormTime = 0, spdDiffTime = 0, spdMinTime = 0, spdMinNormTime = 0, spdMinStatTime = 0;
            int step = 0;
            baseDmg = RunEventSimOnce(stats, out baseGCDs, out baseTime);
            wDmg = RunEventSimOnce(stats + new Statistics(0, 1, 0, 0, 0, 0, 0, 0));
            statDmg = RunEventSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, 0, 0));
            critDmg = RunEventSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 1, 0, 0));
            dtrDmg = RunEventSimOnce(stats + new Statistics(0, 0, 0, 0, 1, 0, 0, 0));
            do
            {
                spdMinNorm += ++step;
                spdMinNormDmg = RunEventSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMinNorm, 0), out spdMinNormGCDs, out spdMinNormTime);
                if (!(spdMinNormTime <= baseTime || spdMinNormDmg >= baseDmg) && step > 1)
                {
                    spdMinNorm -= step;
                    step = 0;
                }
            } while ((spdMinNormTime <= baseTime || spdMinNormDmg >= baseDmg) || step == 0);
            step = 0;
            do
            {
                spdMin += ++step;
                spdMinDmg = RunEventSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMin + spdMinNorm, 0), out spdMinGCDs, out spdMinTime);
                if (!(spdMinTime <= spdMinNormTime || spdMinDmg >= spdMinNormDmg) && step > 1)
                {
                    spdMin -= step;
                    step = 0;
                }
            } while ((spdMinTime <= spdMinNormTime || spdMinDmg >= spdMinNormDmg) || step == 0);
            spdMinStatDmg = RunEventSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, -1 * (spdMin + spdMinNorm), 0), out spdMinStatGCDs, out spdMinStatTime);
            step = 0;
            do
            {
                spdNorm += ++step;
                spdNormDmg = RunEventSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdNorm, 0), out spdNormGCDs, out spdNormTime);
                if (!(spdNormTime >= baseTime || spdNormDmg <= baseDmg) && step > 1)
                {
                    spdNorm -= step;
                    step = 0;
                }
            } while ((spdNormTime >= baseTime || spdNormDmg <= baseDmg) || step == 0);
            step = 0;
            do
            {
                spdDiff += ++step;
                spdDiffDmg = RunEventSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdNorm + spdDiff, 0), out spdDiffGCDs, out spdDiffTime);
                if (!(spdDiffTime >= spdNormTime || spdDiffDmg <= spdNormDmg) && step > 1)
                {
                    spdDiff -= step;
                    step = 0;
                }
            } while ((spdDiffTime >= spdNormTime || spdDiffDmg <= spdNormDmg) || step == 0);

            double wdmgDelta = wDmg - baseDmg;
            double statDelta = statDmg - baseDmg;
            double critDelta = critDmg - baseDmg;
            double dtrDelta = dtrDmg - baseDmg;
            double spdDelta = spdDiffDmg - baseDmg;
            double spdMinDelta = baseDmg - spdMinDmg;
            double spdDiffMinDelta = spdDiffDmg - spdMinDmg;
            double spdMinStatDelta = spdMinStatDmg - spdMinDmg;

            double wdmgWeight = wdmgDelta / statDelta;
            double critweight = critDelta / statDelta;
            double dtrweight = dtrDelta / statDelta;
            //double spdWeight = spdDelta / statDelta / spdDiff;
            double spdWeight = (spdDelta / /*((spdDiffGCDs - baseGCDs) / 2.0) /*/ statDelta) / (spdNorm + spdDiff);
            double spdMinWeight = (spdMinDelta / /*((baseGCDs - spdMinGCDs) / 2.0) /*/ spdMinStatDelta) / (spdMin + spdMinNorm);
            double spdMinDiffWeight = (spdDiffMinDelta / /*((spdDiffGCDs - spdMinGCDs) / 4.0) /*/ spdMinStatDelta) / (spdMin + spdMinNorm + spdNorm + spdDiff);
            double avgSpdWeight = (Math.Min(spdWeight, 1.0) + Math.Min(spdMinWeight, 1.0) + Math.Min(spdMinDiffWeight, 1.0)) / 3.0;
            avgSpdWeight = Math.Min(avgSpdWeight, 2.0 * Math.Max(dtrweight, critweight));

            return new StatWeights(wdmgWeight, 1, dtrweight, critweight, avgSpdWeight);
        }

        protected override double RunSimOnce(Statistics stats, out int GCDCount, out double simTime, bool ignoreResources = false)
        {
            ResetCachedValues();
            double totalDmg = 0.0;
            double totalTime = 0.0;
            GCDCount = 0;
            int failCount = 0;
            stats.mainStat = (int)(stats.mainStat * 1.03);

            for (int i = 0; i < SimIterations + failCount; i++)
            {
                RNG = new Random(i);

                BuffType mode = BuffType.None;
                bool nextSpell = false;
                int maxMP = (int)Math.Round((int)(stats.pie * 1.03) * 6.74 + 9580, 0); // SCH in party for sake of argument
                int MP = maxMP;
                double leyLinesDuration = 0.0;
                double leyLinesRecast = 0.0;
                double sharpCastDuration = 0.0;
                double sharpCastRecast = 0.0;
                double enochianDuration = 0.0;
                double enochianRecast = 0.0;
                int enochianRefreshes = 0;
                double ThunderDuration = 0.0;
                double ThunderCloudDuration = 0.0;
                double FireStarterDuration = 0.0;
                double thunderDmgMod = 1.0;
                double swiftRecast = 0.0;
                double swiftDuration = 0.0;
                double ragingRecast = 0.0;
                double ragingDuration = 0.0;
                int ragingCount = 0;
                double modeDuration = 0.0;
                double convertRecast = 0.0;
                double transposeRecast = 0.0;
                double sureCastRecast = 0.0, quellingRecast = 0.0, lethargyRecast = 0.0, virusRecast = 0.0;
                double manaWallRecast = 0.0, manawardRecast = 0.0, apocRecast = 0.0, eye4eyeRecast = 0.0;
                bool regenThunder = false;
                bool regenBlizzard = false;
                double spelldamage = 0.0;
                bool sharpcastThunder = false;

                bool failRun = false;
                double currentTime = 0.0;
                double currentDmg = 0.0;
                double GCD = 0.0;
                int tempGCDCount = 0;
                double animationDelay = 0.0;
                double animationDelayMax = 0.5;
                double castComplete = 0.0;
                double nextTick = (double)RNG.Next(3001) / 1000.0;
                SpellType castingSpell = SpellType.None;
                SpellType lastSpell = SpellType.None;
                SpellType prevSpell = SpellType.None;

                do
                {
                    ThunderDuration = Math.Round(ThunderDuration - 0.001, 3);
                    swiftRecast = Math.Round(swiftRecast - 0.001, 3);
                    swiftDuration = Math.Round(swiftDuration - 0.001, 3);
                    convertRecast = Math.Round(convertRecast - 0.001, 3);
                    ragingRecast = Math.Round(ragingRecast - 0.001, 3);
                    ragingDuration = Math.Round(ragingDuration - 0.001, 3);
                    transposeRecast = Math.Round(transposeRecast - 0.001, 3);
                    modeDuration = Math.Round(modeDuration - 0.001, 3);

                    leyLinesDuration = Math.Round(leyLinesDuration - 0.001, 3);
                    leyLinesRecast = Math.Round(leyLinesRecast - 0.001, 3);
                    sharpCastDuration = Math.Round(sharpCastDuration - 0.001, 3);
                    sharpCastRecast = Math.Round(sharpCastRecast - 0.001, 3);
                    enochianDuration = Math.Round(enochianDuration - 0.001, 3);
                    enochianRecast = Math.Round(enochianRecast - 0.001, 3);

                    if (modeDuration <= 0.0 && mode != BuffType.None)
                    {
                        mode = BuffType.None;
                    }

                    sureCastRecast = Math.Round(sureCastRecast - 0.001, 3);
                    lethargyRecast = Math.Round(lethargyRecast - 0.001, 3);
                    virusRecast = Math.Round(virusRecast - 0.001, 3);
                    manaWallRecast = Math.Round(manaWallRecast - 0.001, 3);
                    manawardRecast = Math.Round(manawardRecast - 0.001, 3);
                    quellingRecast = Math.Round(quellingRecast - 0.001, 3);
                    apocRecast = Math.Round(apocRecast - 0.001, 3);
                    eye4eyeRecast = Math.Round(eye4eyeRecast - 0.001, 3);

                    FireStarterDuration = Math.Round(FireStarterDuration - 0.001, 3);
                    ThunderCloudDuration = Math.Round(ThunderCloudDuration - 0.001, 3);

                    GCD = Math.Round(GCD - 0.001, 3);
                    animationDelay = Math.Round(animationDelay - 0.001, 3);
                    currentTime = Math.Round(currentTime + 0.001, 3);

                    if (currentTime >= nextTick)
                    {
                        if (ThunderDuration >= 0)
                        {
                            spelldamage = SpellDamage(SpellType.ThunderDoT, stats, mode, thunderDmgMod);
                            currentDmg += spelldamage;
                            if (RNG.Next(100) < 10 || sharpcastThunder)
                            {
                                ThunderCloudDuration = 12.0;// = true;
                                sharpcastThunder = false;
                            }
                        }
                        /* MP recovery */
                        MP = (int)Math.Min(MP + MPTicAmt(maxMP, mode), maxMP);
                        nextTick = GetNextServerTick(nextTick);
                    }

                    if (animationDelay <= 0)
                    {
                        if (currentTime >= castComplete && castingSpell != SpellType.None) // If we just finished casting a spell, process the damage.
                        {
                            spelldamage = SpellDamage(castingSpell, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                            currentDmg += spelldamage;
                            if (castingSpell == SpellType.Flare)
                            {
                                MP = 0;
                                mode = BuffType.AF3;
                                modeDuration = 12.0;
                            }
                            else
                            {
                                MP -= MPCost(castingSpell, mode);
                            }
                            if (FireStarterDuration > 0)
                            {
                                nextSpell = true;
                            }
                            if (castingSpell == SpellType.Fire3)
                            {
                                mode = BuffType.AF3;
                                modeDuration = 12.0;
                            }
                            else if (castingSpell == SpellType.Blizzard3)
                            {
                                mode = BuffType.UI3;
                                regenThunder = false;
                                regenBlizzard = false;
                                modeDuration = 12.0;
                            }
                            else if (castingSpell == SpellType.Blizzard4)
                            {
                                enochianDuration = 30.0 - ++enochianRefreshes * 5.0;
                            }
                            else if (castingSpell == SpellType.Blizzard)
                            {
                                if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
                                {
                                    regenBlizzard = true;
                                    modeDuration = 12.0;
                                    if (mode == BuffType.UI2)
                                    {
                                        mode = BuffType.UI3;
                                    }
                                    else if (mode == BuffType.UI)
                                    {
                                        mode = BuffType.UI2;
                                    }
                                }
                                else if (mode == BuffType.None)
                                {
                                    mode = BuffType.UI;
                                    modeDuration = 12.0;
                                }
                                else
                                {
                                    mode = BuffType.None;
                                }
                            }
                            else if (castingSpell == SpellType.Thunder || castingSpell == SpellType.Thunder2 || castingSpell == SpellType.Thunder3)
                            {
                                ThunderDuration = 18 + (castingSpell == SpellType.Thunder2 ? 3 : (castingSpell == SpellType.Thunder3 ? 6 : 0));
                                thunderDmgMod = (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0);
                                if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
                                {
                                    regenThunder = true;
                                }
                                if (sharpCastDuration > 0)
                                {
                                    sharpcastThunder = true;
                                    sharpCastDuration = 0;
                                }
                            }
                            else if (castingSpell == SpellType.Fire)
                            {
                                if (RNG.Next(10) < 4 || sharpCastDuration > 0)
                                {
                                    FireStarterDuration = 12.0;
                                    if (sharpCastDuration > 0)
                                    {
                                        sharpCastDuration = 0;
                                    }
                                }
                                if (mode == BuffType.AF || mode == BuffType.AF2 || mode == BuffType.AF3)
                                {
                                    modeDuration = 12.0;
                                    if (mode == BuffType.AF2)
                                    {
                                        mode = BuffType.AF3;
                                    }
                                    else if (mode == BuffType.AF)
                                    {
                                        mode = BuffType.AF2;
                                    }
                                }
                                else if (mode == BuffType.None)
                                {
                                    mode = BuffType.AF;
                                    modeDuration = 12.0;
                                }
                                else
                                {
                                    mode = BuffType.None;
                                }
                            }
                            tempGCDCount++;
                            prevSpell = lastSpell;
                            lastSpell = castingSpell;
                            castingSpell = SpellType.None;
                        }

                        if (currentTime >= castComplete && GCD <= 0) // If we're done casting and the GCD is over, pick an action
                        {
                            if (enochianDuration < 0 && (enochianRecast > 0 && mode == BuffType.AF3)) // If enochian fell off and isn't back yet, go to 2.0 rotation until it's back and we're in ice mode.
                            {
                                if (ThunderCloudDuration > 0 && (ThunderCloudDuration < GCDSpeed(stats.speed, leyLinesDuration > 0) || ThunderDuration < GCDSpeed(stats.speed, leyLinesDuration > 0) || (ragingDuration > 0 && ragingDuration < GCDSpeed(stats.speed, leyLinesDuration > 0))))// ||
                                { // Use thunderclouds if thundercloud is about to fall off, thunder Dot is about to fall off, or raging strikes is about to fall off
                                    spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0), ThunderCloudDuration > 0);
                                    currentDmg += spelldamage;
                                    ThunderCloudDuration = 0.0;
                                    thunderDmgMod = (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0);
                                    if (ThunderDuration > 0)
                                    {
                                        double tempDuration = ThunderDuration - (nextTick - currentTime);
                                        if (tempDuration >= 0)
                                        {
                                            int tics = 1 + (int)(tempDuration / 3);
                                        }
                                    }
                                    ThunderDuration = 24;
                                    GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Thunder3;
                                    if (mode == BuffType.UI3)
                                    {
                                        regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                    }
                                    if (FireStarterDuration > 0)
                                    {
                                        nextSpell = true;
                                    }
                                    tempGCDCount++;
                                    animationDelay = animationDelayMax;
                                }
                                else if (mode == BuffType.None) // Opener
                                {
                                    if (ThunderDuration <= 0)
                                    {
                                        castingSpell = SpellType.Thunder2;
                                    }
                                    else
                                    {
                                        castingSpell = SpellType.Fire3;
                                    }
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                    GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                }
                                else if (mode == BuffType.UI)
                                {
                                    if (!regenThunder && MP >= MPCost(SpellType.Thunder2))
                                    {
                                        castingSpell = SpellType.Thunder2;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    if (MP >= MPCost(SpellType.Fire3, mode) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, mode, leyLinesDuration > 0) > nextTick)
                                    {
                                        castingSpell = SpellType.Fire3;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                                else if (mode == BuffType.UI3)
                                {
                                    // ***

                                    /* Potential conditions for using a thunder:
                                     * 1. Just do it if not done yet
                                     * 2. Expected value of thunder (initial DMG + expected Dot DMG - overwritten DoT Damage) > Blizzard value
                                     * 3. Applicable to all, don't use if thundercloud
                                     */
                                    if (ThunderCloudDuration <= 0 && MP > MPCost(SpellType.Thunder) && // Need to have at least enough MP for Thunder
                                        //!regenThunder) // 1. Just do it if not done yet
                                        //2. Expected value of thunder (whichever we have the MP for) - lost DoT damage due to clipping > Blizzard
                                        (MP > MPCost(SpellType.Thunder2) ?
                                                SpellDamage(SpellType.Thunder2, stats) * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder2, mode, leyLinesDuration > 0) ? 1.2 : 1) +
                                                    SpellDamage(SpellType.ThunderDoT, stats) * 7 * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder2, mode, leyLinesDuration > 0) ? 1.2 : 1) :
                                                SpellDamage(SpellType.Thunder, stats) * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesDuration > 0) ? 1.2 : 1) +
                                                    SpellDamage(SpellType.ThunderDoT, stats) * 6 * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesDuration > 0) ? 1.2 : 1)) -
                                        ThunderDuration / 3 * SpellDamage(SpellType.ThunderDoT, stats) * thunderDmgMod >
                                        SpellDamage(SpellType.Blizzard, stats, mode))
                                    { // Thunder 1 or 2
                                        if (MP >= MPCost(SpellType.Thunder2) && !(MP > (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesDuration > 0) < nextTick))
                                        {
                                            castingSpell = SpellType.Thunder2;
                                            castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        }
                                        else if (MP >= MPCost(SpellType.Thunder, mode))
                                        {
                                            castingSpell = SpellType.Thunder;
                                            castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        }
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if ((maxMP - (MP - MPCost(SpellType.Scathe))) < (int)(maxMP * 0.62) && FireStarterDuration > 0 && currentTime + GCDSpeed(stats.speed, leyLinesDuration > 0) > nextTick + animationDelayMax)
                                    { // Scathe
                                        spelldamage = SpellDamage(SpellType.Scathe, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        MP -= MPCost(SpellType.Scathe, mode);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Scathe;
                                        nextSpell = true;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (FireStarterDuration < 0 && ((maxMP - MP) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, BuffType.UI3, leyLinesDuration > 0) > nextTick))
                                    { // Fire 3
                                        castingSpell = SpellType.Fire3;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (MP < (int)(maxMP * 0.62) || ((maxMP - (MP - MPCost(SpellType.Blizzard, BuffType.UI3))) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Blizzard, BuffType.UI3, leyLinesDuration > 0) < nextTick
                                        //&& ((SpellDamage(wdmg,intel,dtr,crit,SpellType.Blizzard,BuffType.UI3,false)+SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire3,BuffType.UI3,false))/(CastSpeed(stats.speed,SpellType.Blizzard,BuffType.UI3) + CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3))) > 
                                        //    (SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire3,BuffType.UI3,false)/(CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3) + Math.Max(0,nextTick - currentTime - CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3)))
                                        //+ ((SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire,BuffType.AF3,false) * (CastSpeed(stats.speed, SpellType.Blizzard, BuffType.UI3) - Math.Max(0,nextTick - currentTime - CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3))))/Math.Pow(CastSpeed(stats.speed,SpellType.Fire,BuffType.AF3),2))
                                        //)
                                        ))
                                    { // Blizzard
                                        castingSpell = SpellType.Blizzard;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                                else // AF1 or AF3
                                {
                                    if (mode == BuffType.AF3 && MP > MPCost(SpellType.Fire, mode) * 4 + MPCost(SpellType.Flare, mode) && ragingRecast <= 0 && convertRecast < GCDSpeed(stats.speed, leyLinesDuration > 0) * 4)
                                    { // Raging Strikes
                                        ragingRecast = 180.0;
                                        ragingDuration = 20.0;
                                        ragingCount++;
                                        animationDelay = animationDelayMax;
                                        if (FireStarterDuration > 0)
                                        {
                                            nextSpell = true;
                                        }
                                    }
                                    else if (MP < MPCost(SpellType.Fire, BuffType.AF3) && MP >= MPCost(SpellType.Flare) && swiftRecast <= 0 &&
                                        (ragingRecast > 0 && convertRecast < GCDSpeed(stats.speed, leyLinesDuration > 0) - animationDelayMax))
                                    { // Swiftcast (for Flare)
                                        swiftRecast = 60.0;
                                        swiftDuration = 10.0;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (MP > MPCost(SpellType.Flare) && swiftDuration > 0 && convertRecast < GCDSpeed(stats.speed, leyLinesDuration > 0) - animationDelayMax)
                                    { // Flare
                                        spelldamage = SpellDamage(SpellType.Flare, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        MP = 0;
                                        animationDelay = animationDelayMax;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        swiftDuration = 0.0;
                                        if (FireStarterDuration > 0)
                                        {
                                            nextSpell = true;
                                        }
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Flare;
                                        tempGCDCount++;
                                        modeDuration = 12.0;
                                        mode = BuffType.AF3;
                                    }
                                    else if (FireStarterDuration > 0 && nextSpell)
                                    { // Fire 3 (Instant)
                                        spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        nextSpell = false;
                                        FireStarterDuration = 0;
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Fire3;
                                        tempGCDCount++;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        animationDelay = animationDelayMax;
                                        modeDuration = 12.0;
                                        mode = BuffType.AF3;
                                    }
                                    else if (MP > MPCost(SpellType.Fire, BuffType.AF3) + MPCost(SpellType.Blizzard3, BuffType.AF3))
                                    { // Fire
                                        if (swiftDuration < 0)
                                        {
                                            castingSpell = SpellType.Fire;
                                            castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        }
                                        else
                                        {
                                            spelldamage = SpellDamage(SpellType.Fire, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                            currentDmg += spelldamage;
                                            MP -= MPCost(SpellType.Fire, mode);
                                            castingSpell = SpellType.None;
                                            prevSpell = lastSpell;
                                            lastSpell = SpellType.Fire;
                                            swiftDuration = 0;
                                            if (FireStarterDuration > 0)
                                            {
                                                nextSpell = true;
                                            }
                                            if (RNG.Next(10) < 4)
                                            {
                                                FireStarterDuration = 12.0;
                                            }
                                            castingSpell = SpellType.None;
                                            prevSpell = lastSpell;
                                            lastSpell = SpellType.Fire;
                                            tempGCDCount++;
                                            animationDelay = animationDelayMax;
                                            modeDuration = 12.0;
                                        }
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (MP > MPCost(SpellType.Blizzard3, BuffType.AF3))
                                    { // Blizzard 3
                                        castingSpell = SpellType.Blizzard3;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                            }
                            else if (enochianDuration < 0 && enochianRecast <= 0 || (mode == BuffType.UI3 && enochianRefreshes == 2)) // get enochian going again
                            {
                                if (mode == BuffType.None) // start from scratch
                                {
                                    if (currentTime > 10.0)
                                    {
                                        failCount++;
                                        failRun = true;
                                        break;
                                    }
                                    if (sharpCastRecast <= 0) //CastSpeed(stats.speed, SpellType.Thunder, mode, true) + CastSpeed(
                                    {
                                        sharpCastRecast = 60.0;
                                        sharpCastDuration = 10.0;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (sharpCastDuration > 0)
                                    {
                                        castingSpell = SpellType.Fire;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                                else if (mode == BuffType.AF)
                                {
                                    if (enochianRecast <= 0)
                                    {
                                        enochianRefreshes = 0;
                                        enochianDuration = 30.0 - enochianRefreshes * 5.0;
                                        enochianRecast = 60.0;
                                        animationDelay = animationDelayMax;
                                    }
                                }
                                else if (mode == BuffType.UI3)
                                {
                                    if (ThunderCloudDuration > 0)
                                    {
                                        spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0), ThunderCloudDuration > 0);
                                        currentDmg += spelldamage;
                                        ThunderCloudDuration = 0.0;
                                        thunderDmgMod = (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0);
                                        if (ThunderDuration > 0)
                                        {
                                            double tempDuration = ThunderDuration - (nextTick - currentTime);
                                            if (tempDuration >= 0)
                                            {
                                                int tics = 1 + (int)(tempDuration / 3);
                                            }
                                        }
                                        ThunderDuration = 24;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Thunder3;
                                        regenThunder = true; // note that we got our thunder in.
                                        if (FireStarterDuration > 0)
                                        {
                                            nextSpell = true;
                                        }
                                        tempGCDCount++;
                                        animationDelay = animationDelayMax;
                                    }
                                    if (leyLinesRecast < 0 && enochianDuration < 5 && (ragingRecast > 60 || ragingRecast <= 0))
                                    {
                                        leyLinesRecast = 90.0;
                                        leyLinesDuration = 30.0;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (regenThunder && sharpCastRecast < 0 && ragingRecast > 0) //CastSpeed(stats.speed, SpellType.Thunder, mode, true) + CastSpeed(
                                    {
                                        sharpCastRecast = 60.0;
                                        sharpCastDuration = 10.0;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (!regenThunder)
                                    {
                                        castingSpell = SpellType.Thunder;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (sharpCastRecast < 0 && enochianRecast <= 0.0)
                                    {
                                        sharpCastRecast = 60.0;
                                        sharpCastDuration = 10.0;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (enochianRecast < 0 && sharpcastThunder && !regenBlizzard)
                                    {
                                        castingSpell = SpellType.Blizzard;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (!sharpcastThunder && FireStarterDuration < 0 && ((maxMP - MP) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, BuffType.UI3, leyLinesDuration > 0) > nextTick))
                                    { // Fire 3
                                        castingSpell = SpellType.Fire3;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                                else if (mode == BuffType.AF3 && enochianRecast <= 0)
                                {
                                    enochianRefreshes = 0;
                                    enochianDuration = 30.0 - enochianRefreshes * 5.0;
                                    enochianRecast = 60.0;
                                    animationDelay = animationDelayMax;
                                }
                            }
                            else if (enochianDuration > 0) //enochian is running with enough time to refresh
                            {
                                if (mode == BuffType.AF)
                                {
                                    if (FireStarterDuration > 0)
                                    {
                                        spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        nextSpell = false;
                                        FireStarterDuration = 0;
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Fire3;
                                        tempGCDCount++;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        animationDelay = animationDelayMax;
                                        modeDuration = 12.0;
                                        mode = BuffType.AF3;
                                    }
                                    else
                                    {
                                        castingSpell = SpellType.Fire3;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                                if (mode == BuffType.UI3)
                                {
                                    if (ThunderCloudDuration > 0 && enochianDuration > GCDSpeed(stats.speed, leyLinesDuration > 0) + CastSpeed(stats.speed, SpellType.Blizzard4, mode, leyLinesDuration > GCDSpeed(stats.speed, leyLinesDuration > 0)))
                                    {
                                        spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0), ThunderCloudDuration > 0);
                                        currentDmg += spelldamage;
                                        ThunderCloudDuration = 0.0;
                                        thunderDmgMod = (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0);
                                        if (ThunderDuration > 0)
                                        {
                                            double tempDuration = ThunderDuration - (nextTick - currentTime);
                                            if (tempDuration >= 0)
                                            {
                                                int tics = 1 + (int)(tempDuration / 3);
                                            }
                                        }
                                        ThunderDuration = 24;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Thunder3;
                                        if (mode == BuffType.UI3)
                                        {
                                            regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                        }
                                        if (FireStarterDuration > 0)
                                        {
                                            nextSpell = true;
                                        }
                                        tempGCDCount++;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (!regenThunder && MP > MPCost(SpellType.Thunder, mode) && enochianDuration > CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesDuration > 0) + CastSpeed(stats.speed, SpellType.Blizzard4, mode, leyLinesDuration > CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesDuration > 0)))
                                    {
                                        castingSpell = SpellType.Thunder;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (enochianDuration < 16.5 && MP > MPCost(SpellType.Blizzard4, mode))
                                    {
                                        castingSpell = SpellType.Blizzard4;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (FireStarterDuration < 0 && ((maxMP - MP) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, BuffType.UI3, leyLinesDuration > 0) > nextTick))
                                    { // Fire 3
                                        castingSpell = SpellType.Fire3;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                                else if (mode == BuffType.AF3)
                                {
                                    if (ThunderCloudDuration > 0 && enochianDuration > GCDSpeed(stats.speed, leyLinesDuration > 0) + CastSpeed(stats.speed, SpellType.Blizzard4, mode, leyLinesDuration > GCDSpeed(stats.speed, leyLinesDuration > 0)))
                                    {
                                        spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0), ThunderCloudDuration > 0);
                                        currentDmg += spelldamage;
                                        ThunderCloudDuration = 0.0;
                                        thunderDmgMod = (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0);
                                        if (ThunderDuration > 0)
                                        {
                                            double tempDuration = ThunderDuration - (nextTick - currentTime);
                                            if (tempDuration >= 0)
                                            {
                                                int tics = 1 + (int)(tempDuration / 3);
                                            }
                                        }
                                        ThunderDuration = 24;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Thunder3;
                                        if (mode == BuffType.UI3)
                                        {
                                            regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                        }
                                        if (FireStarterDuration > 0)
                                        {
                                            nextSpell = true;
                                        }
                                        tempGCDCount++;
                                        animationDelay = animationDelayMax;
                                    }
                                    if (leyLinesRecast < 0 && ragingRecast > 90.0)
                                    {
                                        leyLinesRecast = 90.0;
                                        leyLinesDuration = 30.0;
                                        animationDelay = animationDelayMax;
                                    }
                                    if (sharpCastDuration > 0 || (enochianDuration < CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0) && enochianRefreshes == 2 && MP > MPCost(SpellType.Fire, mode) + MPCost(SpellType.Blizzard3, mode)))
                                    { // if sharpcast is up, or enochian is going to fall off before we can get in another Fire 4 and we can still do B3 after a Fire, cast it.
                                        castingSpell = SpellType.Fire;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (nextSpell && FireStarterDuration > 0 && ((MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && ragingDuration > 0 && convertRecast <= 0.0) ||
                                        (MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && (enochianRefreshes == 2 || enochianDuration > GCDSpeed(stats.speed, leyLinesDuration > 0) + GCDSpeed(stats.speed, leyLinesDuration > GCDSpeed(stats.speed, leyLinesDuration > 0)) + CastSpeed(stats.speed, SpellType.Blizzard4, BuffType.UI3, leyLinesDuration > GCDSpeed(stats.speed, leyLinesDuration > 0) + GCDSpeed(stats.speed, leyLinesDuration > GCDSpeed(stats.speed, leyLinesDuration > 0)))))))
                                    { // if we have a firestarter and we're going to lose astral fire before we can throw it if we were to do another fire 4
                                        spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        nextSpell = false;
                                        FireStarterDuration = 0;
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Fire3;
                                        tempGCDCount++;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        animationDelay = animationDelayMax;
                                        modeDuration = 12.0;
                                        mode = BuffType.AF3;
                                    }
                                    else if (MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && ragingDuration > 0 && convertRecast <= 0.0 && swiftRecast <= 0.0)
                                    {
                                        swiftDuration = 10.0;
                                        swiftRecast = 60.0;
                                        animationDelay = animationDelayMax;
                                    }
                                    else if (swiftDuration > 0)
                                    {
                                        // Flare
                                        spelldamage = SpellDamage(SpellType.Flare, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        MP = 0;
                                        animationDelay = animationDelayMax;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        swiftDuration = 0.0;
                                        if (FireStarterDuration > 0)
                                        {
                                            nextSpell = true;
                                        }
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Flare;
                                        tempGCDCount++;
                                        modeDuration = 12.0;
                                        mode = BuffType.AF3;
                                    }
                                    else if (MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) || (enochianDuration < CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0) +
                                        CastSpeed(stats.speed, SpellType.Blizzard3, mode, leyLinesDuration > CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0)) +
                                        CastSpeed(stats.speed, SpellType.Blizzard4, BuffType.UI3, leyLinesDuration > CastSpeed(stats.speed, SpellType.Blizzard3, mode, leyLinesDuration > CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0))) &&
                                        enochianRefreshes < 2))
                                    { // if not enough MP to keep going or we're going to run out of enochian before we could refresh it, drop into ice
                                        castingSpell = SpellType.Blizzard3;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (nextSpell && FireStarterDuration > CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0) && modeDuration < CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0))
                                    { // if we have a firestarter and we're going to lose astral fire before we can throw it if we were to do another fire 4
                                        spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingDuration > 0.0 ? 1.2 : 1.0) * (enochianDuration > 0.0 ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        nextSpell = false;
                                        FireStarterDuration = 0;
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Fire3;
                                        tempGCDCount++;
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                        animationDelay = animationDelayMax;
                                        modeDuration = 12.0;
                                        mode = BuffType.AF3;
                                    }
                                    else if (FireStarterDuration < 0 && modeDuration < CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0) + CastSpeed(stats.speed, SpellType.Fire, mode, leyLinesDuration > CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesDuration > 0)) &&
                                        MP > MPCost(SpellType.Fire, mode) + MPCost(SpellType.Blizzard3, mode))
                                    { // if we're going to lose astral fire before we can finish another Fire 4 and Fire
                                        castingSpell = SpellType.Fire;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                    else if (ragingRecast < 0 && enochianRefreshes == 0)
                                    {
                                        ragingRecast = 180.0;
                                        ragingDuration = 20.0;
                                        ragingCount++;
                                        animationDelay = animationDelayMax;
                                        if (FireStarterDuration > 0)
                                        {
                                            nextSpell = true;
                                        }
                                    }
                                    else
                                    {
                                        castingSpell = SpellType.Fire4;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesDuration > 0), 3);
                                        GCD = GCDSpeed(stats.speed, leyLinesDuration > 0);
                                    }
                                }
                            }
                        }
                        // Off-GCD actions while not actively casting a spell
                        if (castingSpell == SpellType.None && animationDelay <= 0.0)
                        {
                            if (enochianDuration > 0 && ragingDuration > 0 && leyLinesRecast <= 0.0)
                            {
                                leyLinesRecast = 90.0;
                                leyLinesDuration = 30.0;
                                animationDelay = animationDelayMax;
                            }
                            if (enochianDuration > 0 && ragingDuration > 0 && (lastSpell == SpellType.Fire3 || lastSpell == SpellType.Flare) && convertRecast < 0 && MP + (int)(maxMP * 0.3) < maxMP)
                            {
                                convertRecast = 180;
                                MP += (int)(maxMP * 0.3);
                                animationDelay = animationDelayMax;
                            }
                            else if (enochianDuration < 0 && enochianRecast < 0 && mode == BuffType.UI3 && lastSpell == SpellType.Thunder3)
                            {
                                enochianRefreshes = 0;
                                enochianDuration = 30.0 - enochianRefreshes * 5.0;
                                enochianRecast = 60.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (MP == maxMP && FireStarterDuration > 0 && mode.ToString().Contains("UI") && (enochianDuration > 16.5 || enochianRecast < 0))
                            { // Transpose out of UI3 before tossing a firestarter
                                transposeRecast = 12.0;
                                mode = BuffType.AF;
                                modeDuration = 12.0;
                                animationDelay = animationDelayMax;
                            }
                            else if ((enochianDuration < 0 && enochianRecast > 0) && mode == BuffType.AF3 && MP > MPCost(SpellType.Fire, mode) && lastSpell == SpellType.Fire3 && GCD > animationDelayMax && swiftRecast < 0 && convertRecast > 60 && FireStarterDuration < 0)
                            {
                                swiftDuration = 10.0;
                                swiftRecast = 60.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (mode == BuffType.AF3 && MP < MPCost(SpellType.Blizzard3, BuffType.AF3))
                            {
                                // if we're out of MP, convert if we have it, otherwise transpose
                                if (convertRecast <= 0 && ragingRecast > 0)
                                {
                                    convertRecast = 180;
                                    MP = (int)(maxMP * 0.3);
                                    animationDelay = animationDelayMax;
                                }
                                else if (transposeRecast <= 0.0 && FireStarterDuration <= 0.0)
                                {
                                    transposeRecast = 12.0;
                                    mode = BuffType.UI;
                                    modeDuration = 12.0;
                                    animationDelay = animationDelayMax;
                                    regenThunder = false;
                                    regenBlizzard = false;
                                }
                            }
                        }
                    }
                } while (ragingCount < SimMinutes / 3 + 1);
                if (!failRun)
                {
                    totalTime += currentTime;
                    totalDmg += currentDmg;
                    GCDCount += tempGCDCount;
                }
            }
            GCDCount /= SimIterations;
            simTime = totalTime / SimIterations;
            return totalDmg / totalTime;
        }
        public override double SetValue(GearSet set)
        {
            return RunSimOnce(set.totalStats);
        }

        public enum SpellType
        {
            ThunderDoT,
            Thunder,
            Thunder2,
            Thunder3,
            Fire,
            Fire3,
            Fire4,
            Flare,
            Blizzard,
            Blizzard3,
            Blizzard4,
            Scathe,
            None
        }

        public enum BuffType
        {
            AF,
            AF2,
            AF3,
            UI,
            UI2,
            UI3,
            None
        }

        private static int MPCost(SpellType spell, BuffType mode = BuffType.None)
        {
            switch (spell)
            {
                case SpellType.Blizzard:
                    if (mode == BuffType.AF)
                    {
                        return 177;
                    }
                    else if (mode == BuffType.AF3 || mode == BuffType.AF2)
                    {
                        return 89;
                    }
                    else
                    {
                        return 354;
                    }
                case SpellType.Blizzard3:
                    if (mode == BuffType.AF)
                    {
                        return 531;
                    }
                    else if (mode == BuffType.AF3 || mode == BuffType.AF2)
                    {
                        return 265;
                    }
                    else
                    {
                        return 1061;
                    }
                case SpellType.Fire:
                    if (mode == BuffType.AF || mode == BuffType.AF3)
                    {
                        return 2122;
                    }
                    else if (mode == BuffType.UI)
                    {
                        return 531;
                    }
                    else if (mode == BuffType.UI3 || mode == BuffType.UI2)
                    {
                        return 265;
                    }
                    else
                    {
                        return 1061;
                    }
                case SpellType.Fire4:
                    return 884 * 2;
                case SpellType.Blizzard4:
                    return 884;
                case SpellType.Fire3:
                    if (mode == BuffType.AF || mode == BuffType.AF3)
                    {
                        return 3536;
                    }
                    else if (mode == BuffType.UI)
                    {
                        return 884;
                    }
                    else if (mode == BuffType.UI3 || mode == BuffType.UI2)
                    {
                        return 442;
                    }
                    else
                    {
                        return 1768;
                    }
                case SpellType.Flare:
                    return 884;
                case SpellType.Scathe:
                case SpellType.Thunder:
                    return 707;
                case SpellType.Thunder2:
                    return 1061;
                case SpellType.Thunder3:
                    return 1414;
                default:
                    return 0;
            }
        }

        private int MPTicAmt(int maxMP, BuffType mode)
        {
            if (mode == BuffType.AF || mode == BuffType.AF2 || mode == BuffType.AF3)
            {
                return 0;
            }
            double recoveryPct = 0.02;
            if (mode == BuffType.UI)
            {
                recoveryPct += 0.2;
            }
            else if (mode == BuffType.UI2)
            {
                recoveryPct += 0.4;
            }
            else if (mode == BuffType.UI3)
            {
                recoveryPct += 0.6;
            }

            return (int)(maxMP * recoveryPct);
        }

        private double SpellDamage(SpellType spell, Statistics stats, double damageBonus, bool ThunderCloud)
        {
            return SpellDamage(spell, stats, BuffType.None, damageBonus, ThunderCloud);
        }
        private double SpellDamage(SpellType spell, Statistics stats, BuffType mode = BuffType.None, double damageBonus = 1.0, bool ThunderCloud = false)
        {
            double potency;
            switch (spell)
            {
                case SpellType.Scathe:
                    potency = 120; // average potency given 20% chance trait for +100 potency
                    break;
                case SpellType.ThunderDoT:
                    potency = 40;
                    break;
                case SpellType.Thunder:
                    potency = 30;
                    break;
                case SpellType.Thunder2:
                    potency = 50;
                    break;
                case SpellType.Thunder3:
                    potency = 70;
                    if (ThunderCloud)
                    {
                        potency += (40 * 8);
                    }
                    break;
                case SpellType.Fire:
                    potency = 180;
                    if (mode == BuffType.AF3)
                    {
                        potency *= 1.875;
                    }
                    else if (mode == BuffType.AF2)
                    {
                        potency *= 1.6;
                    }
                    else if (mode == BuffType.AF)
                    {
                        potency *= 1.4;
                    }
                    else if (mode.ToString().Contains("UI"))
                    {
                        potency *= 0.5;
                    }
                    break;
                case SpellType.Fire3:
                    potency = 240;
                    if (mode == BuffType.AF3)
                    {
                        potency *= 1.875;
                    }
                    else if (mode == BuffType.AF2)
                    {
                        potency *= 1.6;
                    }
                    else if (mode == BuffType.AF)
                    {
                        potency *= 1.4;
                    }
                    else if (mode.ToString().Contains("UI"))
                    {
                        potency *= 0.5;
                    }
                    break;
                case SpellType.Flare:
                    potency = 260;
                    if (mode == BuffType.AF3)
                    {
                        potency *= 1.875;
                    }
                    else if (mode == BuffType.AF2)
                    {
                        potency *= 1.6;
                    }
                    else if (mode == BuffType.AF)
                    {
                        potency *= 1.4;
                    }
                    else if (mode.ToString().Contains("UI"))
                    {
                        potency *= 0.5;
                    }
                    break;
                case SpellType.Fire4:
                    potency = 280;
                    if (mode == BuffType.AF3)
                    {
                        potency *= 1.875;
                    }
                    else if (mode == BuffType.AF2)
                    {
                        potency *= 1.6;
                    }
                    else if (mode == BuffType.AF)
                    {
                        potency *= 1.4;
                    }
                    break;
                case SpellType.Blizzard:
                    potency = 180;
                    if (mode.ToString().Contains("AF"))
                    {
                        potency *= 0.5;
                    }
                    break;
                case SpellType.Blizzard3:
                    potency = 240;
                    if (mode.ToString().Contains("AF"))
                    {
                        potency *= 0.5;
                    }
                    break;
                case SpellType.Blizzard4:
                    potency = 280;
                    break;
                default:
                    potency = 0;
                    break;
            }
            damageBonus *= 1.3;
            damageBonus *= (spell == SpellType.ThunderDoT ? Core.Common.CalculateDoTSpeedScalar(stats.speed) : 1.0);

            baseSkillDmg = Core.Common.CalculateDamage(stats, true, damageBonus);

            double spellDamage = baseSkillDmg * potency / 100;
            return spellDamage;
        }

        private double CastSpeed(int speed, SpellType spell, BuffType mode, bool leyLines)
        {
            double castTime;
            switch (spell)
            {
                case SpellType.Flare:
                    castTime = CastSpeed(4.0, speed, false);
                    break;
                case SpellType.Fire3:
                case SpellType.Blizzard3:
                case SpellType.Thunder3:
                    castTime = CastSpeed(3.5, speed, false);
                    break;
                case SpellType.Thunder2:
                case SpellType.Blizzard4:
                case SpellType.Fire4:
                    castTime = CastSpeed(3.0, speed, false);
                    break;
                case SpellType.Fire:
                case SpellType.Blizzard:
                case SpellType.Thunder:
                    castTime = CastSpeed(2.5, speed, false);
                    break;
                default:
                    castTime = 0.0;
                    break;
            }
            if (((spell.ToString().Contains("Fire") || spell.ToString().Contains("Flare")) && mode == BuffType.UI3) || (spell.ToString().Contains("Blizzard") && mode == BuffType.AF3))
            {
                castTime /= 2.0;
            }
            castTime = (double)Math.Round(castTime * (leyLines ? 0.85 : 1), 3);
            return castTime;
        }

        private double GCDSpeed(int speed, bool leyLines)
        {
            return GCDSpeed(speed) * (leyLines ? 0.85 : 1);
        }

        private enum SimEventType
        {
            ServerTick,
            GCDEnd,
            CastEnd,
            DelayEnd,
            TimeCheck,
        }
        private class SimEvents
        {
            public SimEventType EventType { get; set; }
            public string EventSource { get; set; }
            public SimEvents(SimEventType type, string source)
            {
                EventType = type;
                EventSource = source;
            }
        }
        private void AddEvent(ref SortedList<double, List<SimEvents>> dict, double key, SimEvents value)
        {
            try
            {
                List<SimEvents> values;
                if (!dict.TryGetValue(key, out values))
                {
                    values = new List<SimEvents>();
                    dict[key] = values;

                }
                values.Add(value);
            }
            catch
            {

            }
        }
        protected double RunEventSimOnce(Statistics stats)
        {
            int GCDCount;
            double simTime;
            return RunEventSimOnce(stats, out GCDCount, out simTime, false);
        }
        protected double RunEventSimOnce(Statistics stats, out int GCDCount, out double simTime, bool ignoreResources = false)
        {
            ResetCachedValues();
            double totalDmg = 0.0;
            double totalTime = 0.0;
            GCDCount = 0;
            int failCount = 0;
            stats.mainStat = (int)(stats.mainStat * 1.03);

            for (int i = 0; i < SimIterations + failCount; i++)
            {
                RNG = new Random(i);

                BuffType mode = BuffType.None;
                bool nextSpell = false;
                int maxMP = (int)Math.Round((int)(stats.pie * 1.03) * 6.74 + 9580, 0); // SCH in party for sake of argument
                int MP = maxMP;
                double leyLinesBuffEnd = 0.0;
                double leyLinesRecastEnd = 0.0;
                double sharpCastBuffEnd = 0.0;
                double sharpCastRecastEnd = 0.0;
                double enochianBuffEnd = 0.0;
                double enochianRecastEnd = 0.0;
                int enochianRefreshes = 0;
                double ThunderDoTEnd = 0.0;
                double ThunderCloudBuffEnd = 0.0;
                double FireStarterBuffEnd = 0.0;
                double thunderDmgMod = 1.0;
                double swiftRecastEnd = 0.0;
                double swiftBuffEnd = 0.0;
                double ragingRecastEnd = 0.0;
                double ragingBuffEnd = 0.0;
                int ragingCount = 0;
                double modeBuffEnd = 0.0;
                double convertRecastEnd = 0.0;
                double transposeRecastEnd = 0.0;
                double sureCastRecastEnd = 0.0, quellingRecastEnd = 0.0, lethargyRecastEnd = 0.0, virusRecastEnd = 0.0;
                double manaWallRecastEnd = 0.0, manawardRecastEnd = 0.0, apocRecastEnd = 0.0, eye4eyeRecastEnd = 0.0;
                bool regenThunder = false;
                bool regenBlizzard = false;
                double spelldamage = 0.0;
                bool sharpcastThunder = false;

                bool failRun = false;
                double currentTime = 0.0;
                double currentDmg = 0.0;
                double GCD = 0.0;
                int tempGCDCount = 0;
                double animationDelayEnd = 0.0;
                double animationDelayMax = 0.5;
                double castComplete = 0.0;
                double nextTick = (double)RNG.Next(3001) / 1000.0;
                SpellType castingSpell = SpellType.None;
                SpellType lastSpell = SpellType.None;
                SpellType prevSpell = SpellType.None;

                SortedList<double, List<SimEvents>> eventDict = new SortedList<double, List<SimEvents>>();
                AddEvent(ref eventDict, nextTick, new SimEvents(SimEventType.ServerTick, "Server Tick"));
                int eventIndex = 0;
                do
                {
                    // Select next action if we can
                    if (castingSpell == SpellType.None && GCD <= currentTime && animationDelayEnd <= currentTime) // If we're done casting and the GCD is over, pick an action
                    {
                        if (enochianBuffEnd <= currentTime && (enochianRecastEnd > currentTime && mode == BuffType.AF3)) // If enochian fell off and isn't back yet, go to 2.0 rotation until it's back and we're in ice mode.
                        {
                            if (ThunderCloudBuffEnd > currentTime && (ThunderCloudBuffEnd <= currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) || ThunderDoTEnd <= currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) || (ragingBuffEnd > currentTime && ragingBuffEnd <= currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime))))
                            { // Use thunderclouds if thundercloud is about to fall off, thunder Dot is about to fall off, or raging strikes is about to fall off
                                spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0), ThunderCloudBuffEnd > currentTime);
                                currentDmg += spelldamage;
                                ThunderCloudBuffEnd = currentTime;
                                thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                if (ThunderDoTEnd > currentTime)
                                {
                                    double tempDuration = ThunderDoTEnd - currentTime;
                                    if (tempDuration >= 0)
                                    {
                                        int tics = 1 + (int)(tempDuration / 3);
                                    }
                                }
                                ThunderDoTEnd = currentTime + 24.0;
                                GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "T3 Cloud"));
                                castingSpell = SpellType.None;
                                prevSpell = lastSpell;
                                lastSpell = SpellType.Thunder3;
                                if (mode == BuffType.UI3)
                                {
                                    regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                }
                                if (FireStarterBuffEnd > currentTime)
                                {
                                    nextSpell = true;
                                }
                                tempGCDCount++;
                                animationDelayEnd = animationDelayMax + currentTime;
                                AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "T3 Cloud"));
                            }
                            else if (mode == BuffType.None) // Opener
                            {
                                if (ThunderDoTEnd <= currentTime)
                                {
                                    castingSpell = SpellType.Thunder2;
                                }
                                else
                                {
                                    castingSpell = SpellType.Fire3;
                                }
                                castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, castingSpell.ToString()));
                                AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, castingSpell.ToString()));
                            }
                            else if (mode == BuffType.UI)
                            {
                                if (!regenThunder && MP >= MPCost(SpellType.Thunder2))
                                {
                                    castingSpell = SpellType.Thunder2;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Thunder 2"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Thunder 2"));
                                }
                                if (MP >= MPCost(SpellType.Fire3, mode) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, mode, leyLinesBuffEnd > currentTime) > nextTick)
                                {
                                    castingSpell = SpellType.Fire3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 3"));
                                }
                            }
                            else if (mode == BuffType.UI3)
                            {
                                // ***

                                /* Potential conditions for using a thunder:
                                 * 1. Just do it if not done yet
                                 * 2. Expected value of thunder (initial DMG + expected Dot DMG - overwritten DoT Damage) > Blizzard value
                                 * 3. Applicable to all, don't use if thundercloud
                                 */
                                if (ThunderCloudBuffEnd <= currentTime && MP > MPCost(SpellType.Thunder) && // Need to have at least enough MP for Thunder
                                    //!regenThunder) // 1. Just do it if not done yet
                                    //2. Expected value of thunder (whichever we have the MP for) - lost DoT damage due to clipping > Blizzard
                                    (MP > MPCost(SpellType.Thunder2) ?
                                            SpellDamage(SpellType.Thunder2, stats) * (ragingBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Thunder2, mode, leyLinesBuffEnd > currentTime) ? 1.2 : 1) +
                                                SpellDamage(SpellType.ThunderDoT, stats) * 7 * (ragingBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Thunder2, mode, leyLinesBuffEnd > currentTime) ? 1.2 : 1) :
                                            SpellDamage(SpellType.Thunder, stats) * (ragingBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesBuffEnd > currentTime) ? 1.2 : 1) +
                                                SpellDamage(SpellType.ThunderDoT, stats) * 6 * (ragingBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesBuffEnd > currentTime) ? 1.2 : 1)) -
                                    (ThunderDoTEnd - currentTime) / 3 * SpellDamage(SpellType.ThunderDoT, stats) * thunderDmgMod >
                                    SpellDamage(SpellType.Blizzard, stats, mode))
                                { // Thunder 1 or 2
                                    if (MP >= MPCost(SpellType.Thunder2) && !(MP > (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesBuffEnd > currentTime) < nextTick))
                                    {
                                        castingSpell = SpellType.Thunder2;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    }
                                    else if (MP >= MPCost(SpellType.Thunder, mode))
                                    {
                                        castingSpell = SpellType.Thunder;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    }
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, castingSpell.ToString()));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, castingSpell.ToString()));
                                }
                                else if ((maxMP - (MP - MPCost(SpellType.Scathe))) < (int)(maxMP * 0.62) && FireStarterBuffEnd > currentTime && currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) > nextTick + animationDelayMax)
                                { // Scathe
                                    spelldamage = SpellDamage(SpellType.Scathe, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    MP -= MPCost(SpellType.Scathe, mode);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Scathe;
                                    nextSpell = true;
                                    animationDelayEnd = animationDelayMax + currentTime;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Scathe"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Scathe"));
                                }
                                else if (FireStarterBuffEnd <= currentTime && ((maxMP - MP) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, BuffType.UI3, leyLinesBuffEnd > currentTime) > nextTick))
                                { // Fire 3
                                    castingSpell = SpellType.Fire3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 3"));
                                }
                                else if (MP < (int)(maxMP * 0.62) || ((maxMP - (MP - MPCost(SpellType.Blizzard, BuffType.UI3))) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Blizzard, BuffType.UI3, leyLinesBuffEnd > currentTime) < nextTick
                                    //&& ((SpellDamage(wdmg,intel,dtr,crit,SpellType.Blizzard,BuffType.UI3,false)+SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire3,BuffType.UI3,false))/(CastSpeed(stats.speed,SpellType.Blizzard,BuffType.UI3) + CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3))) > 
                                    //    (SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire3,BuffType.UI3,false)/(CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3) + Math.Max(0,nextTick - currentTime - CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3)))
                                    //+ ((SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire,BuffType.AF3,false) * (CastSpeed(stats.speed, SpellType.Blizzard, BuffType.UI3) - Math.Max(0,nextTick - currentTime - CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3))))/Math.Pow(CastSpeed(stats.speed,SpellType.Fire,BuffType.AF3),2))
                                    //)
                                    ))
                                { // Blizzard
                                    castingSpell = SpellType.Blizzard;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Blizzard"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard"));
                                }
                            }
                            else // AF1 or AF3
                            {
                                if (mode == BuffType.AF3 && MP > MPCost(SpellType.Fire, mode) * 4 + MPCost(SpellType.Flare, mode) && ragingRecastEnd <= currentTime && convertRecastEnd < GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) * 4)
                                { // Raging Strikes
                                    ragingRecastEnd = 180.0 + currentTime;
                                    ragingBuffEnd = 20.0 + currentTime;
                                    ragingCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Raging Strikes"));
                                }
                                else if (MP < MPCost(SpellType.Fire, BuffType.AF3) && MP >= MPCost(SpellType.Flare) && swiftRecastEnd <= 0 &&
                                    (ragingRecastEnd > currentTime && convertRecastEnd < GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) - animationDelayMax))
                                { // Swiftcast (for Flare)
                                    swiftRecastEnd = 60.0 + currentTime;
                                    swiftBuffEnd = 10.0 + currentTime;
                                    animationDelayEnd = animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Swiftcast"));
                                }
                                else if (MP > MPCost(SpellType.Flare) && swiftBuffEnd > currentTime && convertRecastEnd <= currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) - animationDelayMax)
                                { // Flare
                                    spelldamage = SpellDamage(SpellType.Flare, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    MP = 0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    swiftBuffEnd = 0.0;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Flare;
                                    tempGCDCount++;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "SC Flare"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "SC Flare"));
                                }
                                else if (FireStarterBuffEnd > currentTime && nextSpell)
                                { // Fire 3 (Instant)
                                    spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    nextSpell = false;
                                    FireStarterBuffEnd = 0;
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Fire3;
                                    tempGCDCount++;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "F3 Starter"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "F3 Starter"));
                                }
                                else if (MP > MPCost(SpellType.Fire, BuffType.AF3) + MPCost(SpellType.Blizzard3, BuffType.AF3))
                                { // Fire
                                    if (swiftBuffEnd < 0)
                                    {
                                        castingSpell = SpellType.Fire;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                        AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire"));
                                    }
                                    else
                                    {
                                        spelldamage = SpellDamage(SpellType.Fire, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        MP -= MPCost(SpellType.Fire, mode);
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Fire;
                                        swiftBuffEnd = 0;
                                        if (FireStarterBuffEnd > currentTime)
                                        {
                                            nextSpell = true;
                                        }
                                        if (RNG.Next(10) < 4)
                                        {
                                            FireStarterBuffEnd = currentTime + 12.0;
                                        }
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Fire;
                                        tempGCDCount++;
                                        animationDelayEnd = animationDelayMax;
                                        AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "SC Fire"));
                                        modeBuffEnd = currentTime + 12.0;
                                    }
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire"));
                                }
                                else if (MP > MPCost(SpellType.Blizzard3, BuffType.AF3))
                                { // Blizzard 3
                                    castingSpell = SpellType.Blizzard3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Blizzard 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard 3"));
                                }
                            }
                        }
                        else if (enochianBuffEnd <= currentTime && enochianRecastEnd <= currentTime || (mode == BuffType.UI3 && enochianRefreshes == 2)) // get enochian going again
                        {
                            if (mode == BuffType.None) // start from scratch
                            {
                                if (currentTime >= 10.0)
                                {
                                    failCount++;
                                    failRun = true;
                                    break;
                                }
                                if (sharpCastRecastEnd <= currentTime) //CastSpeed(stats.speed, SpellType.Thunder, mode, true) + CastSpeed(
                                {
                                    sharpCastRecastEnd = currentTime + 60.0;
                                    sharpCastBuffEnd = currentTime + 10.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Sharpcast"));
                                }
                                else if (sharpCastBuffEnd > currentTime)
                                {
                                    castingSpell = SpellType.Fire;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire"));
                                }
                            }
                            else if (mode == BuffType.AF)
                            {
                                if (enochianRecastEnd <= currentTime)
                                {
                                    enochianRefreshes = 0;
                                    enochianBuffEnd = currentTime + 30.0 - enochianRefreshes * 5.0;
                                    enochianRecastEnd = currentTime + 60.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Enochian"));
                                }
                            }
                            else if (mode == BuffType.UI3)
                            {
                                if (ThunderCloudBuffEnd > currentTime)
                                {
                                    spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0), ThunderCloudBuffEnd > currentTime);
                                    currentDmg += spelldamage;
                                    ThunderCloudBuffEnd = 0.0;
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    if (ThunderDoTEnd > currentTime)
                                    {
                                        double tempDuration = ThunderDoTEnd - currentTime;
                                        if (tempDuration >= 0)
                                        {
                                            int tics = 1 + (int)(tempDuration / 3);
                                        }
                                    }
                                    ThunderDoTEnd = currentTime + 24;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Thunder3;
                                    regenThunder = true; // note that we got our thunder in.
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    tempGCDCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "T3 Cloud"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "T3 Cloud"));
                                }
                                if (leyLinesRecastEnd <= currentTime && enochianBuffEnd <= currentTime + 5 && (ragingRecastEnd > currentTime + 60 || ragingRecastEnd <= currentTime))
                                {
                                    leyLinesRecastEnd = currentTime + 90.0;
                                    leyLinesBuffEnd = currentTime + 30.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Ley Lines"));
                                }
                                else if (regenThunder && sharpCastRecastEnd <= currentTime && ragingRecastEnd > currentTime) //CastSpeed(stats.speed, SpellType.Thunder, mode, true) + CastSpeed(
                                {
                                    sharpCastRecastEnd = currentTime + 60.0;
                                    sharpCastBuffEnd = currentTime + 10.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Sharpcast"));
                                }
                                else if (!regenThunder)
                                {
                                    castingSpell = SpellType.Thunder;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Thunder"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Thunder"));
                                }
                                else if (sharpCastRecastEnd <= currentTime && enochianRecastEnd <= currentTime)
                                {
                                    sharpCastRecastEnd = currentTime + 60.0;
                                    sharpCastBuffEnd = currentTime + 10.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Sharpcast"));
                                }
                                else if (enochianRecastEnd <= currentTime && sharpcastThunder && !regenBlizzard)
                                {
                                    castingSpell = SpellType.Blizzard;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Blizzard"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard"));
                                }
                                else if (!sharpcastThunder && FireStarterBuffEnd <= currentTime && ((maxMP - MP) < (int)(maxMP * 0.62) && (currentTime + CastSpeed(stats.speed, SpellType.Fire3, BuffType.UI3, leyLinesBuffEnd > currentTime) > nextTick) || MP == maxMP))
                                { // Fire 3
                                    castingSpell = SpellType.Fire3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 3"));
                                }
                                else if (MP == maxMP && FireStarterBuffEnd > currentTime && FireStarterBuffEnd - currentTime < animationDelayMax)
                                {// just toss it, no time to transpose
                                    spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    nextSpell = false;
                                    FireStarterBuffEnd = 0;
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Fire3;
                                    tempGCDCount++;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "F3 Starter"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "F3 Starter"));
                                }
                            }
                            else if (mode == BuffType.AF3 && enochianRecastEnd <= currentTime)
                            {
                                enochianRefreshes = 0;
                                enochianBuffEnd = currentTime + 30.0 - enochianRefreshes * 5.0;
                                enochianRecastEnd = currentTime + 60.0;
                                animationDelayEnd = currentTime + animationDelayMax;
                                AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Enochian"));
                            }
                        }
                        else if (enochianBuffEnd > currentTime) //enochian is running with enough time to refresh
                        {
                            if (mode == BuffType.AF)
                            {
                                if (FireStarterBuffEnd > currentTime)
                                {
                                    spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    nextSpell = false;
                                    FireStarterBuffEnd = 0;
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Fire3;
                                    tempGCDCount++;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "F3 Starter"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "F3 Starter"));
                                }
                                else
                                {
                                    castingSpell = SpellType.Fire3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 3"));
                                }
                            }
                            else if (mode == BuffType.UI3)
                            {
                                if (ThunderCloudBuffEnd > currentTime && enochianBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Blizzard4, mode, leyLinesBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime)))
                                {
                                    spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0), ThunderCloudBuffEnd > currentTime);
                                    currentDmg += spelldamage;
                                    ThunderCloudBuffEnd = 0.0;
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    if (ThunderDoTEnd > currentTime)
                                    {
                                        double tempDuration = ThunderDoTEnd - currentTime;
                                        if (tempDuration >= 0)
                                        {
                                            int tics = 1 + (int)(tempDuration / 3);
                                        }
                                    }
                                    ThunderDoTEnd = currentTime + 24;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Thunder3;
                                    if (mode == BuffType.UI3)
                                    {
                                        regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                    }
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    tempGCDCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "T3 Cloud"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "T3 Cloud"));
                                }
                                else if (!regenThunder && MP > MPCost(SpellType.Thunder, mode) && enochianBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Blizzard4, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Thunder, mode, leyLinesBuffEnd > currentTime)))
                                {
                                    castingSpell = SpellType.Thunder;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Thunder"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Thunder"));
                                }
                                else if (enochianBuffEnd <= currentTime + 16.5 && MP > MPCost(SpellType.Blizzard4, mode))
                                {
                                    castingSpell = SpellType.Blizzard4;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Blizzard 4"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard 4"));
                                }
                                else if (FireStarterBuffEnd <= currentTime && ((maxMP - MP) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, BuffType.UI3, leyLinesBuffEnd > currentTime) > nextTick))
                                { // Fire 3
                                    castingSpell = SpellType.Fire3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 3"));
                                }
                                else
                                {
                                    AddEvent(ref eventDict, nextTick - CastSpeed(stats.speed, SpellType.Fire3, mode, leyLinesBuffEnd > nextTick) + 0.001, new SimEvents(SimEventType.TimeCheck, "MP Wait"));
                                }
                            }
                            else if (mode == BuffType.AF3)
                            {
                                if (ThunderCloudBuffEnd > currentTime && enochianBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Blizzard4, mode, leyLinesBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime)))
                                {
                                    spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0), ThunderCloudBuffEnd > currentTime);
                                    currentDmg += spelldamage;
                                    ThunderCloudBuffEnd = 0.0;
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    if (ThunderDoTEnd > currentTime)
                                    {
                                        double tempDuration = ThunderDoTEnd - currentTime;
                                        if (tempDuration >= 0)
                                        {
                                            int tics = 1 + (int)(tempDuration / 3);
                                        }
                                    }
                                    ThunderDoTEnd = currentTime + 24;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Thunder3;
                                    if (mode == BuffType.UI3)
                                    {
                                        regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                    }
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    tempGCDCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "T3 Cloud"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "T3 Cloud"));
                                }
                                else if (leyLinesRecastEnd <= currentTime && ragingRecastEnd > currentTime + 90.0)
                                {
                                    leyLinesRecastEnd = currentTime + 90.0;
                                    leyLinesBuffEnd = currentTime + 30.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Ley Lines"));
                                }
                                else if (sharpCastBuffEnd > currentTime || (enochianBuffEnd < currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime) && enochianRefreshes == 2 && MP > MPCost(SpellType.Fire, mode) + MPCost(SpellType.Blizzard3, mode)))
                                { // if sharpcast is up, or enochian is going to fall off before we can get in another Fire 4 and we can still do B3 after a Fire, cast it.
                                    castingSpell = SpellType.Fire;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire"));
                                }
                                else if (nextSpell && FireStarterBuffEnd > currentTime && ((MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && ragingBuffEnd > currentTime && convertRecastEnd <= currentTime) ||
                                    (MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && (enochianRefreshes == 2 || enochianBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime)) + CastSpeed(stats.speed, SpellType.Blizzard4, BuffType.UI3, leyLinesBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime)))))))
                                { // if we have a firestarter and we're going to lose astral fire before we can throw it if we were to do another fire 4
                                    spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    nextSpell = false;
                                    FireStarterBuffEnd = 0;
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Fire3;
                                    tempGCDCount++;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "F3 Starter"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "F3 Starter"));
                                }
                                else if (MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && ragingBuffEnd > currentTime && convertRecastEnd <= currentTime && swiftRecastEnd <= currentTime)
                                {
                                    swiftBuffEnd = currentTime + 10.0;
                                    swiftRecastEnd = currentTime + 60.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Swiftcast"));
                                }
                                else if (swiftBuffEnd > currentTime)
                                {
                                    // Flare
                                    spelldamage = SpellDamage(SpellType.Flare, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    MP = 0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    swiftBuffEnd = 0.0;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Flare;
                                    tempGCDCount++;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "SC Flare"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "SC Flare"));
                                }
                                else if (MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) || (enochianBuffEnd <= currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime) +
                                    CastSpeed(stats.speed, SpellType.Blizzard3, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)) +
                                    CastSpeed(stats.speed, SpellType.Blizzard4, BuffType.UI3, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Blizzard3, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime))) &&
                                    enochianRefreshes < 2))
                                { // if not enough MP to keep going or we're going to run out of enochian before we could refresh it, drop into ice
                                    castingSpell = SpellType.Blizzard3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Blizzard 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard 3"));
                                }
                                else if (nextSpell && FireStarterBuffEnd <= currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime) && modeBuffEnd <= currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime))
                                { // if we have a firestarter and we're going to lose astral fire before we can throw it if we were to do another fire 4
                                    spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    nextSpell = false;
                                    FireStarterBuffEnd = 0;
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Fire3;
                                    tempGCDCount++;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "F3 Starter"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "F3 Starter"));
                                }
                                else if (FireStarterBuffEnd <= currentTime && modeBuffEnd <= currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Fire, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)) &&
                                    MP > MPCost(SpellType.Fire, mode) + MPCost(SpellType.Blizzard3, mode))
                                { // if we're going to lose astral fire before we can finish another Fire 4 and Fire
                                    castingSpell = SpellType.Fire;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire"));
                                }
                                else if (ragingRecastEnd <= currentTime && enochianRefreshes == 0)
                                {
                                    ragingRecastEnd = currentTime + 180.0;
                                    ragingBuffEnd = currentTime + 20.0;
                                    ragingCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Raging Strikes"));
                                }
                                else if (MP >= MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && modeBuffEnd >= currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Fire, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)))
                                { // Fire 4 if we won't accidentally drop AF3
                                    castingSpell = SpellType.Fire4;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire 4"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 4"));
                                }
                                else // drop UI 3, something is wrong.
                                {
                                    castingSpell = SpellType.Blizzard3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Blizzard 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard 3"));
                                }
                            }
                        }
                    }
                    // Off-GCD actions while not actively casting a spell
                    if (castingSpell == SpellType.None && animationDelayEnd <= currentTime)
                    {
                        if (enochianBuffEnd > currentTime && ragingBuffEnd > currentTime && leyLinesRecastEnd <= currentTime)
                        {
                            leyLinesRecastEnd = currentTime + 90.0;
                            leyLinesBuffEnd = currentTime + 30.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Ley Lines"));
                        }
                        if (enochianBuffEnd > currentTime && ragingBuffEnd > currentTime && (lastSpell == SpellType.Fire3 || lastSpell == SpellType.Flare) && convertRecastEnd <= currentTime && MP + (int)(maxMP * 0.3) < maxMP)
                        {
                            convertRecastEnd = currentTime + 180;
                            MP += (int)(maxMP * 0.3);
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Convert"));
                        }
                        else if (enochianBuffEnd <= currentTime && enochianRecastEnd <= currentTime && ((mode == BuffType.UI3 && lastSpell == SpellType.Thunder3) || mode == BuffType.AF3))
                        {
                            enochianRefreshes = 0;
                            enochianBuffEnd = currentTime + 30.0 - enochianRefreshes * 5.0;
                            enochianRecastEnd = currentTime + 60.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Enochian"));
                        }
                        else if (MP == maxMP && FireStarterBuffEnd > currentTime && mode.ToString().Contains("UI") && (enochianBuffEnd > currentTime + 16.5 || enochianRecastEnd <= currentTime))
                        { // Transpose out of UI3 before tossing a firestarter
                            transposeRecastEnd = currentTime + 12.0;
                            mode = BuffType.AF;
                            modeBuffEnd = currentTime + 12.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Transpose"));
                        }
                        else if ((enochianBuffEnd <= currentTime && enochianRecastEnd > currentTime) && mode == BuffType.AF3 && MP > MPCost(SpellType.Fire, mode) && lastSpell == SpellType.Fire3 && GCD > currentTime + animationDelayMax && swiftRecastEnd <= currentTime && convertRecastEnd > currentTime + 60 && FireStarterBuffEnd <= currentTime)
                        {
                            swiftBuffEnd = currentTime + 10.0;
                            swiftRecastEnd = currentTime + 60.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Swiftcast"));
                        }
                        else if (mode == BuffType.AF3 && MP < MPCost(SpellType.Blizzard3, BuffType.AF3))
                        {
                            // if we're out of MP, convert if we have it, otherwise transpose
                            if (convertRecastEnd <= currentTime && ragingRecastEnd > currentTime)
                            {
                                convertRecastEnd = currentTime + 180;
                                MP = (int)(maxMP * 0.3);
                                animationDelayEnd = currentTime + animationDelayMax;
                                AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Convert"));
                            }
                            else if (transposeRecastEnd <= currentTime && FireStarterBuffEnd <= currentTime)
                            {
                                transposeRecastEnd = currentTime + 12.0;
                                mode = BuffType.UI;
                                modeBuffEnd = currentTime + 12.0;
                                animationDelayEnd = currentTime + animationDelayMax;
                                regenThunder = false;
                                regenBlizzard = false;
                                AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Transpose"));
                            }
                        }
                    }

                    if (eventDict.Keys.Count > 0)
                    {
                        // Process next event(s)
                        currentTime = eventDict.Keys[eventIndex];
                        List<SimEvents> nextEvents = eventDict.Values[eventIndex++];

                        if (modeBuffEnd <= currentTime && mode != BuffType.None)
                        {
                            mode = BuffType.None;
                            if (castingSpell == SpellType.Blizzard4 || castingSpell == SpellType.Fire4)
                            {
                                castComplete = 0;
                                castingSpell = SpellType.None;
                            }
                        }

                        foreach (SimEvents nextEvent in nextEvents)
                        {
                            if (nextEvent.EventType == SimEventType.ServerTick)
                            {
                                if (ThunderDoTEnd > currentTime)
                                {
                                    spelldamage = SpellDamage(SpellType.ThunderDoT, stats, mode, thunderDmgMod);
                                    currentDmg += spelldamage;
                                    if (RNG.Next(100) < 10 || sharpcastThunder)
                                    {
                                        ThunderCloudBuffEnd = currentTime + 12.0;// = true;
                                        sharpcastThunder = false;
                                    }
                                }
                                /* MP recovery */
                                MP = (int)Math.Min(MP + MPTicAmt(maxMP, mode), maxMP);
                                nextTick = GetNextServerTick(nextTick);
                                AddEvent(ref eventDict, nextTick, new SimEvents(SimEventType.ServerTick, "Server Tick"));
                            }

                            if (nextEvent.EventType == SimEventType.CastEnd) // If we just finished casting a spell, process the damage.
                            {
                                spelldamage = SpellDamage(castingSpell, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                currentDmg += spelldamage;
                                if (castingSpell == SpellType.Flare)
                                {
                                    MP = 0;
                                    mode = BuffType.AF3;
                                    modeBuffEnd = currentTime + 12.0;
                                }
                                else
                                {
                                    MP -= MPCost(castingSpell, mode);
                                }
                                if (FireStarterBuffEnd > currentTime)
                                {
                                    nextSpell = true;
                                }
                                if (castingSpell == SpellType.Fire3)
                                {
                                    mode = BuffType.AF3;
                                    modeBuffEnd = currentTime + 12.0;
                                }
                                else if (castingSpell == SpellType.Blizzard3)
                                {
                                    mode = BuffType.UI3;
                                    regenThunder = false;
                                    regenBlizzard = false;
                                    modeBuffEnd = currentTime + 12.0;
                                }
                                else if (castingSpell == SpellType.Blizzard4)
                                {
                                    enochianBuffEnd = currentTime + 30.0 - ++enochianRefreshes * 5.0;
                                }
                                else if (castingSpell == SpellType.Blizzard)
                                {
                                    if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
                                    {
                                        regenBlizzard = true;
                                        modeBuffEnd = currentTime + 12.0;
                                        if (mode == BuffType.UI2)
                                        {
                                            mode = BuffType.UI3;
                                        }
                                        else if (mode == BuffType.UI)
                                        {
                                            mode = BuffType.UI2;
                                        }
                                    }
                                    else if (mode == BuffType.None)
                                    {
                                        mode = BuffType.UI;
                                        modeBuffEnd = currentTime + 12.0;
                                    }
                                    else
                                    {
                                        mode = BuffType.None;
                                    }
                                }
                                else if (castingSpell == SpellType.Thunder || castingSpell == SpellType.Thunder2 || castingSpell == SpellType.Thunder3)
                                {
                                    ThunderDoTEnd = currentTime + 18 + (castingSpell == SpellType.Thunder2 ? 3 : (castingSpell == SpellType.Thunder3 ? 6 : 0));
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
                                    {
                                        regenThunder = true;
                                    }
                                    if (sharpCastBuffEnd > currentTime)
                                    {
                                        sharpcastThunder = true;
                                        sharpCastBuffEnd = 0;
                                    }
                                }
                                else if (castingSpell == SpellType.Fire)
                                {
                                    if (RNG.Next(10) < 4 || sharpCastBuffEnd > currentTime)
                                    {
                                        FireStarterBuffEnd = currentTime + 12.0;
                                        if (sharpCastBuffEnd > currentTime)
                                        {
                                            sharpCastBuffEnd = 0;
                                        }
                                    }
                                    if (mode == BuffType.AF || mode == BuffType.AF2 || mode == BuffType.AF3)
                                    {
                                        modeBuffEnd = currentTime + 12.0;
                                        if (mode == BuffType.AF2)
                                        {
                                            mode = BuffType.AF3;
                                        }
                                        else if (mode == BuffType.AF)
                                        {
                                            mode = BuffType.AF2;
                                        }
                                    }
                                    else if (mode == BuffType.None)
                                    {
                                        mode = BuffType.AF;
                                        modeBuffEnd = currentTime + 12.0;
                                    }
                                    else
                                    {
                                        mode = BuffType.None;
                                    }
                                }
                                tempGCDCount++;
                                prevSpell = lastSpell;
                                lastSpell = castingSpell;
                                castingSpell = SpellType.None;
                            }
                        }
                    }
                } while (ragingCount < SimMinutes / 3 + 1);
                if (!failRun)
                {
                    totalTime += currentTime;
                    totalDmg += currentDmg;
                    GCDCount += tempGCDCount;
                }
            }
            GCDCount /= SimIterations;
            simTime = totalTime / SimIterations;
            return totalDmg / totalTime;
        }
        protected double RunNewEventSimOnce(Statistics stats)
        {
            int GCDCount;
            double simTime;
            return RunNewEventSimOnce(stats, out GCDCount, out simTime, false);
        }
        protected double RunNewEventSimOnce(Statistics stats, out int GCDCount, out double simTime, bool ignoreResources = false)
        {
            ResetCachedValues();
            double totalDmg = 0.0;
            double totalTime = 0.0;
            GCDCount = 0;
            int failCount = 0;
            stats.mainStat = (int)(stats.mainStat * 1.03);

            for (int i = 0; i < SimIterations + failCount; i++)
            {
                RNG = new Random(i);

                BuffType mode = BuffType.None;
                bool nextSpell = false;
                int maxMP = (int)Math.Round((int)(stats.pie * 1.03) * 6.74 + 9580, 0); // SCH in party for sake of argument
                int MP = maxMP;
                double leyLinesBuffEnd = 0.0;
                double leyLinesRecastEnd = 0.0;
                double sharpCastBuffEnd = 0.0;
                double sharpCastRecastEnd = 0.0;
                double enochianBuffEnd = 0.0;
                double enochianRecastEnd = 0.0;
                int enochianRefreshes = 0;
                double ThunderDoTEnd = 0.0;
                double ThunderCloudBuffEnd = 0.0;
                double FireStarterBuffEnd = 0.0;
                double thunderDmgMod = 1.0;
                double swiftRecastEnd = 0.0;
                double swiftBuffEnd = 0.0;
                double ragingRecastEnd = 0.0;
                double ragingBuffEnd = 0.0;
                int ragingCount = 0;
                int fire4count = 0;
                double modeBuffEnd = 0.0;
                double convertRecastEnd = 0.0;
                double transposeRecastEnd = 0.0;
                bool regenThunder = false;
                bool regenBlizzard = false;
                double spelldamage = 0.0;
                bool sharpcastThunder = false;

                bool failRun = false;
                double currentTime = 0.0;
                double currentDmg = 0.0;
                double GCD = 0.0;
                int tempGCDCount = 0;
                double animationDelayEnd = 0.0;
                double animationDelayMax = 0.5;
                double castComplete = 0.0;
                double nextTick = (double)RNG.Next(3001) / 1000.0;
                SpellType castingSpell = SpellType.None;
                SpellType lastSpell = SpellType.None;
                SpellType prevSpell = SpellType.None;

                SortedList<double, List<SimEvents>> eventDict = new SortedList<double, List<SimEvents>>();
                AddEvent(ref eventDict, nextTick, new SimEvents(SimEventType.ServerTick, "Server Tick"));
                int eventIndex = 0;
                do
                {
                    // Select next action if we can
                    if (castingSpell == SpellType.None && GCD <= currentTime && animationDelayEnd <= currentTime) // If we're done casting and the GCD is over, pick an action
                    {
                        if (enochianBuffEnd <= currentTime && enochianRecastEnd <= currentTime || (mode == BuffType.UI3 && enochianRefreshes == 2)) // get enochian going again
                        {
                            if (mode == BuffType.None) // start from scratch
                            {
                                if (currentTime >= 10.0)
                                {
                                    // Should never wind up here, so treat it as a failure and discard this run.
                                    failCount++;
                                    failRun = true;
                                    break;
                                }
                                if (sharpCastRecastEnd <= currentTime)
                                {
                                    sharpCastRecastEnd = currentTime + 60.0;
                                    sharpCastBuffEnd = currentTime + 10.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Sharpcast"));
                                }
                                else
                                {
                                    castingSpell = SpellType.Fire;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire"));
                                }
                            }
                            else if (mode == BuffType.AF)
                            {
                                if (enochianRecastEnd <= currentTime)
                                {
                                    enochianRefreshes = 0;
                                    enochianBuffEnd = currentTime + 30.0;
                                    enochianRecastEnd = currentTime + 60.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Enochian"));
                                }
                            }
                            else if (mode == BuffType.UI3)
                            {
                                /* Enochian Reset Umbral Phase logic/priorities:
                                 * 
                                 * Thunder 1 if enough MP (Replace with ThunderCloud if it's up)
                                 * Blizzard 1 if not enough MP for Thunder
                                 * Make sure MP will be full before the next two actions:
                                 * SharpCast <- highest priority, the sooner we get back to F4s, the better
                                 * Fire 3 (MP should be full when it lands) < highest priority after Sharpcast
                                 * Use Enochian during the fast UI3 Fire 3 -> GCD finish hang time
                                 */
                                if (ThunderCloudBuffEnd > currentTime && !regenThunder)
                                { // Use ThunderCloud instead of T1 if available, but don't re-use during this UI3 phase.
                                    spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0), ThunderCloudBuffEnd > currentTime);
                                    currentDmg += spelldamage;
                                    ThunderCloudBuffEnd = 0.0;
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    ThunderDoTEnd = currentTime + 24;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Thunder3;
                                    regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    tempGCDCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "T3 Cloud"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "T3 Cloud"));
                                }
                                else if (sharpCastRecastEnd <= currentTime &&MP + MPTicAmt(maxMP, mode) >= maxMP && nextTick < currentTime + animationDelayMax + CastSpeed(stats.speed, SpellType.Fire3, mode, leyLinesBuffEnd > currentTime + animationDelayMax))
                                { // Sharpcast if MP will be full by the time Sharp -> F3 completes.
                                    sharpCastRecastEnd = currentTime + 60.0;
                                    sharpCastBuffEnd = currentTime + 10.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Sharpcast"));
                                }
                                else if (sharpCastBuffEnd > currentTime || (MP > MPCost(SpellType.Fire3, mode) && (MP == maxMP || (maxMP - MP <= MPTicAmt(maxMP, mode) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, mode, leyLinesBuffEnd > currentTime) > nextTick))))
                                { // Fire 3 once Sharpcast is on.
                                    castingSpell = SpellType.Fire3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 3"));
                                }
                                else if (!regenThunder && MP > MPCost(SpellType.Thunder, mode))
                                {
                                    castingSpell = SpellType.Thunder;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Thunder"));
                                }
                                else if (!regenBlizzard && MP > MPCost(SpellType.Blizzard, mode))
                                { // Use Blizzard if we don't have enough MP for Thunder 1 
                                    castingSpell = SpellType.Blizzard;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard"));
                                }
                                else
                                { // Shouldn't end up here, but leaving this for breakpointing for debugging purposes.
                                }
                            }
                            else if (mode == BuffType.AF3 && enochianRecastEnd <= currentTime)
                            {
                                // Use Enochian during the fast UI3 Fire 3 -> GCD finish hang time
                                enochianRefreshes = 0;
                                enochianBuffEnd = currentTime + 30.0;
                                enochianRecastEnd = currentTime + 60.0;
                                animationDelayEnd = currentTime + animationDelayMax;
                                AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Enochian"));
                            }
                        }
                        else if (enochianBuffEnd > currentTime) //enochian is running with enough time to refresh
                        {
                            if (mode == BuffType.AF)
                            {
                                if (FireStarterBuffEnd > currentTime)
                                {
                                    spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    nextSpell = false;
                                    FireStarterBuffEnd = 0;
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Fire3;
                                    tempGCDCount++;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "F3 Starter"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "F3 Starter"));
                                }
                                else
                                {
                                    // Should never wind up here, so treat it as a failure and discard this run.
                                    failCount++;
                                    failRun = true;
                                    break;
                                }
                            }
                            else if (mode == BuffType.UI3)
                            {
                                /* Enochian Refresh Umbral Ice Phase logic/priorities:
                                 * 
                                 * For all following actions, make sure using it won't cause enochian to fall off (cast times of spell+B4 < remaining enochian duration)
                                 * ThunderCloud if it's up
                                 * B4 if enough MP
                                 * F3 if MP will be full by the time it finishes
                                 * Thunder 1 if enough MP but not enough for B4
                                 * Blizzard 1 if not enough MP for Thunder 1
                                 * Wait if we've done Thunder/B1 and B4 and will still finish F3 before MP is full
                                 */
                                if (ThunderCloudBuffEnd > currentTime && !regenThunder && GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Blizzard4, mode, leyLinesBuffEnd > currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime)) < enochianBuffEnd - currentTime)
                                {
                                    spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0), ThunderCloudBuffEnd > currentTime);
                                    currentDmg += spelldamage;
                                    ThunderCloudBuffEnd = 0.0;
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    ThunderDoTEnd = currentTime + 24;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Thunder3;
                                    regenThunder = true; // if we were recovering MP, note that we got our thunder in.
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    tempGCDCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "T3 Cloud"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "T3 Cloud"));
                                }
                                else if (MP > MPCost(SpellType.Blizzard4, mode) && enochianBuffEnd - currentTime < 16)
                                { // Blizzard 4 if we haven't done so yet and we have enough MP
                                    castingSpell = SpellType.Blizzard4;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard 4"));
                                }
                                else if (MP > MPCost(SpellType.Fire3, mode) && (MP == maxMP || (maxMP - MP <= MPTicAmt(maxMP, mode) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, mode, leyLinesBuffEnd > currentTime) > nextTick)))
                                { // if enough MP for Fire 3, and we're either already full or will fill before the Fire 3 can finish.
                                    castingSpell = SpellType.Fire3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Fire 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 3"));
                                }
                                else if (MP > MPCost(SpellType.Thunder) && !regenThunder)
                                {
                                    castingSpell = SpellType.Thunder;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Thunder"));
                                }
                                else if (MP > MPCost(SpellType.Blizzard, mode) && !regenBlizzard)
                                {
                                    castingSpell = SpellType.Blizzard;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard"));
                                }
                                else
                                { // How to prevent this from getting added a bunch? Does it matter?
                                    AddEvent(ref eventDict, nextTick - CastSpeed(stats.speed, SpellType.Fire3, mode, leyLinesBuffEnd > nextTick) + 0.001, new SimEvents(SimEventType.TimeCheck, "MP Wait"));
                                }
                            }
                            else if (mode == BuffType.AF3)
                            {
                                /* AF3 Opener:
                                 * FireStarter, Ley/RS, F4x3, F1, SC, F4, Convert, F4x2, B3
                                 * 
                                 * AF3 basics:
                                 * 1. Don't drop enochian, be ready to swap to Ice if needed
                                 * 2. Don't drop AF3, be ready to drop a F1 or FireStarter if needed
                                 * 3. Do whatever the highest PPS action is after satisfying the above
                                 * 
                                 * AF3 Priorities/logic
                                 * B3 if MP is low, or if otherwise we'd lose Enochian before we could get to B4 (unless we're going to refresh it anyways, ie refreshes == 2)
                                 * F1 if we can't get another spell + F1 in before AF3 ends (change to Firestarter if it's up)
                                 * [Cooldowns]
                                 *   Ley Lines if Raging Strikes is either up, or will not be up imminently
                                 *   Raging Strikes if we're at the start of a fresh enochian fire cycle
                                 *   Swiftcast if the last spell was Fire 1 (or FireStarter) and RS is up.
                                 * ThunderCloud if it's up
                                 * Fire 4 
                                 */
                                if (MP > MPCost(SpellType.Blizzard3, mode) && ((MP < MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode) && convertRecastEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)) ||
                                    enochianBuffEnd < currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Blizzard3, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)) +
                                    CastSpeed(stats.speed, SpellType.Blizzard4, BuffType.UI3, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Blizzard3, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)))))
                                { // blizzard 3 if we have enough mp, and if either we're out of mp for more fire 4 without convert up, or if enochian's duration is less than F4 + B3 + B4's cumulative cast time.
                                    castingSpell = SpellType.Blizzard3;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "Blizzard 3"));
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Blizzard 3"));
                                }
                                else if (FireStarterBuffEnd > currentTime && nextSpell && (fire4count >= 2 || FireStarterBuffEnd < currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)))
                                {
                                    spelldamage = SpellDamage(SpellType.Fire3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                    currentDmg += spelldamage;
                                    nextSpell = false;
                                    FireStarterBuffEnd = 0;
                                    fire4count = 0;
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Fire3;
                                    tempGCDCount++;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    modeBuffEnd = currentTime + 12.0;
                                    mode = BuffType.AF3;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "F3 Starter"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "F3 Starter"));
                                }
                                else if (MP > MPCost(SpellType.Fire) && modeBuffEnd <= currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime) + CastSpeed(stats.speed, SpellType.Fire, mode, leyLinesBuffEnd > currentTime + CastSpeed(stats.speed, SpellType.Fire4, mode, leyLinesBuffEnd > currentTime)))
                                { // Fire 1 if enough MP and if AF3's duration is less than another F4 + F1
                                    castingSpell = SpellType.Fire;
                                    castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire"));
                                }
                                else if (leyLinesRecastEnd < currentTime && (ragingRecastEnd < currentTime + animationDelayMax || ragingRecastEnd > currentTime + 75) && lastSpell == SpellType.Fire3)
                                {
                                    leyLinesRecastEnd = currentTime + 90.0;
                                    leyLinesBuffEnd = currentTime + 30.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Ley Lines"));
                                }
                                else if (ragingRecastEnd < currentTime && lastSpell == SpellType.Fire3 && MP > maxMP * 0.75)
                                {
                                    ragingRecastEnd = currentTime + 180.0;
                                    ragingBuffEnd = currentTime + 20.0;
                                    ragingCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Raging Strikes"));
                                }
                                else if (swiftRecastEnd < currentTime && (lastSpell == SpellType.Fire || (lastSpell == SpellType.Fire3 && MP < maxMP * 0.75)))
                                {
                                    swiftBuffEnd = currentTime + 10.0;
                                    swiftRecastEnd = currentTime + 60.0;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Swiftcast"));
                                }
                                else if (ThunderCloudBuffEnd > currentTime && 8 - LostThunderTicks(ThunderDoTEnd, nextTick) > 0)
                                { // thundercloud is a DPS increase over F4 as long as it adds at least one tick to the overall DoT time
                                    spelldamage = SpellDamage(SpellType.Thunder3, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0), ThunderCloudBuffEnd > currentTime);
                                    currentDmg += spelldamage;
                                    ThunderCloudBuffEnd = 0.0;
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    ThunderDoTEnd = currentTime + 24;
                                    GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                    castingSpell = SpellType.None;
                                    prevSpell = lastSpell;
                                    lastSpell = SpellType.Thunder3;
                                    if (FireStarterBuffEnd > currentTime)
                                    {
                                        nextSpell = true;
                                    }
                                    tempGCDCount++;
                                    animationDelayEnd = currentTime + animationDelayMax;
                                    AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "T3 Cloud"));
                                    AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "T3 Cloud"));
                                }
                                else if (MP >= MPCost(SpellType.Fire4, mode) + MPCost(SpellType.Blizzard3, mode))
                                {
                                    if (swiftBuffEnd > currentTime)
                                    {
                                        spelldamage = SpellDamage(SpellType.Fire4, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                        currentDmg += spelldamage;
                                        MP -= MPCost(SpellType.Fire4, mode);
                                        animationDelayEnd = currentTime + animationDelayMax;
                                        GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                        swiftBuffEnd = 0.0;
                                        if (FireStarterBuffEnd > currentTime)
                                        {
                                            nextSpell = true;
                                        }
                                        fire4count++;
                                        castingSpell = SpellType.None;
                                        prevSpell = lastSpell;
                                        lastSpell = SpellType.Fire4;
                                        tempGCDCount++;
                                        AddEvent(ref eventDict, GCD, new SimEvents(SimEventType.GCDEnd, "SC Fire 4"));
                                        AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "SC Fire 4"));
                                    }
                                    else
                                    {
                                        castingSpell = SpellType.Fire4;
                                        castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode, leyLinesBuffEnd > currentTime), 3);
                                        GCD = Math.Round(currentTime + GCDSpeed(stats.speed, leyLinesBuffEnd > currentTime), 3);
                                        AddEvent(ref eventDict, castComplete, new SimEvents(SimEventType.CastEnd, "Fire 4"));
                                    }
                                }
                                else
                                { // how did we get here? debugging breakpoint...

                                }
                            }
                        }
                        else
                        { // another weird situation/breakpoint spot.

                        }
                    }
                    // Off-GCD actions while not actively casting a spell
                    if (castingSpell == SpellType.None && animationDelayEnd <= currentTime)
                    {
                        if (enochianBuffEnd > currentTime && leyLinesRecastEnd <= currentTime)
                        {
                            leyLinesRecastEnd = currentTime + 90.0;
                            leyLinesBuffEnd = currentTime + 30.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            if (FireStarterBuffEnd > currentTime)
                            {
                                nextSpell = true;
                            }
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Ley Lines"));
                        }
                        else if (enochianBuffEnd > currentTime && leyLinesBuffEnd > currentTime && ragingRecastEnd <= currentTime)
                        {
                            ragingRecastEnd = currentTime + 180.0;
                            ragingBuffEnd = currentTime + 20.0;
                            ragingCount++;
                            animationDelayEnd = currentTime + animationDelayMax;
                            if (FireStarterBuffEnd > currentTime)
                            {
                                nextSpell = true;
                            }
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Raging Strikes"));
                        }
                        else if (enochianBuffEnd > currentTime && ragingBuffEnd > currentTime && lastSpell == SpellType.Fire4 && convertRecastEnd <= currentTime && MP + (int)(maxMP * 0.3) < maxMP)
                        {
                            convertRecastEnd = currentTime + 180;
                            MP += (int)(maxMP * 0.3);
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Convert"));
                        }
                        else if (enochianBuffEnd <= currentTime && enochianRecastEnd <= currentTime && ((mode == BuffType.UI3 && lastSpell == SpellType.Thunder3) || mode == BuffType.AF3))
                        {
                            enochianRefreshes = 0;
                            enochianBuffEnd = currentTime + 30.0 - enochianRefreshes * 5.0;
                            enochianRecastEnd = currentTime + 60.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Enochian"));
                        }
                        else if (MP == maxMP && FireStarterBuffEnd > currentTime && mode.ToString().Contains("UI") && (enochianBuffEnd > currentTime + 16.5 || enochianRecastEnd <= currentTime))
                        { // Transpose out of UI3 before tossing a firestarter
                            transposeRecastEnd = currentTime + 12.0;
                            mode = BuffType.AF;
                            modeBuffEnd = currentTime + 12.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Transpose"));
                        }
                        else if ((enochianBuffEnd <= currentTime && enochianRecastEnd > currentTime) && mode == BuffType.AF3 && MP > MPCost(SpellType.Fire, mode) && lastSpell == SpellType.Fire3 && GCD > currentTime + animationDelayMax && swiftRecastEnd <= currentTime && convertRecastEnd > currentTime + 60 && FireStarterBuffEnd <= currentTime)
                        {
                            swiftBuffEnd = currentTime + 10.0;
                            swiftRecastEnd = currentTime + 60.0;
                            animationDelayEnd = currentTime + animationDelayMax;
                            AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Swiftcast"));
                        }
                        else if (mode == BuffType.AF3 && MP < MPCost(SpellType.Blizzard3, BuffType.AF3))
                        {
                            // if we're out of MP, convert if we have it, otherwise transpose
                            if (convertRecastEnd <= currentTime && ragingRecastEnd > currentTime)
                            {
                                convertRecastEnd = currentTime + 180;
                                MP = (int)(maxMP * 0.3);
                                animationDelayEnd = currentTime + animationDelayMax;
                                AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Convert"));
                            }
                            else if (transposeRecastEnd <= currentTime && FireStarterBuffEnd <= currentTime)
                            {
                                transposeRecastEnd = currentTime + 12.0;
                                mode = BuffType.UI;
                                modeBuffEnd = currentTime + 12.0;
                                animationDelayEnd = currentTime + animationDelayMax;
                                regenThunder = false;
                                regenBlizzard = false;
                                AddEvent(ref eventDict, animationDelayEnd, new SimEvents(SimEventType.DelayEnd, "Transpose"));
                            }
                        }
                    }

                    if (eventDict.Keys.Count > 0)
                    {
                        // Process next event(s)
                        currentTime = eventDict.Keys[eventIndex];
                        List<SimEvents> nextEvents = eventDict.Values[eventIndex++];

                        if (modeBuffEnd <= currentTime && mode != BuffType.None)
                        {
                            mode = BuffType.None;
                            if (castingSpell == SpellType.Blizzard4 || castingSpell == SpellType.Fire4)
                            {
                                castComplete = 0;
                                castingSpell = SpellType.None;
                            }
                        }

                        foreach (SimEvents nextEvent in nextEvents)
                        {
                            if (nextEvent.EventType == SimEventType.ServerTick)
                            {
                                if (ThunderDoTEnd > currentTime)
                                {
                                    spelldamage = SpellDamage(SpellType.ThunderDoT, stats, mode, thunderDmgMod);
                                    currentDmg += spelldamage;
                                    if (RNG.Next(100) < 10 || sharpcastThunder)
                                    {
                                        ThunderCloudBuffEnd = currentTime + 12.0;// = true;
                                        sharpcastThunder = false;
                                    }
                                }
                                /* MP recovery */
                                MP = (int)Math.Min(MP + MPTicAmt(maxMP, mode), maxMP);
                                nextTick = GetNextServerTick(nextTick);
                                AddEvent(ref eventDict, nextTick, new SimEvents(SimEventType.ServerTick, "Server Tick"));
                            }

                            if (nextEvent.EventType == SimEventType.CastEnd) // If we just finished casting a spell, process the damage.
                            {
                                spelldamage = SpellDamage(castingSpell, stats, mode, (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0));
                                currentDmg += spelldamage;

                                if (castingSpell == SpellType.Fire4)
                                {
                                    fire4count++;
                                }
                                else
                                {
                                    fire4count = 0;
                                }

                                if (castingSpell == SpellType.Flare)
                                {
                                    MP = 0;
                                    mode = BuffType.AF3;
                                    modeBuffEnd = currentTime + 12.0;
                                }
                                else
                                {
                                    MP -= MPCost(castingSpell, mode);
                                }
                                if (FireStarterBuffEnd > currentTime)
                                {
                                    nextSpell = true;
                                }
                                if (castingSpell == SpellType.Fire3)
                                {
                                    mode = BuffType.AF3;
                                    modeBuffEnd = currentTime + 12.0;
                                }
                                else if (castingSpell == SpellType.Blizzard3)
                                {
                                    mode = BuffType.UI3;
                                    regenThunder = false;
                                    regenBlizzard = false;
                                    modeBuffEnd = currentTime + 12.0;
                                }
                                else if (castingSpell == SpellType.Blizzard4)
                                {
                                    enochianBuffEnd = currentTime + 30.0 - ++enochianRefreshes * 5.0;
                                }
                                else if (castingSpell == SpellType.Blizzard)
                                {
                                    if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
                                    {
                                        regenBlizzard = true;
                                        modeBuffEnd = currentTime + 12.0;
                                        if (mode == BuffType.UI2)
                                        {
                                            mode = BuffType.UI3;
                                        }
                                        else if (mode == BuffType.UI)
                                        {
                                            mode = BuffType.UI2;
                                        }
                                    }
                                    else if (mode == BuffType.None)
                                    {
                                        mode = BuffType.UI;
                                        modeBuffEnd = currentTime + 12.0;
                                    }
                                    else
                                    {
                                        mode = BuffType.None;
                                    }
                                }
                                else if (castingSpell == SpellType.Thunder || castingSpell == SpellType.Thunder2 || castingSpell == SpellType.Thunder3)
                                {
                                    ThunderDoTEnd = currentTime + 18 + (castingSpell == SpellType.Thunder2 ? 3 : (castingSpell == SpellType.Thunder3 ? 6 : 0));
                                    thunderDmgMod = (ragingBuffEnd > currentTime ? 1.2 : 1.0) * (enochianBuffEnd > currentTime ? 1.05 : 1.0);
                                    if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
                                    {
                                        regenThunder = true;
                                    }
                                    if (sharpCastBuffEnd > currentTime)
                                    {
                                        sharpcastThunder = true;
                                        sharpCastBuffEnd = 0;
                                    }
                                }
                                else if (castingSpell == SpellType.Fire)
                                {
                                    if (RNG.Next(10) < 4 || sharpCastBuffEnd > currentTime)
                                    {
                                        FireStarterBuffEnd = currentTime + 12.0;
                                        if (sharpCastBuffEnd > currentTime)
                                        {
                                            sharpCastBuffEnd = 0;
                                        }
                                    }
                                    if (mode == BuffType.AF || mode == BuffType.AF2 || mode == BuffType.AF3)
                                    {
                                        modeBuffEnd = currentTime + 12.0;
                                        if (mode == BuffType.AF2)
                                        {
                                            mode = BuffType.AF3;
                                        }
                                        else if (mode == BuffType.AF)
                                        {
                                            mode = BuffType.AF2;
                                        }
                                    }
                                    else if (mode == BuffType.None)
                                    {
                                        mode = BuffType.AF;
                                        modeBuffEnd = currentTime + 12.0;
                                    }
                                    else
                                    {
                                        mode = BuffType.None;
                                    }
                                }
                                tempGCDCount++;
                                prevSpell = lastSpell;
                                lastSpell = castingSpell;
                                castingSpell = SpellType.None;
                            }
                        }
                    }
                } while (ragingCount < SimMinutes / 3 + 1);
                if (!failRun)
                {
                    totalTime += currentTime;
                    totalDmg += currentDmg;
                    GCDCount += tempGCDCount;
                }
            }
            GCDCount /= SimIterations;
            simTime = totalTime / SimIterations;
            return totalDmg / totalTime;
        }
        private string PrintDict(SortedList<double, List<SimEvents>> eventDict)
        {
            string output = "";
            for (int index = 0; index < eventDict.Values.Count; index++)
            {
                for (int eindex = 0; eindex < eventDict.Values[index].Count; eindex++)
                {
                    output += eventDict.Keys[index] + " - " + (eventDict.Values[index])[eindex].EventType + " - " + (eventDict.Values[index])[eindex].EventSource + "\n";
                }
            }
            return output;
        }
        private int LostThunderTicks(double thunderDoTEndTime, double nextTickTime)
        {
            if (thunderDoTEndTime > nextTickTime)
            {
                return (int)(thunderDoTEndTime - nextTickTime) / 3 + 1;
            }
            return 0;
        }
    }
}
