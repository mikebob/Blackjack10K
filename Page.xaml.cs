using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Blackjack10K
{
    public partial class Page : UserControl
    {
        Deck d;
        Player dlr,plr;
        Visibility vi = Visibility.Visible, co = Visibility.Collapsed;
        double totalMoney=1000;
        bool isDouble=false;
        public Page(){
            InitializeComponent();
            Stand.Click += (s, e) => { DealerPlay(); };
            Dbl.Click += (s, e) => {
                isDouble = true;
                DealCard(true, d.Draw());
                if (plr.HasBust()) SetMoney(false, 2);
                else DealerPlay();
            };
            PlayerMoney.Text="of $"+totalMoney;
        }
        private void SetMoney(bool win, double mult){
            try {
                double g = double.Parse(Wager.Text) * mult;
                PlayerMoney.Text = "of $" + (totalMoney += (win) ? g : g * -1);
            } catch (Exception) { Wager.Text = "Invalid"; }
            Deal.Visibility = vi;
            Hit.Visibility = co;
            Stand.Visibility = co;
            Dbl.Visibility = co;
            Deal.Content = string.Format("You {0} Deal again?", (dlr.Hand.GetSumOfHand() == plr.Hand.GetSumOfHand()) ? "push." :
                win ? "win!":"lose.");
            Wager.IsEnabled=true;
        }
        private void DealCard(bool dealtToMe,Card c){
            string x = string.Format(CultureInfo.InvariantCulture, "<StackPanel xmlns='http://schemas.microsoft.com/client/2007' Margin='2' Orientation='Horizontal' Height='34' Width='50' Background='White'><TextBlock Text='{0}' FontSize='24' Margin='2,0,0,0' /><Image Source='img/{1}.png' Width='12' Height='12' Margin='0,4,0,0' VerticalAlignment='Top' /></StackPanel>",
                (int)c.FaceVal > 10 ? c.FaceVal.ToString().Substring(0,1) : ((int)c.FaceVal).ToString(), c.Suit.ToString());
            if (dealtToMe){
                PlayerCards.Children.Add((StackPanel)XamlReader.Load(x));
                plr.CurrentDeck=d;
                plr.Hit(c);
            }else{
                DealerCards.Children.Add((StackPanel)XamlReader.Load(x));
                dlr.CurrentDeck=d;
                dlr.Hit(c);
            }
        }
        private void Hit_Click(object sender, RoutedEventArgs e){
            Dbl.Visibility=co;
            DealCard(true, d.Draw());
            if (plr.HasBust())SetMoney(false,1);
        }
        public void DealerPlay(){
            double m=isDouble?2:1;
            if(dlr.Hand.GetSumOfHand()<17){
                DealCard(false, d.Draw());
                DealerPlay();
            }
            else if(dlr.HasBust())SetMoney(true,m);
            else if (dlr.Hand.GetSumOfHand() <= 21 && dlr.Hand.GetSumOfHand() >= 17){
                if (dlr.Hand.GetSumOfHand() > plr.Hand.GetSumOfHand()) SetMoney(false,m);
                else if (dlr.Hand.GetSumOfHand() == plr.Hand.GetSumOfHand()){
                    SetMoney(true,m);
                    SetMoney(false,m);
                }
                else SetMoney(true,m);
            }
            isDouble = false;
        }
        private void Deal_Click(object sender, RoutedEventArgs e){
            PlayerCards.Children.Clear();
            DealerCards.Children.Clear();

            Wager.IsEnabled = false;

            d = new Deck();
            d.Shuffle();

            dlr = new Player();
            dlr.NewHand();
            DealCard(false, d.Draw());

            plr = new Player();
            plr.NewHand();

            DealCard(true, d.Draw());
            DealCard(true, d.Draw());

            if (plr.HasBlackJack())SetMoney(true, 1.5);
            else
            {
                Deal.Visibility = co;
                Hit.Visibility = vi;
                Stand.Visibility = vi;
                Dbl.Visibility = vi;
            }
        }
    }
    public class Player{
        private List<Card> cards = new List<Card>();
        public BlackJackHand Hand { get; private set; }
        public Deck CurrentDeck { get; set; }
        public Player(){this.Hand = new BlackJackHand();}
        public BlackJackHand NewHand(){
            this.Hand = new BlackJackHand();
            return this.Hand;
        }
        public bool HasBlackJack() { return (Hand.GetSumOfHand() == 21); }
        public bool HasBust() { return (Hand.GetSumOfHand() > 21); }
        public void Hit(Card c){
            Hand.Cards.Add(c);
        }
    }
    public enum Suit { Diamonds, Spades, Clubs, Hearts }
    public enum FaceValue { Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14 }
    public class Card{
        private readonly Suit suit;
        private readonly FaceValue faceVal;
        public Suit Suit { get { return suit; } }
        public FaceValue FaceVal { get { return faceVal; } }
        public Card(Suit suit, FaceValue faceVal){
            this.suit = suit;
            this.faceVal = faceVal;
        }
    }
    public class Deck{
        protected List<Card> c = new List<Card>();
        public Card this[int position] { get { return (Card)c[position]; } }
        public Deck(){
            foreach (Suit s in GetEnumValues<Suit>()){
                foreach (FaceValue faceVal in GetEnumValues<FaceValue>())
                    c.Add(new Card(s, faceVal));
            }
        }
        public Card Draw(){
            Card card = c[0];
            c.RemoveAt(0);
            return card;
        }
        public void Shuffle(){
            Random random = new Random();
            for (int i = 0; i < c.Count; i++){
                int index1 = i;
                int index2 = random.Next(c.Count);
                SwapCard(index1, index2);
            }
        }
        private void SwapCard(int index1, int index2){
            Card card = c[index1];
            c[index1] = c[index2];
            c[index2] = card;
        }
        public static T[] GetEnumValues<T>(){
            var type = typeof(T);
            return (from field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    where field.IsLiteral
                    select (T)field.GetValue(type)).ToArray();
        }
    }
    public class Hand{
        protected List<Card> cards = new List<Card>();
        public int NumCards { get { return cards.Count; } }
        public List<Card> Cards { get { return cards; } }
    }
    public class BlackJackHand : Hand{
        public int GetSumOfHand(){
            int val=0,numAces=0;
            foreach (Card c in cards){
                if (c.FaceVal == FaceValue.Ace){
                    numAces++;
                    val+=11;
                }
                else if (c.FaceVal == FaceValue.Jack || c.FaceVal == FaceValue.Queen || c.FaceVal == FaceValue.King)
                    val += 10;
                else val += (int)c.FaceVal;
            }
            while(val>21&&numAces>0){
                val-=10;
                numAces--;
            }
            return val;
        }    
	}
}
