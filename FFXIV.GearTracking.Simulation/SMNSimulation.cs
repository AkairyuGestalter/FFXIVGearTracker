using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.Simulation
{
	class SMNSimulation : Simulation
	{
		public SMNSimulation()
			: base(Job.Summoner)
		{
		}

		protected override double RunSimOnce(Statistics stats, out int GCDCount, out double simTime, bool ignoreResources = false)
		{
			ResetCachedValues();
			double totalDmg = 0.0;
			GCDCount = 0;

            stats.mainStat = (int)(stats.mainStat * 1.03);

			for (int i = 0; i < SimIterations; i++)
			{
				RNG = new Random(i);

				int aetherflowStacks = 3;
				int aetherflowMax = 3;
                int aethertrailStacks = 0;
                int aethertrailMax = 3;

                int currentMP = (int)Math.Round((int)(stats.pie * 1.03) * 7.24 + 1463, 0);
				int mpMax = 3020;
				double Bio2Duration = 0.0;
				double MiasmaDuration = 0.0;
				double Miasma2Duration = 0.0;
				double BioDuration = 0.0;
				double shadowFlareDuration = 0.0;
				double festerRecast = 0.0;
				double energyDrainRecast = 0.0;
				double aetherflowRecast = 0.0;
				double contagionRecast = 0.0;
				double aerialSlashRecast = 0.0;
				double speedBoostDuration = 0.0;

				double RouseDuration = 0.0;
				double RouseRecast = 0.0;
				double SpurDuration = 0.0;
				double SpurRecast = 0.0;

                double EnkindleRecast = 0.0;
                double dreadwyrmRecast = 0.0;
                double dreadwyrmDuration = 0.0;
                double painflareRecast = 0.0;
                double tridisasterRecast = 0.0;

				double Bio2Mod = 0.0;
				double MiasmaMod = 0.0;
				double Miasma2Mod = 0.0;
				double BioMod = 0.0;
				double FlareMod = 0.0;
				double ragingDuration = 0.0;
				double ragingRecast = 0.0;
				double swiftRecast = 0.0;

				double currentTime = 0.0;
				double GCD = 0.0;
				double petGCD = 0.0;
				double animationDelay = 0.0;
				double animationDelayMax = 0.5;
				double petAnimationDelay = 0.0;
				double castComplete = 0.0;
				double petCastComplete = 0.0;
				bool petActive = false;
				double nextTick = (double)RNG.Next(3001) / 1000.0;
				string castingSpell = "";
				string petSpell = "";

				do
                {
                    EnkindleRecast = Math.Round(EnkindleRecast - 0.001, 3);
                    dreadwyrmRecast = Math.Round(dreadwyrmRecast - 0.001, 3);
                    dreadwyrmDuration = Math.Round(dreadwyrmDuration - 0.001, 3);
                    painflareRecast = Math.Round(painflareRecast - 0.001, 3);
                    tridisasterRecast = Math.Round(tridisasterRecast - 0.001, 3);
                    swiftRecast = Math.Round(swiftRecast - 0.001, 3);
                    RouseDuration = Math.Round(RouseDuration - 0.001, 3);
                    RouseRecast = Math.Round(RouseRecast - 0.001, 3);
                    SpurDuration = Math.Round(SpurDuration - 0.001, 3);
                    SpurRecast = Math.Round(SpurRecast - 0.001, 3);
                    ragingDuration = Math.Round(ragingDuration - 0.001, 3);
                    ragingRecast = Math.Round(ragingRecast - 0.001, 3);
                    Bio2Duration = Math.Round(Bio2Duration - 0.001, 3);
                    MiasmaDuration = Math.Round(MiasmaDuration - 0.001, 3);
                    Miasma2Duration = Math.Round(Miasma2Duration - 0.001, 3);
                    BioDuration = Math.Round(BioDuration - 0.001, 3);
                    shadowFlareDuration = Math.Round(shadowFlareDuration - 0.001, 3);
                    festerRecast = Math.Round(festerRecast - 0.001, 3);
                    energyDrainRecast = Math.Round(energyDrainRecast - 0.001, 3);
                    aetherflowRecast = Math.Round(aetherflowRecast - 0.001, 3);
                    GCD = Math.Round(GCD - 0.001, 3);
                    petGCD = Math.Round(petGCD - 0.001, 3);
                    contagionRecast = Math.Round(contagionRecast - 0.001, 3);
                    aerialSlashRecast = Math.Round(aerialSlashRecast - 0.001, 3);
                    speedBoostDuration = Math.Round(speedBoostDuration - 0.001, 3);
                    animationDelay = Math.Round(animationDelay - 0.001, 3);
                    petAnimationDelay = Math.Round(petAnimationDelay - 0.001, 3);
                    currentTime = Math.Round(currentTime + 0.001, 3);

					if (animationDelay <= 0)
					{
                        if (ragingRecast <= 0.0 && string.IsNullOrWhiteSpace(castingSpell) && dreadwyrmDuration > 0)
						{
							ragingRecast = 180.0;
							ragingDuration = 20.0;
							animationDelay = animationDelayMax;
						}

						if (animationDelay <= 0.0 && currentTime >= castComplete && GCD <= 0)
						{
							if (!string.IsNullOrWhiteSpace(castingSpell))
							{ // If we were actively casting something, process it's impact
								switch (castingSpell)
								{
									case "Bio II":
										Bio2Mod = (ragingDuration > 0.0 ? 1.2 : 1.0);
										Bio2Duration = 30.0;
										currentMP -= 186;
										break;
									case "Miasma":
										MiasmaMod = (ragingDuration > 0.0 ? 1.2 : 1.0);
										totalDmg += SpellDamage(castingSpell, stats) * (ragingDuration > 0.0 ? 1.2 : 1.0);
										MiasmaDuration = 24.0;
										currentMP -= 133;
										break;
									case "Shadow Flare":
										FlareMod = (ragingDuration > 0.0 ? 1.2 : 1.0);
										shadowFlareDuration = 30.0;
										currentMP -= 212;
										break;
									case "Ruin":
										totalDmg += SpellDamage("Ruin", stats) * (ragingDuration > 0.0 ? 1.2 : 1.0);
										currentMP -= 79;
										break;
                                    case "Ruin III":
                                        totalDmg += SpellDamage("Ruin III", stats) * (ragingDuration > 0.0 ? 1.2 : 1.0);
                                        currentMP -= 79;
                                        break;
								}
								GCDCount++;
								castingSpell = "";
								petActive = true;
							}
							if ((Bio2Duration - GCDSpeed(stats.speed) < 3.0 || (ragingDuration > 0.0 && Bio2Mod < 1.2)) && (ignoreResources || currentMP >= 186)) // Ok to overlap DoT times to keep active as long as we don't lose more than one tick
							{
								castingSpell = "Bio II";
								castComplete = currentTime + GCDSpeed(stats.speed);
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1);
							}
							else if ((MiasmaDuration - GCDSpeed(stats.speed) < 3.0 || (ragingDuration > 0.0 && MiasmaMod < 1.2)) && (ignoreResources || currentMP >= 133))
							{
								castingSpell = "Miasma";
								castComplete = currentTime + GCDSpeed(stats.speed);
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1);
							}
							else if ((BioDuration < 3.0 || (ragingDuration > 0.0 && BioMod < 1.2)) && (ignoreResources || currentMP >= 106))
							{
								GCDCount++;
								BioMod = (ragingDuration > 0.0 ? 1.2 : 1.0);
								animationDelay = animationDelayMax;
								currentMP -= 106;
								GCD = GCDSpeed(stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1);
								BioDuration = 18.0;
								castingSpell = "";
								petActive = true;
							}
							else if ((contagionRecast <= 0.0 && ragingDuration > 0.0) && (ignoreResources || currentMP >= 186))
							{
								GCDCount++;
								totalDmg += SpellDamage("Miasma II", stats) * (ragingDuration > 0.0 ? 1.2 : 1.0);
								currentMP -= 186;
								Miasma2Mod = (ragingDuration > 0.0 ? 1.2 : 1.0);
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1);
								Miasma2Duration = 15.0;
								castingSpell = "";
								petActive = true;
							}
							else if ((shadowFlareDuration - CastSpeed(3.0, stats.speed) < 3.0 || (ragingDuration > 0.0 && FlareMod < 1.2)) && (ignoreResources || currentMP >= 212))
							{
								animationDelay = animationDelayMax + (swiftRecast <= 0.0 ? animationDelayMax : 0); //add an additional 1/2 second of "animation delay" for placing the shadow flare, and another for possible swiftcase usage
								GCD = GCDSpeed(stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1) + animationDelayMax + (swiftRecast <= 0.0 ? animationDelayMax : 0);
								if (swiftRecast <= 0.0)
								{
									swiftRecast = 60.0;
									FlareMod = (ragingDuration > 0.0 ? 1.2 : 1.0);
									shadowFlareDuration = 30.0 + animationDelay; // fudge duration for swiftcast usage, we'll de-fudge in DoT tick calculation
									currentMP -= 212;
									GCDCount++;
									castingSpell = "";
									petActive = true;
								}
								else
								{
									castingSpell = "Shadow Flare";
									castComplete = currentTime + CastSpeed(3.0, stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1) + animationDelay;
								}
							}
							else if ((Bio2Duration > 0.0 && MiasmaDuration > 0.0 && BioDuration > 0.0 && shadowFlareDuration > 0.0) && ((aetherflowStacks == 0 && aetherflowRecast <= animationDelayMax) ||
								(festerRecast <= animationDelayMax && aetherflowStacks > 0) || ragingRecast <= animationDelayMax || SpurRecast <= animationDelayMax || RouseRecast <= animationDelayMax) && (ignoreResources || currentMP >= 133)) // use Ruin II if one of the off-GCDs should be used prior to the next GCD
							{
								GCDCount++;
								totalDmg += SpellDamage("Ruin II", stats) * (ragingDuration > 0.0 ? 1.2 : 1.0);
								currentMP -= 133;
								GCD = GCDSpeed(stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1);
								castingSpell = "";
								animationDelay = animationDelayMax;
								petActive = true;
							}
							else if ((Bio2Duration > 0.0 && MiasmaDuration > 0.0 && BioDuration > 0.0 && shadowFlareDuration > 0.0) && (ignoreResources || currentMP >= 79)) // if nothing is coming off recast soon, use Ruin to conserve MP
							{
								castingSpell = "Ruin";
								castComplete = currentTime + GCDSpeed(stats.speed);
								animationDelay = animationDelayMax;
								GCD = GCDSpeed(stats.speed) * (speedBoostDuration > 0.0 ? 0.8 : 1);
								petActive = true;
							}
						}

						if (animationDelay <= 0.0 && GCD > animationDelayMax && string.IsNullOrWhiteSpace(castingSpell)) // If we're not actively casting anything, but we have time, try for Aetherflow/Fester/Rouse/Spur
						{
							if (aetherflowStacks > 0 && festerRecast <= 0 && Bio2Duration > 0 && MiasmaDuration > 0 && BioDuration > 0 && (ignoreResources || currentMP >= mpMax / 4 || ragingDuration > 0.0))
							{ // only fester if we have stacks, it's off recast and all DoTs are up
								totalDmg += SpellDamage("Fester", stats) * (ragingDuration > 0.0 ? 1.2 : 1.0);
								festerRecast = 10.0;
								aetherflowStacks--;
								animationDelay = animationDelayMax;
								petActive = true;
							}
							else if (energyDrainRecast <= 0.0 && (aetherflowStacks > 1 || (aetherflowStacks > 0 && aetherflowRecast < ragingRecast)) && currentMP < mpMax / 4 && !ignoreResources)
							{ // Energy Drain if we're low, but hold onto a stack for Fester unless aetherflow will be back before raging strikes
								totalDmg += SpellDamage("Energy Drain", stats) * (ragingDuration > 0.0 ? 1.2 : 1.0);
								currentMP = Math.Min(mpMax, currentMP + 266);
								energyDrainRecast = 3.0;
								aetherflowStacks--;
								animationDelay = animationDelayMax;
								petActive = true;
							}
							else if (aetherflowStacks == 0 && aetherflowRecast <= 0)
							{
								// MP recovery
								currentMP = (int)Math.Min(mpMax, currentMP + mpMax * .2);
								aetherflowStacks = aetherflowMax;
								aetherflowRecast = 60.0;
								animationDelay = animationDelayMax;
							}
							else if (RouseRecast <= 0)
							{
								RouseDuration = 20.0;
								RouseRecast = 90.0;
								animationDelay = animationDelayMax;
							}
							else if (SpurRecast <= 0)
							{
								SpurDuration = 20.0;
								SpurRecast = 120.0;
								animationDelay = animationDelayMax;
							}
						}
					}

					// pet actions
					if (currentTime >= petCastComplete && !string.IsNullOrWhiteSpace(petSpell))
					{
						totalDmg += SpellDamage(petSpell, stats) * (1 + (RouseDuration > 0 ? 0.4 : 0) + (SpurDuration > 0 ? 0.4 : 0));
						if ((double)RNG.Next(100000) / 1000.0 <= Common.CalculateCritRate(stats.crit, 1) * 100.0)
						{
							if (RNG.Next(10) < 2)
							{
								speedBoostDuration = 8.0;
							}
						}
						petSpell = "";
					}
					else if (petGCD <= 0.0 && petActive && string.IsNullOrWhiteSpace(petSpell) && petAnimationDelay <= 0.0)
					{
						petSpell = "Wind Blade";
						petCastComplete = currentTime + CastSpeed(1.0, stats.speed);
						petGCD = CastSpeed(3.0, stats.speed);
					}
					if (petAnimationDelay <= 0.0 && string.IsNullOrWhiteSpace(petSpell))
					{
						// contagion if all DoTs are up, hold if raging strikes will be back before contagion's recast is up
						// also make sure if raging strikes is up that we had the chance to get all DoTs up, including Miasma2
						if (contagionRecast <= 0.0 && (Bio2Duration > 0.0 && MiasmaDuration > 0.0 && BioDuration > 0.0 && Miasma2Duration > 0.0) && ragingRecast > 60.0 &&
							(ragingDuration > 0.0 ? (Bio2Mod > 1.0 && MiasmaMod > 1.0 && BioMod > 1.0 && Miasma2Mod > 1.0) : true))
						{
							contagionRecast = 60.0;
							Bio2Duration += 15.0;
							MiasmaDuration += 15.0;
							Miasma2Duration += 15.0;
							BioDuration += 15.0;
							petGCD = animationDelayMax;
						}
						//Aerial Slash, not on pet GCD, instant 30s recast, don't mess with petGCD though
						/*else if (aerialSlashRecast <= 0.0 && petGCD > animationDelayMax)
						{
							totalDmg += SpellDamage("Aerial Slash", stats) * (1 + (RouseDuration > 0 ? 0.4 : 0) + (SpurDuration > 0 ? 0.4 : 0));
							aerialSlashRecast = 30.0;
							petAnimationDelay = animationDelayMax;
						}*/
					}


					if (currentTime >= nextTick)
					{
						if (Bio2Duration >= 0)
						{ //35 pot
                            totalDmg += SpellDamage("Bio2DoT", stats, Bio2Mod);
						}
						if (MiasmaDuration >= 0)
						{ //35 pot
                            totalDmg += SpellDamage("MiasmaDoT", stats, MiasmaMod);
						}
						if (Miasma2Duration >= 0)
						{ //10 pot
                            totalDmg += SpellDamage("Miasma2DoT", stats, Miasma2Mod);
						}
						if (BioDuration >= 0)
						{ // 40 pot
                            totalDmg += SpellDamage("BioDoT", stats, BioMod);
						}
						if (shadowFlareDuration >= 0 && shadowFlareDuration <= 30.0)
						{ // 25 pot
                            totalDmg += SpellDamage("FlareDoT", stats, FlareMod);
						}
						currentMP = Math.Min(currentMP + (int)(mpMax * 0.02), mpMax);
						nextTick = GetNextServerTick(nextTick);
					}
				} while (currentTime <= SimMinutes * 60);
			}
			GCDCount /= SimIterations;
            simTime = 0;
			return totalDmg;
		}

		private double SpellDamage(string spellName, Statistics stats, double damageBuff = 1.0)
		{
			double potency;
			switch (spellName)
			{
				case "Miasma2DoT":
					potency = 10;
					break;
				case "Aerial Slash":
					potency = 90;
					break;
				case "Wind Blade":
					potency = 100;
					break;
				case "Miasma":
				case "Miasma II":
					potency = 20;
					break;
				case "MiasmaDoT":
				case "Bio2Dot":
					potency = 35;
					break;
				case "BioDoT":
					potency = 40;
					break;
				case "FlareDoT":
					potency = 25;
					break;
				case "Ruin":
				case "Ruin II":
					potency = 80;
					break;
				case "Fester":
					potency = 300;
					break;
				case "Energy Drain":
					potency = 150;
					break;
                case "Ruin III":
                    potency = 120;
                    break;
                case "Painflare":
                    potency = 200;
                    break;
                case "Deathflare":
                    potency = 400;
                    break;
				default:
					potency = 0.0;
					break;
			}

            damageBuff *= 1.3; // Maim and Mend
            damageBuff *= (spellName.Contains("DoT") ? Common.CalculateDoTSpeedScalar(stats.speed) : 1);
			
            baseSkillDmg = Common.CalculateDamage(stats, true, damageBuff); //Maim and Mend

			double spellDamage = baseSkillDmg * potency * 0.01005;
			return spellDamage;
		}
	}
}
