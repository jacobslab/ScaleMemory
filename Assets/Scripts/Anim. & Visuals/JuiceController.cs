using UnityEngine;
using System.Collections;

public class JuiceController : MonoBehaviour
{

	public delegate void JuiceTogglerDelegate(bool isJuicy);
	public JuiceTogglerDelegate ToggleJuice;


	//soundtrack & sound
	//AudioController deals with juice directly

	//PARTICLES
	//special object particles
	//feedback particles & explosion
	//fireworks and coins in GUI

	//ANIMATIONS
	//treasure chest opening -- taken care of in DefaultObject.cs
	//object small to large -- taken care of in SpawnableObject.cs

	//MAKE SURE THIS HAPPENS BEFORE THE FIRST LOG & OTHER CALCULATIONS.
	//CALLED IN EXPERIMENT --> AWAKE()
	public void Init()
	{
		//ToggleJuice += Toggle;
		//ToggleJuice(Config_CoinTask.isJuice);
	}


	public static void PlayParticles(ParticleSystem particles)
	{
			particles.Stop();
			particles.Play();
		
	}

}
