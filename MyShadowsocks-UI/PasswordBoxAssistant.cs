using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyShadowsocks_UI {
    public static class PasswordBoxAssistant {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxAssistant),
                new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBoxAssistant),
                new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached(
            "IsUpdating", typeof(bool), typeof(PasswordBoxAssistant));

        
        public static void SetAttach(DependencyObject dp, bool value) {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp) {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetPassword(DependencyObject dp) {
            return (string)dp.GetValue(PasswordProperty);
        }
        public static void SetPassword(DependencyObject dp, string value) {
            dp.SetValue(PasswordProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp) {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value) {
            dp.SetValue(IsUpdatingProperty, value);
        }




        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PasswordBox box = d as PasswordBox;
            if(box == null) return;
            box.PasswordChanged -= PasswordChanged;
            if(!(bool)GetIsUpdating(box)) {
                box.Password = (string)e.NewValue;
            }
            box.PasswordChanged += PasswordChanged;
            
        }

        


        private static void PasswordChanged(object sender, RoutedEventArgs e) {
            PasswordBox box = sender as PasswordBox;
            SetIsUpdating(box, true);
            SetPassword(box, box.Password);
            SetIsUpdating(box, false);
        }

        private static void Attach(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PasswordBox box = d as PasswordBox;
            if(box == null) return;
            if((bool)e.OldValue) {
                box.PasswordChanged -= PasswordChanged;
            }
            if((bool)e.NewValue) {
                box.PasswordChanged -= PasswordChanged;
            }

        }

        


    }
}
