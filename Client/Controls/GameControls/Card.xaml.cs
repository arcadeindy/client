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
    public partial class Card : UserControl
    {
        public CardModel CardModel { get; set; }

        static int CARD_WIDTH = 162;
        static int CARD_HEIGHT = 226;

        public Card()
        {
            InitializeComponent();
        }

        public void SetCard(CardModel card)
        {
            this.CardModel = card;

            BitmapImage cardFrontImageBitmap = new BitmapImage(
                new Uri("pack://application:,,,/Unity.Assets;component/Assets/Game/Cards/CardsSpritesheet.png")
            );

            int x;
            int y;

            if (card != null)
            {
                x = 4 + ((int)card.Face * CARD_WIDTH) + (int)card.Face;

                int suitY = 0;
                if (card.Suit == CardModel.CardSuit.Hearts)
                    suitY = 1;
                if (card.Suit == CardModel.CardSuit.Diamonds)
                    suitY = 2;
                if (card.Suit == CardModel.CardSuit.Clubs)
                    suitY = 3;
                if (card.Suit == CardModel.CardSuit.Spades)
                    suitY = 4;
                y = 3 + (CARD_HEIGHT * suitY) + suitY;
            }
            else
            {
                x = 3;
                y = 1;
            }

            BitmapSource cardFrontBitmap = new CroppedBitmap(cardFrontImageBitmap, new Int32Rect(x, y, CARD_WIDTH, CARD_HEIGHT));
            CardFront.Source = cardFrontBitmap;
        }
    }
}
