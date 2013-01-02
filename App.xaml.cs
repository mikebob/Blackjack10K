using System.Windows;

namespace Blackjack10K
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += (s, e) => { this.RootVisual = new Page(); };
            InitializeComponent();
        }
    }
}
