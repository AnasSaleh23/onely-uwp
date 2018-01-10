using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Onely.Elements
{
    public class PlaylistOpenButton : Button
    {
        public string PlaylistId
        {
            get
            {
                return (string)GetValue(PlaylistIdProperty);
            }
            set { SetValue(PlaylistIdProperty, value); }
        }
        public static readonly DependencyProperty PlaylistIdProperty = DependencyProperty.Register("PlaylistId", typeof(string), typeof(PlaylistDeleteButton), new PropertyMetadata(""));
    }

    public class PlaylistAppendButton : AppBarButton
    {
        public string PlaylistId
        {
            get
            {
                return (string)GetValue(PlaylistIdProperty);
            }
            set { SetValue(PlaylistIdProperty, value); }
        }
        public static readonly DependencyProperty PlaylistIdProperty = DependencyProperty.Register("PlaylistId", typeof(string), typeof(PlaylistDeleteButton), new PropertyMetadata(""));
    }

    public class PlaylistDeleteButton : AppBarButton
    {
        public string PlaylistId
        {
            get
            {
                return (string)GetValue(PlaylistIdProperty);
            }
            set { SetValue(PlaylistIdProperty, value); }
        }
        public static readonly DependencyProperty PlaylistIdProperty = DependencyProperty.Register("PlaylistId", typeof(string), typeof(PlaylistDeleteButton), new PropertyMetadata(""));
    }

}
