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
    }
}
