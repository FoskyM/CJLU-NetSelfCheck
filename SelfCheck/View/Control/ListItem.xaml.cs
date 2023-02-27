using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SelfCheck.View.Control
{
    /// <summary>
    /// ListItem.xaml 的交互逻辑
    /// </summary>
    public partial class ListItem : UserControl
    {
        public ListItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ListItem), new PropertyMetadata(null));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        public string Status
        {
            get { 
                return (string)GetValue(StatusProperty); 
            }
            set {
                if (value.ToLower() == "ok")
                {
                    Status_OK.Visibility = Visibility.Visible;
                    Status_Error.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Status_OK.Visibility = Visibility.Collapsed;
                    Status_Error.Visibility = Visibility.Visible;
                }
                SetValue(StatusProperty, value); 
            }
        }

    }
}
