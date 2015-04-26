using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.WinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bool dataLoaded = false;

                if (File.Exists(Properties.Settings.Default.SaveFile))
                {
                    dataLoaded = Common.Load(Properties.Settings.Default.SaveFile);
                }
                if (!dataLoaded)
                {
                    dataLoaded = FFXIVGearTracker.Common.Load(Properties.Settings.Default.SaveFile.Replace(".DAT", ".LEGACY"));
                    ConvertLegacyData();
                }
                if (!dataLoaded)
                {
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.DamageFormula))
                    {
                        Common.DamageFormula = Properties.Settings.Default.DamageFormula;
                    }
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.CritFormula))
                    {
                        Common.CritFormula = Properties.Settings.Default.CritFormula;
                    }
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.HealingFormula))
                    {
                        Common.HealingFormula = Properties.Settings.Default.HealingFormula;
                    }
                    Common.HighestTurn = Properties.Settings.Default.HighestTurn;
                    Common.GearTableVisible = Properties.Settings.Default.GearTableVisible;
                    Common.GearTablePoppedOut = Properties.Settings.Default.GearTablePoppedOut;
                    Common.VitPerSTR = Properties.Settings.Default.VitPerSTR;
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.AccuracyRequirements))
                    {
                        string[] weights = Properties.Settings.Default.AccuracyRequirements.Split(',');
                        for (int i = 0; i < weights.Length && i < Enum.GetValues(typeof(Job)).Length; i++)
                        {
                            int temReq;
                            if (int.TryParse(weights[i], out temReq))
                            {
                                if (temReq == 0)
                                {
                                    temReq = 341;
                                }
                                Common.accuracyRequirements[i] = temReq;
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.GearFile))
                    {
                        try
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(List<Item>));
                            FileStream fs = new FileStream(Properties.Settings.Default.GearFile, FileMode.Open);
                            List<Item> items = (List<Item>)ser.Deserialize(fs);
                            fs.Close();
                            foreach (Item i in items)
                            {
                                if (!Common.gearDictionary.Keys.Contains(i.name))
                                {
                                    Common.gearDictionary.Add(i.name, i);
                                }
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.FoodFile))
                    {
                        try
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(List<Food>));
                            FileStream fs = new FileStream(Properties.Settings.Default.FoodFile, FileMode.Open);
                            List<Food> food = (List<Food>)ser.Deserialize(fs);
                            fs.Close();
                            foreach (Food f in food)
                            {
                                if (!Common.foodDictionary.Keys.Contains(f.name))
                                {
                                    Common.foodDictionary.Add(f.name, f);
                                }
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.CharacterFile))
                    {
                        try
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(List<Character>));
                            FileStream fs = new FileStream(Properties.Settings.Default.CharacterFile, FileMode.Open);
                            List<Character> chars = (List<Character>)ser.Deserialize(fs);
                            fs.Close();
                            foreach (Character c in chars)
                            {
                                if (!Common.charDictionary.Keys.Contains(c.charName))
                                {
                                    Common.charDictionary.Add(c.charName, c);
                                }
                            }
                        }
                        catch { }
                    }
                }

                Application.Run(new MainForm());

                if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SaveFile))
                {
                    Properties.Settings.Default.SaveFile = AppDomain.CurrentDomain.BaseDirectory + @"FFXIVGearTracker.DAT";
                }
                Common.Save(Properties.Settings.Default.SaveFile);
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
            {
            }
        }

        private static void ConvertLegacyData()
        {
            Common.accuracyRequirements = FFXIVGearTracker.Common.accuracyRequirements;
            Common.AutoAttackDamageFormula = FFXIVGearTracker.Common.AutoAttackDamageFormula;
            Common.CritFormula = FFXIVGearTracker.Common.CritFormula;
            Common.DamageFormula = FFXIVGearTracker.Common.DamageFormula;
            Common.GearTablePoppedOut = FFXIVGearTracker.Common.GearTablePoppedOut;
            Common.GearTableVisible = FFXIVGearTracker.Common.GearTableVisible;
            Common.HealingFormula = FFXIVGearTracker.Common.HealingFormula;
            Common.HighestTurn = FFXIVGearTracker.Common.HighestTurn;
            Common.ParryFormula = FFXIVGearTracker.Common.ParryFormula;
            Common.SimulateWeights = FFXIVGearTracker.Common.SimulateWeights;
            Common.SpdReductionFormula = FFXIVGearTracker.Common.SpdReductionFormula;
            Common.speedBreakPoints = FFXIVGearTracker.Common.speedBreakPoints;
            Common.UseSpeedBreakPoint = FFXIVGearTracker.Common.UseSpeedBreakPoint;
            Common.VitPerSTR = FFXIVGearTracker.Common.VitPerSTR;

            try
            {
                foreach (KeyValuePair<string, FFXIVGearTracker.Character> charEntry in FFXIVGearTracker.Common.charDictionary)
                {
                    Common.charDictionary.Add(charEntry.Key, ConvertLegacyCharacter(charEntry.Value));
                }
                foreach (KeyValuePair<string, FFXIVGearTracker.Item> itemEntry in FFXIVGearTracker.Common.gearDictionary)
                {
                    Common.gearDictionary.Add(itemEntry.Key, ConvertLegacyItem(itemEntry.Value));
                }
                foreach (KeyValuePair<string, FFXIVGearTracker.Food> foodEntry in FFXIVGearTracker.Common.foodDictionary)
                {
                    Common.foodDictionary.Add(foodEntry.Key, ConvertLegacyFood(foodEntry.Value));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static Character ConvertLegacyCharacter(FFXIVGearTracker.Character oldC)
        {
            Character c = new Character();

            c.accuracyNeeds = oldC.accuracyNeeds;
            c.charName = oldC.charName;
            c.clearedTurn = oldC.clearedTurn;
            c.currentJob = (Job)(int)oldC.currentJob;
            c.relicTier = oldC.relicTier;
            c.tomeTier = oldC.tomeTier;
            c.ownedItems = oldC.ownedItems;

            c.ownedAccReqListA = oldC.ownedAccReqListA;
            c.ownedAccReqListB = oldC.ownedAccReqListB;

            for (int i = 0; i < c.accuracyNeeds.Length; i++)
            {
                c.currentDamage[i] = ConvertLegacyGearSet(oldC.currentDamage[i]);
                c.currentCoilFoodA[i] = ConvertLegacyGearSet(oldC.currentCoilFoodA[i]);
                c.currentCoilFoodB[i] = ConvertLegacyGearSet(oldC.currentCoilFoodB[i]);
                if (oldC.ownedDamage != null)
                {
                    c.ownedDamage[i] = ConvertLegacyGearSet(oldC.ownedDamage[i]);
                    c.ownedCoilFoodA[i] = ConvertLegacyGearSet(oldC.ownedCoilFoodA[i]);
                    c.ownedCoilFoodB[i] = ConvertLegacyGearSet(oldC.ownedCoilFoodB[i]);
                }
                else
                {
                    c.ownedDamage[i] = new GearSet();
                    c.ownedCoilFoodA[i] = new GearSet();
                    c.ownedCoilFoodB[i] = new GearSet();
                }
                c.progressionDamage[i] = ConvertLegacyGearSet(oldC.progressionDamage[i]);
                c.progressionCoilFoodA[i] = ConvertLegacyGearSet(oldC.progressionCoilFoodA[i]);
                c.progressionCoilFoodB[i] = ConvertLegacyGearSet(oldC.progressionCoilFoodB[i]);
                c.idealDamage[i] = ConvertLegacyGearSet(oldC.idealDamage[i]);
                c.idealCoilFoodA[i] = ConvertLegacyGearSet(oldC.idealCoilFoodA[i]);
                c.idealCoilFoodB[i] = ConvertLegacyGearSet(oldC.idealCoilFoodB[i]);
            }

            return c;
        }

        private static GearSet ConvertLegacyGearSet(FFXIVGearTracker.GearSet oldSet)
        {
            GearSet newSet = new GearSet();

            newSet.baseStats = ConvertLegacyStatistics(oldSet.baseStats);
            newSet.body = ConvertLegacyItem(oldSet.body);
            newSet.ears = ConvertLegacyItem(oldSet.ears);
            newSet.feet = ConvertLegacyItem(oldSet.feet);
            newSet.gearStats = ConvertLegacyStatistics(oldSet.gearStats);
            newSet.gearWeights = ConvertLegacyWeights(oldSet.gearWeights);
            newSet.hands = ConvertLegacyItem(oldSet.hands);
            newSet.head = ConvertLegacyItem(oldSet.head);
            newSet.leftRing = ConvertLegacyItem(oldSet.leftRing);
            newSet.legs = ConvertLegacyItem(oldSet.legs);
            newSet.mainHand = ConvertLegacyItem(oldSet.mainHand);
            newSet.meal = ConvertLegacyFood(oldSet.meal);
            newSet.neck = ConvertLegacyItem(oldSet.neck);
            newSet.offHand = ConvertLegacyItem(oldSet.offHand);
            newSet.rightRing = ConvertLegacyItem(oldSet.rightRing);
            newSet.totalStats = ConvertLegacyStatistics(oldSet.totalStats);
            newSet.totalTomeCost = oldSet.totalTomeCost;
            newSet.waist = ConvertLegacyItem(oldSet.waist);
            newSet.wrists = ConvertLegacyItem(oldSet.wrists);

            return newSet;
        }

        private static StatWeights ConvertLegacyWeights(FFXIVGearTracker.StatWeights oldW)
        {
            StatWeights weights = new StatWeights();

            weights.blockRateWeight = oldW.blockRateWeight;
            weights.blockStrengthWeight = oldW.blockStrengthWeight;
            weights.critWeight = oldW.critWeight;
            weights.dtrWeight = oldW.dtrWeight;
            weights.parryWeight = oldW.parryWeight;
            weights.pieWeight = oldW.pieWeight;
            weights.spdWeight = oldW.spdWeight;
            weights.statWeight = oldW.statWeight;
            weights.vitWeight = oldW.vitWeight;
            weights.wdmgWeight = oldW.wdmgWeight;

            return weights;
        }

        private static Item ConvertLegacyItem(FFXIVGearTracker.Item oldItem)
        {
            Item newItem = new Item();

            if (oldItem.canEquip != null)
            {
                foreach (FFXIVGearTracker.Job j in oldItem.canEquip)
                {
                    newItem.canEquip.Add((Job)(int)j);
                }
            }

            newItem.equipSlot = (GearSlot)(int)oldItem.equipSlot;
            newItem.itemStats = ConvertLegacyStatistics(oldItem.itemStats);
            newItem.name = oldItem.name;
            newItem.relicTier = oldItem.relicTier;
            newItem.sourceTurn = oldItem.sourceTurn;
            newItem.tomeCost = oldItem.tomeCost;
            newItem.tomeTier = oldItem.tomeTier;
            newItem.twoHand = oldItem.twoHand;
            newItem.unique = oldItem.unique;

            return newItem;
        }

        private static Statistics ConvertLegacyStatistics(FFXIVGearTracker.Statistics oldStats)
        {
            Statistics newStats = new Statistics();

            newStats.acc = oldStats.acc;
            newStats.autoAttackDelay = oldStats.autoAttackDelay;
            newStats.blockRate = oldStats.blockRate;
            newStats.blockStrength = oldStats.blockStrength;
            newStats.crit = oldStats.crit;
            newStats.det = oldStats.det;
            newStats.itemLevel = oldStats.itemLevel;
            newStats.mainStat = oldStats.mainStat;
            newStats.parry = oldStats.parry;
            newStats.pie = oldStats.pie;
            newStats.speed = oldStats.speed;
            newStats.vit = oldStats.vit;
            newStats.weaponDamage = oldStats.weaponDamage;

            return newStats;
        }

        private static Food ConvertLegacyFood(FFXIVGearTracker.Food oldFood)
        {
            Food newFood = new Food();

            newFood.accCap = oldFood.accCap;
            newFood.accPct = oldFood.accPct;
            newFood.critCap = oldFood.critCap;
            newFood.critPct = oldFood.critPct;
            newFood.detCap = oldFood.detCap;
            newFood.detPct = oldFood.detPct;
            newFood.name = oldFood.name;
            newFood.parryCap = oldFood.parryCap;
            newFood.parryPct = oldFood.parryPct;
            newFood.pieCap = oldFood.pieCap;
            newFood.piePct = oldFood.piePct;
            newFood.speedCap = oldFood.speedCap;
            newFood.speedPct = oldFood.speedPct;
            newFood.vitCap = oldFood.vitCap;
            newFood.vitPct = oldFood.vitPct;

            return newFood;
        }
    }
}
