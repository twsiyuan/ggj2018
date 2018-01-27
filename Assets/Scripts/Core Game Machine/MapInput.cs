﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Map))]
public class MapInput : MonoBehaviour, IDragSensorManager
{	
	public class SelectStartEventArgs : System.EventArgs{
		public IStation Station {
			get;
			set;
		}
	}

	public class SelectEndEventArgs : System.EventArgs{
		public IStation[] Stations {
			get;
			set;
		}
	}

	public event System.EventHandler SelectStarted;

	public event System.EventHandler SelectEnded;

    #region [ Fields / Properties - Sensor]

	Map map;
    
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private int start = -1;

    [SerializeField]
    private int end = -1;

    [SerializeField]
    private Image[] marks;

    [SerializeField]
    private Image[] imageLinkers;

    [SerializeField]
    private EmptyGraphic[] sensors;

    [SerializeField]
    private List<int> busTargets = new List<int>();

    private Dictionary<string, Image> linkers = new Dictionary<string, Image>();

    private Vector2 pressPos;
    
    #endregion


    #region [ Sensors ]
    
    void Start()
    {
        InitTable();

        InitLinkers();

        InitSensors();
    }

    private void InitTable()
    {
        map = GetComponent<Map>();
    }

    private void InitLinkers()
    {
        foreach (var linker in imageLinkers)
            linkers.Add(linker.name, linker);

        imageLinkers = null;
    }
    
    private void InitSensors()
    {
        var id = 0;
		foreach (var s in this.map.GetAllStations()) {
			var g = s.Transform.GetComponent<MaskableGraphic> ();
			TableSlotDragSensor.Init (g, this, id++);
		}

        sensors = null;
    }

    public void RegisterSensor(int sensorID)
    {
        var station = map.GetStation(sensorID);
        if(!station.IsMainStation)
            return;

        if(start == end)
            end = -1;

        start = sensorID;

        RegisterSensorHook();
    }

    public void OverlapSensor(int sensorID)
    {
        if(start == -1)
            return;

        if(!IsNeighbor(sensorID))
            return;

        end = sensorID;

        var match = string.Format("{0}-{1}", Mathf.Min(start, end), Mathf.Max(start, end));
        EnableLinker(match, true);

        start = end;

        busTargets.Add(end);

        OverlapSensorHook();
    }

    private void EnableLinker(string index, bool enable)
    {
		if (linkers.ContainsKey(index))
		{
        	var img = linkers[index];
        	img.color = enable? new Color32(150, 150, 150, 255) : new Color32(150, 150, 150, 50);
		}
    }

    bool IsNeighbor(int id)
    {
        var station = map.GetStation(id);
        return station.IsNeighbor(start);
    }

    public void SplitSensor(int sensorID)
    {
        if(end == -1)
            return;

        SplitSensorHook();

        end = -1;
    }

    public void RemoveSensor(int sensorID)
    {
        RemoveSensorHook();

        start = -1;

        end = -1;
    }

    public virtual void DragSensor(Vector2 pos) { }

    protected virtual void RegisterSensorHook()
    { 
         busTargets.Add(start);

		if (this.SelectStarted != null) {
			this.SelectStarted (this, new SelectStartEventArgs(){
				Station = map.GetStation(start),
			});
		}
    }

    protected virtual void OverlapSensorHook()
    {

    }

    protected virtual void SplitSensorHook() { }

    protected virtual void RemoveSensorHook()
    {
		if (this.SelectEnded != null) {
			this.SelectEnded (this, new SelectEndEventArgs(){
				Stations = busTargets.Select(v => map.GetStation(v)).ToArray(),
			});
		}

		#if UNITY_EDITOR
		Debug.LogFormat ("Selected Bus: {0}", string.Join(", ", this.busTargets.Select(v => v.ToString()).ToArray()));
		#endif

        busTargets.Clear();

        foreach (var linker in linkers)
            EnableLinker(linker.Key, false);
    }
    
    #endregion
    
}