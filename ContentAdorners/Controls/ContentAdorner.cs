using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace ContentAdorners.Controls
{
    public class ContentAdorner : Adorner
    {
        private VisualCollection children;
        private FrameworkElement child;

        #region ctor/dtor
        public ContentAdorner(UIElement element) : base(element)
        {
            children = new VisualCollection(this);
            child = new ContentControl();
            child.SetBinding(ContentControl.ContentTemplateProperty, new Binding() { Path = new PropertyPath(AdornerTemplateProperty), Source = element });
            if (element is FrameworkElement)
            {
                this.DataContext = ((FrameworkElement)element).DataContext;
                child.SetBinding(ContentControl.DataContextProperty, new Binding { Path = new PropertyPath(DataContextProperty), Source = this });
            }

            children.Add(child);
            AddLogicalChild(child);
            Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

        #region Dependencies
        public static DataTemplate GetAdornerTemplate(DependencyObject element)
        {
            return (DataTemplate)element.GetValue(AdornerTemplateProperty);
        }

        public static void SetAdornerTemplate(DependencyObject element, DataTemplate value)
        {
            element.SetValue(AdornerTemplateProperty, value);
        }

        public static readonly DependencyProperty AdornerTemplateProperty =
            DependencyProperty.RegisterAttached("AdornerTemplate", typeof(DataTemplate), typeof(ContentAdorner), new PropertyMetadata(null, OnAdornerTemplatePropertyChanged));
        #endregion

        #region Events
        private static void OnAdornerTemplatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = (FrameworkElement)sender;
            if (e.NewValue != null)
                if (target.IsLoaded)
                    ApplyContentAdorner(target);
                else
                    target.Loaded += OnAdornerTargetLoaded;
        }

        private static void OnAdornerTargetLoaded(object sender, RoutedEventArgs e)
        {
            var target = (FrameworkElement)sender;
            target.Loaded -= OnAdornerTargetLoaded;
            ApplyContentAdorner(target);
        }
        #endregion

        #region Overrides
        protected override Visual GetVisualChild(int index)
        {
            return this.children[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return this.children.Count;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.child.Measure(constraint);
            return AdornedElement.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Point location = new Point(0, 0);
            Rect rect = new Rect(location, finalSize);
            this.child.Arrange(rect);
            return this.child.RenderSize;
        }
        #endregion

        #region Helpers
        private static void ApplyContentAdorner(FrameworkElement target)
        {
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null) return;
            var adorner = new ContentAdorner(target);
            layer.Add(adorner);
        }
        #endregion
    }
}
