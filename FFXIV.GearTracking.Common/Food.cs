using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV.GearTracking.Core
{
	[Serializable]
    public class Food
    {
        public string name;
        public double vitPct;
        public int vitCap;
        public double detPct;
        public int detCap;
        public double critPct;
        public int critCap;
        public double speedPct;
        public int speedCap;
        public double accPct;
        public int accCap;
		public double piePct;
		public int pieCap;
		public double parryPct;
		public int parryCap;

        public Food()
            : this("None", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
        {
        }
        public Food(string foodname, double vitmod, int vitmax, double accmod, int accmax, double detmod, int detmax, double critmod, int critmax, double spdmod, int spdmax, double piemod, int piemax, double parrymod, int parrymax)
        {
            name = foodname;
            vitPct = vitmod;
            vitCap = vitmax;
            accPct = accmod;
            accCap = accmax;
            detPct = detmod;
            detCap = detmax;
            critPct = critmod;
            critCap = critmax;
            speedPct = spdmod;
            speedCap = spdmax;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
