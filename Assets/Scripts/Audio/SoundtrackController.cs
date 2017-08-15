using UnityEngine;
using System.Collections;

public class SoundtrackController : MonoBehaviour {

	public AudioSource[] MainGameSongs;
	AudioSource currentSong;
	int currentSongIndex = -1; //when we play the next song, currentSongIndex will ++;

	// Use this for initialization
	void Start () {
		if (Config.isSoundtrack) {
			InitSoundtrack ();
		}
	}

	void InitSoundtrack(){
		if (MainGameSongs.Length > 0) {
			currentSong = MainGameSongs[0];
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Config.isSoundtrack) {
			CheckIfSongOver ();
		}
	}

	void CheckIfSongOver(){
		if (currentSong != null) {
			if (!currentSong.isPlaying) {
				PlayNextSong ();
			}
		}
	}

	void PlayNextSong(){
		currentSongIndex++;

		if (currentSongIndex >= MainGameSongs.Length) {
			currentSongIndex = 0;
		}

		currentSong = MainGameSongs [currentSongIndex];
		PlayAudio (currentSong);
	}

	//TODO: move to juice controller?
	public static void PlayAudio(AudioSource audio){
		if (Config.isJuice) {
			audio.Stop ();
			audio.Play (); 
		}
	}

}
