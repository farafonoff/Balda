using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace Balda
{
    public partial class Splash : UserControl
    {
        public Splash()
        {
            InitializeComponent();
            string st = AppResources.AppName.ToUpper();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < st.Length; ++i)
            {
                sb.Append(st[i]).Append(" ");
            }
            text.Text = sb.ToString();
        }
    }
}
