using CoinPokerCommonLib;
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
    /// <summary>
    /// Interaction logic for Card.xaml
    /// </summary>
    public partial class Chips : UserControl
    {
        public double chips = 0;

        private double[] numbers = { 5000, 2000, 1000, 500, 250, 100, 50, 20, 10, 5, 1, 0.5, 0.25, 0.05, 0.01 };

        private int numColumns = 0;
        private int stackHeight = 0;

        public GameControls.Seat seat;

        public Chips(GameControls.Seat seat)
        {
            InitializeComponent();
            this.seat = seat;
        }

        public void SetChips(decimal chips)
        {
            this.ChipsStack.Children.Clear();
            this.AddChips(chips);
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public void AddChips(decimal chips)
        {
            if (chips == 0) return;

            this.chips += (double)chips;

            //Dodajemy do planszy
            int i = 0;
            double rest = (double)chips;

            while (rest > 0)
            {
                if (rest >= numbers[i])
                {
                    int x = (int)(rest / numbers[i]);
                    rest = (double)Math.Round(rest - numbers[i] * x, 2); //Odjęcie wypłaconych środków od reszty

                    //Tworzymy zetony
                    for (int num = 0; num <= x; num++) {
                        GameControls.Chip chip = new GameControls.Chip((decimal)numbers[i]);
                        chip.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                        chip.Margin = new Thickness(numColumns*15, 0, 0, stackHeight * 2);
                        this.ChipsStack.Children.Add(chip);

                        stackHeight++;
                    }
                }

                i++;
            }

            if (this.chips > 0)
            {
                ChipsStackAlt.Visibility = Visibility.Visible;
                ChipsStackAltText.Text = this.chips.ToString();
            }
            else
            {
                ChipsStackAlt.Visibility = Visibility.Hidden;
            }
        }

        public void Clear()
        {
            this.chips = 0;
            this.ChipsStack.Children.Clear();
        }
    }
}
