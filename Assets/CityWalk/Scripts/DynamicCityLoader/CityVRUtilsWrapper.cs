using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
namespace CitySceneLoader {
    using System.Runtime.InteropServices;

    internal class CityVRUtilsWrapper {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || WIN64 || _WIN64
        // Encrypt
        [DllImport("cityvr-utils-unity", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void JRDYUTIUSH(ulong dataPtr, int len);

        // Decrypt
        [DllImport("cityvr-utils-unity", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VULBICVIOO(ulong dataPtr, int len);
#else
    internal void encrypt(byte[] data, int len) {
//        throw new NotImplementedException(" plugin not supported on current platform");
    }

    internal void decrypt(byte[] data, int len) {
  //      throw new NotImplementedException(" plugin not supported on current platform");
    }
#endif
    }
}

