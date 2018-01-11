using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Onely.Elements
{

    public class PlaylistActionButton : Button
    {
        public string PlaylistId
        {
            get
            {
                return (string)GetValue(PlaylistIdProperty);
            }
            set { SetValue(PlaylistIdProperty, value); }
        }
        public static readonly DependencyProperty PlaylistIdProperty = DependencyProperty.Register("PlaylistId", typeof(string), typeof(PlaylistActionButton), new PropertyMetadata(""));

        public string PlaylistName
        {
            get
            {
                return (string)GetValue(PlaylistNameProperty);
            }
            set { SetValue(PlaylistNameProperty, value); }
        }
        public static readonly DependencyProperty PlaylistNameProperty = DependencyProperty.Register("PlaylistName", typeof(string), typeof(PlaylistActionButton), new PropertyMetadata(""));
    }

}
