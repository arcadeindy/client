using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoinPoker
{
    public static class Preferences
    {
        public class ComboItem
        {
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public static event Action OnPreferencesChangedEvent;
        public static void OnPreferencesChanged(){
            if (Preferences.OnPreferencesChangedEvent != null)
                Preferences.OnPreferencesChangedEvent();
        }

        public static List<ComboItem> GetBackgroundList(){
            List<ComboItem> tableBackgroundList = new List<ComboItem>();
            tableBackgroundList.Add(new ComboItem()
            {
                Name = "Czarny metal",
            });
            tableBackgroundList.Add(new ComboItem()
            {
                Name = "Nowoczesny",
            });
            tableBackgroundList.Add(new ComboItem()
            {
                Name = "Biały metal",
            });
            return tableBackgroundList;
        }

        public static List<ComboItem> GetTableList()
        {
            List<ComboItem> tableList = new List<ComboItem>();
            tableList.Add(new ComboItem()
            {
                Name = "Niebieski",
            });
            tableList.Add(new ComboItem()
            {
                Name = "Błękitny",
            });
            tableList.Add(new ComboItem()
            {
                Name = "Ciemno zielony",
            });
            tableList.Add(new ComboItem()
            {
                Name = "Zielony",
            });
            tableList.Add(new ComboItem()
            {
                Name = "Czerwony",
            });

            return tableList;
        }
    }
}
