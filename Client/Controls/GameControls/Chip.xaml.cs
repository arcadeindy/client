using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoinPoker.GameControls
{
    public static class ChipColorDatabase
    {
        public class ChipColor
        {
            public decimal value;
            public string color;
            public ChipColor(decimal value, string color)
            {
                this.value = value;
                this.color = color;
            }
        }

        public static List<ChipColor> colorChips { get; set; }

        private static double[] numbers = { 5000, 2000, 1000, 500, 250, 100, 50, 20, 10, 5, 1, 0.5, 0.25, 0.05, 0.01 };

        static ChipColorDatabase()
        {
            colorChips = new List<ChipColor>();
            colorChips.Add(new ChipColor(0.01m, "#302F11"));
            colorChips.Add(new ChipColor(0.05m, "#4D4B26"));
            colorChips.Add(new ChipColor(0.25m, "#9E7A2B"));
            colorChips.Add(new ChipColor(0.5m,  "#7A9E2B"));
            colorChips.Add(new ChipColor(1, "#2B9E40"));
            colorChips.Add(new ChipColor(5, "#2B9E89"));
            colorChips.Add(new ChipColor(10, "#2B6A9E"));
            colorChips.Add(new ChipColor(20, "#3E2B9E"));
            colorChips.Add(new ChipColor(50, "#852B9E"));
            colorChips.Add(new ChipColor(100, "#F71B1F"));
            colorChips.Add(new ChipColor(250, "#F71BCB"));
            colorChips.Add(new ChipColor(500, "#ECF71B"));
            colorChips.Add(new ChipColor(1000, "#1BF7EC"));
            colorChips.Add(new ChipColor(2000, "#878787"));
            colorChips.Add(new ChipColor(5000, "#1A1A1A"));
        }

        public static Color GetChipColor(decimal value)
        {
            var colorHex = colorChips.FirstOrDefault(c => c.value == value).color;
            return (Color)ColorConverter.ConvertFromString(colorHex);
        }
    }

    /// <summary>
    /// Interaction logic for Chip.xaml
    /// </summary>
    public partial class Chip : UserControl
    {
        public Chip(decimal chip)
        {
            InitializeComponent();
            string chipName;

            (this.Resources["ChipColor"] as SolidColorBrush).Color = (Color)ChipColorDatabase.GetChipColor(chip);

            DataContext = this;
        }
    }
}
