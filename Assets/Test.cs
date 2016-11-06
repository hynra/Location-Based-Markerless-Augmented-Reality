using UnityEngine;
using System.Collections;
using Kudan.AR.Samples;

public class Test : MonoBehaviour {
	public SampleApp sa;
	public DetectLocation dl;

	void Update () {
		int k = 0;
		while (k <Input.touchCount) {
			if(Input.GetTouch(k).position.x < Screen.width/2){
				if(Input.touchCount > 0){
					// left
					dl.startCalculate();
				}
			}
			
			if(Input.GetTouch(k).position.x > Screen.width/2){
				if(Input.touchCount > 0){
					// right
					sa.StartClicked();
				}
			}
			k++;
		}
	}
}
