using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;

namespace FFXIVGearTracker
{
	public class ExportImport
	{
		public List<Item> gear;
		public List<Food> food;

		public ExportImport()
		{
			gear = new List<Item>();
			food = new List<Food>();
		}

		public bool Export(string filename, bool append = false)
		{
			try
			{
				object[] a = new object[2];
				if (append)
				{
					List<Item> tempGear = gear.ToList<Item>();
					List<Food> tempFood = food.ToList<Food>();
					Import(filename);
					foreach (Item i in tempGear)
					{
						if (!gear.Contains(i))
						{
							gear.Add(i);
						}
					}
					foreach (Food f in tempFood)
					{
						if (!food.Contains(f))
						{
							food.Add(f);
						}
					}
				}
				a[0] = gear;
				a[1] = food;
				Stream fs = File.Open(filename, FileMode.Create);
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(fs, a);
				fs.Close();
				return true;
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.ToString()); //Better error messages
				return false;
			}
		}

		public bool Import(string filename)
		{
			try
			{
				object[] a;
				Stream fs = File.Open(filename, FileMode.Open);
				BinaryFormatter formatter = new BinaryFormatter();
				a = formatter.Deserialize(fs) as object[];
				fs.Close();

				gear = (List<Item>)a[0];
				food = (List<Food>)a[1];

				foreach (Item i in gear)
				{
					if (!Common.gearDictionary.ContainsKey(i.name))
					{
						Common.gearDictionary.Add(i.name, i);
					}
				}
				foreach (Food f in food)
				{
					if (!Common.foodDictionary.ContainsKey(f.name))
					{
						Common.foodDictionary.Add(f.name, f);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.ToString());
				return false;
			}
		}
	}
}
