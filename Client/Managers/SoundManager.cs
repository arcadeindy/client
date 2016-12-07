using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CoinPoker.Managers
{
    public class SoundManager
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        public static void Play(string name){
            Assembly assembly;

            int NewVolume = ((ushort.MaxValue / 10) * Properties.Settings.Default.SoundValue/10);
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);

            assembly = Assembly.GetExecutingAssembly();
            SoundPlayer sp = new SoundPlayer("Assets/UnitySound/" + name + ".wav");
            sp.Play();
        }
    }
}
