using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.Simulation
{
    class BRDSimulation : Simulation
    {
        private double criticalModifier = 0;
        private double critRate = -1;
        private double autoCritModifier = 0;
        private double autoCritRate = -1;
        private double hawkEyeBaseDmg = -1;
        private double baseAutoDmg = -1;
        private double hawkEyeAutoDmg = -1;
        private bool WanderMinuet = true;

        public BRDSimulation()
            : base(Job.Bard)
        {
        }

        protected override double RunSimOnce(Statistics stats, out int GCDCount, out double simTime, bool ignoreResources = false)
        {
            ResetCachedValues();
            double totalDmg = 0.0;
            GCDCount = 0;
            double critmod;
            Random riverProc;

            stats.mainStat = (int)(stats.mainStat * 1.03);

            for (int i = 0; i < SimIterations; i++)
            {
                RNG = new Random(i);
                riverProc = new Random(i);

                double straightShotDuration = 0.0;
                double bloodLetterRecast = 0.0;
                double InvigorateRecast = 0.0;
                double windBiteDuration = 0.0;
                double windBitecritMod = 0.0;
                double windBiteCritChance = 0.0;
                double windBitedmgMod = 0.0;
                bool windbiteHawkEye = false;
                double venomBiteDuration = 0.0;
                double venomBitecritMod = 0.0;
                double venomBiteCritChance = 0.0;
                double venomBitedmgMod = 0.0;
                bool venombiteHawkEye = false;
                bool flamingArrowHawkEye = false;
                double flamingArrowDuration = 0.0;
                double flamingArrowRecast = 0.0;
                double flamingArrowCritMod = 0.0;
                double flamingArrowDmgMod = 0.0;

                double InnerReleaseDuration = 0.0;
                double InnerReleaseRecast = 0.0;

                double bloodforbloodRecast = 0.0;
                double bloodforbloodDuration = 0.0;
                double barrageDuration = 0.0;
                double barrageRecast = 0.0;
                double ragingStrikesRecast = 0.0;
                double ragingStrikesDuration = 0.0;
                double hawkeyeRecast = 0.0;
                double hawkeyeDuration = 0.0;
                double bluntArrowRecast = 0.0;
                double repellingShotRecast = 0.0;

                double empArrowRecast = 0.0;
                double sidewinderRecast = 0.0;
                bool barrageBonus = false;

                double castComplete = 0.0;
                string castingAction = "";

                bool straighterShot = false;

                double GCD = 0.0;

                double currentTime = 0.0;
                double nextTick = (double)RNG.Next(3001) / 1000.0;

                int TP = 1000;

                double animationDelay = 0.0;
                double animationDelayMax = 0.5;
                double autoattackDelay = 0.0;
                double autoattackDelayMax = stats.autoAttackDelay;

                do
                {
                    empArrowRecast = Math.Round(empArrowRecast - 0.001, 3);
                    sidewinderRecast = Math.Round(sidewinderRecast - 0.001, 3);
                    windBiteDuration = Math.Round(windBiteDuration - 0.001, 3);
                    venomBiteDuration = Math.Round(venomBiteDuration - 0.001, 3);
                    InvigorateRecast = Math.Round(InvigorateRecast - 0.001, 3);
                    bloodLetterRecast = Math.Round(bloodLetterRecast - 0.001, 3);
                    straightShotDuration = Math.Round(straightShotDuration - 0.001, 3);
                    animationDelay = Math.Round(animationDelay - 0.001, 3);
                    autoattackDelay = Math.Round(autoattackDelay - 0.001, 3);
                    GCD = Math.Round(GCD - 0.001, 3);
                    InnerReleaseDuration = Math.Round(InnerReleaseDuration - 0.001, 3);
                    InnerReleaseRecast = Math.Round(InnerReleaseRecast - 0.001, 3);
                    ragingStrikesDuration = Math.Round(ragingStrikesDuration - 0.001, 3);
                    ragingStrikesRecast = Math.Round(ragingStrikesRecast - 0.001, 3);
                    bloodforbloodDuration = Math.Round(bloodforbloodDuration - 0.001, 3);
                    bloodforbloodRecast = Math.Round(bloodforbloodRecast - 0.001, 3);
                    barrageDuration = Math.Round(barrageDuration - 0.001, 3);
                    barrageRecast = Math.Round(barrageRecast - 0.001, 3);
                    hawkeyeDuration = Math.Round(hawkeyeDuration - 0.001, 3);
                    hawkeyeRecast = Math.Round(hawkeyeRecast - 0.001, 3);
                    repellingShotRecast = Math.Round(repellingShotRecast - 0.001, 3);
                    bluntArrowRecast = Math.Round(bluntArrowRecast - 0.001, 3);
                    currentTime = Math.Round(currentTime + 0.001, 3);

                    critmod = (InnerReleaseDuration > 0.0 ? 10.0 : 0.0) + (straightShotDuration > 0.0 ? 10.0 : 0.0);
                    if (barrageBonus && barrageDuration <= 0.0)
                    {
                        barrageBonus = false;
                    }
                    
                    if (currentTime >= nextTick) // Process server ticks
                    {
                        if (windBiteDuration >= 0)
                        {
                            totalDmg += SkillDamage("WindDoT", stats, windBitecritMod, windbiteHawkEye) * windBitedmgMod;
                            double windBiteCritProc = (double)Math.Round((double)RNG.Next(100000) / 1000.0, 4);
                            bool windRiverProc = riverProc.Next(2) > 0;
                            if (windBiteCritProc <= windBiteCritChance && windRiverProc)
                            {
                                //if (RNG.Next(2) > 0)
                                //{
                                bloodLetterRecast = -50.0;
                                //}
                            }
                        }
                        if (venomBiteDuration >= 0)
                        {
                            totalDmg += SkillDamage("VenomDoT", stats, venomBitecritMod, venombiteHawkEye) * venomBitedmgMod;
                            double venomBiteCritProc = (double)Math.Round((double)RNG.Next(100000) / 1000.0, 4);
                            bool venomRiverProc = riverProc.Next(2) > 0;
                            if (venomBiteCritProc <= venomBiteCritChance && venomRiverProc)
                            {
                                //if (RNG.Next(2) > 0)
                                //{
                                bloodLetterRecast = -50.0;
                                //}
                            }
                        }
                        if (flamingArrowDuration >= 0)
                        {
                            totalDmg += SkillDamage("Flaming Arrow DoT", stats, flamingArrowCritMod, flamingArrowHawkEye) * flamingArrowDmgMod;
                        }
                        TP = Math.Min(TP + 60, 1000);
                        nextTick = GetNextServerTick(nextTick);
                    }

                    if (autoattackDelay <= 0.0 && !WanderMinuet)
                    {
                        double autoattackdmg = AutoAttackDamage(stats, critmod, hawkeyeDuration > 0.0);
                        totalDmg += autoattackdmg * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                        autoattackDelay = autoattackDelayMax;
                    }

                    if (animationDelay <= 0.0) // It's been long enough since the last action to trigger a new one.
                    {
                        if (currentTime >= castComplete && !string.IsNullOrWhiteSpace(castingAction))
                        { //process the completion of an action cast under wanderer's minuet
                            totalDmg += SkillDamage(castingAction, stats, critmod, hawkeyeDuration > 0.0, straighterShot, WanderMinuet, barrageBonus) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                            TP -= TPCost(castingAction);
                            if (barrageBonus)
                            {
                                barrageBonus = false;
                            }

                            if (castingAction.Equals("Windbite"))
                            {
                                windBiteDuration = 18;
                                if (windBitecritMod != critmod)
                                {
                                    windBitecritMod = critmod;
                                    windBiteCritChance = (double)Math.Round(Core.Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
                                }
                                windBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1) * 1.3;
                                windbiteHawkEye = hawkeyeDuration > 0.0;
                            }
                            else if (castingAction.Equals("Venomous Bite"))
                            {
                                venomBiteDuration = 18;
                                if (venomBitecritMod != critmod)
                                {
                                    venomBitecritMod = critmod;
                                    venomBiteCritChance = (double)Math.Round(Core.Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
                                }
                                venomBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1) * 1.3;
                                venombiteHawkEye = hawkeyeDuration > 0.0;
                            }
                            else if (castingAction.Equals("Iron Jaws"))
                            {
                                if (windBiteDuration > 0)
                                {
                                    windBiteDuration = 18;
                                    if (windBitecritMod != critmod)
                                    {
                                        windBitecritMod = critmod;
                                        windBiteCritChance = (double)Math.Round(Core.Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
                                    }
                                    windBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1) * 1.3;
                                    windbiteHawkEye = hawkeyeDuration > 0.0;
                                }
                                if (venomBiteDuration > 0)
                                {
                                    venomBiteDuration = 18;
                                    if (venomBitecritMod != critmod)
                                    {
                                        venomBitecritMod = critmod;
                                        venomBiteCritChance = (double)Math.Round(Core.Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
                                    }
                                    venomBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1) * 1.3;
                                    venombiteHawkEye = hawkeyeDuration > 0.0;
                                }
                            }
                            else if (castingAction.Equals("Heavy Shot"))
                            {
                                straighterShot = RNG.Next(10) < 2;
                            }
                            else if (castingAction.Equals("Straight Shot"))
                            {
                                straightShotDuration = 20.0;
                            }
                            else if (castingAction.Equals("Empyreal Arrow"))
                            {
                                empArrowRecast = 15.0;
                            }

                            GCDCount++;
                            castingAction = "";
                        }
                        if (GCD <= 0.0 && currentTime >= castComplete)
                        {
                            if (!WanderMinuet)
                            {
                                if ((straighterShot || straightShotDuration <= GCDSpeed(stats.speed)) && (ignoreResources || TP >= TPCost("Straight Shot")))
                                { // Make sure we keep straight shot up and use procs
                                    totalDmg += SkillDamage("Straight Shot", stats, critmod, hawkeyeDuration > 0.0, straighterShot, WanderMinuet, barrageBonus) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                    straighterShot = false;
                                    if (barrageBonus)
                                    {
                                        barrageBonus = false;
                                    }
                                    TP -= TPCost("Straight Shot");
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                    straightShotDuration = 20.0;
                                    GCDCount++;
                                }
                                else if (windBiteDuration <= GCDSpeed(stats.speed) || windBitecritMod < critmod && (ignoreResources || TP >= TPCost("Windbite")))
                                {
                                    totalDmg += SkillDamage("Windbite", stats, critmod, hawkeyeDuration > 0.0, WanderMinuet, barrageBonus) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                    windBiteDuration = 18;
                                    if (barrageBonus)
                                    {
                                        barrageBonus = false;
                                    }
                                    TP -= TPCost("Windbite");
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                    if (windBitecritMod != critmod)
                                    {
                                        windBitecritMod = critmod;
                                        windBiteCritChance = (double)Math.Round(Core.Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
                                    }
                                    windBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                    windbiteHawkEye = hawkeyeDuration > 0.0;
                                    GCDCount++;
                                }
                                else if (venomBiteDuration <= GCDSpeed(stats.speed) || venomBitecritMod < critmod && (ignoreResources || TP >= TPCost("Venomous Bite")))
                                {
                                    totalDmg += SkillDamage("Venomous Bite", stats, critmod, hawkeyeDuration > 0.0, WanderMinuet, barrageBonus) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                    venomBiteDuration = 18;
                                    if (barrageBonus)
                                    {
                                        barrageBonus = false;
                                    }
                                    TP -= TPCost("Venomous Bite");
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                    if (venomBitecritMod != critmod)
                                    {
                                        venomBitecritMod = critmod;
                                        venomBiteCritChance = (double)Math.Round(Core.Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
                                    }
                                    venombiteHawkEye = hawkeyeDuration > 0.0;
                                    venomBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                    GCDCount++;
                                }
                                else if (ignoreResources || TP >= TPCost("Heavy Shot"))
                                {
                                    totalDmg += SkillDamage("Heavy Shot", stats, critmod, hawkeyeDuration > 0.0, WanderMinuet, barrageBonus) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                    straighterShot = RNG.Next(10) < 2;
                                    if (barrageBonus)
                                    {
                                        barrageBonus = false;
                                    }
                                    TP -= TPCost("Heavy Shot");
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                    GCDCount++;
                                }
                            }
                            else
                            {
                                if ((straighterShot || straightShotDuration <= GCDSpeed(stats.speed) + CastSpeed(1.5, stats.speed)) && (ignoreResources || TP >= TPCost("Straight Shot")))
                                { // Make sure we keep straight shot up and use procs
                                    if (straighterShot)
                                    {
                                        totalDmg += SkillDamage("Straight Shot", stats, critmod, hawkeyeDuration > 0.0, straighterShot, WanderMinuet, barrageBonus) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                        if (barrageBonus)
                                        {
                                            barrageBonus = false;
                                        }
                                        straighterShot = false;
                                        TP -= TPCost("Straight Shot");
                                        animationDelay = animationDelayMax;
                                        GCD = GCDSpeed(stats.speed);
                                        straightShotDuration = 20.0;
                                        GCDCount++;
                                    }
                                    else
                                    {
                                        castingAction = "Straight Shot";
                                        castComplete = Math.Round(currentTime + CastSpeed(1.5, stats.speed), 3);
                                        animationDelay = animationDelayMax;
                                        GCD = GCDSpeed(stats.speed);
                                    }
                                }
                                else if ((windBiteDuration > CastSpeed(1.5, stats.speed) && venomBiteDuration > CastSpeed(1.5, stats.speed) &&
                                    (windBiteDuration <= GCDSpeed(stats.speed) + CastSpeed(1.5, stats.speed) || venomBiteDuration <= GCDSpeed(stats.speed) + CastSpeed(1.5, stats.speed))) &&
                                    (ignoreResources || TP >= TPCost("Iron Jaws")))
                                { // If both DoTs are up but either will fall off before we can act again, Iron Jaws
                                    castingAction = "Iron Jaws";
                                    castComplete = Math.Round(currentTime + CastSpeed(1.5, stats.speed), 3);
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                }
                                else if (windBiteDuration <= GCDSpeed(stats.speed) + CastSpeed(1.5, stats.speed) || windBitecritMod < critmod && (ignoreResources || TP >= TPCost("Windbite")))
                                {
                                    castingAction = "Windbite";
                                    castComplete = Math.Round(currentTime + CastSpeed(1.5, stats.speed), 3);
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                }
                                else if (venomBiteDuration <= GCDSpeed(stats.speed) || venomBitecritMod < critmod && (ignoreResources || TP >= TPCost("Venomous Bite")))
                                {
                                    castingAction = "Venomous Bite";
                                    castComplete = Math.Round(currentTime + CastSpeed(1.5, stats.speed), 3);
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                }
                                else if (ignoreResources || TP >= TPCost("Heavy Shot"))
                                {
                                    castingAction = "Heavy Shot";
                                    castComplete = Math.Round(currentTime + CastSpeed(1.5, stats.speed), 3);
                                    animationDelay = animationDelayMax;
                                    GCD = GCDSpeed(stats.speed);
                                }
                            }
                        }
                        if (animationDelay <= 0 && currentTime >= castComplete)
                        {
                            if (bloodLetterRecast <= 0.0 && GCD >= animationDelayMax && (currentTime + Math.Max(GCD, 0) + animationDelayMax >= nextTick && (currentTime + Math.Max(venomBiteDuration, 0) >= nextTick || currentTime + Math.Max(windBiteDuration, 0) >= nextTick)))
                            { // Prioritize Bloodletters above everything else if DoTs are ticking and we're going to hit a server tick before we can act again following the next GCD
                                totalDmg += SkillDamage("Bloodletter", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                bloodLetterRecast = 12.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (InnerReleaseRecast <= 0.0 && GCD >= animationDelayMax)
                            {
                                InnerReleaseDuration = 15.0;
                                InnerReleaseRecast = 60.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (bloodLetterRecast <= 0.0 /*&& straightShotDuration > 0*/ && GCD >= animationDelayMax)
                            { // Prioritize Bloodletters above other off-GCDs but only if straight shot is up
                                totalDmg += SkillDamage("Bloodletter", stats, critmod, hawkeyeDuration > 0.0, WanderMinuet) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                bloodLetterRecast = 12.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (empArrowRecast <= 0.0 && (barrageBonus || barrageRecast > 15.0))
                            { // Empyreal Arrow if barrage is up or emp arrow will be back before its recast is done
                                castingAction = "Empyreal Arrow";
                                castComplete = Math.Round(currentTime + CastSpeed(1.5, stats.speed), 3);
                                animationDelay = animationDelayMax;
                            }
                            else if (TP <= (1000 - 460) && InvigorateRecast <= 0 && GCD >= animationDelayMax)
                            {
                                TP = Math.Min(TP + 400, 1000);
                                InvigorateRecast = 120.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (ragingStrikesRecast <= 0.0 && GCD >= animationDelayMax)
                            {
                                ragingStrikesDuration = 20.0;
                                ragingStrikesRecast = 180.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (bloodforbloodRecast <= 0.0 && GCD >= animationDelayMax)
                            {
                                bloodforbloodDuration = 20.0;
                                bloodforbloodRecast = 80.0;
                                animationDelay = animationDelayMax; // buffs are faster
                            }
                            else if (hawkeyeRecast <= 0.0 && GCD >= animationDelayMax)
                            {
                                hawkeyeDuration = 20.0;
                                hawkeyeRecast = 90.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (barrageRecast <= 0.0 && GCD > animationDelayMax)
                            {
                                barrageRecast = 90.0;
                                barrageDuration = 10.0;
                                barrageBonus = true;
                                animationDelay = animationDelayMax;
                            }
                            else if (flamingArrowRecast <= 0.0 && GCD >= animationDelayMax)
                            {
                                flamingArrowRecast = 60.0;
                                flamingArrowDuration = 30.0;
                                flamingArrowCritMod = critmod;
                                flamingArrowDmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1) * (WanderMinuet ? 1.3 : 1);
                                flamingArrowHawkEye = hawkeyeDuration > 0;
                                animationDelay = animationDelayMax;
                            }
                            else if (windBiteDuration > 0 && venomBiteDuration > 0 && sidewinderRecast <= 0.0)
                            {
                                totalDmg += SkillDamage("Sidewinder", stats, critmod, hawkeyeDuration > 0.0, WanderMinuet) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                sidewinderRecast = 60.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (bluntArrowRecast <= 0.0 && GCD >= animationDelayMax)
                            {
                                totalDmg += SkillDamage("Blunt Arrow", stats, critmod, hawkeyeDuration > 0.0, WanderMinuet) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                bluntArrowRecast = 30.0;
                                animationDelay = animationDelayMax;
                            }
                            else if (repellingShotRecast <= 0.0 && GCD >= animationDelayMax * 2)
                            {
                                totalDmg += SkillDamage("Repelling Shot", stats, critmod, hawkeyeDuration > 0.0, WanderMinuet) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
                                repellingShotRecast = 30.0;
                                animationDelay = animationDelayMax * 2;
                            }
                        }
                    }
                } while (currentTime <= SimMinutes * 60);
            }
            GCDCount /= SimIterations;
            simTime = 0;
            return totalDmg;
        }

        protected override void ResetCachedValues()
        {
            base.ResetCachedValues();
            criticalModifier = 0;
            critRate = -1;
            autoCritModifier = 0;
            autoCritRate = -1;
            hawkEyeBaseDmg = -1;
            baseAutoDmg = -1;
            hawkEyeAutoDmg = -1;
        }

        private int TPCost(string skillName)
        {
            switch (skillName)
            {
                case "Windbite":
                case "Venomous Bite":
                    return 80;
                case "Straight Shot":
                    return 70;
                case "Heavy Shot":
                case "Empyreal Arrow":
                case "Iron Jaws":
                    return 60;
                default:
                    return 0;
            }
        }

        private double AutoAttackDamage(Statistics stats, double critModifier, bool hawkEye = false)
        {
            if (baseAutoDmg < 0)
            {
                baseAutoDmg = Core.Common.CalculateAutoAttackDamage(stats, false);
            }
            if (autoCritRate < 0 || autoCritModifier != critModifier)
            {
                autoCritModifier = critModifier;
                autoCritRate = Core.Common.CalculateCritRate(stats.crit, autoCritModifier);
            }
            if (hawkEye && hawkEyeAutoDmg < 0)
            {
                Statistics hawkEyestats = stats;
                hawkEyestats.mainStat = (int)(stats.mainStat * 1.15);
                hawkEyeAutoDmg = Core.Common.CalculateAutoAttackDamage(hawkEyestats, false);
            }
            double autoAttackDamage = (hawkEye ? hawkEyeAutoDmg : baseAutoDmg) * (1 - autoCritRate + Core.Common.CalculateCritBonus(stats.crit) * autoCritRate);
            return autoAttackDamage;
        }
        private double SkillDamage(string skillName, Statistics stats, bool hawkEye = false, bool straighterShot = false, bool minuet = false, bool barrage = false)
        {
            return SkillDamage(skillName, stats, 0.0, hawkEye, straighterShot, minuet);
        }
        private double SkillDamage(string skillName, Statistics stats, double critModifier = 0.0, bool hawkEye = false, bool straighterShot = false, bool minuet = false, bool barrage = false)
        {
            double potency;
            switch (skillName)
            {
                case "Windbite":
                    potency = 60;
                    break;
                case "WindDoT":
                    potency = 45;
                    break;
                case "Venomous Bite":
                case "Iron Jaws":
                    potency = 100;
                    break;
                case "VenomDoT":
                    potency = 35;
                    break;
                case "Straight Shot":
                    potency = 140;
                    break;
                case "Heavy Shot":
                case "Bloodletter":
                    potency = 150;
                    break;
                case "Blunt Arrow":
                    potency = 50;
                    break;
                case "Repelling Shot":
                    potency = 80;
                    break;
                case "Flaming Arrow DoT":
                    potency = 35;
                    break;
                case "Empyreal Arrow":
                    potency = 220;
                    break;
                case "Sidewinder":
                    potency = 250;
                    break;
                default:
                    potency = 0.0;
                    break;
            }
            if (baseSkillDmg < 0)
            {
                baseSkillDmg = Core.Common.CalculateDamage(stats, false) * 1.2;
            }
            if (critRate < 0 || criticalModifier != critModifier)
            {
                criticalModifier = critModifier;
                critRate = Core.Common.CalculateCritRate(stats.crit, criticalModifier);
            }
            if (hawkEye && hawkEyeBaseDmg < 0)
            {
                Statistics hawkEyestats = stats;
                hawkEyestats.mainStat = (int)(stats.mainStat * 1.15);
                hawkEyeBaseDmg = Core.Common.CalculateDamage(hawkEyestats, false) * 1.2;
            }
            double critBonus = Core.Common.CalculateCritBonus(stats.crit);
            double skillDamage = (hawkEye ? hawkEyeBaseDmg : baseSkillDmg) * potency * 0.01005;
            if (skillName.Contains("DoT"))
            {
                skillDamage *= 1.0 + Core.Common.CalculateDoTSpeedScalar(stats.speed);
                skillDamage *= 1.0 - critRate + critBonus * critRate;
            }
            else
            {
                skillDamage *= ((skillName == "Straight Shot" && straighterShot) ? critBonus : (barrage ? 1.0 : 1.0 - critRate + critBonus * critRate));
            }
            skillDamage *= (minuet ? 1.3 : 1);
            return skillDamage;
        }
    }
}
