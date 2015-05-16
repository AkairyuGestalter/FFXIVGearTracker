using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FFXIV.GearTracking.Core
{
    [Serializable]
    public class Item : IComparable, INotifyPropertyChanged
    {
        public List<Job> canEquip;
        public string name;
        public bool unique;
        public bool twoHand;
        public int sourceTurn;
        public int tomeCost;
        public double tomeTier;
        public double relicTier;
        public GearSlot equipSlot;
        public Statistics itemStats;
        public ObservableCollection<Equippable> equipList;

        public ObservableCollection<Equippable> EquipList
        {
            get { return equipList; }
            set
            {
                equipList = value;
                OnPropertyChanged("EquipList");
                OnPropertyChanged("EquipString");
            }
        }
        public string EquipString
        {
            get
            {
                string ret = "";
                foreach (Equippable e in equipList)
                {
                    if (e.CanEquip)
                    {
                        ret += Common.GetJobDescription(e.JobName) + ",";
                    }
                }
                if (string.IsNullOrWhiteSpace(ret))
                {
                    ret = "None";
                }
                else
                {
                    ret = ret.Substring(0, ret.Length - 1);
                }
                return ret;
            }
        }
        public bool Owned
        {
            get
            {
                return Common.activeChar.ownedItems.Contains(this.name);
            }
            set
            {
                if (!Common.activeChar.ownedItems.Contains(this.name))
                {
                    Common.activeChar.ownedItems.Add(this.name);
                }
            }
        }
        public double Value
        {
            get { return itemStats.Value(Common.activeChar.currentWeights); }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public bool IsUnique
        {
            get { return unique; }
            set { unique = value; }
        }
        public bool IsTwoHand
        {
            get { return twoHand; }
            set { twoHand = value; }
        }
        public int SourceTurn
        {
            get { return sourceTurn; }
            set { sourceTurn = value; }
        }
        public int TomeCost
        {
            get { return tomeCost; }
            set { tomeCost = value; }
        }
        public double TomeTier
        {
            get { return tomeTier; }
            set { tomeTier = value; }
        }
        public double RelicTier
        {
            get { return relicTier; }
            set { relicTier = value; }
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
            if (obj is Item)
            {
                return name.Equals(((Item)obj).name);
            }
            return false;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
