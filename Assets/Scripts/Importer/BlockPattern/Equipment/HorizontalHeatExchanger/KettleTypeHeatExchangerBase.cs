using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chiyoda;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Importer.BlockPattern.Equipment.HorizontalHeatExchanger
{
    public class KettleTypeHeatExchangerBase : HorizontalHeatExchangerBase    // TODO HorizontalHeatExchangerBaseの共用部分を共通処理にリファクタリングすべき
    {
        public KettleTypeHeatExchangerBase(Document doc, string equipmentShapeName)
            : base(doc, equipmentShapeName)
        {
        }

        protected override void SetBlockPatternInfo()
        {
            BaseEquipment.LeafEdge.ObjectName = "Base" ;
            var factory = new PipingLayoutFactory( Doc, IdfFolderPath, BaseEquipment ) ;
            
            var layoutsUpper = new List<PipingLayout>
            {
                factory.Create( PipingLayoutFactory.UpperLayout.B_1_OUT_V ),
                factory.Create( PipingLayoutFactory.UpperLayout.B_1_T_OUT ),
            } ;

            var layoutsLower = new List<PipingLayout>
            {
                factory.Create( PipingLayoutFactory.LowerLayout.B_1_OUT_L ),
                factory.Create( PipingLayoutFactory.LowerLayout.B_1_IN ),
                factory.Create( PipingLayoutFactory.LowerLayout.B_1_T_IN ),
            } ;
            
            List<(string name, string nozzle, Edge pattern)> upPipes 
                = layoutsUpper.Select( l => ( l.BlockName, l.NozzleName, (Edge)l.GetRuledBlockPattern() ) ).ToList() ;
            upPipes.Add((string.Empty, string.Empty, null));
      
            List<(string name, string nozzle, Edge pattern)> lowPipes 
                = layoutsLower.Select( l => ( l.BlockName, l.NozzleName, (Edge)l.GetRuledBlockPattern() ) ).ToList() ;
            lowPipes.Add((string.Empty, string.Empty, null));
            
            upPipes.Take( 2 ).ForEach( p => BaseBp.AddEdge( p.pattern ) ) ;
            lowPipes.Take( 3 ).ForEach( p => BaseBp.AddEdge( p.pattern ) ) ;
            
            // 配管の取り替えルール設定（上部）
            for ( int i = 0 ; i < 2 ; ++i ) {
                var prop = BaseBp.RegisterUserDefinedProperty( upPipes[ i ].nozzle, i, new Dictionary<string, double>
                {
                    // TODO: 名称は適当なので要修正
                    { "Pipe", 0 },
                    { "Pipe+Flange", 1 },
                    { "Empty", 2 }
                } ) ;
                prop.AddUserDefinedRule( 
                    new InterChangeablePipingRule( HorizontalHeatExchanger.PipingLayout.GetInterChangeableName(), GetNozzle( BaseBp, upPipes[ i ].nozzle ), upPipes,
                        HorizontalHeatExchanger.PipingLayout.GetKeepProperties() ) ) ;
            }

            upPipes.Take( 2 ).ForEach( p => p.pattern.RuleList.BindChangeEvents( true ) ) ;
            lowPipes.Take( 3 ).ForEach( p => p.pattern.RuleList.BindChangeEvents( true ) ) ;
            
            BaseBp.RuleList.BindChangeEvents( true ) ;
        }
    }
}
