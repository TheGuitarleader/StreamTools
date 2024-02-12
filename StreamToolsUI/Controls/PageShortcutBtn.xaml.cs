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

namespace StreamToolsUI.Controls
{
    /// <summary>
    /// Interaction logic for PageShortcutBtn.xaml
    /// </summary>
    public partial class PageShortcutBtn : UserControl
    {
        public PageShortcutBtn()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click
        {
            add { PageBtn.Click += value; }
            remove { PageBtn.Click -= value; }
        }

        public static readonly DependencyProperty ShortcutNameProperty = DependencyProperty.Register("ShortcutName", typeof(string), typeof(PageShortcutBtn), new PropertyMetadata());
        public string ShortcutName
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageBrush), typeof(PageShortcutBtn), new PropertyMetadata());
        public ImageBrush Icon
        {
            get { return (ImageBrush)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
    }
}
