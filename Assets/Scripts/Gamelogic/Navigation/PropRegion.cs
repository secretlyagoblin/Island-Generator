using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropRegion
{
    private Vector3 center;
    

    public PropRegion(Rect bounds, Guid id)
    {

    }


    // Update is called once per frame
    public void Update(GameObject player)
    {
        var distance = Vector3.Distance(center, player.transform.position);

        if(distance > 300)
        {
            //either unload or stay unloaded, possibly also lower the update tick so we can skip tests

            return;
        }

        if (distance > 150)
        {
            //start loading, raycast big objects and start loading small objects into chunks
        }

        if (distance > 50)
        {
            //ensure everything is loaded in big, details start to get streamed in

            //iterate over subobject arrays
        }


    }
}
