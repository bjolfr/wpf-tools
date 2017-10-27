using ContentAdorners.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ContentAdorners.Behaviors
{
    public class TextBoxLimitedInputBehavior : Behavior<TextBox>
    {
        private Adorner _adorner = null;

        #region Properties
        public int MaxLineLength
        {
            get
            {
                var val = (int)GetValue(MaxLineLengthProperty);
                if (val <= 0 && AssociatedObject != null)
                {
                    val = GetMaxLineLength(AssociatedObject);
                    if (val <= 0) val = AssociatedObject.MaxLength;
                }
                return val;
            }
            set { SetValue(MaxLineLengthProperty, value); }
        }

        public int MaxLines
        {
            get
            {
                var val = (int)GetValue(MaxLinesProperty);
                if (val <= 0 && AssociatedObject != null)
                {
                    val = GetMaxLines(AssociatedObject);
                    if (val <= 0) val = AssociatedObject.MaxLines;
                }
                return val;
            }
            set { SetValue(MaxLinesProperty, value); }
        }

        public DataTemplate WarningTemplate
        {
            get
            {
                var val = (DataTemplate)GetValue(WarningTemplateProperty);
                if (val == null && AssociatedObject != null) val = GetWarningTemplate(AssociatedObject);
                return val;
            }
            set { SetValue(WarningTemplateProperty, value); }
        }

        public string WarningMessage
        {
            get
            {
                var val = (string)GetValue(WarningMessageProperty);
                if (val == null && AssociatedObject != null) val = GetWarningMessage(AssociatedObject);
                return val;
            }
            set { SetValue(WarningMessageProperty, value); }
        }

        public string MergeWith
        {
            get { return (string)GetValue(MergeWithProperty); }
            set { SetValue(MergeWithProperty, value); }
        }
        #endregion

        #region Dependency Properties

        #region MaxLineLength
        public static readonly DependencyProperty MaxLineLengthProperty =
            DependencyProperty.RegisterAttached("MaxLineLength", typeof(int), typeof(TextBoxLimitedInputBehavior), new PropertyMetadata(0));

        public static int GetMaxLineLength(DependencyObject obj)
        {
            return (int)obj.GetValue(MaxLineLengthProperty);
        }

        public static void SetMaxLineLength(DependencyObject obj, int value)
        {
            obj.SetValue(MaxLineLengthProperty, value);
        }
        #endregion MaxLineLength

        #region MaxLines
        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.RegisterAttached("MaxLines", typeof(int), typeof(TextBoxLimitedInputBehavior), new PropertyMetadata(0));

        public static int GetMaxLines(DependencyObject obj)
        {
            return (int)obj.GetValue(MaxLinesProperty);
        }

        public static void SetMaxLines(DependencyObject obj, int value)
        {
            obj.SetValue(MaxLinesProperty, value);
        }
        #endregion MaxLines

        #region WarningTemplate
        public static readonly DependencyProperty WarningTemplateProperty =
            DependencyProperty.RegisterAttached("WarningTemplate", typeof(DataTemplate), typeof(TextBoxLimitedInputBehavior), new PropertyMetadata(null));

        public static DataTemplate GetWarningTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(WarningTemplateProperty);
        }

        public static void SetWarningTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(WarningTemplateProperty, value);
        }
        #endregion WarningTemplate

        #region WarningMessage
        public static readonly DependencyProperty WarningMessageProperty =
            DependencyProperty.RegisterAttached("WarningMessage", typeof(string), typeof(TextBoxLimitedInputBehavior), new PropertyMetadata(null));

        public static string GetWarningMessage(DependencyObject obj)
        {
            return (string)obj.GetValue(WarningMessageProperty);
        }

        public static void SetWarningMessage(DependencyObject obj, string value)
        {
            obj.SetValue(WarningMessageProperty, value);
        }
        #endregion WarningMessage

        #region MergeWith
        public static readonly DependencyProperty MergeWithProperty =
            DependencyProperty.Register("MergeWith", typeof(string), typeof(TextBoxLimitedInputBehavior), new PropertyMetadata(String.Empty));
        #endregion MergeWith

        #endregion

        #region Overrides
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
        }
        #endregion

        #region Events
        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var kc = (int)e.Key;
            if ((kc < 34 && e.Key != Key.Return && e.Key != Key.Space && e.Key != Key.Back && e.Key != Key.Delete)
                || (kc > 89 && kc < 140)
                || (kc > 154)) return;

            var tbx = (TextBox)sender; if (tbx.IsReadOnly) return;
            int cli = tbx.GetLineIndexFromCharacterIndex(tbx.CaretIndex),
                cll = tbx.GetLineLength(cli),
                cml = tbx.MaxLines - 1,
                ttl = tbx.LineCount;

            if (cll > 2 && tbx.GetLineText(cli).Substring(cll - 2).Equals(Environment.NewLine)) cll -= 2;
            if (!String.IsNullOrEmpty(MergeWith) && MergeWith.Length > 3) cll += MergeWith.Length;

            if (cll >= MaxLineLength && e.Key != Key.Return) e.Handled = true;
            if (ttl >= MaxLines && e.Key == Key.Return) e.Handled = true;
            if (e.Key == Key.Back || e.Key == Key.Delete) e.Handled = false;

            if (_adorner != null) _adorner.Visibility = e.Handled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            if (WarningTemplate == null) return;

            ContentAdorner.SetAdornerTemplate(AssociatedObject, WarningTemplate);

            var layer = AdornerLayer.GetAdornerLayer(sender as System.Windows.Media.Visual); if (layer == null) return;
            var adorners = layer.GetAdorners(sender as UIElement); if (adorners == null) return;
            _adorner = (from a in adorners where a is ContentAdorner select a).LastOrDefault();
            if (_adorner != null) _adorner.DataContext = WarningMessage;
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_adorner != null) _adorner.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
