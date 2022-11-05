using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class StructureAnimation : MonoBehaviour
{
    private const string _geometryDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\geometry.csv";

    private Dictionary<string, List<string>> _members = new Dictionary<string, List<string>>();
    private Dictionary<string, GameObject> _joints = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        //get geometry members into a dictionary by name with vertices as the value
        var geometryData = ReadCSV(_geometryDataFilepath);
        for (int i = 3; i < geometryData.Count; i++) //formated to start at index 3
        {
            //0 = name, 3 = start, 4 = end
            var parts = geometryData[i];
            string name = parts[0];
            if (_members.ContainsKey(name)) { continue; }

            string start = parts[3];
            string end = parts[4];

            //add joint gameobjects to scene
            if (!_joints.ContainsKey(start))
            {
                var go = new GameObject();
                go.name = start;
                _joints.Add(start, go);
            }
            if (!_joints.ContainsKey(end))
            {
                var go = new GameObject();
                go.name = end;
                _joints.Add(end, go);
            }

            //create member
            _members.Add(name, new List<string>() { start, end });
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private List<string[]> ReadCSV(string filepath)
    {
        var lines = File.ReadAllLines(filepath);

        List<string[]> data = new List<string[]>();
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            data.Add(parts);
        }
        return data;
    }
}