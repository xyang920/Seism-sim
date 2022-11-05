using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;

public class StructureAnimation : MonoBehaviour
{
    [Header("Animation")]
    public float AnimationSpeed = 0.1f;

    public GameObject Platform;

    [Header("Prefab Game Objects")]
    public GameObject JointPrefab;

    public GameObject FramePrefab;

    private const string _beamsDataFilepath = @"\Data\beams.csv";
    private const string _bracesDataFilepath = @"\Data\braces.csv";
    private const string _columnsDataFilepath = @"\Data\columns.csv";
    private const string _jointDataFilepath = @"\Data\joints.csv";
    private const string _frameSectionFilepath = @"\Data\frameSection.csv";
    private const string _timeDisplacementDataFilepath = @"\Data\timeDisplacement.csv";
    private const string _groundDisplacementDataFilepath = @"\Data\groundDisplacement.csv";

    private const float _mmToFeet = 0.00328084f;

    //string = frame, List<string> = 2 x Joint names
    private Dictionary<string, List<string>> _connections = new Dictionary<string, List<string>>();

    private Dictionary<string, Joint> _joints = new Dictionary<string, Joint>();
    private Dictionary<string, GameObject> _frames = new Dictionary<string, GameObject>();

    private List<float[]> timeDisplacementData = new List<float[]>();
    private List<float[]> groundDisplacementData = new List<float[]>();
    private float _time = 0;
    private int _currentTimeDisplacementIndex = 0;

    // Start is called before the first frame update
    private void Start()
    {
        //get relative file paths
        string m_Path = Application.dataPath;
        Debug.Log(m_Path);
        string beamsDataFilepath = m_Path + _beamsDataFilepath;
        string bracesDataFilepath = m_Path + _bracesDataFilepath;
        string columnsDataFilepath = m_Path + _columnsDataFilepath;
        string jointDataFilepath = m_Path + _jointDataFilepath;
        string frameSectionFilepath = m_Path + _frameSectionFilepath;
        string timeDisplacementDataFilepath = m_Path + _timeDisplacementDataFilepath;
        string goundDisplacementDataFilepath = m_Path + _groundDisplacementDataFilepath;

        //get time displacement
        var strTimeDisplacementData = ReadCSV(timeDisplacementDataFilepath);
        for (int i = 3; i < strTimeDisplacementData.Count; i++)
        {
            var row = strTimeDisplacementData[i];
            List<float> floats = new List<float>();
            foreach (var str in row)
            {
                floats.Add(float.Parse(str));
            }
            timeDisplacementData.Add(floats.ToArray());
        }

        //get time displacement
        var strGroundDisplacementData = ReadCSV(goundDisplacementDataFilepath);
        for (int i = 3; i < strTimeDisplacementData.Count; i++)
        {
            var row = strTimeDisplacementData[i];
            List<float> floats = new List<float>();
            foreach (var str in row)
            {
                floats.Add(float.Parse(str));
            }
            groundDisplacementData.Add(floats.ToArray());
        }

        //get frame sections, this doens't need to be global
        var frameSectionData = ReadCSV(frameSectionFilepath);
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
        var jointData = ReadCSV(jointDataFilepath);
        for (int i = 3; i < jointData.Count; i++) //formated to start at index 3
        {
            //0 = name, 2 = Story, 5 = X, 7 = Z (translate to Y for Unity purposes)
            var row = jointData[i];
            if (row.Length <= 7) { continue; }
            string name = row[0];
            if (_joints.ContainsKey(name)) { continue; }

            float X = float.Parse(row[5]);
            float Y = float.Parse(row[7]);//CAD Z Value
            var pos = new Vector3(X, Y, 0);
            //Debug.Log(name + ", " + pos);
            var go = GameObject.Instantiate(JointPrefab, pos, Quaternion.identity);

            //get story by stripping everything but numerics and converting to int
            int story = 0;
            string storyText = row[2];
            if (storyText.ToLower() != "base")
            {
                storyText = GetNumbers(row[2]);
                Debug.Log(storyText);
                int.TryParse(storyText, out story);
            }
            //setup Joint Object
            var joint = new Joint(name, pos, story, go);
            _joints.Add(name, joint);
        }

        //get geometry members into a dictionary by name with vertices as the value
        List<string[]> geometryData = new List<string[]>();
        var beams = ReadCSV(beamsDataFilepath); beams.RemoveRange(0, Mathf.Min(3, beams.Count));
        geometryData.AddRange(beams);
        var braces = ReadCSV(bracesDataFilepath); braces.RemoveRange(0, Mathf.Min(3, braces.Count));
        geometryData.AddRange(braces);
        var columns = ReadCSV(columnsDataFilepath); columns.RemoveRange(0, Mathf.Min(3, columns.Count));
        geometryData.AddRange(columns);
        for (int i = 0; i < geometryData.Count; i++)
        {
            //0 = name, 3 = start, 4 = end
            var parts = geometryData[i];
            string name = parts[0];
            if (_connections.ContainsKey(name)) { continue; }

            string start = parts[3];
            string end = parts[4];

            //skip if the joints don't exist (they should exist)
            if (!_joints.ContainsKey(start) || !_joints.ContainsKey(end)) { continue; }

            //create member
            _connections.Add(name, new List<string>() { start, end });
            var frame = GameObject.Instantiate(FramePrefab);
            frame.name = "Frame " + name;
            _frames.Add(name, frame);

            //update frame section
            //Debug.Log(name);
            if (!frameSections.ContainsKey(name)) { continue; }
            var frameSection = frameSections[name];
            //Debug.Log(frameSection[2]);
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
        _time += Time.deltaTime;
        if (_time >= AnimationSpeed)
        {
            _time = 0;
            var displacement = timeDisplacementData[_currentTimeDisplacementIndex];
            _currentTimeDisplacementIndex++;//increment so that next update we get the next frame
            if (_currentTimeDisplacementIndex >= timeDisplacementData.Count) { _currentTimeDisplacementIndex = 0; } //loop

            var groundDisplacement = groundDisplacementData[_currentTimeDisplacementIndex];

            float ground = groundDisplacement[1];

            //update position by level
            foreach (var kvp in _joints)
            {
                var joint = kvp.Value;
                var adjust = displacement[joint.Story];
                var pos = joint.Position;
                pos.x += adjust * _mmToFeet;
                pos.x += ground * _mmToFeet;
                joint.GameObject.transform.position = pos;
            }
        }

        //Update members based on joints
        foreach (KeyValuePair<string, GameObject> kvp in _frames)
        {
            var name = kvp.Key;
            var frame = kvp.Value;
            var jointNames = _connections[name];
            var startJoint = _joints[jointNames[0]];
            var endJoint = _joints[jointNames[1]];
            var startGO = startJoint.GameObject;
            var endGO = endJoint.GameObject;

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

    private static string GetNumbers(string input)
    {
        return new string(input.Where(c => char.IsDigit(c)).ToArray());
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

public class Joint
{
    public Vector3 Position;
    public string Name;
    public int Story;
    public GameObject GameObject;

    public Joint(string name, Vector3 position, int story, GameObject gameObject)
    {
        Position = position;
        Name = name;
        Story = story;
        GameObject = gameObject;
        GameObject.name = "Joint " + name;
    }
}