using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.UI
{
  public class UnitElementList : MonoBehaviour
  {
    [SerializeField] private UnitListBox unitListBox;

    public IElement Target { get; private set; }

    public ICollection TargetCollection { get; private set; }

    private void Start()
    {
      unitListBox.ClearItem();
    }

    public void SetTarget<T>(IElement elm, ICollection<T> collection)
    {
      var targetCol = new Common.CollectionProxy<T>(collection);

      if (collection == null)
      {
        targetCol = null;
      }

      if (elm == null != (targetCol == null))
        throw new ArgumentException($"`{nameof(elm)}' and `{nameof(collection)}' must be both null or both nonnull.");

      if (ReferenceEquals(Target, elm) && ProxiedReferenceEqual(TargetCollection, targetCol)) return;

      if (Target != null)
      {
        Target.AfterNewlyChildrenChanged -= Target_ChildrenChanged;
        Target.AfterHistoricallyChildrenChanged -= Target_ChildrenChanged;

        foreach (var e in TargetCollection.OfType<IElement>())
        {
          e.AfterNewlyValueChanged -= Element_ValueChanged;
          e.AfterHistoricallyValueChanged -= Element_ValueChanged;
        }
      }

      Target = elm;
      TargetCollection = targetCol;
//      unitListBox.ClearItem();

      if (Target != null)
      {
        Target.AfterNewlyChildrenChanged += Target_ChildrenChanged;
        Target.AfterHistoricallyChildrenChanged += Target_ChildrenChanged;

        if (TargetCollection != null)
        {
          foreach (var e in TargetCollection.OfType<IElement>())
          {
            e.AfterNewlyValueChanged += Element_ValueChanged;
            e.AfterHistoricallyValueChanged += Element_ValueChanged;
          }
        }
      }

//      if (targetCol == null) return;
//
//      foreach (var e in targetCol.OfType<IElement>())
//      {
//        unitListBox.CreateItem(e);
//      }
    }

    private static bool ProxiedReferenceEqual(IEnumerable col1, IEnumerable col2)
    {
      var obj1 = col1 is Common.ICollectionProxy proxy1 ? proxy1.BaseCollection : col1;
      var obj2 = col2 is Common.ICollectionProxy proxy2 ? proxy2.BaseCollection : col2;
      return ReferenceEquals(obj1, obj2);
    }

    private void Target_ChildrenChanged(object sender, ItemChangedEventArgs<IElement> e)
    {
      // 不要アイテムの削除
      e.RemovedItems.ForEach(elm => unitListBox.RemoveItem(elm));

      // 必要アイテムの追加 (AddedItemsがTargetCollection麾下である保証はないため、TargetCollectionを追う)
      TargetCollection.OfType<IElement>().ForEach((elm, i) =>
      {
        if (unitListBox.ItemMap.TryGetValue(elm, out var item))
        {
        }
        else
        {
          unitListBox.CreateItem(elm, i);
        }
      });
    }

    private void Element_ValueChanged(object sender, EventArgs e)
    {
      unitListBox.UpdateItem((IElement) sender);
    }
  }
}