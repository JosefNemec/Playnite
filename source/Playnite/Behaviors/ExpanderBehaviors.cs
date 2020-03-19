using Playnite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Playnite.Behaviors
{
    public class ExpanderBehaviors
    {
        private static readonly DependencyProperty SaveStateProperty =
            DependencyProperty.RegisterAttached("SaveState", typeof(bool), typeof(ExpanderBehaviors));

        public static bool GetSaveState(DependencyObject obj)
        {
            return (bool)obj.GetValue(SaveStateProperty);
        }

        public static void SetSaveState(DependencyObject obj, bool value)
        {
            obj.SetValue(SaveStateProperty, value);
        }

        private static readonly DependencyProperty SaveStateIdProperty =
            DependencyProperty.RegisterAttached("SaveStateId", typeof(string), typeof(ExpanderBehaviors));

        public static string GetSaveStateId(DependencyObject obj)
        {
            return (string)obj.GetValue(SaveStateIdProperty);
        }

        public static void SetSaveStateId(DependencyObject obj, string value)
        {
            obj.SetValue(SaveStateIdProperty, value);
        }
    }
}
