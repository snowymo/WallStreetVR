using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Simplygon.Unity.EditorPlugin;
using UnityEditor;

public class SimplygonSubmitter : EditorWindow
{

    public enum MeshType
    {
        Buildings,
        Ground,
    }

    private struct ModelInfo
    {
        public Transform Parent;

        public int MinRow;
        public int MinCol;

        public int MaxRow;
        public int MaxCol;

        public int Count;

        public List<List<GameObject>> MeshGroups;
    }

    private static readonly Dictionary<MeshType, Regex> regexDict = new Dictionary<MeshType, Regex>(){
        { MeshType.Buildings, new Regex("region_(\\d+)_(\\d+)_\\d+", RegexOptions.Compiled) },
        { MeshType.Ground, new Regex("\\w+_(\\d+)_(\\d+)", RegexOptions.Compiled) }
    };

    private string cityName = "NewSF";
    private MeshType meshType;
    private string lastConfigPath;
    private int combineSize = 2;
    private int maxConcurrentJob = 2;
    private float maxSubmissionInterval = 300;
    private bool queuePaused = true;

    private Vector2 scrollPosition;

    private ModelInfo modelInfo;

    private Queue<Transform> submissionQueue = new Queue<Transform>();
    private volatile bool isSubmitting;
    private float lastSubmissionTime = 0;

    [MenuItem("Citywalk/Simplygon Submitter")]
    private static void OpenWindow()
    {
        EditorWindow.GetWindow<SimplygonSubmitter>();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        cityName = EditorGUILayout.TextField("City Name", cityName);
        meshType = (MeshType)EditorGUILayout.EnumPopup("Mesh Type", meshType);
        combineSize = EditorGUILayout.IntField("Combine Size", combineSize);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Analyze"))
        {
            parseModelInfo();
        }

        if (GUILayout.Button("Organize"))
        {
            organizeModels();
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();

        if (GUILayout.Button("Add Selection To Queue"))
        {
            addToQueue();
        }

        if (submissionQueue.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();

            if (queuePaused)
            {
                if (GUILayout.Button("Start"))
                {
                    queuePaused = false;
                    Repaint();
                }
            }
            else
            {
                if (GUILayout.Button("Pause"))
                {
                    queuePaused = true;
                    Repaint();
                }
            }

            if (GUILayout.Button("Clear"))
            {
                queuePaused = true;
                submissionQueue.Clear();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (submissionQueue.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Submit Queue");
            EditorGUI.indentLevel++;

            foreach (var item in submissionQueue)
            {
                EditorGUILayout.LabelField(item.name);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        EditorGUILayout.ObjectField("Parent", modelInfo.Parent, typeof(GameObject), true);
        EditorGUILayout.LabelField("Child Count", modelInfo.Count.ToString());
        EditorGUILayout.LabelField("Rows", modelInfo.MinRow + "-" + modelInfo.MaxRow);
        EditorGUILayout.LabelField("Columns", modelInfo.MinCol + "-" + modelInfo.MaxCol);

        if (modelInfo.MeshGroups != null)
        {
            EditorGUILayout.LabelField("Mesh Groups");
            EditorGUI.indentLevel++;
            for (int i = 0; i < modelInfo.MeshGroups.Count; i++)
            {
                EditorGUILayout.LabelField(i.ToString());
                EditorGUI.indentLevel++;
                for (int j = 0; j < modelInfo.MeshGroups[i].Count; j++)
                {
                    EditorGUILayout.ObjectField(j.ToString(), modelInfo.MeshGroups[i][j], typeof(GameObject), true);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void Update()
    {
        var jobManager = SharedData.Instance.GeneralManager.JobManager;
        if (jobManager == null)
        {
            return;
        }

        if (!queuePaused &&
            !isSubmitting &&
            submissionQueue.Count > 0 &&
                (jobManager.ProcessingJobCount < maxConcurrentJob ||
                 (Time.time - lastSubmissionTime) > maxSubmissionInterval)
            )
        {
            var item = submissionQueue.Dequeue();
            submitItem(item);
            lastSubmissionTime = Time.time;
            Repaint();
        }
    }

    private void parseModelInfo()
    {
        var topSelections = Selection.GetTransforms(SelectionMode.TopLevel);
        var selections = Selection.GetTransforms(SelectionMode.Deep);
        if (selections.Length == 0)
        {
            return;
        }

        modelInfo = new ModelInfo()
        {
            Parent = (topSelections.Length == 1 ? topSelections[0] : null),
            MinRow = int.MaxValue,
            MinCol = int.MaxValue,
            MaxRow = 0,
            MaxCol = 0,
            Count = 0,

            MeshGroups = new List<List<GameObject>>()
        };

        var matrix = new Dictionary<int, Dictionary<int, List<GameObject>>>();
        Regex regex = regexDict[meshType];

        for (int i = 0; i < selections.Length; i++)
        {
            GameObject child = selections[i].gameObject;

            Match m = regex.Match(child.name);
            if (m.Success)
            {
                int row = int.Parse(m.Groups[1].Value);
                int col = int.Parse(m.Groups[2].Value);

                if (row < modelInfo.MinRow)
                {
                    modelInfo.MinRow = row;
                }
                if (row > modelInfo.MaxRow)
                {
                    modelInfo.MaxRow = row;
                }

                if (col < modelInfo.MinCol)
                {
                    modelInfo.MinCol = col;
                }
                if (col > modelInfo.MaxCol)
                {
                    modelInfo.MaxCol = col;
                }

                if (!matrix.ContainsKey(row))
                {
                    matrix[row] = new Dictionary<int, List<GameObject>>();
                }

                if (!matrix[row].ContainsKey(col))
                {
                    matrix[row][col] = new List<GameObject>();
                }
                matrix[row][col].Add(child);

                modelInfo.Count++;
            }
        }

        for (int i = modelInfo.MinRow; i <= modelInfo.MaxRow; i += combineSize)
        {
            for (int j = modelInfo.MinCol; j <= modelInfo.MaxCol; j += combineSize)
            {
                List<GameObject> group = new List<GameObject>();

                for (int a = 0; a < combineSize; a++)
                {
                    for (int b = 0; b < combineSize; b++)
                    {
                        if (matrix.ContainsKey(i + a) && matrix[i + a].ContainsKey(j + b))
                        {
                            group.AddRange(matrix[i + a][j + b]);
                        }
                    }
                }

                if (group.Count > 0)
                {
                    modelInfo.MeshGroups.Add(group);
                }
            }
        }
    }

    private void organizeModels()
    {
        if (modelInfo.Parent == null)
        {
            Debug.LogError("Must select only one parent.");
            return;
        }

        for (int i = 0; i < modelInfo.MeshGroups.Count; i++)
        {

            GameObject group = new GameObject(string.Format("{0}_{1}_g{2:000}", cityName, modelInfo.Parent.name, i));
            Undo.RegisterCreatedObjectUndo(group, "Creating Group");
            Undo.SetTransformParent(group.transform, modelInfo.Parent, "Parenting Group");

            for (int j = 0; j < modelInfo.MeshGroups[i].Count; j++)
            {
                Undo.SetTransformParent(modelInfo.MeshGroups[i][j].transform, group.transform, "Parenting Model");
            }
        }
    }

    private void addToQueue()
    {
        SharedData.Instance.Settings.DownloadAssetsAutomatically = true;

        var topSelections = Selection.GetTransforms(SelectionMode.TopLevel);

        if (topSelections == null || topSelections.Length == 0)
        {
            Debug.LogError("Must select some groups");
            return;
        }

        Array.Sort(topSelections, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        for (int i = 0; i < topSelections.Length; i++)
        {
            submissionQueue.Enqueue(topSelections[i]);
        }

        Repaint();
    }

    private void submitItem(Transform group)
    {
        /*
        if (!group.name.StartsWith(string.Format("{0}_{1}_g", cityName, group.parent.name)))
        {
            Debug.LogError("Not an expected group to submit: " + group.name);
            return;
        }
        */

        Selection.activeTransform = group;
        SharedData.Instance.SelectionManager.OnSelectionChange();
        if (SharedData.Instance.SelectionManager.SelectedPrefabs.Count != 0)
        {

            isSubmitting = true;

            string jobName = group.name;
            SharedData.Instance.GeneralManager.CreateJob(jobName,
                "myPriority",
                SharedData.Instance.SelectionManager.SelectedPrefabs,
                () =>
                {
                    Debug.Log("Simplygon job submitted: " + jobName);
                    isSubmitting = false;
                });
        }
        else
        {
            Debug.LogError("Simplygon cannot find any selected object for group " + group.name);
        }
    }
}
