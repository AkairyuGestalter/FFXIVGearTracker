using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;

namespace FFXIV.GearTracking.Core
{
    public enum Job
    {
        Paladin,
        Warrior,
        Monk,
        Dragoon,
        Bard,
        BlackMage,
        WhiteMage,
        Summoner,
        Scholar,
        Ninja
    }
    public enum GearSlot
    {
        MainHand,
        OffHand,
        Head,
        Body,
        Hands,
        Waist,
        Legs,
        Feet,
        Neck,
        Ears,
        Wrists,
        Ring
    }
    [Serializable]
    public struct Statistics
    {
        public int weaponDamage;
        public double autoAttackDelay;
        public int blockRate;
        public int blockStrength;
        public int mainStat;
        public int vit;
        public int det;
        public int crit;
        public int speed;
        public int acc;
        public int itemLevel;
        public int pie;
        public int parry;

        public int MainStat
        {
            get { return mainStat; }
            set { mainStat = value; }
        }
        public int VIT
        {
            get { return vit; }
            set { vit = value; }
        }

        public Statistics(int iLvl, int dmg, int stat, int vitality, int determination, int critrate, int speedstat, int accuracy, int piety, int parrying, int blockrate, int blockstrength, double delay)
        {
            weaponDamage = dmg;
            mainStat = stat;
            vit = vitality;
            det = determination;
            crit = critrate;
            speed = speedstat;
            acc = accuracy;
            itemLevel = iLvl;
            pie = piety;
            parry = parrying;
            blockRate = blockrate;
            blockStrength = blockstrength;
            autoAttackDelay = delay;
        }

        public Statistics(int iLvl, int dmg, int stat, int vitality, int determination, int critrate, int speedstat, int accuracy, int piety, int parrying)
            : this(iLvl, dmg, stat, vitality, determination, critrate, speedstat, accuracy, piety, parrying, 0, 0, 0)
        {
        }
        public Statistics(int iLvl, int dmg, int stat, int vitality, int determination, int critrate, int speedstat, int accuracy)
            : this(iLvl, dmg, stat, vitality, determination, critrate, speedstat, accuracy, 0, 0, 0, 0, 0)
        {
        }

        public static Statistics operator +(Statistics a, Statistics b)
        {
            return new Statistics((a.itemLevel + b.itemLevel) / 2, a.weaponDamage + b.weaponDamage, a.mainStat + b.mainStat, a.vit + b.vit, a.det + b.det, a.crit + b.crit, a.speed + b.speed, a.acc + b.acc, a.pie + b.pie, a.parry + b.parry, a.blockRate + b.blockRate, a.blockStrength + b.blockStrength, a.autoAttackDelay + b.autoAttackDelay);
        }
        public static Statistics operator -(Statistics a, Statistics b)
        {
            return new Statistics((a.itemLevel - b.itemLevel) / 2, a.weaponDamage - b.weaponDamage, a.mainStat - b.mainStat, a.vit - b.vit, a.det - b.det, a.crit - b.crit, a.speed - b.speed, a.acc - b.acc, a.pie - b.pie, a.parry - b.parry, a.blockRate - b.blockRate, a.blockStrength - b.blockStrength, a.autoAttackDelay - b.autoAttackDelay);
        }
        public double Value(StatWeights weights)
        {
            return weaponDamage * weights.wdmgWeight + mainStat * weights.statWeight + det * weights.dtrWeight + crit * weights.critWeight + speed * weights.spdWeight + pie * weights.pieWeight + vit * weights.vitWeight + parry * weights.parryWeight + blockRate * weights.blockRateWeight + blockStrength * weights.blockStrengthWeight;
        }

        public override string ToString()
        {
            return "WDMG: " + weaponDamage + ", Stat: " + mainStat + ", Acc: " + acc + ", DET: " + det + ", Crit: " + crit + ", Speed: " + speed;
        }
    }
    public class Common
    {
        public static int[] accuracyRequirements = { 512, 512, 492, 492, 492, 471, 341, 492, 341, 492 };
        public static int[] speedBreakPoints = { 341, 442, 341, 341, 341, 341, 341, 341, 341, 495 };
        public static List<Item> allGear = new List<Item>();
        public static Dictionary<string, Item> gearDictionary = new Dictionary<string, Item>();
        public static List<Food> allFood = new List<Food>();
        public static Dictionary<string, Food> foodDictionary = new Dictionary<string, Food>();
        public static List<Character> allChars = new List<Character>();
        public static Dictionary<string, Character> charDictionary = new Dictionary<string, Character>();
        public static ObservableCollection<Item> gearDictWPF = new ObservableCollection<Item>();
        public static ObservableCollection<Character> charDictWPF = new ObservableCollection<Character>();
        public ObservableCollection<Item> GearDictionary
        {
            get
            {
                return gearDictWPF;
            }
            set
            {
                gearDictWPF = value;
            }
        }
        public ObservableCollection<Character> CharacterDictionary
        {
            get { return charDictWPF; }
            set { charDictWPF = value; }
        }
        public static Character activeChar;
        public Character ActiveCharacter
        {
            get { return activeChar; }
            set { activeChar = value; }
        }

        [NonSerialized]
        public const string DefaultDamageFormula = "WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0010800 - 1";
        [NonSerialized]
        public const string DefaultAutoAttackDamageFormula = "(WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0022597 - 1) * (DELAY / 3.0)";
        [NonSerialized]
        public const string DefaultHealingFormula = "WD * (0.0114 * MND + 0.00145 * DTR + 0.3736) + (0.21 * MND) + (0.11 * DTR) + (0.00011316 * MND * DTR)";
        [NonSerialized]
        public const string DefaultCritFormula = "(0.0697 * CRIT - 18.437 + CRITPCTMOD) / 100.0";
        [NonSerialized]
        public const string DefaultSpdReductionFormula = ".001952 * 3 + (SPEED - (341 + 3)) * .000952";
        [NonSerialized]
        public const string DefaultParryFormula = "((PARRY - 341) * 0.076 + 5.0) / 100.0";
        [NonSerialized]
        public const double DefaultVitPerSTR = 1.0;
        [NonSerialized]
        public const int DefaultHighestTurn = 13;
        [NonSerialized]
        public const double DefaultHighestRaid = 3.04;
        [NonSerialized]
        public const bool DefaultSimulateWeights = false;
        [NonSerialized]
        public const bool DefaultUseSpeedBreakPoint = false;

        public static string DamageFormula = "WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0010800 - 1";
        public static string AutoAttackDamageFormula = "WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0022597 - 1) * (DELAY / 3.0)";
        public static string HealingFormula = "WD * (0.0114 * MND + 0.00145 * DTR + 0.3736) + (0.21 * MND) + (0.11 * DTR) + (0.00011316 * MND * DTR)";
        public static string CritFormula = "(0.0697 * CRIT - 18.437 + CRITPCTMOD) / 100.0";
        public static string SpdReductionFormula = ".001952 * 3 + (SPEED - (341 + 3)) * .000952";
        public static string ParryFormula = "((PARRY - 341) * 0.076 + 5.0) / 100.0";
        public static double VitPerSTR = 1.0;
        public static int HighestTurn = 13;
        public static double HighestRaid = 3.04;

        public static bool GearTableVisible = false;
        public static bool GearTablePoppedOut = false;
        public static bool SimulateWeights = false;
        public static bool UseSpeedBreakPoint = false;

        public Common()
        {
        }

        public static bool Save(string filename)
        {
            try
            {
                FieldInfo[] fields = typeof(Common).GetFields(BindingFlags.Static | BindingFlags.Public);
                int nonSerCount = 0;
                foreach (FieldInfo field in fields)
                {
                    if (field.IsNotSerialized)
                    {
                        nonSerCount++;
                    }
                }
                object[,] a = new object[fields.Length - nonSerCount, 2];
                int i = 0;
                foreach (FieldInfo field in fields)
                {
                    if (!field.IsNotSerialized)
                    {
                        a[i, 0] = field.Name;
                        a[i, 1] = field.GetValue(null);
                        i++;
                    }
                }
                if (File.Exists(filename))
                {
                    File.Copy(filename, filename + ".bak", true);
                }
                Stream f = File.Open(filename, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(f, a);
                f.Close();
                return true;
            }
            catch (Exception ex)
            {
                if (File.Exists(filename + ".bak"))
                {
                    File.Copy(filename + ".bak", filename, true);
                }
                return false;
            }
        }

        public static bool Load(string filename)
        {
            Stream f;
            try
            {
                FieldInfo[] fields = typeof(Common).GetFields(BindingFlags.Static | BindingFlags.Public);
                object[,] a;
                f = File.Open(filename, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                a = formatter.Deserialize(f) as object[,];
                f.Close();
                //if (a.GetLength(0) != fields.Length - ) return false;

                foreach (FieldInfo field in fields)
                    for (int i = 0; i < a.Length / 2; i++)
                        if (field.Name == (a[i, 0] as string))
                            field.SetValue(null, a[i, 1]);

                if (gearDictWPF.Count == 0 && gearDictionary.Count > 0)
                {
                    foreach (KeyValuePair<string, Item> i in gearDictionary)
                    {
                        if (i.Value.tomeTier > 3 && i.Value.itemStats.itemLevel < 140)
                        {
                            i.Value.tomeTier = (i.Value.tomeTier / 10 + 2);
                        }
                        gearDictWPF.Add(i.Value);
                    }
                }
                if (charDictWPF.Count == 0 && charDictionary.Count > 0)
                {
                    foreach (KeyValuePair<string, Character> c in charDictionary)
                    {
                        charDictWPF.Add(c.Value);
                    }
                }
                if (activeChar == null && charDictWPF.Count > 0)
                {
                }
                foreach (KeyValuePair<string, Item> i in gearDictionary)
                {
                    if (i.Value.tomeTier > 3 && i.Value.itemStats.itemLevel < 140)
                    {
                        i.Value.tomeTier = (i.Value.tomeTier / 10 + 2);
                    }
                    if (i.Value.sourceRaid == 0 && i.Value.sourceTurn > 0)
                    {
                        i.Value.sourceRaid = 2 + (double)i.Value.sourceTurn / 100.0;
                    }
                }
                foreach (Item i in gearDictWPF)
                {
                    if (i.tomeTier > 3 && i.itemStats.itemLevel < 140)
                    {
                        i.tomeTier = (i.tomeTier / 10 + 2);
                    }
                    if (i.sourceRaid == 0 && i.sourceTurn > 0)
                    {
                        i.sourceRaid = 2 + (double)i.sourceTurn / 100.0;
                    }
                }
                foreach (KeyValuePair<string, Character> c in charDictionary)
                {
                    if (c.Value.clearedRaid == 0 && c.Value.clearedTurn > 0)
                    {
                        c.Value.clearedRaid = 2.0 + c.Value.clearedTurn / 100.0;
                    }
                }
                foreach (Character c in charDictWPF)
                {
                    if (c.clearedRaid == 0 && c.clearedTurn > 0)
                    {
                        c.clearedRaid = 2.0 + c.clearedTurn / 100.0;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
            }
            return true;
        }

        public static double CalculateDoTSpeedScalar(int speed)
        {
            return 1 + (speed - 354) / 7722.0;
        }
        public static double CalculateCritBonus(int crit)
        {
            return (crit - 354) / (858.0 * 5.0) + 1.45;
        }
        public static double CalculateCritRate(int crit, double critMod)
        {
            return (crit - 354) / (858.0 * 5.0) + 0.05 + critMod / 100.0; // level 60
            try
            {
                string critForm = CritFormula.Replace("CRITPCTMOD", critMod.ToString()).Replace("CRIT", crit.ToString());
                Expression critVal = new Expression(critForm);
                return (double)critVal.Evaluate();
            }
            catch
            {
                return 0.0;
            }
        }
        public static double CalculateAutoAttackDamage(Statistics stats, bool includeCrit = true)
        {
            return CalculateAutoAttackDamage(stats, 0.0, includeCrit);
        }
        public static double CalculateAutoAttackDamage(Statistics stats, double critMod, bool includeCrit = true)
        {
            return CalculateAutoAttackDamage(stats.weaponDamage, stats.mainStat, stats.det, stats.autoAttackDelay, stats.crit, critMod, includeCrit);
        }
        public static double CalculateAutoAttackDamage(int weaponDamage, int mainStat, int determination, double delay, int crit, double critMod, bool includeCrit = true)
		{
            return (((weaponDamage * delay / 3.0) / 33.04 + 1) * (mainStat / 6.92) * (determination / 6715.0 + 1) * 1 / delay) * (includeCrit ? (1 - CalculateCritRate(crit, critMod) + CalculateCritBonus(crit) * CalculateCritRate(crit, critMod)) : 1); //level 60
			try
			{
				string damageForm = AutoAttackDamageFormula.Replace("WD", weaponDamage.ToString()).Replace("STAT", mainStat.ToString()).Replace("DTR", determination.ToString()).Replace("DELAY", delay.ToString());
				Expression damageVal = new Expression(damageForm);
				return (double)damageVal.Evaluate() * (1 + (includeCrit ? 0.5 * CalculateCritRate(crit, critMod) : 0));
			}
			catch
			{
				return 0.0;
			}
		}
        public static double CalculateDamage(Statistics stats, bool includeCrit = true, double damageBonus = 1.0)
        {
            return CalculateDamage(stats, 0.0, includeCrit, damageBonus);
        }
        public static double CalculateDamage(Statistics stats, double critmod, bool includeCrit = true, double damageBonus = 1.0)
        {
            return CalculateDamage(stats.weaponDamage, stats.mainStat, stats.det, stats.crit, critmod, includeCrit, damageBonus);
        }
        public static double CalculateDamage(int weaponDamage, int mainStat, int determination, int crit, double critMod, bool includeCrit = true, double damageBonus = 1.0)
        {
            //return ((weaponDamage / 25.0 + 1) * (mainStat / 9.0) * (determination / 7290.0 + 1) * damageBonus) * (includeCrit ? 1 - CalculateCritRate(crit, critMod) + CalculateCritBonus(crit) * CalculateCritRate(crit, critMod) : 1); // level 60 DRG
            return ((weaponDamage * 0.0403757 + 1) * (mainStat * 0.1122727 - 1.9037031) * (determination * 0.0001331 + 1) * damageBonus) * (includeCrit ? 1 - CalculateCritRate(crit, critMod) + CalculateCritBonus(crit) * CalculateCritRate(crit, critMod) : 1); // level 60 BLM
            try
            {
                string damageForm = DamageFormula.Replace("WD", weaponDamage.ToString()).Replace("STAT", mainStat.ToString()).Replace("DTR", determination.ToString());
                Expression damageExpr = new Expression(damageForm);
                double damageVal = (double)damageExpr.Evaluate();
                double critVal = 1 + (includeCrit ? 0.5 * CalculateCritRate(crit, critMod) : 0);
                return damageVal * critVal;
            }
            catch
            {
                return 0.0;
            }
        }
        public static double CalculateDamage(int weaponDamage, int mainStat, int determination, int critRate, bool includeCrit = true, double damageBonus = 1.0)
        {
            return CalculateDamage(weaponDamage, mainStat, determination, critRate, 0.0, includeCrit, damageBonus);
        }

        public static double CalculateSpdReduction(int speed)
        {
            return ((speed - 354) / 2641.0); // level 60
            try
            {
                string speedForm = SpdReductionFormula.Replace("SPEED", speed.ToString());
                Expression speedVal = new Expression(speedForm);
                return (double)speedVal.Evaluate();
            }
            catch
            {
                return 0.0;
            }
        }

        public static double CalculateHealing(Job j, int weaponDamage, int mind, int determination, int critRate, double critMod)
        {
            try
            {
                string healForm = HealingFormula.Replace("WD", weaponDamage.ToString()).Replace("MND", mind.ToString()).Replace("DTR", determination.ToString());
                string critForm = CritFormula.Replace("CRITPCTMOD", critMod.ToString()).Replace("CRIT", critRate.ToString());
                Expression healVal = new Expression(healForm);
                Expression critVal = new Expression(critForm);
                double directHeal = (j == Job.Scholar ? (4 + 3 + 1.5) / 3 : 1) * (double)healVal.Evaluate() * (1 + CalculateCritBonus(critRate) * (double)critVal.Evaluate()); // Normalize 
                double shieldHeal = (3 + 3 * (double)critVal.Evaluate() + 1.5) * (double)healVal.Evaluate() * (1 + CalculateCritBonus(critRate) * (double)critVal.Evaluate()) / 3;
                return (directHeal + (j == Job.Scholar ? shieldHeal : 0)) / (j == Job.Scholar ? 2 : 1);
            }
            catch
            {
                return 0.0;
            }
        }
        public static double CalculateHealing(Job j, int weaponDamage, int mind, int determination, int critRate)
        {
            return CalculateHealing(j, weaponDamage, mind, determination, critRate, 0.0);
        }

        public static double CalculateEHP(Job j, int vit, int str, int parry, int blockrate, int blockstrength, int spd)
        {
            int baseHP = (int)(((int)(vit * 14.5) - (j == Job.Paladin ? 804 : 889)) * (j == Job.Warrior ? 1.25 : 1));
            double avgBlockRate = (j == Job.Paladin ? (0.1 + (((int)(blockrate / 10)) * 0.01)) : 0);
            avgBlockRate = avgBlockRate * (1 - 15.0 / 180.0) + avgBlockRate * 1.6 * 15.0 / 180.0;
            double avgBlockAmt = 0.1 + (((int)(blockstrength / 10)) * 0.01);
            string parryForm = ParryFormula.Replace("PARRY", parry.ToString());
            Expression parryVal = new Expression(parryForm);
            double avgParryRate = (double)parryVal.Evaluate() * (1 - avgBlockRate);
            int tempStr = str - 242;
            double avgParryAmt = 0.1 + tempStr * 0.0004;
            //double effectiveHP = (baseHP * 1 / (1 - (avgBlockRate * avgBlockAmt + avgParryRate * avgParryAmt + (j == Job.Paladin ? 0.2 : 0))));
            //Shield Oath + Foresight + Vengeance + 
            //	Inner Beast + Sentinel + Rampart + Hallowed
            double avgPctDamageReduction = (j == Job.Paladin ? 0.2 : 0) + (0.2 * 20.0 / (j == Job.Warrior ? 90.0 : 120)) + (j == Job.Warrior ? 0.3 * 30.0 / 120.0 : 0) +
                (j == Job.Warrior ? 0.2 * 3.0 / (GCDSpeed(spd) * 6.0) : 0) + (j == Job.Paladin ? 0.4 * 10.0 / 180.0 : 0) + (j == Job.Paladin ? 0.2 * 20.0 / 90.0 : 0) + (j == Job.Paladin ? 1 * 10.0 / 420.0 : 0);
            double effectiveHP = ((baseHP / (1 - avgPctDamageReduction)) / (1 - avgBlockRate * avgBlockAmt)) / (1 - avgParryRate * avgParryAmt);
            return effectiveHP;
        }

        private static double GCDSpeed(int speed)
        {
            return (double)Math.Round((decimal)(2.50256 - CalculateSpdReduction(speed)));
        }
    }
}
