using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using MaterialUI;
using UnityEngine;

public class DiscreteSliderSupporter : MonoBehaviour
{
  [SerializeField]
  MaterialSlider materialSlider;

  private void Start()
  {
    //ImportManager.Instance().OnImportActionList.Add((level) =>
    //{
    //  SliderSet(level);
    //});
  }

  void SliderSet(int level)
  {
    //var value = (float)inch / materialSlider.Coef;
    //materialSlider.SliderValueChange(level);
  }

  public void OnSliderValueChanged( float value )
  {
    //var doc = DocumentCollection.Instance.Current;
    //if ( null == doc ) return;

    //int baseDiameter = (int)value;
    //foreach ( var bp in doc.BlockPatterns ) {
    //  //if ( Chiyoda.CAD.BP.BlockPatternType.Type.EndTopTypePump == bp.Type ) {
    //  //  bp.ChangeBaseDiameter( baseDiameter );
    //  //}
    //}
  }

}
