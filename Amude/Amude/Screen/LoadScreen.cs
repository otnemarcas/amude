using System.Windows.Forms;
using Amude.Core;

namespace Amude.Screen
{
    public partial class LoadScreen : Form
    {
        private int value;

        public LoadScreen()
        {
            InitializeComponent();
            value = 0;
            progressBar.Value = value;
        }

        void ProgressBarCallBack(int max)
        {
            progressBar.Maximum = max;
            progressBar.Value = ++value;
            Application.DoEvents();
        }

        public ProgressBarCallBack GetProgressBarCallBack()
        {
            return new ProgressBarCallBack(this.ProgressBarCallBack);
        }
    }

    
}
