using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.Simulation
{
    class DRGSimulation : Simulation
    {
        public DRGSimulation()
            : base(Job.Dragoon)
        {
        }

        protected override double RunSimOnce(Statistics stats, out int GCDCount, out double simTime, bool ignoreResources = false)
        {
            ResetCachedValues();
            double totalDmg = 0.0;
            GCDCount = 0;
            double critMod;

            for (int i = 0; i < SimIterations; i++)
            {
                RNG = new Random(i);

                double heavyThrustDuration = 0.0;
                double disemBowelDuration = 0.0;
                double chaosThrustDuration = 0.0;
                double chaosDmgMod = 0.0;
                double chaosCritMod = 0.0;
                double phlebotomizeDuration = 0.0;
                double phlebDmgMod = 0.0;
                double phlebCritMod = 0.0;

                double InvigorateRecast = 0.0;
                double InternalReleaseDuration = 0.0;
                double InternalReleaseRecast = 0.0;
                double bloodforbloodDuration = 0.0;
                double bloodforbloodRecast = 0.0;
                double powerSurgeDuration = 0.0;
                double powerSurgeRecast = 0.0;
                double lifeSurgeDuration = 0.0;
                double lifeSurgeRecast = 0.0;
                double jumpRecast = 0.0;
                double spineshatterRecast = 0.0;
                double dragonFireRecast = 0.0;
                double legSweepRecast = 0.0;

                double bloodOfDragonDuration = 0.0;
                double bloodOfDragonRecast = 0.0;
                double battleLitanyDuration = 0.0;
                double battleLitanyRecast = 0.0;
                double gierskogulRecast = 0.0;
                bool gierskogulOk = false;

                int TP = 1000;
                double GCD = 0;

                double currentTime = 0;
                double nextTick = (double)RNG.Next(3001) / 1000.0;

                double animationDelay = 0.0;
                double animationDelayMax = Math.Round(GCDSpeed(stats.speed) / 3.0 - 0.001, 3); // simulate ability to get maximum of 2 oGCDs in between a GCD, including animation lock for the GCD itself
                double autoattackDelay = 0.0;
                double autoattackDelayMax = stats.autoAttackDelay;

                string comboSkill = "";

                do
                {
                    autoattackDelay = Math.Round(autoattackDelay - 0.001, 3);
                    heavyThrustDuration = Math.Round(heavyThrustDuration - 0.001, 3);
                    disemBowelDuration = Math.Round(disemBowelDuration - 0.001, 3);
                    chaosThrustDuration = Math.Round(chaosThrustDuration - 0.001, 3);
                    phlebotomizeDuration = Math.Round(phlebotomizeDuration - 0.001, 3);
                    animationDelay = Math.Round(animationDelay - 0.001, 3);
                    InternalReleaseDuration = Math.Round(InternalReleaseDuration - 0.001, 3);
                    InternalReleaseRecast = Math.Round(InternalReleaseRecast - 0.001, 3);
                    bloodforbloodDuration = Math.Round(bloodforbloodDuration - 0.001, 3);
                    bloodforbloodRecast = Math.Round(bloodforbloodRecast - 0.001, 3);
                    powerSurgeDuration = Math.Round(powerSurgeDuration - 0.001, 3);
                    powerSurgeRecast = Math.Round(powerSurgeRecast - 0.001, 3);
                    lifeSurgeDuration = Math.Round(lifeSurgeDuration - 0.001, 3);
                    lifeSurgeRecast = Math.Round(lifeSurgeRecast - 0.001, 3);
                    jumpRecast = Math.Round(jumpRecast - 0.001, 3);
                    spineshatterRecast = Math.Round(spineshatterRecast - 0.001, 3);
                    dragonFireRecast = Math.Round(dragonFireRecast - 0.001, 3);
                    legSweepRecast = Math.Round(legSweepRecast - 0.001, 3);

                    bloodOfDragonDuration = Math.Round(bloodOfDragonDuration - 0.001, 3);
                    bloodOfDragonRecast = Math.Round(bloodOfDragonRecast - 0.001, 3);
                    battleLitanyDuration = Math.Round(battleLitanyDuration - 0.001, 3);
                    battleLitanyRecast = Math.Round(battleLitanyRecast - 0.001, 3);
                    gierskogulRecast = Math.Round(gierskogulRecast - 0.001, 3);

                    GCD = Math.Round(GCD - 0.001, 3);
                    currentTime = Math.Round(currentTime + 0.001, 3);

                    critMod = (InternalReleaseDuration > 0.0 ? 10.0 : 0.0) + (battleLitanyDuration > 0.0 ? 15.0 : 0.0);
                    
                    if (currentTime >= nextTick)
                    {
                        if (chaosThrustDuration >= 0)
                        {
                            totalDmg += SkillDamage("ChaosDoT", stats, chaosDmgMod, chaosCritMod);
                        }
                        if (phlebotomizeDuration >= 0)
                        {
                            totalDmg += SkillDamage("PhlebDoT", stats, phlebDmgMod, phlebCritMod);
                        }
                        TP = Math.Min(TP + 60, 1000);
                        nextTick = GetNextServerTick(nextTick);
                    }

                    if (autoattackDelay <= 0.0)
                    {
                        totalDmg += Core.Common.CalculateAutoAttackDamage(stats, critMod);
                        autoattackDelay = autoattackDelayMax;
                    }

                    if (animationDelay <= 0)
                    {
                        if (GCD <= 0)
                        {
                            if ((heavyThrustDuration <= GCDSpeed(stats.speed) * 2 && string.IsNullOrWhiteSpace(comboSkill)) && (ignoreResources || TP >= TPCost("Heavy Thrust")))
                            {
                                totalDmg += SkillDamage("Heavy Thrust", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                                TP -= TPCost("Heavy Thrust");
                                heavyThrustDuration = 24.0;
                                animationDelay = animationDelayMax;
                                GCD = GCDSpeed(stats.speed);
                                comboSkill = "";
                                GCDCount++;
                                //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                            }
                            else if (disemBowelDuration > 0 && phlebotomizeDuration <= GCDSpeed(stats.speed) * 2 && string.IsNullOrWhiteSpace(comboSkill) && (ignoreResources || TP >= TPCost("Phlebotomize")))
                            {
                                totalDmg += SkillDamage("Phlebotomize", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                                TP -= TPCost("Phlebotomize");
                                phlebotomizeDuration = 24.0;
                                animationDelay = animationDelayMax;
                                phlebCritMod = critMod;
                                phlebDmgMod = 1 * (heavyThrustDuration > 0.0 ? 1.15 : 1) * (bloodforbloodDuration > 0.0 ? 1.3 : 1);
                                GCD = GCDSpeed(stats.speed);
                                comboSkill = "";
                                GCDCount++;
                                //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                            }
                            else if (disemBowelDuration <= GCDSpeed(stats.speed) * 5 && string.IsNullOrWhiteSpace(comboSkill) && (ignoreResources || TP >= TPCost("Impulse Drive")))
                            { // start rear combo - Impulse Drive
                                totalDmg += SkillDamage("Impulse Drive", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                                TP -= TPCost("Impulse Drive");
                                animationDelay = animationDelayMax;
                                GCD = GCDSpeed(stats.speed);
                                comboSkill = "Disembowel";
                                GCDCount++;
                                //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                            }
                            else if (string.IsNullOrWhiteSpace(comboSkill) && (ignoreResources || TP >= TPCost("True Thrust")))
                            { // start main combo - True Thrust
                                totalDmg += SkillDamage("True Thrust", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                                TP -= TPCost("True Thrust");
                                animationDelay = animationDelayMax;
                                GCD = GCDSpeed(stats.speed);
                                comboSkill = "Vorpal Thrust";
                                GCDCount++;
                                //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                            }
                            else if (!string.IsNullOrWhiteSpace(comboSkill) && (ignoreResources || TP >= TPCost(comboSkill)))
                            { // do the next skill in the combo
                                totalDmg += SkillDamage(comboSkill, stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod, lifeSurgeDuration > 0.0);
                                TP -= TPCost(comboSkill);
                                GCD = GCDSpeed(stats.speed);
                                switch (comboSkill)
                                {
                                    case "Vorpal Thrust":
                                        comboSkill = "Full Thrust";
                                        break;
                                    case "Full Thrust":
                                        comboSkill = (bloodOfDragonDuration >= GCD ? ComboFinisher() : "");
                                        break;
                                    case "Disembowel":
                                        disemBowelDuration = 30.0;
                                        comboSkill = "Chaos Thrust";
                                        break;
                                    case "Chaos Thrust":
                                        chaosThrustDuration = 30.0;
                                        chaosCritMod = critMod;
                                        chaosDmgMod = 1 * (heavyThrustDuration > 0.0 ? 1.15 : 1) * (bloodforbloodDuration > 0.0 ? 1.3 : 1);
                                        comboSkill = (bloodOfDragonDuration >= GCD ? ComboFinisher() : "");
                                        break;
                                    case "Wheeling Thrust":
                                    case "Fang and Claw":
                                        if (bloodOfDragonDuration > 0.0)
                                        {
                                            bloodOfDragonDuration = Math.Min(bloodOfDragonDuration + 15.0, 30.0);
                                            if (bloodOfDragonDuration >= GCDSpeed(stats.speed) * 5 + 10.0)
                                            {
                                                gierskogulOk = true;
                                            }
                                        }
                                        comboSkill = "";
                                        break;
                                    default:
                                        break;
                                }
                                animationDelay = animationDelayMax;
                                GCDCount++;
                                //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                            }
                        }
                        //lifesurge
                        if (animationDelay <= 0 && lifeSurgeRecast <= 0 && GCD >= animationDelayMax && (comboSkill == "Full Thrust" || comboSkill == "Wheeling Thrust" || comboSkill == "Fang and Claw"))
                        {
                            lifeSurgeDuration = 10.0;
                            lifeSurgeRecast = 50.0;
                            animationDelay = animationDelayMax;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        if (animationDelay <= 0 && gierskogulRecast <= 0.0 && GCD >= animationDelayMax && (gierskogulOk || bloodOfDragonRecast <= GCD + animationDelayMax) && bloodOfDragonDuration >= 0.0)
                        {
                            totalDmg += SkillDamage("Gierskogul", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                            gierskogulRecast = 10.0;
                            gierskogulOk = false;
                            bloodOfDragonDuration = Math.Round(bloodOfDragonDuration - 10.0, 3);
                            animationDelay = animationDelayMax;
                        }
                        //battle litanty
                        if (animationDelay <= 0 && battleLitanyRecast <= 0 && GCD >= animationDelayMax)
                        {
                            battleLitanyRecast = 180.0;
                            battleLitanyDuration = 20.0;
                            animationDelay = animationDelayMax;
                        }
                        //blood for blood
                        if (animationDelay <= 0 && bloodforbloodRecast <= 0 && GCD >= animationDelayMax)
                        {
                            bloodforbloodDuration = 20.0;
                            bloodforbloodRecast = 80.0;
                            animationDelay = animationDelayMax;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        //internal release
                        if (animationDelay <= 0 && InternalReleaseRecast <= 0 && GCD >= animationDelayMax)
                        {
                            InternalReleaseDuration = 15.0;
                            InternalReleaseRecast = 60.0;
                            animationDelay = animationDelayMax;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        //blood of the dragon
                        if (animationDelay <= 0 && bloodOfDragonRecast <= 0 && GCD >= animationDelayMax)
                        {
                            bloodOfDragonDuration = 15.0;
                            bloodOfDragonRecast = 60.0;
                            animationDelay = animationDelayMax;
                        }
                        //leg sweep
                        if (animationDelay <= 0 && legSweepRecast <= 0 && GCD >= animationDelayMax)
                        {
                            totalDmg += SkillDamage("Leg Sweep", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                            legSweepRecast = 20.0;
                            animationDelay = animationDelayMax;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        //power surge
                        if (animationDelay <= 0 && powerSurgeRecast <= 0 && GCD >= animationDelayMax && jumpRecast <= GCD + animationDelayMax)
                        { // make sure jump is going to be ready before using power surge
                            powerSurgeDuration = 10.0;
                            powerSurgeRecast = 60.0;
                            animationDelay = animationDelayMax;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        //jump
                        if (animationDelay <= 0 && jumpRecast <= 0 && GCD >= animationDelayMax * 1.5)
                        {
                            totalDmg += SkillDamage("Jump", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodOfDragonDuration > 0 ? 1.3 : 1) * (powerSurgeDuration > 0.0 ? 1.5 : 1) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                            powerSurgeDuration = 0.0;
                            jumpRecast = 30.0;
                            animationDelay = (animationDelayMax * 1.5);
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        //dragonfire
                        if (animationDelay <= 0 && dragonFireRecast <= 0 && GCD >= animationDelayMax * 1.5)
                        {
                            totalDmg += SkillDamage("Dragonfire Dive", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                            dragonFireRecast = 120.0;
                            animationDelay = animationDelayMax * 1.5;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        //spineshatter
                        if (animationDelay <= 0 && spineshatterRecast <= 0 && GCD >= animationDelayMax * 1.5)
                        {
                            totalDmg += SkillDamage("Spineshatter Dive", stats, (heavyThrustDuration > 0.0 ? 1.5 : 1.0) * (disemBowelDuration > 0.0 ? 1.0 / 0.9 : 1.0) * (bloodOfDragonDuration > 0 ? 1.3 : 1) * (powerSurgeDuration > 0.0 ? 1.5 : 1) * (bloodforbloodDuration > 0.0 ? 1.3 : 1), critMod);
                            powerSurgeDuration = 0;
                            spineshatterRecast = 60.0;
                            animationDelay = animationDelayMax * 1.5;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                        if (animationDelay <= 0 && TP <= (1000 - 500 - 60) && InvigorateRecast <= 0 && GCD >= animationDelayMax)
                        {
                            TP = Math.Min(TP + 500, 1000);
                            InvigorateRecast = 120.0;
                            //autoattackDelay = Math.Max(autoattackDelay, 0.1);
                        }
                    }
                } while (currentTime <= SimMinutes * 60.0);
            }
            GCDCount /= SimIterations;
            simTime = 0;
            return totalDmg;
        }

        private int TPCost(string skillName)
        {
            switch (skillName)
            {
                case "Phlebotomize":
                    return 90;
                case "Impulse Drive":
                case "True Thrust":
                case "Heavy Thrust":
                    return 70;
                case "Vorpal Thrust":
                case "Full Thrust":
                case "Disembowel":
                case "Chaos Thrust":
                case "Fang and Claw":
                case "Wheeling Thrust":
                    return 60;
                default:
                    return 0;
            }
        }

        private string ComboFinisher()
        {
            if (new Random().Next() % 2 == 0)
            {
                return "Fang and Claw";
            }
            return "Wheeling Thrust";
        }

        private double SkillDamage(string skillName, Statistics stats, double damageBonus, double CriticalModifier, bool lifeSurge = false)
        {
            double potency;
            switch (skillName)
            {
                case "Full Thrust":
                    potency = 360;
                    break;
                case "Wheeling Thrust":
                case "Fang and Claw":
                    potency = 290;
                    break;
                case "Dragonfire Dive":
                case "Chaos Thrust":
                    potency = 250;
                    break;
                case "Disembowel":
                    potency = 220;
                    break;
                case "Jump":
                case "Vorpal Thrust":
                case "Gierskogul":
                    potency = 200;
                    break;
                case "Impulse Drive":
                    potency = 180;
                    break;
                case "Spineshatter Dive":
                case "Heavy Thrust":
                case "Phlebotomize":
                    potency = 170;
                    break;
                case "True Thrust":
                    potency = 150;
                    break;
                case "Leg Sweep":
                    potency = 130;
                    break;
                case "ChaosDoT":
                    potency = 35;
                    break;
                case "PhlebDoT":
                    potency = 30;
                    break;
                default:
                    potency = 0.0;
                    break;
            }

            if (skillName.Contains("DoT"))
            {
                damageBonus *= Core.Common.CalculateDoTSpeedScalar(stats.speed);
            }

            double baseDamage = Core.Common.CalculateDamage(stats, false, damageBonus);
            double critBonus = Core.Common.CalculateCritBonus(stats.crit);
            double critRate = Core.Common.CalculateCritRate(stats.crit, CriticalModifier);

            if (!skillName.Contains("DoT"))
            {
                baseDamage *= (lifeSurge ? critBonus : (1.0 - critRate + critBonus * critRate));
            }
            else
            {
                baseDamage *= 1.0 - critRate + critBonus * critRate;
            }

            return baseDamage * potency * 0.01005;
        }
    }
}
