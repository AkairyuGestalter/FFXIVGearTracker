using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.Simulation
{
	class BLMSimulation : Simulation
	{
		public BLMSimulation()
			: base(Job.BlackMage)
		{
		}

		public override StatWeights RunSimulation(Statistics stats)
		{
			double baseDmg, wDmg, statDmg, critDmg, dtrDmg, spdNormDmg, spdDiffDmg, spdMinDmg, spdMinNormDmg, spdMinStatDmg;
			int spdNorm = 0, spdDiff = 0, spdMin = 0, spdMinNorm = 0, baseGCDs, spdNormGCDs, spdDiffGCDs, spdMinGCDs, spdMinNormGCDs;
			baseDmg = RunSimOnce(stats, out baseGCDs);
			wDmg = RunSimOnce(stats + new Statistics(0, 1, 0, 0, 0, 0, 0, 0));
			statDmg = RunSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, 0, 0));
			critDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 1, 0, 0));
            dtrDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 1, 0, 0, 0));
            do
            {
                spdMinNorm++;
                spdMinNormDmg = RunSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMinNorm, 0), out spdMinNormGCDs);
            } while (spdMinNormGCDs >= baseGCDs || spdMinNormDmg >= baseDmg);
            do
            {
                spdMin++;
                spdMinDmg = RunSimOnce(stats - new Statistics(0, 0, 0, 0, 0, 0, spdMin + spdMinNorm, 0), out spdMinGCDs);
            } while (spdMinGCDs >= spdMinNormGCDs || spdMinDmg >= spdMinNormDmg);
            spdMinStatDmg = RunSimOnce(stats + new Statistics(0, 0, 1, 0, 0, 0, -1 * (spdMin + spdMinNorm), 0));
            do
            {
                spdNorm++;
                spdNormDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdNorm, 0), out spdNormGCDs);
            } while (spdNormGCDs <= baseGCDs || spdNormDmg <= baseDmg);
			do
			{
				spdDiff++;
				spdDiffDmg = RunSimOnce(stats + new Statistics(0, 0, 0, 0, 0, 0, spdNorm + spdDiff, 0), out spdDiffGCDs);
            } while (spdDiffGCDs <= spdNormGCDs || spdDiffDmg <= spdNormDmg);
			
			double wdmgDelta = wDmg - baseDmg;
			double statDelta = statDmg - baseDmg;
			double critDelta = critDmg - baseDmg;
			double dtrDelta = dtrDmg - baseDmg;
            double spdDelta = spdDiffDmg - spdNormDmg;
            double spdMinDelta = baseDmg - spdMinDmg;
            double spdDiffMinDelta = spdDiffDmg - spdMinDmg;
            double spdMinStatDelta = spdMinStatDmg - spdMinDmg;

			double wdmgWeight = wdmgDelta / statDelta;
			double critweight = critDelta / statDelta;
			double dtrweight = dtrDelta / statDelta;
            double spdWeight = (spdDelta / statDelta) / (spdNorm + spdDiff);
            double spdMinWeight = (spdMinDelta / spdMinStatDelta) / (spdMin + spdMinNorm);
            double spdMinDiffWeight = (spdDiffMinDelta / spdMinStatDelta) / (spdMin + spdMinNorm + spdNorm + spdDiff);
            double avgSpdWeight = (spdWeight + spdMinWeight + spdMinDiffWeight) / 3.0;

			return new StatWeights(wdmgWeight, 1, dtrweight, critweight, avgSpdWeight);
		}

		protected override double RunSimOnce(Statistics stats, out int GCDCount, bool ignoreResources = false)
		{
			ResetCachedValues();
			double totalDmg = 0.0;
			GCDCount = 0;

            stats.mainStat = (int)(stats.mainStat * 1.03);

			for (int i = 0; i < SimIterations; i++)
			{
				RNG = new Random(i);

				BuffType mode = BuffType.None;
				bool nextSpell = false;
				int maxMP = (int)Math.Round((int)(stats.pie * 1.03) * 8.23 + 1662, 0); // SCH in party for sake of argument
				int MP = maxMP;
				double ThunderDuration = 0.0;
				double ThunderCloudDuration = 0.0;
				double FireStarterDuration = 0.0;
				double thunderDmgMod = 1.0;
				double swiftRecast = 0.0;
				double swiftDuration = 0.0;
				double ragingRecast = 0.0;
				double ragingDuration = 0.0;
				double modeDuration = 0.0;
				double convertRecast = 0.0;
				double transposeRecast = 0.0;
				double sureCastRecast = 0.0, quellingRecast = 0.0, lethargyRecast = 0.0, virusRecast = 0.0;
				double manaWallRecast = 0.0, manawardRecast = 0.0, apocRecast = 0.0, eye4eyeRecast = 0.0;
				bool regenThunder = false;
				bool regenBlizzard = false;
				double spelldamage = 0.0;

				double currentTime = 0.0;
				double GCD = 0.0;
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
                            spelldamage = SpellDamage(SpellType.ThunderDoT, stats) * thunderDmgMod;
                            totalDmg += spelldamage;
                            if (RNG.Next(100) < 5)
                            {
                                ThunderCloudDuration = 12.0;// = true;
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
							spelldamage = SpellDamage(castingSpell, stats, mode);
							totalDmg += spelldamage;
							if (castingSpell == SpellType.Flare)
							{
								MP = 0;
								mode = BuffType.AF3;
								modeDuration = 10.0;
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
								modeDuration = 10.0;
							}
							else if (castingSpell == SpellType.Blizzard3)
							{
								mode = BuffType.UI3;
								regenThunder = false;
								regenBlizzard = false;
								modeDuration = 10.0;
							}
							else if (castingSpell == SpellType.Blizzard)
							{
								if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
								{
									regenBlizzard = true;
									modeDuration = 10.0;
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
									modeDuration = 10.0;
								}
								else
								{
									mode = BuffType.None;
								}
							}
							else if (castingSpell == SpellType.Thunder || castingSpell == SpellType.Thunder2 || castingSpell == SpellType.Thunder3)
							{
								ThunderDuration = 18 + (castingSpell == SpellType.Thunder2 ? 3 : (castingSpell == SpellType.Thunder3 ? 6 : 0));
								if (mode == BuffType.UI3 || mode == BuffType.UI2 || mode == BuffType.UI)
								{
									regenThunder = true;
								}
							}
							else if (castingSpell == SpellType.Fire)
							{
								if (RNG.Next(10) < 4)
								{
									FireStarterDuration = 12.0;
								}
								if (mode == BuffType.AF || mode == BuffType.AF2 || mode == BuffType.AF3)
								{
									modeDuration = 10.0;
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
									modeDuration = 10.0;
								}
								else
								{
									mode = BuffType.None;
								}
							}
							GCDCount++;
							prevSpell = lastSpell;
							lastSpell = castingSpell;
							castingSpell = SpellType.None;
						}

						if (currentTime >= castComplete && GCD <= 0) // If we're done casting and the GCD is over, pick an action
						{
							if (ThunderCloudDuration > 0 && (ThunderCloudDuration < GCDSpeed(stats.speed) || ThunderDuration < GCDSpeed(stats.speed) || (ragingDuration > 0 && ragingDuration < GCDSpeed(stats.speed))))// ||
							{ // Use thunderclouds if thundercloud is about to fall off, thunder Dot is about to fall off, or raging strikes is about to fall off
								spelldamage = SpellDamage(SpellType.Thunder3, stats, ThunderCloudDuration > 0) * (ragingDuration > 0.0 ? 1.2 : 1);
								totalDmg += spelldamage;
								ThunderCloudDuration = 0.0;
								thunderDmgMod = (ragingDuration > 0.0 ? 1.2 : 1);
								if (ThunderDuration > 0)
								{
									double tempDuration = ThunderDuration - (nextTick - currentTime);
									if (tempDuration >= 0)
									{
										int tics = 1 + (int)(tempDuration / 3);
									}
								}
								ThunderDuration = 24;
								GCD = GCDSpeed(stats.speed);
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
								GCDCount++;
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
								castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
								GCD = GCDSpeed(stats.speed);
							}
							else if (mode == BuffType.UI)
							{
								if (!regenThunder && MP >= MPCost(SpellType.Thunder2))
								{
									castingSpell = SpellType.Thunder2;
									castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									GCD = GCDSpeed(stats.speed);
								}
								if (MP >= MPCost(SpellType.Fire3, mode) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, mode) > nextTick)
								{
									castingSpell = SpellType.Fire3;
									castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									GCD = GCDSpeed(stats.speed);
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
											SpellDamage(SpellType.Thunder2, stats) * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder2, mode) ? 1.2 : 1) +
												SpellDamage(SpellType.ThunderDoT, stats) * 7 * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder2, mode) ? 1.2 : 1) :
											SpellDamage(SpellType.Thunder, stats) * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder, mode) ? 1.2 : 1) +
												SpellDamage(SpellType.ThunderDoT, stats) * 6 * (ragingDuration >= CastSpeed(stats.speed, SpellType.Thunder, mode) ? 1.2 : 1)) -
									ThunderDuration / 3 * SpellDamage(SpellType.ThunderDoT, stats) * thunderDmgMod >
									SpellDamage(SpellType.Blizzard, stats, mode))
								{ // Thunder 1 or 2
									if (MP >= MPCost(SpellType.Thunder2) && !(MP > (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Thunder, mode) < nextTick))
									{
										castingSpell = SpellType.Thunder2;
										castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									}
									else if (MP >= MPCost(SpellType.Thunder, mode))
									{
										castingSpell = SpellType.Thunder;
										castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									}
									GCD = GCDSpeed(stats.speed);
								}
								else if ((maxMP - (MP - MPCost(SpellType.Scathe))) < (int)(maxMP * 0.62) && FireStarterDuration > 0 && currentTime + GCDSpeed(stats.speed) > nextTick + animationDelayMax)
								{ // Scathe
									spelldamage = SpellDamage(SpellType.Scathe, stats) * (ragingDuration > 0.0 ? 1.2 : 1);
									totalDmg += spelldamage;
									MP -= MPCost(SpellType.Scathe, mode);
									GCD = GCDSpeed(stats.speed);
									castingSpell = SpellType.None;
									prevSpell = lastSpell;
									lastSpell = SpellType.Scathe;
									nextSpell = true;
									animationDelay = animationDelayMax;
								}
								else if (FireStarterDuration < 0 && ((maxMP - MP) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Fire3, BuffType.UI3) > nextTick))
								{ // Fire 3
									castingSpell = SpellType.Fire3;
									castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									GCD = GCDSpeed(stats.speed);
								}
								else if (MP < (int)(maxMP * 0.62) || ((maxMP - (MP - MPCost(SpellType.Blizzard, BuffType.UI3))) < (int)(maxMP * 0.62) && currentTime + CastSpeed(stats.speed, SpellType.Blizzard, BuffType.UI3) < nextTick
									//&& ((SpellDamage(wdmg,intel,dtr,crit,SpellType.Blizzard,BuffType.UI3,false)+SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire3,BuffType.UI3,false))/(CastSpeed(stats.speed,SpellType.Blizzard,BuffType.UI3) + CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3))) > 
									//    (SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire3,BuffType.UI3,false)/(CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3) + Math.Max(0,nextTick - currentTime - CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3)))
									//+ ((SpellDamage(wdmg,intel,dtr,crit,SpellType.Fire,BuffType.AF3,false) * (CastSpeed(stats.speed, SpellType.Blizzard, BuffType.UI3) - Math.Max(0,nextTick - currentTime - CastSpeed(stats.speed,SpellType.Fire3,BuffType.UI3))))/Math.Pow(CastSpeed(stats.speed,SpellType.Fire,BuffType.AF3),2))
									//)
									))
								{ // Blizzard
									castingSpell = SpellType.Blizzard;
									castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									GCD = GCDSpeed(stats.speed);
								}
							}
							else // AF1 or AF3
							{
								if (mode == BuffType.AF3 && MP > MPCost(SpellType.Fire, mode) * 4 + MPCost(SpellType.Flare, mode) && ragingRecast <= 0 && convertRecast < GCDSpeed(stats.speed) * 4)
								{ // Raging Strikes
									ragingRecast = 180.0;
									ragingDuration = 20.0;
									animationDelay = animationDelayMax;
									if (FireStarterDuration > 0)
									{
										nextSpell = true;
									}
								}
								else if (MP < MPCost(SpellType.Fire, BuffType.AF3) && MP >= MPCost(SpellType.Flare) && swiftRecast <= 0 &&
									(ragingRecast > 0 && convertRecast < GCDSpeed(stats.speed) - animationDelayMax))
								{ // Swiftcast (for Flare)
									swiftRecast = 60.0;
									swiftDuration = 10.0;
									animationDelay = animationDelayMax;
								}
								else if (MP > MPCost(SpellType.Flare) && swiftDuration > 0 && convertRecast < GCDSpeed(stats.speed) - animationDelayMax)
								{ // Flare
									spelldamage = SpellDamage(SpellType.Flare, stats, mode) * (ragingDuration > 0.0 ? 1.2 : 1);
									totalDmg += spelldamage;
									MP = 0;
									animationDelay = animationDelayMax;
									GCD = GCDSpeed(stats.speed);
									swiftDuration = 0.0;
									if (FireStarterDuration > 0)
									{
										nextSpell = true;
									}
									castingSpell = SpellType.None;
									prevSpell = lastSpell;
									lastSpell = SpellType.Flare;
									GCDCount++;
									modeDuration = 10.0;
									mode = BuffType.AF3;
								}
								else if (FireStarterDuration > 0 && nextSpell)
								{ // Fire 3 (Instant)
									spelldamage = SpellDamage(SpellType.Fire3, stats, mode) * (ragingDuration > 0.0 ? 1.2 : 1);
									totalDmg += spelldamage;
									nextSpell = false;
									FireStarterDuration = 0;
									castingSpell = SpellType.None;
									prevSpell = lastSpell;
									lastSpell = SpellType.Fire3;
									GCDCount++;
									GCD = GCDSpeed(stats.speed);
									animationDelay = animationDelayMax;
									modeDuration = 10.0;
									mode = BuffType.AF3;
								}
								else if (MP > MPCost(SpellType.Fire, BuffType.AF3) + MPCost(SpellType.Blizzard3, BuffType.AF3))
								{ // Fire
									if (swiftDuration < 0)
									{
										castingSpell = SpellType.Fire;
										castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									}
									else
									{
										spelldamage = SpellDamage(SpellType.Fire, stats, mode) * (ragingDuration > 0.0 ? 1.2 : 1);
										totalDmg += spelldamage;
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
										GCDCount++;
										animationDelay = animationDelayMax;
										modeDuration = 10.0;
									}
									GCD = GCDSpeed(stats.speed);
								}
								else if (MP > MPCost(SpellType.Blizzard3, BuffType.AF3))
								{ // Blizzard 3
									castingSpell = SpellType.Blizzard3;
									castComplete = Math.Round(currentTime + CastSpeed(stats.speed, castingSpell, mode), 3);
									GCD = GCDSpeed(stats.speed);
								}
							}
						}
						// Off-GCD actions while not actively casting a spell
						if (castingSpell == SpellType.None)
						{
							if (MP == maxMP && FireStarterDuration > 0 && mode.ToString().Contains("UI"))
							{ // Transpose out of UI3 before tossing a firestarter
								transposeRecast = 12.0;
								mode = BuffType.AF;
								modeDuration = 10.0;
								animationDelay = animationDelayMax;
							}
							else if (mode == BuffType.AF3 && MP > MPCost(SpellType.Fire, mode) && lastSpell == SpellType.Fire3 && GCD > animationDelayMax && swiftRecast < 0 && convertRecast > 60 && FireStarterDuration < 0)
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
									modeDuration = 10.0;
									animationDelay = animationDelayMax;
									regenThunder = false;
									regenBlizzard = false;
								}
							}
						}
					}
				} while (currentTime <= SimMinutes * 60);
			}
			GCDCount /= SimIterations;
			return totalDmg;
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
			Flare,
			Blizzard,
			Blizzard3,
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
						return 53;
					}
					else if (mode == BuffType.AF3 || mode == BuffType.AF2)
					{
						return 26;
					}
					else
					{
						return 106;
					}
				case SpellType.Blizzard3:
					if (mode == BuffType.AF)
					{
						return 159;
					}
					else if (mode == BuffType.AF3 || mode == BuffType.AF2)
					{
						return 79;
					}
					else
					{
						return 319;
					}
				case SpellType.Fire:
					if (mode == BuffType.AF || mode == BuffType.AF3)
					{
						return 638;
					}
					else if (mode == BuffType.UI)
					{
						return 159;
					}
					else if (mode == BuffType.UI3 || mode == BuffType.UI2)
					{
						return 79;
					}
					else
					{
						return 319;
					}
				case SpellType.Fire3:
					if (mode == BuffType.AF || mode == BuffType.AF3)
					{
						return 1064;
					}
					else if (mode == BuffType.UI)
					{
						return 266;
					}
					else if (mode == BuffType.UI3 || mode == BuffType.UI2)
					{
						return 133;
					}
					else
					{
						return 532;
					}
				case SpellType.Flare:
					return 266;
				case SpellType.Scathe:
				case SpellType.Thunder:
					return 212;
				case SpellType.Thunder2:
					return 319;
				case SpellType.Thunder3:
					return 425;
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

		private double SpellDamage(SpellType spell, Statistics stats, bool ThunderCloud)
		{
			return SpellDamage(spell, stats, BuffType.None, ThunderCloud);
		}
		private double SpellDamage(SpellType spell, Statistics stats, BuffType mode = BuffType.None, bool ThunderCloud = false)
		{
			double potency;
			switch (spell)
			{
				case SpellType.Scathe:
					potency = 120; // average potency given 20% chance trait for +100 potency
					break;
				case SpellType.ThunderDoT:
					potency = 35;
					break;
				case SpellType.Thunder:
					potency = 30;
					break;
				case SpellType.Thunder2:
					potency = 50;
					break;
				case SpellType.Thunder3:
					potency = 60;
					if (ThunderCloud)
					{
						potency += (35 * 8);
					}
					break;
				case SpellType.Fire:
					potency = 170;
					if (mode == BuffType.AF3)
					{
						potency *= 1.8;
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
						potency *= 1.8;
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
						potency *= 1.8;
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
				case SpellType.Blizzard:
					potency = 170;
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
				default:
					potency = 0;
					break;
			}
			if (baseSkillDmg < 0)
			{
				baseSkillDmg = Core.Common.CalculateDamage(stats) * 1.3; //Magic and Mend II
			}
			double spellDamage = baseSkillDmg * potency / 100.0;
			return spellDamage;
		}

		private double CastSpeed(int speed, SpellType spell, BuffType mode)
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
			castTime = (double)Math.Round(castTime, 3);
			return castTime;
		}
	}
}
