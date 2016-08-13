using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BulbaGO.Base.Bots;

namespace BulbaGO.UI
{
    public partial class MainForm : Form
    {
        private List<Bot> _bots;
        private Dictionary<string, BotControl> _botControls = new Dictionary<string, BotControl>();
        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            _bots = await BotFactory.GetAllBots();
            _bots.ForEach(b => BotsList.Items.Add(b.Username));
            _botControls = _bots.ToDictionary(b => b.Username, b => BotControl.GetInstance(this, b));
        }

        private void BotsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listbox = sender as ListBox;
            if (listbox != null)
            {
                var selectedBot = listbox.SelectedItem as string;
                if (selectedBot != null)
                {
                    var botControl = _botControls[selectedBot];
                    if (!BottomSplit.Panel2.Controls.Contains(botControl))
                    {
                        BottomSplit.Panel2.Controls.Add(botControl);
                    }
                    botControl.BringToFront();
                    //BottomSplit.Panel2.Controls.Add(_botControls[selectedBot]);
                }
            }
        }

    }
}
