using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Announcement : MonoBehaviour {

  /*
     You can use SunVox library freely, 
     but the following text should be included in your products (e.g. in About window):

     SunVox modular synthesizer
     Copyright (c) 2008 - 2018, Alexander Zolotov <nightradio@gmail.com>, WarmPlace.ru

     Ogg Vorbis 'Tremor' integer playback codec
     Copyright (c) 2002, Xiph.org Foundation
  */
  public static Announcement Instance;

  public Text Text;

  void Start ()
  {
    MaryTTS.Instance.Announce( "Attention all personnel, we're fucking screwed. " +
                               "Syndicate ship is approaching. " +
                               "Central Command doesn't give a damn, so I guess we're on our own." );
  }
  
  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    } //else gets destroyed by parent
  }

  public void InitAndPlay( byte[] sound )
  {
    try
    {
      int ver = SunVox.sv_init( "0", 44100, 2, 0 );
      if ( ver >= 0 )
      {
        int major = ( ver >> 16 ) & 255;
        int minor1 = ( ver >> 8 ) & 255;
        int minor2 = ( ver ) & 255;
        log( String.Format( "SunVox lib version: {0}.{1}.{2}", major, minor1, minor2 ) );

        SunVox.sv_open_slot( 0 );

        log( "Loading SunVox project from file..." );
        var path = "Assets/StreamingAssets/announcement.sunvox"; // This path is correct only for standalone
        if ( SunVox.sv_load( 0, path ) == 0 )
        {
          log( "Loaded." );
        }
        else
        {
          log( "Load error." );
          SunVox.sv_volume( 0, 256 );
        }

        //sampler module id 5 is hardcoded for demo
        SunVox.sv_sampler_load_from_memory(0, 5, sound, sound.Length, -1); 
        SunVox.sv_set_autostop( 0, 1 );
        SunVox.sv_play_from_beginning (0); //play announcement tune
        StartCoroutine( SpeakAnnouncement() );
      }
      else
      {
        log( "sv_init() error " + ver );
      }
    }
    catch ( Exception e )
    {
      log( "Exception: " + e );
    }
  }

  private IEnumerator SpeakAnnouncement() {
    yield return new WaitForSeconds( 3f );
//    SunVox.sv_stop (0);
      //sampler module id 5 is hardcoded for demo
    SunVox.sv_send_event (0, 0, 60, 128, 5 + 1, 0, 0);
//    SunVox.sv_send_event (0, 0, 64, 128, mod_num + 1, 0, 0);


    yield break;
  }

  private void log (string msg) {
    Debug.Log (msg);
    Text.text = Text.text + "\n" + msg;
  }

  private void OnDestroy () {
    if (!enabled) return;

    SunVox.sv_close_slot (0);
    SunVox.sv_deinit ();
  }

}