using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSpaceRecorder : MonoBehaviour
{
    private struct PlayerSpace
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public float Scale { get; private set; }

        public PlayerSpace(Vector3 position, Quaternion rotation, float scale) : this()
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    private static readonly string LOCATION_FILE_PATH = "locations.csv";
    private Dictionary<string, PlayerSpace> locations = new Dictionary<string, PlayerSpace>();
    private Dictionary<KeyCode, string> locationKeys = new Dictionary<KeyCode, string>();

    void Start()
    {
        for (int i = (int)KeyCode.Alpha0; i <= (int)KeyCode.Alpha9; i++)
        {
            var keyCode = (KeyCode)i;
            locationKeys.Add(keyCode, (i - (int)KeyCode.Alpha0).ToString());
        }

        for (int i = (int)KeyCode.A; i <= (int)KeyCode.Z; i++)
        {
            var keyCode = (KeyCode)i;
            locationKeys.Add(keyCode, keyCode.ToString());
        }
    }

    void Update()
    {
        handleKeyDown();
    }

    private void handleKeyDown()
    {
        if (!Input.GetKey(KeyCode.Tab) && !Input.GetKey(KeyCode.Tab))
        {
            return;
        }

        bool isSaving = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        foreach (var item in locationKeys)
        {
            KeyCode keyCode = item.Key;
            string keyName = item.Value;

            if (!Input.GetKeyDown(keyCode))
            {
                continue;
            }

            Debug.Log("KeyDown: " + keyCode);

            if (isSaving)
            {
                Vector3 position = TrackingSpaceController.Instance.transform.position;
                Quaternion rotation = TrackingSpaceController.Instance.transform.rotation;
                float scale = TrackingSpaceController.Instance.transform.localScale.x;

                locations[keyName] = new PlayerSpace(position, rotation, scale);
                saveFile();

                Debug.Log(string.Format("Saved location {0}", keyName));
            }
            else
            {
                loadFile();

                if (locations.ContainsKey(keyName))
                {
                    PlayerSpace space = locations[keyName];
                    TrackingSpaceController.Instance.transform.position = space.Position;
                    TrackingSpaceController.Instance.transform.rotation = space.Rotation;
                    TrackingSpaceController.Instance.transform.localScale = space.Scale * Vector3.one;

                    Debug.Log(string.Format("Teleport player to location {0}", keyName));
                }
            }
        }
    }

    private void loadFile()
    {
        if (!File.Exists(LOCATION_FILE_PATH))
        {
            return;
        }

        using (var reader = new StreamReader(LOCATION_FILE_PATH))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("#"))
                {
                    continue;
                }

                string[] elements = line.Split(',');

                try
                {
                    string key = elements[0]; 

                    float x = float.Parse(elements[1]);
                    float y = float.Parse(elements[2]);
                    float z = float.Parse(elements[3]);

                    float rx = float.Parse(elements[4]);
                    float ry = float.Parse(elements[5]);
                    float rz = float.Parse(elements[6]);
                    float rw = float.Parse(elements[7]);

                    float s = float.Parse(elements[8]);

                    locations[key] = new PlayerSpace(
                        new Vector3(x, y, z), 
                        new Quaternion(rx, ry, rz, rw), 
                        s
                    );
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError("Failed to parse location record: " + line);
                }
            }
        }
    }

    private void saveFile()
    {
        using (var writer = new StreamWriter(LOCATION_FILE_PATH))
        {
            writer.WriteLine("# name,x,y,z,rx,ry,rz,rw,scale");
            foreach (var item in locations)
            {
                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
                    item.Key, 

                    item.Value.Position.x, 
                    item.Value.Position.y, 
                    item.Value.Position.z,

                    item.Value.Rotation.x, 
                    item.Value.Rotation.y, 
                    item.Value.Rotation.z,
                    item.Value.Rotation.w,

                    item.Value.Scale
                );
            }
        }
    }
}
