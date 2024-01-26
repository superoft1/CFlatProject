using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Presenter
{
  interface IEntityPresenter
  {
    bool IsRaised( IElement element );

    void Raise( IElement element );

    void Update( IElement element );

    void TransformUpdate( IElement element );

    void Destroy( IElement element );

    void Processed();
  }
}
