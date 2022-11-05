using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class StructureAnimation : MonoBehaviour
{
    public GameObject JointPrefab;
    public GameObject FramePrefab;

    private const string _beamsDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\beams.csv";
    private const string _bracesDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\braces.csv";
    private const string _columnsDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\columns.csv";
    private const string _jointDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\joints.csv";
    private const string _frameSectionFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\frameSection.csv";
    private const string _timeDisplacementDataFilepath = @"C:\Users\mconway\GitHub\Seism-sim\Unity\Assets\Data\timeDisplacement.csv";

    private const float _mmToFeet = 0.00328084f;

    //string = frame, List<string> = 2 x Joint names
    private Dictionary<string, List<string>> _connections = new Dictionary<string, List<string>>();

    private Dictionary<string, GameObject> _joints = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _frames = new Dictionary<string, GameObject>();
    private Dictionary<int, List<string>> _jointsByStory = new Dictionary<int, List<string>>();

    private List<string[]> timeDisplacementData = new List<string[]>();

    // Start is called before the first frame update
    private void Start()
    {
        //get time displacement
        timeDisplacementData = ReadCSV(_timeDisplacementDataFilepath);

        //get frame sections, this doens't need to be global
        var frameSectionData = ReadCSV(_frameSectionFilepath);
        Dictionary<string, string[]> frameSections = new Dictionary<string, string[]>();
        for (int i = 3; i < frameSectionData.Count; i++)
        {
            //0 = name
            string[] parts = frameSectionData[i];
            string name = parts[0];
            if (frameSections.ContainsKey(name)) { continue; }
            frameSections.Add(name, parts);
        }

        //create joints
        var jointData = ReadCSV(_jointDataFilepath);
        for (int i = 3; i < jointData.Count; i++) //formated to start at index 3
        {
            //0 = name, 5 = X, 7 = Z (translate to Y for Unity purposes)
            var row = jointData[i];
            if (row.Length <= 7) { continue; }
            string name = row[0];
            if (_joints.ContainsKey(name)) { continue; }

            float X = float.Parse(row[5]);
            float Y = float.Parse(row[7]);//CAD Z Value
            var pos = new Vector3(X, Y, 0);
            //Debug.Log(name + ", " + pos);
            var joint = GameObject.Instantiate(JointPrefab, pos, Quaternion.identity);
            joint.name = "Joint " + name;
            _joints.Add(name, joint);
        }

        //get geometry members into a dictionary by name with vertices as the value
        List<string[]> geometryData = new List<string[]>();
        var beams = ReadCSV(_beamsDataFilepath); beams.RemoveRange(0, Mathf.Min(3, beams.Count));
        geometryData.AddRange(beams);
        var braces = ReadCSV(_bracesDataFilepath); braces.RemoveRange(0, Mathf.Min(3, braces.Count));
        geometryData.AddRange(braces);
        var columns = ReadCSV(_columnsDataFilepath); columns.RemoveRange(0, Mathf.Min(3, columns.Count));
        geometryData.AddRange(columns);
        for (int i = 0; i < geometryData.Count; i++)
        {
            //0 = name, 3 = start, 4 = end
            var parts = geometryData[i];
            string name = parts[0];
            if (_connections.ContainsKey(name)) { continue; }

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
            _connections.Add(name, new List<string>() { start, end });
            var frame = GameObject.Instantiate(FramePrefab);
            frame.name = "Frame " + name;
            _frames.Add(name, frame);

            //update frame section
            Debug.Log(name);
            if (!frameSections.ContainsKey(name)) { continue; }
            var frameSection = frameSections[name];
            Debug.Log(frameSection[2]);
            float totalDepth = float.Parse(frameSection[2]) * _mmToFeet;
            float flangeWidth = float.Parse(frameSection[3]) * _mmToFeet;
            float flangeThickness = float.Parse(frameSection[4]) * _mmToFeet;
            float webThickness = float.Parse(frameSection[5]) * _mmToFeet;
            if (!frame.GetComponent<Frame>()) { continue; }
            frame.GetComponent<Frame>().UpdateSection(totalDepth, flangeWidth, flangeThickness, webThickness);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //Update joint displacement

        //Update members based on joints
        foreach (KeyValuePair<string, GameObject> kvp in _frames)
        {
            var name = kvp.Key;
            var frame = kvp.Value;
            var jointNames = _connections[name];
            var startGO = _joints[jointNames[0]];
            var endGO = _joints[jointNames[1]];

            var pos = (startGO.transform.position + endGO.transform.position) / 2.00f;
            var dir = endGO.transform.position - startGO.transform.position;
            var length = dir.magnitude;
            //Debug.Log(dir);

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