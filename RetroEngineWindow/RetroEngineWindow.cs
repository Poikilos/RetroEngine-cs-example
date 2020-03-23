#region License
/*
Exclusive use by Expert Multimedia
*/
#endregion License

#region Original Credits / License
//-----------------------------------------------------------------------------
//Copyright pending Jake Gustafson
//-----------------------------------------------------------------------------
#endregion Original Credits / License

using System;
using Tao.Sdl;
using System.Runtime.InteropServices;
using ExpertMultimedia;
using System.Windows.Forms;

namespace ExpertMultimedia {
	#region Class Documentation
	/// <summary>
	/// RetroEngine graphical client. Needs reference to
	/// ExpertMultimedia.dll
	/// Requires Tao assemblies and references.
	/// Putting them in the same folder is easiest.
	/// </summary>
	/// <remarks>
	/// To Escape (exit) push Esc then 'y' or click yes.
	/// Special thanks to David Hudson (jendave@yahoo.com)and
	/// Will Weisser (ogl@9mm.com) for the Rectangles example
	/// in the Tao svn repository.  See http://www.go-mono.com/tao
	/// </remarks>
	#endregion Class Documentation
	public unsafe class RetroEngineWindow {
		public static string sErrBy="RetroEngine SDL Window";
		private static string sFuncNow {
			get {
				try{return RetroEngine.statusq.sFuncNow;}
				catch (Exception exn) {e = e; return "";}
			}
			set {
				try{RetroEngine.statusq.sFuncNow=value;}
				catch (Exception exn) {e = e;}
			}
		}
		public static string sLastErr {
			get {
				try{return RetroEngine.statusq.Deq();}
				catch (Exception exn) {e = e; return "";}
			}
			set {
				try{RetroEngine.statusq.Enq(Base.DateTimePathString(true)+" -- "+sErrBy+", during "+sFuncNow+": "+value);}
				catch(Exception exn) {e = e;}
			}
		}
		private static string sLogLine {
			set { try{RetroEngine.statusq.Enq(value);}
				catch(Exception exn) {e = e;} }
		}
		//private string sErr="(none recorded)";
		//private static string sFuncNow="(before initialization)";
		//private bool bErr=false;
		//public bool bErrLog=true;
		public RetroEngine retroengine;
		public byte[] byarrKeysNow=null;
		int iTargetBPP;
		int iTargetWidth;
		int iTargetHeight;
		int iTargetBytesTotal;
		int iTargetChunks64Total;

		public bool UpdateScreenVars(int iBPP) {
			sFuncNow="UpdateScreenVars("+iBPP.ToString()+")";
			try {
				//NYI allow retroengine size to be larger and scrollable?
				// OR just make retroengine "windows" be scrollable
				iTargetBPP = iBPP;
				iTargetWidth = retroengine.Width;
				iTargetHeight = retroengine.Height;
				iTargetBytesTotal=iTargetBPP/8*iTargetWidth*iTargetHeight;
				iTargetChunks64Total=iTargetBytesTotal/8;
			}
			catch (Exception exn) {
				sLastErr="Exception error, can't read screen format--"+e.ToString();
				return false;
			}
			return true;
		}
		
		#region Run()
		/// <summary>
		/// 
		/// </summary>
		public void Run() {
			//sLogToFile=Environment.NewLine+"//  Comment:  Starting Window.  "+HTMLPage.DateTimePathString(true);
			bool bGood=true;
			retroengine=new RetroEngine(RetroEngine.sDataFolderSlash+"website.html",800,600);
			sFuncNow="Run()";
			int flags = (Sdl.SDL_HWSURFACE|Sdl.SDL_DOUBLEBUF|Sdl.SDL_ANYFORMAT);
			bGood=UpdateScreenVars(24); //called again after surface bitdepth is found
			bool bContinue = true;
			Random rand = new Random();
			string sFileTestMusic = RetroEngine.sDataFolderSlash+"music-test.ogg";
			string sFileTestSound = RetroEngine.sDataFolderSlash+"sound-test.wav";
			Sdl.SDL_Event sdleventX;
			try {
				int iResultInit = Sdl.SDL_Init(Sdl.SDL_INIT_EVERYTHING);
				IntPtr iptrSurfaceBackbuffer = Sdl.SDL_SetVideoMode(
					iTargetWidth, 
					iTargetHeight, 
					iTargetBPP, 
					flags);
				
				//debug NYI: replace with Tao.OpenAl
				
				int iResult = SdlMixer.Mix_OpenAudio(
					SdlMixer.MIX_DEFAULT_FREQUENCY, 
					(short) SdlMixer.MIX_DEFAULT_FORMAT, 
					2, 
					1024);
				/*
				IntPtr iptrChunkMusic = SdlMixer.Mix_LoadMUS(sFileTestMusic);
				IntPtr iptrChunkSound = SdlMixer.Mix_LoadWAV(sFileTestSound);
				SdlMixer.MusicFinishedDelegate delMusicFinished=new SdlMixer.MusicFinishedDelegate(this.MusicHasStopped);
				SdlMixer.Mix_HookMusicFinished(delMusicFinished);

				iResult = SdlMixer.Mix_PlayMusic (iptrChunkMusic, 2);
				if (iResult == -1) {
					sLastErr="Music Error: " + Sdl.SDL_GetError();
				}
				iResult = SdlMixer.Mix_PlayChannel(1,iptrChunkSound,1);
				if (iResult == -1) {
					sLastErr="Sound Error: " + Sdl.SDL_GetError();
				}
				*/

				int rmask = 0x00000000;//doesn't matter since shifted all the way right
				int gmask = 0x00ff0000;
				int bmask = 0x0000ff00;
				int amask = 0x000000ff;

				IntPtr videoInfoPointer = Sdl.SDL_GetVideoInfo();
				if(videoInfoPointer == IntPtr.Zero) {
					throw new ApplicationException(string.Format("Video query failed: {0}", Sdl.SDL_GetError()));
				}

				Sdl.SDL_VideoInfo videoInfo = (Sdl.SDL_VideoInfo)
					Marshal.PtrToStructure(videoInfoPointer, 
					typeof(Sdl.SDL_VideoInfo));

				Sdl.SDL_PixelFormat pixelFormat = (Sdl.SDL_PixelFormat)
					Marshal.PtrToStructure(videoInfo.vfmt, 
					typeof(Sdl.SDL_PixelFormat));
				try {
					UpdateScreenVars(pixelFormat.BitsPerPixel);
					retroengine.WriteLine("hw_available:"+videoInfo.hw_available);
					retroengine.WriteLine("wm_available:"+videoInfo.wm_available);
					retroengine.WriteLine("blit_hw:"+videoInfo.blit_hw);
					retroengine.WriteLine("blit_hw_CC:"+videoInfo.blit_hw_CC);
					retroengine.WriteLine("blit_hw_A:"+videoInfo.blit_hw_A);
					retroengine.WriteLine("blit_sw:"+videoInfo.blit_sw);
					retroengine.WriteLine("blit_hw_CC:"+videoInfo.blit_hw_CC);
					retroengine.WriteLine("blit_hw_A:"+videoInfo.blit_hw_A);
					retroengine.WriteLine("blit_fill:"+videoInfo.blit_fill);
					retroengine.WriteLine("video_mem:"+videoInfo.video_mem);
					retroengine.WriteLine("BitsPerPixel:"+pixelFormat.BitsPerPixel);
					retroengine.WriteLine("BytesPerPixel:"+pixelFormat.BytesPerPixel);
					retroengine.WriteLine("Rmask:"+pixelFormat.Rmask);
					retroengine.WriteLine("Gmask:"+pixelFormat.Gmask);
					retroengine.WriteLine("Bmask:"+pixelFormat.Bmask);
					retroengine.WriteLine("Amask:"+pixelFormat.Amask);
				}
				catch (Exception exn) {
					sLastErr="Exception error displaying screen info--"+e.ToString();
				}
				int numeventarr= 10;
				Sdl.SDL_Event[] eventarr = new Sdl.SDL_Event[numeventarr];
				//eventarr[0].type = Sdl.SDL_KEYDOWN;
				//eventarr[0].key.keysym.sym = (int)Sdl.SDLK_p;
				//eventarr[1].type = Sdl.SDL_KEYDOWN;
				//eventarr[1].key.keysym.sym = (int)Sdl.SDLK_z;
				//int iResult2 = Sdl.SDL_PeepEvents(eventarr, numeventarr, Sdl.SDL_ADDEVENT, Sdl.SDL_KEYDOWNMASK);
				//retroengine.WriteLine("Addevent iResult: " + iResult2);
				//TODO: discard clicks if interface is in a transition

				int iSDLKeys=65536;
				int iNow=0;
				int[] iarrAsciiOfSDLK=new int[iSDLKeys];
				try {
					//debug NYI finish this:
					iNow=(int)Sdl.SDLK_ASTERISK;
					if (iNow>=iSDLKeys) iSDLKeys=iNow+1;
					iarrAsciiOfSDLK[iNow]=42;
					iarrAsciiOfSDLK[Sdl.SDLK_0]=48;
					iarrAsciiOfSDLK[Sdl.SDLK_a]=97;
					iarrAsciiOfSDLK[Sdl.SDLK_RSHIFT]=-32;//since a - 32 is A
					iarrAsciiOfSDLK[Sdl.SDLK_LSHIFT]=-32;//since a - 32 is A
					//debug NYI the rest of the above integers
					retroengine.TextMessage("This is a long test sentence to test writing text over the edge of the screen.");
				}
				catch (Exception exn) {
					sLastErr="Exception while setting iarrAsciiOfSDLK; maximum array size needed was "+iSDLKeys.ToString()+"--"+e.ToString();
				}
				Sdl.SDL_WM_SetCaption(retroengine.sCaption,"default");
				Sdl.SDL_EnableKeyRepeat(0,9999);
				Sdl.SDL_EnableUNICODE(1);
				#region Main Event Loop
				while (bContinue == true) {
					Sdl.SDL_PumpEvents();
					iResult = Sdl.SDL_PollEvent(out sdleventX);
					try {
						if (sdleventX.type==Sdl.SDL_QUIT) {
							try {
								if (true) bContinue=false;//TODO: if (retroengine.ConfirmQuit()) bContinue=false;
									
							}
							catch (Exception exn) {
								sFuncNow="Run()";
								sLastErr="Exception error trying to confirm, so quitting without confirmation--"+e.ToString();
							}
						}
						else if (sdleventX.type==Sdl.SDL_KEYDOWN) {
							try {
								if (sdleventX.key.keysym.sym == (int)Sdl.SDLK_ESCAPE) {
									//((sdleventX.key.keysym.sym == (int)Sdl.SDLK_ESCAPE) ||
									//(sdleventX.key.keysym.sym == (int)Sdl.SDLK_q)) 
									try {
										if (true) bContinue=false; //TODO: if (retroengine.ConfirmQuit()) bContinue=false;
											
									}
									catch (Exception exn) {
										sFuncNow="Run()";
										sLastErr="Exception error trying to confirm Esc (escape), so quitting without confirmation--"+e.ToString();
									}
								}
								//else if (sdleventX.key.keysym.sym == (int)Sdl.SDLK_p) {
								//	retroengine.WriteLine("Key p event was added");
								//}
								//else if (sdleventX.key.keysym.sym == (int)Sdl.SDLK_z) {
								//	retroengine.WriteLine("Key z event was added");
								//}
								if ((int)sdleventX.key.keysym.sym!=0) {
									retroengine.KeyEvent(sdleventX.key.keysym.sym, sdleventX.key.keysym.unicode, true);
									//string sNow=Char.ToString((char)sdleventX.key.keysym.unicode);
									//short shNow=sdleventX.key.keysym.unicode;
									//retroengine.WriteLine("key="+shNow.ToString()+"    ");
								}
							}
							catch (Exception exn) {
								sLastErr="Exception during KEYDOWN processing--"+e.ToString();
							}
						}
						else if (sdleventX.type == Sdl.SDL_KEYUP) {
							//Sdl.SDL_
							//byarrKeysNow=Sdl.SDL_GetKeyState(256); //debug internationalization
							//ushort wKey=Base.GetUnsignedLossless(sdleventX.key.keysym.scancode);
							//sdleventX.key.type
							retroengine.KeyEvent(sdleventX.key.keysym.sym, 0, false);
							//retroengine.WriteLine("keyup="+sdleventX.key.keysym.sym.ToString()+"    ");
							
						}
						else if (sdleventX.type == Sdl.SDL_MOUSEMOTION) {
							retroengine.WriteLine("mousemove=("+sdleventX.motion.x+","+sdleventX.motion.y+")");
						}
						else if (sdleventX.type == Sdl.SDL_MOUSEBUTTONDOWN) {
							retroengine.WriteLine("mousedown=("+sdleventX.button.x+","+sdleventX.motion.y+")");
						}
						else if (sdleventX.type == Sdl.SDL_MOUSEBUTTONUP) {
							retroengine.WriteLine("mouseup=("+sdleventX.button.x+","+sdleventX.motion.y+")");
						}
					}
					catch (Exception exn) {
						sLastErr="Exception during controller input processing--"+e.ToString();
					}

					try {
						//if (iTryNow<iTargetBytesTotal){
						//	byarrTemp[iTryNow]=255;
						//	iTryNow++;
						//}
						//if (iTryNow%30!=0) continue;
						
						//debug performance: run Draw code below as separate thread.
						bGood=retroengine.DrawFrame();
						iResult = Sdl.SDL_LockSurface(iptrSurfaceBackbuffer);
						if (iTargetBPP==32) {
							fixed (byte* lpSrc=retroengine.gbScreen.byarrData) {
								byte* lpSrcNow=lpSrc;
								Sdl.SDL_Surface* lpsurface=(Sdl.SDL_Surface*)iptrSurfaceBackbuffer;
								byte* lpDestNow= (byte*)lpsurface->pixels;
								for (int i=iTargetChunks64Total; i!=0; i--) {
									*((UInt64*)lpDestNow) = *((UInt64*)lpSrcNow);
										lpDestNow+=8;
										lpSrcNow+=8;
								}
							}
						}
						else if (iTargetBPP==24) {
							fixed (byte* lpSrc=retroengine.gbScreen.byarrData) {
								byte* lpSrcNow=lpSrc;
								Sdl.SDL_Surface* lpsurface=(Sdl.SDL_Surface*)iptrSurfaceBackbuffer;
								byte* lpDestNow= (byte*)lpsurface->pixels;
								for (int i=(iTargetBytesTotal/3)-1; i!=0; i--) {//-1 to avoid overwrite problems
									*((UInt32*)lpDestNow) = *((UInt32*)lpSrcNow);
										lpDestNow+=3;
										lpSrcNow+=4;
								}
								//now copy the odd pixel:
								*lpDestNow=*lpSrcNow;
								lpDestNow++;
								lpSrcNow++;
								*lpDestNow=*lpSrcNow;
								lpDestNow++;
								lpSrcNow++;
								*lpDestNow=*lpSrcNow;
							}
						}
						else if (iTargetBPP<24) {
							//NYI add other bit depths.
							sLastErr="Wrong bit depth.";
							bContinue=false;
						}
						iResult = Sdl.SDL_UnlockSurface(iptrSurfaceBackbuffer);
						iResult = Sdl.SDL_Flip(iptrSurfaceBackbuffer);
					} 
					catch (Exception) {
						 sLastErr="Exception error copying buffer to screen";
					}
				}//end while bContinue
				#endregion Main Event Loop
				
				if (iTargetBPP<24) {
					MessageBox.Show("You must change your Display settings to 32-bit (recommended) or at least 24-bit to run this program.");
				}
			}
			catch (Exception exn) {
				sFuncNow="Run()";
				sLastErr="Exception error--"+e.ToString();
				//Sdl.SDL_Quit();
			}
			try {
				if (true) {//TODO: if (RetroEngine.statusq.TrackedErrors>0) {
					RetroEngine.statusq.Enq(".statusq {"
					                         + RetroEngine.statusq.DumpTrackedErrorsToStyleNotation()
					                         + "}"
					                        ); 
					
				}
			
			}
			catch (Exception exn) {
				sFuncNow="Run, after main event loop";
				sLastErr="Exception error saving exception stats--"+e.ToString();
			}
		}
		#endregion Run()

		private void MusicHasStopped() {
			try {
				retroengine.WriteLine("The Music has stopped!");
			}
			catch (Exception exn) {
				sLastErr="Exception error accessing retroengine--"+e.ToString();
			}
		}

		#region Main()
		[STAThread]
		static void Main() {
			RetroEngineWindow retroenginewindow = new RetroEngineWindow();
			retroenginewindow.Run();
		}
		#endregion Main()
	}
}
