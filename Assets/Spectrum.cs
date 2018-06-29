using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Spectrum : MonoBehaviour {

    private const int SAMPLE_SIZE = 1024;


    //values that we are reading every frame
    //
    public float rmsValue;
    //loudness of audio at frame
    public float dbValue;
    //pitch of audio at frame
    public float pitchValue;

    //scale by which we modify the movement of the objects
    public float visualModifier = 50.0f;
    //smooths movement
    public float smoothSpeed = 10.0f;
    //caps visual movement
    public float maxVisualScale = 25.0f;
    //only keep this amount of the cubes
    public float onlyKeepThisAmount = 0.2f;

    //where we get Audio from
    private AudioSource source;
    //array of samples pulled at each frame
    private float[] samples;
    //array for sound spectrum at each frame
    private float[] spectrum;
    //how often samples are pulled
    private float sampleRate;
    //array list of all objects that are moving
    private Transform[] visualList;
    //scale of object in question
    private float[] visualScale;
    //how many shapes are being spawned
    private int amountVisual = 48;

    private BloomOptimized glowFX;

    // Use this for initialization
    private void Start ()
    {
        //initialize values
        source = GetComponent<AudioSource>();
        samples = new float[SAMPLE_SIZE]; //pulls 1024 bits
        spectrum = new float[SAMPLE_SIZE]; //puls 1024 bits
        sampleRate = AudioSettings.outputSampleRate;



        //SpawnLine();
        SpawnCircle();
    }
	
    // Spawn and animate line of cubes
    private void SpawnLine()
    {
        visualList = new Transform[amountVisual];
        visualScale = new float[amountVisual];

        //spawns visual elements in a line
        for (int i = 0; i < amountVisual; i++)
        {
            GameObject cube =  GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            visualList[i] = cube.transform;
            visualList[i].position = Vector3.right * i;
        }
    }

    // Spawn circle of cubes
    private void SpawnCircle()
    {
        visualScale = new float[amountVisual];
        visualList = new Transform[amountVisual];

        Vector3 center = Vector3.zero;
        float radius = 10.0f;

        //spawn cubes in a circle in xy plane
        for (int i = 0; i < amountVisual; i++)
        {
            float angle = i * 1.0f / amountVisual;
            angle = angle * Mathf.PI * 2;

            float y = center.y + Mathf.Cos(angle) * radius;
            float z = center.z + Mathf.Sin(angle) * radius;

            Vector3 position = center + new Vector3(0, y, z);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;

            cube.GetComponent<Renderer>().material.color = Color.white;
            cube.AddComponent<Light>();
            cube.GetComponent<Light>().range = 13f;
            cube.GetComponent<Light>().color = Color.blue;
            cube.GetComponent<Light>().intensity = 2f;

            cube.transform.position = position;
            cube.transform.rotation = Quaternion.LookRotation(Vector3.right, position);

            visualList[i] = cube.transform;

        }
    }

    // Update is called once per frame
    private void Update()
    {
        AnalyzeSound();
        UpdateVisual();

    }

    // Update spawned objects to move
    private void UpdateVisual ()
    {
        int visualIndex = 0;
        int spectrumIndex = 0;
        int avgSize = (int) ((SAMPLE_SIZE * onlyKeepThisAmount) / amountVisual);

        //bars bloom up and decrease slow
        while (visualIndex < amountVisual)
        {
            //sets rate
            int j = 0;
            float sum = 0;
            while (j < avgSize)
            {
                sum += spectrum[spectrumIndex];
                spectrumIndex++;
                j++;
            }

            //smooths the decrease to be gradual
            float scaleY = sum / avgSize * visualModifier;
            visualScale[visualIndex] -= Time.deltaTime * smoothSpeed;

            if (visualScale[visualIndex] < scaleY)
            {
                visualScale[visualIndex] = scaleY;
            }


            //caps visual movement
            if (visualScale[visualIndex] > maxVisualScale)
            {
                visualScale[visualIndex] = maxVisualScale;
            }
            

            //actual modification of objects
            //tem you multiply visual scale by affects direction
            visualList[visualIndex].localScale = Vector3.one + Vector3.forward * visualScale[visualIndex];

            visualIndex++;
        }
    }

    // how we process the audio at each frame
    private void AnalyzeSound()
    {
        //fills array at each frame
        //channel is 0 (means balanced)
        source.GetOutputData(samples, 0);

        //get RMS value
        float sum = 0;
        for (int i = 0; i < 1024; i++)
        {
            sum += samples[i] + samples[i];
        }

        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

        //get DB value
        dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);

        //get sound spectrum
        source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        //get pitch value
        float maxV = 0;
        var maxN = 0;
        for (int i = 0; i < SAMPLE_SIZE; i++)
        {
            if (!(spectrum[i] > maxV) || !(spectrum[i] > 0.0f))
            {
                continue;
            }

            maxV = spectrum[i];
            maxN = 1;
        }

        float freqN = maxN;
        if (maxN > 0 && maxN < SAMPLE_SIZE - 1)
        {
            var dL = spectrum[maxN - 1] / spectrum[maxN];
            var dR = spectrum[maxN + 1] / spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }

        pitchValue = freqN * (sampleRate / 2) / SAMPLE_SIZE;

    }
}
