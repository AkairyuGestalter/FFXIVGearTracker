using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;

namespace FFXIVGearTracker
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

		[NonSerialized]
		public const string DefaultDamageFormula = "WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0010800 - 1";
		[NonSerialized]
		public static string DefaultAutoAttackDamageFormula = "(WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0022597 - 1) * (DELAY / 3.0)";
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
        public const bool DefaultSimulateWeights = false;
        [NonSerialized]
        public static bool DefaultUseSpeedBreakPoint = false;

        public static string DamageFormula = "WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0010800 - 1";
		public static string AutoAttackDamageFormula = "WD * 0.2714745 + STAT * 0.1006032 + (DTR - 202) * 0.0241327 + WD * STAT * 0.0036167 + WD * (DTR - 202) * 0.0022597 - 1) * (DELAY / 3.0)";
		public static string HealingFormula = "WD * (0.0114 * MND + 0.00145 * DTR + 0.3736) + (0.21 * MND) + (0.11 * DTR) + (0.00011316 * MND * DTR)";
        public static string CritFormula = "(0.0697 * CRIT - 18.437 + CRITPCTMOD) / 100.0";
		public static string SpdReductionFormula = ".001952 * 3 + (SPEED - (341 + 3)) * .000952";
		public static string ParryFormula = "((PARRY - 341) * 0.076 + 5.0) / 100.0";
		public static double VitPerSTR = 1.0;
        public static int HighestTurn = 5;

		public static bool GearTableVisible = false;
		public static bool GearTablePoppedOut = false;
        public static bool SimulateWeights = false;
        public static bool UseSpeedBreakPoint = false;

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
					for (int i = 0; i < a.Length/2; i++)
						if (field.Name == (a[i, 0] as string))
							field.SetValue(null, a[i, 1]);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
            finally
            {
                
            }
		}

		public static double CalculateCritRate(int crit, double critMod)
		{
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
		public static double CalculateDamage(Statistics stats, bool includeCrit = true)
		{
			return CalculateDamage(stats, 0.0, includeCrit);
		}
		public static double CalculateDamage(Statistics stats, double critmod, bool includeCrit = true)
		{
			return CalculateDamage(stats.weaponDamage, stats.mainStat, stats.det, stats.crit, critmod, includeCrit);
		}
        public static double CalculateDamage(int weaponDamage, int mainStat, int determination, int crit, double critMod, bool includeCrit = true)
        {
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
        public static double CalculateDamage(int weaponDamage, int mainStat, int determination, int critRate, bool includeCrit = true)
        {
            return CalculateDamage(weaponDamage, mainStat, determination, critRate, 0.0, includeCrit);
        }

		public static double CalculateSpdReduction(int speed)
		{
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
				double directHeal = (j == Job.Scholar? (4 + 3 + 1.5) / 3 : 1 ) * (double)healVal.Evaluate() * (1 + 0.5 * (double)critVal.Evaluate()); // Normalize 
				double shieldHeal = (3 + 3 * (double)critVal.Evaluate() + 1.5) * (double)healVal.Evaluate() * (1 + 0.5 * (double)critVal.Evaluate()) / 3;
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
			double avgBlockRate = (j == Job.Paladin ? (0.1 + (((int)(blockrate / 10)) * 0.01)): 0);
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
			double avgPctDamageReduction = (j == Job.Paladin? 0.2 : 0) + (0.2 * 20.0 / (j == Job.Warrior ? 90.0 : 120)) + (j == Job.Warrior ? 0.3 * 30.0 / 120.0 : 0) + 
				(j == Job.Warrior ? 0.2 * 3.0 / (GCDSpeed(spd) * 6.0) : 0) + (j == Job.Paladin ? 0.4 * 10.0 / 180.0 : 0) + (j == Job.Paladin ? 0.2 * 20.0 / 90.0 : 0) + (j == Job.Paladin ? 1 * 10.0 / 420.0 : 0);
			double effectiveHP = ((baseHP / (1 - avgPctDamageReduction)) / (1 - avgBlockRate * avgBlockAmt)) / (1 - avgParryRate * avgParryAmt);
			return effectiveHP;
		}

		private static double GCDSpeed(int speed)
		{
			return (double)Math.Round((decimal)(2.5 - (speed > 344 ? (.001952 * 3 + (speed - (341 + 3)) * .000952) : (speed - 341) * .001952)), 3);
		}
    }
}
