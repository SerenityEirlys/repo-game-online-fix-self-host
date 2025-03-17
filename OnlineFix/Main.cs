using HarmonyLib;
using IniParser;
using Photon.Realtime;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OnlineFix
{
    public class Main
    {
        private const int ErrorExitCode = 4919;
        private static string _appId;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("shell32.dll")]
        static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        public static void Init()
        {
            try
            {
                LoadEmbeddedAssemblies();
                _appId = GetAppIdFromConfig();
                ExecutePatch();
            }
            catch (Exception ex)
            {
                MessageBox(IntPtr.Zero, "Đã xảy ra lỗi: " + ex.Message, "Lỗi", 16U);
                Environment.Exit(ErrorExitCode);
            }
        }

        public static void InitWithBeepInEx()
        {
            Init();
        }

        private static void ExecutePatch()
        {
            try
            {
                MethodInfo method1 = typeof(LoadBalancingPeer).GetMethod("OpAuthenticate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method1 == null)
                {
                    MessageBox(IntPtr.Zero, "Không tìm thấy phương thức để patch!", "Lỗi", 16U);
                    Environment.Exit(ErrorExitCode);
                }

                Harmony harmony = new Harmony("auth.patch");
                MethodInfo method2 = typeof(Main).GetMethod("PatchOpAuthenticate", BindingFlags.Static | BindingFlags.NonPublic);
                if (method2 == null)
                {
                    MessageBox(IntPtr.Zero, "Không tìm thấy phương thức patch.", "Lỗi", 16U);
                    Environment.Exit(ErrorExitCode);
                }

                harmony.Patch(method1, new HarmonyMethod(method2));
                // Hiển thị thông báo thành công
                MessageBox(IntPtr.Zero, "Success || Discord Server: discord.gg/saolonkebi 〢" + _appId, "Thành công", 64U);

                // Mở tệp discord.url sau khi nhấn OK
                string discordUrlPath = Path.Combine(Directory.GetCurrentDirectory(), "discord.url");
                if (File.Exists(discordUrlPath))
                {
                    try
                    {
                        ShellExecute(IntPtr.Zero, "open", discordUrlPath, null, null, 1);
                    }
                    catch (Exception ex)
                    {
                        MessageBox(IntPtr.Zero, "Không thể mở tệp discord.url: " + ex.Message, "Cảnh báo", 48U);
                    }
                }
                else
                {
                    MessageBox(IntPtr.Zero, "Không tìm thấy tệp discord.url trong thư mục hiện tại!", "Lỗi", 16U);
                }
            }
            catch (Exception ex)
            {
                MessageBox(IntPtr.Zero, "Đã xảy ra lỗi: " + ex.Message, "Lỗi", 16U);
                Environment.Exit(ErrorExitCode);
            }
        }

        private static bool PatchOpAuthenticate(ref string appId, ref AuthenticationValues authValues)
        {
            appId = _appId;
            if (authValues != null)
            {
                authValues.AuthType = (CustomAuthenticationType)(int)byte.MaxValue;
            }
            return true;
        }

        private static void LoadEmbeddedAssemblies()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
            string[] assembliesToLoad = new string[]
            {
                "0Harmony.dll",
                "INIFileParser.dll"
            };

            foreach (string assemblyFileName in assembliesToLoad)
            {
                string resourceName = Array.Find(manifestResourceNames, name => name.EndsWith(assemblyFileName, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(resourceName))
                {
                    MessageBox(IntPtr.Zero, "Không tìm thấy tài nguyên nhúng " + assemblyFileName + "!", "Lỗi", 16U);
                    Environment.Exit(ErrorExitCode);
                }

                Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(resourceName);
                if (manifestResourceStream == null)
                {
                    MessageBox(IntPtr.Zero, "Không tìm thấy luồng tài nguyên nhúng " + assemblyFileName + "!", "Lỗi", 16U);
                    Environment.Exit(ErrorExitCode);
                }

                MemoryStream destination = new MemoryStream();
                try
                {
                    manifestResourceStream.CopyTo(destination);
                    Assembly.Load(destination.ToArray());
                }
                finally
                {
                    manifestResourceStream.Dispose();
                    destination.Dispose();
                }
            }
        }

        private static string GetAppIdFromConfig()
        {
            var parser = new FileIniDataParser();
            var iniData = parser.ReadFile("OnlineFix.ini");
            string appId = null;
            if (iniData["Main"] != null && iniData["Main"]["PhotonAppId"] != null)
            {
                appId = iniData["Main"]["PhotonAppId"].Trim();
            }

            if (string.IsNullOrEmpty(appId))
            {
                MessageBox(IntPtr.Zero, "Không tìm thấy PhotonAppId trong tệp cấu hình. Vui lòng đảm bảo nó được cài đặt chính xác", "Lỗi", 16U);
                Environment.Exit(ErrorExitCode);
            }

            return appId;
        }
    }
}