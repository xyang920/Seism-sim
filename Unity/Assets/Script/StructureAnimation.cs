using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class StructureAnimation : MonoBehaviour
{
    public GameObject JointPrefab;
    public GameObject FramePrefab;

    private const string _geometryDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\geometry.csv";
    private const string _jointDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\joints.csv";

    private Dictionary<string, List<string>> _members = new Dictionary<string, List<string>>();
    private Dictionary<string, GameObject> _joints = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _frames = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        //create joints
        var jointData = ReadCSV(_jointDataFilepath);
        for (int i = 3; i < jointData.Count; i++) //formated to start at index 3
        {
            //0 = name, 5 = X, 7 = Z (translate to Y for Unity purposes)
            var parts = jointData[i];
            if (parts.Length <= 7) { continue; }
            string name = parts[0];
            if (_joints.ContainsKey(name)) { continue; }

            float X = float.Parse(parts[5]);
            float Y = float.Parse(parts[7]);
            var pos = new Vector3(X, Y, 0);
            Debug.Log(name + ", " + pos);
            var joint = GameObject.Instantiate(JointPrefab, pos, Quaternion.identity);
            joint.name = "Joint " + name;
            _joints.Add(name, joint);
        }

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
            var frame = GameObject.Instantiate(FramePrefab);
            frame.name = "Frame " + name;
            _frames.Add(name, frame);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //TODO: Update joint displacement

        //TODO: Update members based on joints
        foreach (KeyValuePair<string, GameObject> kvp in _frames)
        {
            var name = kvp.Key;
            var frame = kvp.Value;
            var jointNames = _members[name];
            var startGO = _joints[jointNames[0]];
            var endGO = _joints[jointNames[1]];

            var pos = (startGO.transform.position + endGO.transform.position) / 2.00f;
            var dir = endGO.transform.position - startGO.transform.position;
            var length = dir.magnitude;

            frame.transform.position = pos;
            //frame.transform.LookAt(endGO.transform.position);
            var scale = new Vector3(1, 1, length);
            frame.transform.localScale = scale;
            frame.transform.forward = dir;
        }
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