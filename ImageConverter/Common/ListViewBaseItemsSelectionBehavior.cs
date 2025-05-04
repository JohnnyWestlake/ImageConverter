using Microsoft.Xaml.Interactivity;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ImageConverter.Common;

public class ListViewBaseItemsSelectionBehavior : Behavior<ListViewBase>
{
    #region Dependency Properties

    public INotifyCollectionChanged SelectedItems
    {
        get { return (INotifyCollectionChanged)GetValue(SelectedItemsProperty); }
        set { SetValue(SelectedItemsProperty, value); }
    }

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(INotifyCollectionChanged), typeof(ListViewBaseItemsSelectionBehavior), new PropertyMetadata(null, (d, e) =>
        {
            ((ListViewBaseItemsSelectionBehavior)d).OnSelectedItemsChanged(e);
        }));


    #endregion


    long token = 0;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (SelectedItems == null)
        {
            SelectedItems = new ObservableCollection<object>();
        }
        else if (SelectedItems is IEnumerable<object> list)
        {
            foreach (var item in list.ToList())
            {
                AssociatedObject.SelectedItems.Add(item);
            }
        }

        token = AssociatedObject.RegisterPropertyChangedCallback(ListViewBase.ItemsSourceProperty, ItemsSourceChanged);

        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;

        SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
        SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
    }

    private void OnSelectedItemsChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyCollectionChanged c)
        {
            c.CollectionChanged -= SelectedItems_CollectionChanged;
        }

        if (AssociatedObject == null)
            return;

        if (e.NewValue is INotifyCollectionChanged n)
        {
            n.CollectionChanged -= SelectedItems_CollectionChanged;
            n.CollectionChanged += SelectedItems_CollectionChanged;
        }
    }



    void ItemsSourceChanged(DependencyObject source, DependencyProperty property)
    {
        if (SelectedItems is IList<object> list)
        {
            foreach (var item in list.ToList())
            {
                list.Remove(item);
            }
        }
    }

    protected override void OnDetaching()
    {
        AssociatedObject.UnregisterPropertyChangedCallback(ListViewBase.ItemsSourceProperty, token);
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;

        SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;

        base.OnDetaching();
    }

    private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems)
            {
                if (!AssociatedObject.SelectedItems.Contains(item))
                    AssociatedObject.SelectedItems.Add(item);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems)
            {
                if (!AssociatedObject.SelectedItems.Contains(item))
                    AssociatedObject.SelectedItems.Remove(item);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // added to honor SomeCollection.Clear();
            AssociatedObject.DeselectRange(new ItemIndexRange(0, int.MaxValue));
        }

        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedItems is IList list)
        {
            SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;

            foreach (var item in e.RemovedItems)
            {
                list.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                list.Add(item);
            }

            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }
    }

}
