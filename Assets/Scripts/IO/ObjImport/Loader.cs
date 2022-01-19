using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

	public string filelocation;

	// Use this for initialization
	void Start () {

        ObjLoader.LoadObjFile(filelocation);

	}
	

}
