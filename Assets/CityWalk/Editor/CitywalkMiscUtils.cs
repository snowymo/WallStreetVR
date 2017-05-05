using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class CitywalkMiscUtils : MonoBehaviour {

    [MenuItem("Citywalk/Select High Buildings")]
    public static void SelectHighBuildings()
    {
        float cutHeight = 250;

        var objects = Selection.GetFiltered(typeof(Renderer), SelectionMode.Deep);

        if (objects.Length == 0)
        {
            return;
        }

        float minHeight = 99999;
        float maxHeight = 0;
        float avgHeight = 0;

        for (int i = 0; i < objects.Length; i++)
        {
            float height = ((Renderer)objects[i]).bounds.max.y;

            if (height < minHeight)
            {
                minHeight = height;
            }

            if (height > maxHeight)
            {
                maxHeight = height;
            }

            avgHeight += height;
        }

        avgHeight /= objects.Length;

        Debug.Log(string.Format("{0} buildings, minHeight={1}, maxHeight={2}, avgHeight={3}, cut={4}", objects.Length, minHeight, maxHeight, avgHeight, cutHeight));

        List<Object> highBuildings = new List<Object>();

        for (int i = 0; i < objects.Length; i++)
        {
            float height = ((Renderer)objects[i]).bounds.max.y;
            if (height > cutHeight)
            {
                highBuildings.Add(((Renderer)objects[i]).gameObject);
            }
        }

        Selection.objects = highBuildings.ToArray();
    }

}
