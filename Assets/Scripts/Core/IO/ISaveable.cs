using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderingRoad.IO
{
    public interface IStreamable
    {
        ISerialisable ToSerialisable();
    }

    public interface ISerialisable
    {
        IStreamable RestoreAsset();
    }

    public interface IGetSaveableInformation
    {

    }
}
