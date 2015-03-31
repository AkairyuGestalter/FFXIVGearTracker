using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVGearTracker
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

		public BRDSimulation()
			: base(Job.Bard)
		{
		}

		protected override double RunSimOnce(Statistics stats, out int GCDCount, bool ignoreResources = false)
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
					critmod = (InnerReleaseDuration > 0.0 ? 10.0 : 0.0) + (straightShotDuration > 0.0 ? 10.0 : 0.0);
					if (animationDelay <= 0.0) // It's been long enough since the last action to trigger a new one.
					{
						if (GCD <= 0.0)
						{ // If bloodletter is down, and GCD is up, use a GCD skill
							if ((straighterShot || straightShotDuration <= GCDSpeed(stats.speed)) && (ignoreResources || TP >= 70))
							{ // Make sure we keep straight shot up and use procs
								totalDmg += SkillDamage("Straight Shot", stats, critmod, hawkeyeDuration > 0.0, straighterShot) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								straighterShot = false;
								TP -= 70;
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed);
								straightShotDuration = 20.0;
								GCDCount++;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (windBiteDuration <= GCDSpeed(stats.speed) || windBitecritMod < critmod && (ignoreResources || TP >= 80))
							{
								totalDmg += SkillDamage("Windbite", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								windBiteDuration = 18;
								TP -= 80;
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed);
								if (windBitecritMod != critmod)
								{
									windBitecritMod = critmod;
									windBiteCritChance = (double)Math.Round(Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
								}
								windBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								windbiteHawkEye = hawkeyeDuration > 0.0;
								GCDCount++;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (venomBiteDuration <= GCDSpeed(stats.speed) || venomBitecritMod < critmod && (ignoreResources || TP >= 80))
							{
								totalDmg += SkillDamage("Venomous Bite", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								venomBiteDuration = 18;
								TP -= 80;
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed);
								if (venomBitecritMod != critmod)
								{
									venomBitecritMod = critmod;
									venomBiteCritChance = (double)Math.Round(Common.CalculateCritRate(stats.crit, critmod) * 100.0, 4);
								}
								venombiteHawkEye = hawkeyeDuration > 0.0;
								venomBitedmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								GCDCount++;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (ignoreResources || TP >= 60)
							{
								totalDmg += SkillDamage("Heavy Shot", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								straighterShot = RNG.Next(10) < 2;
								TP -= 60;
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed);
								GCDCount++;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
						}
						if (animationDelay <= 0)
						{
							if (bloodLetterRecast <= 0.0 && GCD >= animationDelayMax && (currentTime + Math.Max(GCD, 0) + animationDelayMax >= nextTick && (currentTime + Math.Max(venomBiteDuration, 0) >= nextTick || currentTime + Math.Max(windBiteDuration, 0) >= nextTick)))
							{ // Prioritize Bloodletters above everything else if DoTs are ticking and we're going to hit a server tick before we can act again following the next GCD
								totalDmg += SkillDamage("Bloodletter", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								bloodLetterRecast = 12.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (InnerReleaseRecast <= 0.0 && GCD >= animationDelayMax)
							{
								InnerReleaseDuration = 15.0;
								InnerReleaseRecast = 60.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (bloodLetterRecast <= 0.0 && straightShotDuration > 0 && GCD >= animationDelayMax)
							{ // Prioritize Bloodletters above other off-GCDs but only if straight shot is up
								totalDmg += SkillDamage("Bloodletter", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								bloodLetterRecast = 12.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (TP <= (1000 - 400 - 60) && InvigorateRecast <= 0 && GCD >= animationDelayMax)
							{
								TP = Math.Min(TP + 400, 1000);
								InvigorateRecast = 120.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (ragingStrikesRecast <= 0.0 && GCD >= animationDelayMax)
							{
								ragingStrikesDuration = 20.0;
								ragingStrikesRecast = 180.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (bloodforbloodRecast <= 0.0 && GCD >= animationDelayMax)
							{
								bloodforbloodDuration = 20.0;
								bloodforbloodRecast = 80.0;
								animationDelay = animationDelayMax; // buffs are faster
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (hawkeyeRecast <= 0.0 && GCD >= animationDelayMax)
							{
								hawkeyeDuration = 20.0;
								hawkeyeRecast = 90.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (barrageRecast <= 0.0 && GCD >= animationDelayMax)
							{
								barrageRecast = 90.0;
								barrageDuration = 10.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (flamingArrowRecast <= 0.0 && GCD >= animationDelayMax)
							{
								flamingArrowRecast = 60.0;
								flamingArrowDuration = 30.0;
								flamingArrowCritMod = critmod;
								flamingArrowDmgMod = 1 * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								flamingArrowHawkEye = hawkeyeDuration > 0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (bluntArrowRecast <= 0.0 && GCD >= animationDelayMax)
							{
								totalDmg += SkillDamage("Blunt Arrow", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								bluntArrowRecast = 30.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
							else if (repellingShotRecast <= 0.0 && GCD >= animationDelayMax)
							{
								totalDmg += SkillDamage("Repelling Shot", stats, critmod, hawkeyeDuration > 0.0) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
								repellingShotRecast = 30.0;
								animationDelay = animationDelayMax;
								//autoattackDelay = Math.Max(autoattackDelay, 0.1);
							}
						}
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
							totalDmg += SkillDamage("Flaming Arrow", stats, flamingArrowCritMod, flamingArrowHawkEye) * flamingArrowDmgMod;
						}
						TP = Math.Min(TP + 60, 1000);
						nextTick = GetNextServerTick(nextTick);
					}

					if (autoattackDelay <= 0.0)
					{
						double autoattackdmg = AutoAttackDamage(stats, critmod, hawkeyeDuration > 0.0);
						totalDmg += autoattackdmg * (barrageDuration > 0.0 ? 3 : 1) * (bloodforbloodDuration > 0.0 ? 1.1 : 1) * (ragingStrikesDuration > 0.0 ? 1.2 : 1);
						autoattackDelay = autoattackDelayMax;
					}

					windBiteDuration -= 0.001;
					venomBiteDuration -= 0.001;
					InvigorateRecast -= 0.001;
					bloodLetterRecast -= 0.001;
					straightShotDuration -= 0.001;
					animationDelay -= 0.001;
					autoattackDelay -= 0.001;
					GCD -= 0.001;
					InnerReleaseDuration -= 0.001;
					InnerReleaseRecast -= 0.001;
					ragingStrikesDuration -= 0.001;
					ragingStrikesRecast -= 0.001;
					bloodforbloodDuration -= 0.001;
					bloodforbloodRecast -= 0.001;
					barrageDuration -= 0.001;
					barrageRecast -= 0.001;
					hawkeyeDuration -= 0.001;
					hawkeyeRecast -= 0.001;
					repellingShotRecast -= 0.001;
					bluntArrowRecast -= 0.001;
					currentTime += 0.001;
				} while (currentTime <= SimMinutes * 60);
			}
			GCDCount /= SimIterations;
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

		private double AutoAttackDamage(Statistics stats, double critModifier, bool hawkEye = false)
		{
			if (baseAutoDmg < 0)
			{
				baseAutoDmg = Common.CalculateAutoAttackDamage(stats, false);
			}
			if (autoCritRate < 0 || autoCritModifier != critModifier)
			{
				autoCritModifier = critModifier;
				autoCritRate = Common.CalculateCritRate(stats.crit, autoCritModifier);
			}
			if (hawkEye && hawkEyeAutoDmg < 0)
			{
				Statistics hawkEyestats = stats;
				hawkEyestats.mainStat = (int)(stats.mainStat * 1.15);
				hawkEyeAutoDmg = Common.CalculateAutoAttackDamage(hawkEyestats, false);
			}
			double autoAttackDamage = (hawkEye ? hawkEyeAutoDmg : baseAutoDmg) * (1 + 0.5 * autoCritRate);
			return autoAttackDamage;
		}
		private double SkillDamage(string skillName, Statistics stats, bool hawkEye = false, bool straighterShot = false)
		{
			return SkillDamage(skillName, stats, 0.0, hawkEye, straighterShot);
		}
		private double SkillDamage(string skillName, Statistics stats, double critModifier = 0.0, bool hawkEye = false, bool straighterShot = false)
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
				case "Flaming Arrow":
					potency = 35;
					break;
				default:
					potency = 0.0;
					break;
			}
			if (baseSkillDmg < 0)
			{
				baseSkillDmg = Common.CalculateDamage(stats, false) * 1.2;
			}
			if (critRate < 0 || criticalModifier != critModifier)
			{
				criticalModifier = critModifier;
				critRate = Common.CalculateCritRate(stats.crit, criticalModifier);
			}
			if (hawkEye && hawkEyeBaseDmg < 0)
			{
				Statistics hawkEyestats = stats;
				hawkEyestats.mainStat = (int)(stats.mainStat * 1.15);
				hawkEyeBaseDmg = Common.CalculateDamage(hawkEyestats, false) * 1.2;
			}
			double skillDamage = (hawkEye ? hawkEyeBaseDmg : baseSkillDmg) * potency / 100.0 * ((skillName == "Straight Shot" && straighterShot) ? 1.5 : 1.0 + 0.5 * critRate);
			return skillDamage;
		}
	}
}
