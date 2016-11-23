using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FFXIV.GearTracking.Core
{

    [Serializable]
    public class Item : IComparable//,INotifyPropertyChanged
    {
        public List<Job> canEquip;
        public string name;
        public bool unique;
        public bool twoHand;
        public int sourceTurn;
        public double sourceRaid;
        public int tomeCost;
        public double tomeTier;
        public double relicTier;
        public GearSlot equipSlot;
        public Statistics itemStats;

        public List<Job> CanEquip
        {
            get { return canEquip; }
            set { canEquip = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public GearSlot EquipSlot
        {
            get { return equipSlot; }
            set { equipSlot = value; }
        }
        public Statistics ItemStats
        {
            get { return itemStats; }
            set { itemStats = value; }
        }

        public Item()
        {
            itemStats = new Statistics();
            canEquip = new List<Job>();
        }

        public Item(string itemName)
            : this()
        {
            name = itemName;
        }

        public override string ToString()
        {
            return name;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Item))
            {
                return -1;
            }
            if (itemStats.itemLevel > ((Item)obj).itemStats.itemLevel)
            {
                return -1;
            }
            else if (itemStats.itemLevel < ((Item)obj).itemStats.itemLevel)
            {
                return 1;
            }
            else
            {
                return name.CompareTo(((Item)obj).name);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Item))
            {
                return false;
            }
            else
            {
                return name.Equals(((Item)obj).name);
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Item a, Item b)
        {
            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(Item a, Item b)
        {
            return a.GetHashCode() != b.GetHashCode();
        }

        /*public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }*/
    }
}
